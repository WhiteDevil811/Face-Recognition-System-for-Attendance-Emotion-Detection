using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using workshop2_w;
using WinFormsTimer = System.Windows.Forms.Timer;
using System.Drawing.Drawing2D;

using Emgu.CV.Dnn;

namespace RTSP_Tester
{
    public partial class Form1 : Form
    {
        private VideoCapture capture;
        private bool isRunning = false;
        private CascadeClassifier faceClassifier;
        private LBPHFaceRecognizer recognizer;
        private string lastRecognizedId = null;
        private readonly object _dbLock = new object();
        private List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        private List<string> labels = new List<string>(); // face file IDs
        private Dictionary<string, string> userNames = new Dictionary<string, string>(); // face ID -> actual name
        private Dictionary<string, DateTime> lastAttendanceTime = new Dictionary<string, DateTime>();
        private Dictionary<string, bool> isCheckedIn = new Dictionary<string, bool>();
        private int captureCount = 0;
        private const int MAX_CAPTURES = 10;
        private WinFormsTimer clearUITimer;
        private bool isDarkMode = false;
        private Button btnTheme;
        private Queue<double> accuracyHistory = new Queue<double>();
        private Net emotionNet;
        // Standard FER+ ONNX output order
        private string[] emotionLabels = {
            "Neutral", "Happy", "Surprise", "Sad",
            "Angry", "Disgust", "Fear", "Contempt"
        };
        private bool isMessageShowing = false;
        private readonly TimeSpan ATTENDANCE_COOLDOWN = TimeSpan.FromSeconds(30);
        private string rtspUrl = "rtsp://admin:yisheng%40@192.168.1.64:554/Streaming/Channels/101";
        private string connectionString = "server=127.0.0.1;uid=root;pwd=;database=workshop2;";
        private WinFormsTimer timer;
        bool attendanceHandledThisFrame = false;
        private bool isShowingMessage = false;
        private int frameCount = 0;
        public Form1()
        {
            InitializeComponent();

            try
            {
                string modelPath = Path.Combine(Application.StartupPath, "emotion-ferplus-8.onnx");
                emotionNet = DnnInvoke.ReadNetFromONNX(modelPath);
                statusLabel.Text += " | Emotion model loaded";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load ONNX model: " + ex.Message);
            }

            // Remove: this.Load += Form1_Load;
            // Initialize timers here
            timer = new WinFormsTimer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();

            clearUITimer = new WinFormsTimer();
            clearUITimer.Interval = 10000;
            clearUITimer.Tick += (s, e) => ClearUIFields();

            this.Shown += Form1_Shown; // start streaming after form shows
            this.FormClosing += Form1_FormClosing;

            InitThemeButton();
            faceClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");

            // Optional: status label
            statusLabel.Text = "Initializing system...";
        }

       
        
        private async void Form1_Shown(object sender, EventArgs e)
        {
            await Task.Run(() => {
                LoadTrainedFaces(); // heavy disk I/O
                StartStreaming();   // start RTSP camera
            });
        }

        private void ClearUIFields()
        {
            clearUITimer.Stop(); // Stop so it doesn't repeat

            // Safety check for UI thread
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)ClearUIFields);
                return;
            }

            txt_matric.Clear();
            txt_name.Clear();
            txt_ptj.Clear();
            txt_in.Clear();
            txt_out.Clear();
            label8.Text = "Waiting for detection...";
            listBox1.Items.Clear();
            lastRecognizedId = null; // Important: allows the same person to be detected again
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            label_time.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }





        private void StartStreaming()
        {
            if (isRunning || capture != null)
                return;

            try
            {
                capture = new VideoCapture(rtspUrl, VideoCapture.API.Ffmpeg);
                capture.Set(CapProp.Buffersize, 1); // reduce lag

                if (!capture.IsOpened)
                {
                    this.BeginInvoke((MethodInvoker)(() =>
                    {
                        statusLabel.Text = "❌ Failed to open RTSP";
                    }));
                    return;
                }

                isRunning = true;
                this.BeginInvoke((MethodInvoker)(() => { statusLabel.Text = "🟢 Connected"; }));

                Mat frame = new Mat();
                int frameCount = 0;

                Task.Run(() =>
                {
                    while (isRunning)
                    {
                        try
                        {
                            if (!capture.Read(frame) || frame.IsEmpty)
                            {
                                Thread.Sleep(5);
                                continue;
                            }

                            frameCount++;

                            // ====== Safe clone for UI ======
                            Mat uiFrame = frame.Clone();

                            // Update PictureBox every 3 frames
                            if (frameCount % 3 == 0)
                            {
                                // Offload UI conversion to worker thread
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        using (var img = uiFrame.ToImage<Bgr, byte>())
                                        {
                                            Bitmap bmp = img.ToBitmap(); // managed copy
                                            pictureBox1.BeginInvoke((MethodInvoker)(() =>
                                            {
                                                pictureBox1.Image?.Dispose();
                                                pictureBox1.Image = bmp; // assign safely
                                            }));
                                        }
                                    }
                                    finally
                                    {
                                        uiFrame.Dispose(); // dispose after use
                                    }
                                });
                            }
                            else
                            {
                                uiFrame.Dispose(); // dispose if not used for UI
                            }

                            // ====== Heavy processing every 10 frames ======
                            if (frameCount % 10 == 0)
                            {
                                Mat processFrame = frame.Clone(); // clone for processing

                                Task.Run(() =>
                                {
                                    try
                                    {
                                        using (var currentFrame = processFrame.ToImage<Bgr, byte>())
                                        using (var grayFrame = currentFrame.Convert<Gray, byte>())
                                        {
                                            Rectangle[] faces = faceClassifier.DetectMultiScale(
                                                grayFrame, 1.3, 5, Size.Empty, Size.Empty);

                                            foreach (var rect in faces)
                                            {
                                                if (rect.Width <= 0 || rect.Height <= 0 ||
                                                    rect.X < 0 || rect.Y < 0 ||
                                                    rect.Right > currentFrame.Width || rect.Bottom > currentFrame.Height)
                                                    continue;

                                                currentFrame.Draw(rect, new Bgr(Color.Red), 2);

                                                string displayName = "Unknown";
                                                string currentId = null;

                                                // Face Recognition
                                                if (recognizer != null && labels.Count > 0)
                                                {
                                                    using (var faceImg = grayFrame.Copy(rect).Resize(100, 100, Inter.Cubic))
                                                    {
                                                        var result = recognizer.Predict(faceImg);
                                                        if (result.Label >= 0 && result.Distance < 120)
                                                        {
                                                            currentId = labels[result.Label];
                                                            if (userNames.ContainsKey(currentId))
                                                                displayName = userNames[currentId];

                                                            double dist = result.Distance;
                                                            double accuracy = dist < 80 ? 100.0 - (dist * 0.25)
                                                                            : dist < 120 ? 160.0 - dist
                                                                            : Math.Max(0, 100 - dist);

                                                            displayName += $" ({accuracy:F1}%)";
                                                        }
                                                    }
                                                }

                                                // Emotion Detection
                                                try
                                                {
                                                    if (rect.Width >= 10 && rect.Height >= 10)
                                                    {
                                                        using (var faceCrop = currentFrame.Copy(rect).Convert<Gray, byte>().Resize(64, 64, Inter.Cubic))
                                                        using (Mat faceMat = new Mat())
                                                        {
                                                            faceCrop.Mat.ConvertTo(faceMat, DepthType.Cv32F);
                                                            using (Mat blob = DnnInvoke.BlobFromImage(faceMat, 1.0, new Size(64, 64), new MCvScalar(0), false, false))
                                                            {
                                                                emotionNet.SetInput(blob);
                                                                using (Mat prob = emotionNet.Forward())
                                                                {
                                                                    float[] probArray = new float[8];
                                                                    prob.CopyTo(probArray);

                                                                    var expWeights = probArray.Select(x => Math.Exp((double)x)).ToArray();
                                                                    var sum = expWeights.Sum();
                                                                    var softmax = expWeights.Select(w => (float)(w / sum)).ToArray();

                                                                    int maxIndex = Array.IndexOf(softmax, softmax.Max());
                                                                    string emotion = emotionLabels[maxIndex];
                                                                    float confidence = softmax[maxIndex] * 100;

                                                                    this.BeginInvoke((MethodInvoker)(() =>
                                                                    {
                                                                        string message = GetMotivationMessage(emotion);
                                                                        textBox3.Text = message;
                                                                    }));
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch { }

                                                // Attendance
                                                if (currentId != null && currentId != lastRecognizedId)
                                                {
                                                    lastRecognizedId = currentId;
                                                    ProcessRecognitionAsync(currentId);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Processing error: " + ex.Message);
                                    }
                                    finally
                                    {
                                        processFrame.Dispose(); // always dispose
                                    }
                                });
                            }

                        }
                        catch { Thread.Sleep(5); }
                    }

                    // Cleanup
                    frame.Dispose();
                    capture?.Dispose();
                    capture = null;
                });
            }
            catch (Exception ex)
            {
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    statusLabel.Text = "❌ Error: " + ex.Message;
                }));
            }
        }







        private void ProcessRecognitionAsync(string faceId)
        {
            Task.Run(() =>
            {
                try
                {
                    // 1. Clean the ID
                    string actualId = faceId.Replace("face_", "").Split('_')[0].Trim();
                    string finalRole = "";

                    // 2. SMART SEARCH: Check Lecturer table first, then Student table
                    // This prevents the "Child Row" error by finding where the ID actually lives
                    var lectInfo = GetUserInfoFromDB(actualId, "lecturer");
                    var studInfo = GetUserInfoFromDB(actualId, "student");

                    var info = (Matric: "", Name: "", PTJ: "");

                    if (lectInfo.Matric != null)
                    {
                        info = lectInfo;
                        finalRole = "lecturer";
                    }
                    else if (studInfo.Matric != null)
                    {
                        info = studInfo;
                        finalRole = "student";
                    }
                    else
                    {
                        // Person detected but not found in any database table
                        this.BeginInvoke((MethodInvoker)delegate {
                            label8.Text = "Status: ID not found in Database!";
                        });
                        return;
                    }

                    // 3. Update the UI
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        txt_matric.Text = info.Matric;
                        txt_name.Text = info.Name;
                        txt_ptj.Text = info.PTJ ?? "N/A";

                        listBox1.Items.Clear();
                        listBox1.Items.Add($"{actualId} ({finalRole})");
                    });

                    // 4. Trigger attendance with the CORRECT verified role
                    HandleAttendance(actualId, finalRole);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Async Processing Error: " + ex.Message);
                }
            });
        }



        private (string Matric, string Name, string PTJ) GetUserInfoFromDB(string id, string role)
        {
            try
            {
                using var con = new MySqlConnection(connectionString);
                con.Open();

                if (role == "student")
                {
                    using var cmd = new MySqlCommand(
                        "SELECT stud_id, stud_name FROM stud_info WHERE stud_id = @id", con);

                    cmd.Parameters.AddWithValue("@id", id);

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        return (
                            reader["stud_id"].ToString(),
                            reader["stud_name"].ToString(),
                            null // ✅ student has NO PTJ
                        );
                    }
                }
                else if (role == "lecturer")
                {
                    using var cmd = new MySqlCommand(
                        "SELECT staff_id, lect_name, ptj FROM lect_info WHERE staff_id = @id", con);

                    cmd.Parameters.AddWithValue("@id", id);

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        return (
                            reader["staff_id"].ToString(),
                            reader["lect_name"].ToString(),
                            reader["ptj"].ToString()
                        );
                    }
                }

                return (null, null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB Error: " + ex.Message);
                return (null, null, null);
            }
        }

        private void HandleAttendance(string id, string role)
        {
            // Use a lock to ensure only one database operation happens at a time
            lock (_dbLock)
            {
                try
                {
                    DateTime now = DateTime.Now;

                    // 1. Cooldown Check
                    if (lastAttendanceTime.ContainsKey(id) && (now - lastAttendanceTime[id]) < ATTENDANCE_COOLDOWN)
                    {
                        // Calculate remaining seconds
                        var remaining = ATTENDANCE_COOLDOWN - (now - lastAttendanceTime[id]);

                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            label8.Text = $"ID {id} wait {remaining.Seconds}s to toggle.";
                            clearUITimer.Stop();  // Reset if it was already running
                            clearUITimer.Start(); // Start 10 second countdown
                        });
                        return;
                    }

                    using (var con = new MySqlConnection(connectionString))
                    {
                        con.Open();
                        id = id.Trim(); 
                        string table = role == "student" ? "stud_attendance" : "lect_attendance";
                        string idCol = role == "student" ? "stud_id" : "staff_id";

                        // 2. Determine if Check-in or Check-out
                        // Note: It's safer to check the database for the last status 
                        // rather than relying solely on the local 'isCheckedIn' dictionary.
                        bool needsCheckOut = CheckIfUserIsCurrentlyIn(con, table, idCol, id);

                        if (!needsCheckOut)
                        {
                            // ✅ CHECK IN
                            string sql = $"INSERT INTO {table} ({idCol}, check_in_time, check_out_time) VALUES (@id, @time, '-')";
                            using (var cmd = new MySqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.Parameters.AddWithValue("@time", now);
                                cmd.ExecuteNonQuery();
                            }

                            this.BeginInvoke((MethodInvoker)delegate
                            {
                                txt_in.Text = now.ToString("HH:mm:ss");
                                txt_out.Clear();
                                label8.Text = $"ID {id} Checked In";
                                clearUITimer.Stop();  // Reset if it was already running
                                clearUITimer.Start(); // Start 10 second countdown
                            });
                        }
                        else
                        {
                            // ✅ CHECK OUT
                            string sql = $@"UPDATE {table} 
                                   SET check_out_time = @time 
                                   WHERE {idCol} = @id AND check_out_time = '-' 
                                   ORDER BY id DESC LIMIT 1";
                            using (var cmd = new MySqlCommand(sql, con))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                // CHANGE THIS LINE:
                                cmd.Parameters.AddWithValue("@time", now.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.ExecuteNonQuery();  
                            }

                            this.BeginInvoke((MethodInvoker)delegate
                            {
                                txt_out.Text = now.ToString("HH:mm:ss");
                                label8.Text = $"ID {id} Checked Out";
                                clearUITimer.Stop();  // Reset if it was already running
                                clearUITimer.Start(); // Start 10 second countdown
                            });
                        }

                        // Update local tracking
                        lastAttendanceTime[id] = now;
                    }
                }
                catch (MySqlException ex)
                {
                    // This will pop up a window with the specific MySQL error code and message
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"MySQL Database Error!\n\n" +
                                        $"Error Number: {ex.Number}\n" +
                                        $"Message: {ex.Message}\n\n" +
                                        $"Check if ID exists in info table.",
                                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        label8.Text = $"DB Error #{ex.Number}";
                    });
                }
                catch (Exception ex)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        MessageBox.Show("General Application Error: " + ex.Message);
                    });
                }
            }
        }
        private bool CheckIfUserIsCurrentlyIn(MySqlConnection con, string table, string idCol, string id)
        {
            // It must search for the dash you inserted during Check-In
            string sql = $"SELECT COUNT(*) FROM {table} WHERE {idCol} = @id AND check_out_time = '-'";
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }






        private void LoadTrainedFaces()
        {
            trainingImages.Clear();
            labels.Clear();
            userNames.Clear();

            string recognizerFile = Path.Combine(Application.StartupPath, "recognizer.yml");
            string facesDir = Path.Combine(Application.StartupPath, "TrainedFaces");

            // 1️⃣ If recognizer file exists, load it directly
            if (File.Exists(recognizerFile))
            {
                recognizer?.Dispose();
                recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 80);
                recognizer.Read(recognizerFile);

                LoadLabelsAndNames(facesDir);

                Console.WriteLine("✅ Recognizer loaded | Labels: " + labels.Count);
                return;
            }


            // 2️⃣ If file doesn't exist, train from images
            if (!Directory.Exists(facesDir))
                return; // No faces to train

            using (var con = new MySqlConnection(connectionString))
            {
                con.Open();

                foreach (var roleDir in Directory.GetDirectories(facesDir))
                {
                    string role = Path.GetFileName(roleDir).ToLower();

                    foreach (var imgPath in Directory.GetFiles(roleDir, "*.bmp"))
                    {
                        string faceId = Path.GetFileNameWithoutExtension(imgPath);
                        trainingImages.Add(new Image<Gray, byte>(imgPath));
                        labels.Add(faceId);

                        string name = GetUserNameFromDB_Fast(faceId, role, con);
                        userNames[faceId] = name ?? "Unknown";
                    }
                }
            }

            if (trainingImages.Count == 0)
                return; // nothing to train

            recognizer?.Dispose();
            recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 80);

            // Train: convert images to Mat array and labels to int array
            var mats = trainingImages.ConvertAll(i => i.Mat).ToArray();
            var labelIndices = Enumerable.Range(0, trainingImages.Count).ToArray();
            recognizer.Train(mats, labelIndices);

            // Save recognizer for future runs
            recognizer.Write(recognizerFile);
            Console.WriteLine("✅ Recognizer trained and saved.");

            // Map label index -> faceId
            for (int i = 0; i < labels.Count; i++)
            {
                // Already stored in labels list
                // labels[i] = faceId
            }
        }

        /// <summary>
        /// Maps face IDs to actual names from database, using existing images folder.
        /// </summary>
        private void LoadUserNamesFromImages(string facesDir)
        {
            if (!Directory.Exists(facesDir))
                return;

            using (var con = new MySqlConnection(connectionString))
            {
                con.Open();

                foreach (var roleDir in Directory.GetDirectories(facesDir))
                {
                    string role = Path.GetFileName(roleDir).ToLower();

                    foreach (var imgPath in Directory.GetFiles(roleDir, "*.bmp"))
                    {
                        string faceId = Path.GetFileNameWithoutExtension(imgPath);

                        if (!userNames.ContainsKey(faceId))
                        {
                            string name = GetUserNameFromDB_Fast(faceId, role, con);
                            userNames[faceId] = name ?? "Unknown";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Quickly get user name from database for a given faceId and role.
        /// </summary>
        private string GetUserNameFromDB_Fast(string faceId, string role, MySqlConnection con)
        {
            string table = role == "lecturer" ? "lect_info" : "stud_info";
            string idCol = role == "lecturer" ? "staff_id" : "stud_id";
            string colName = role == "lecturer" ? "lect_name" : "stud_name";

            string actualId = faceId.Replace("face_", "").Split('_')[0];

            using var cmd = new MySqlCommand($"SELECT {colName} FROM {table} WHERE {idCol}=@id", con);
            cmd.Parameters.AddWithValue("@id", actualId);

            return cmd.ExecuteScalar()?.ToString();
        }




        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;

            Thread.Sleep(200); // allow loop to exit

            capture?.Dispose();
            capture = null;

            faceClassifier?.Dispose();
            recognizer?.Dispose();
            emotionNet?.Dispose();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // WRONG: This opens two windows
            using (Form2 f2 = new Form2())
            {
                f2.FormClosed += (s, e) =>
                {
                    RefreshData();
                };
                f2.ShowDialog();
            }
        }
        //=============================================================
        private void InitThemeButton()
        {
            btnTheme = new Button();
            btnTheme.Text = "🌙 Dark Mode";
            btnTheme.Size = new Size(120, 30);
            btnTheme.Location = new Point(20, 10);
            btnTheme.FlatStyle = FlatStyle.Flat;
            btnTheme.FlatAppearance.BorderSize = 0;
            btnTheme.BackColor = Color.FromArgb(96, 125, 139);
            btnTheme.ForeColor = Color.White;
            btnTheme.Click += ToggleTheme;

            Controls.Add(btnTheme);
        }
        private void ToggleTheme(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;

            if (isDarkMode)
                ApplyDarkTheme();
            else
                ApplyLightTheme();
        }
        private void ApplyDarkTheme()
        {
            BackColor = Color.FromArgb(32, 32, 32);

            foreach (Control c in Controls)
            {
                ApplyDarkToControl(c);
            }

            btnTheme.Text = "☀ Light Mode";
        }

        private void ApplyLightTheme()
        {
            BackColor = Color.FromArgb(245, 247, 250);

            foreach (Control c in Controls)
            {
                ApplyLightToControl(c);
            }

            btnTheme.Text = "🌙 Dark Mode";
        }
        private void ApplyDarkToControl(Control c)
        {
            if (c is Panel || c is GroupBox)
                c.BackColor = Color.FromArgb(45, 45, 48);

            if (c is Label)
                c.ForeColor = Color.White;

            if (c is TextBox)
            {
                c.BackColor = Color.FromArgb(30, 30, 30);
                c.ForeColor = Color.White;
            }

            foreach (Control child in c.Controls)
                ApplyDarkToControl(child);
        }

        private void ApplyLightToControl(Control c)
        {
            if (c is Panel || c is GroupBox)
                c.BackColor = Color.White;

            if (c is Label)
                c.ForeColor = Color.Black;

            if (c is TextBox)
            {
                c.BackColor = Color.White;
                c.ForeColor = Color.Black;
            }

            foreach (Control child in c.Controls)
                ApplyLightToControl(child);
        }
        private void MakeRounded(Control control, int radius)
        {
            Rectangle bounds = new Rectangle(0, 0, control.Width, control.Height);
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();

            control.Region = new Region(path);
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            MakeRounded(panel1, 15);
            MakeRounded(groupBox1, 15);
            MakeRounded(button1, 20);
        }



        public void RefreshData()
        {
            Task.Run(() =>
            {
                LoadTrainedFaces();
            });
        }


        private void LoadLabelsAndNames(string facesDir)
        {
            if (!Directory.Exists(facesDir))
                return;

            labels.Clear();
            userNames.Clear();

            using (var con = new MySqlConnection(connectionString))
            {
                con.Open();

                foreach (var roleDir in Directory.GetDirectories(facesDir))
                {
                    string role = Path.GetFileName(roleDir).ToLower();

                    foreach (var imgPath in Directory.GetFiles(roleDir, "*.bmp"))
                    {
                        string faceId = Path.GetFileNameWithoutExtension(imgPath);

                        labels.Add(faceId);

                        string name = GetUserNameFromDB_Fast(faceId, role, con);
                        userNames[faceId] = name ?? "Unknown";
                    }
                }
            }
        }

        private string GetMotivationMessage(string emotion)
        {
            switch (emotion)
            {
                case "Sad":
                    return "It’s okay to feel sad. You’re stronger than you think 💙";

                case "Happy":
                    return "Keep smiling! Your positive energy shines 🌟";

                case "Angry":
                    return "Take a deep breath. You’re in control 🌿";

                case "Fear":
                    return "Be brave. Every step forward matters 💪";

                case "Surprise":
                    return "Embrace the moment. Life is full of surprises ✨";

                case "Disgust":
                    return "Focus on what uplifts you. You’ve got this 🌈";

                case "Contempt":
                    return "Choose kindness. It makes you stronger 🤍";

                case "Neutral":
                default:
                    return "Stay focused and have a great day 😊";
            }
        }


        //=============================================================

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void statusLabel_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
