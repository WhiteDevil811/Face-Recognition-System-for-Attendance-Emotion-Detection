using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;


namespace workshop2_w
{
    public partial class Form2 : Form
    {
        // --- RTSP / Camera ---
        private VideoCapture capture;
        private bool isRunning = false;

        private LBPHFaceRecognizer recognizer;
        private bool recognizerReady = false;

        // --- Training / Captured Data ---
        private List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        private List<string> labels = new List<string>();
        private int ContTrain = 0;


        // --- ADD THESE TWO LINES HERE ---
        private Mat latestFrame = new Mat();
        private readonly object frameLock = new object();



        // --- Database ---
        private string connectionString = "server=127.0.0.1;uid=root;pwd=;database=workshop2;";
        private CascadeClassifier faceClassifier;
        private CascadeClassifier faceLeft;
        private CascadeClassifier faceRight;

        private Task streamingTask;
        public Form2()
        {
            InitializeComponent();
            faceClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
            faceLeft = new CascadeClassifier("haarcascade_profileface.xml");
            faceRight = new CascadeClassifier("haarcascade_profileface.xml");

            // Force link the events manually but remove any old ones first
            this.Load -= Form2_Load;
            this.Load += Form2_Load;

            this.FormClosing -= Form2_FormClosing;
            this.FormClosing += Form2_FormClosing;

            radio_staff.CheckedChanged += (s, e) => SetLecturerFieldsVisible(radio_staff.Checked);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            SetLecturerFieldsVisible(false);
            isRunning = true;

            // Only call this ONCE
            streamingTask = Task.Run(() => StartStreaming());
            Task.Run(() => MonitorDatabaseConnection());
        }



        private void StartStreaming()
        {
            try
            {
                capture = new VideoCapture("rtsp://admin:yisheng%40@192.168.1.64:554/Streaming/Channels/101", VideoCapture.API.Ffmpeg);

                if (capture == null || !capture.IsOpened)
                {
                    this.Invoke((MethodInvoker)(() => MessageBox.Show("Failed to open RTSP stream.")));
                    return;
                }
                // ✅ ADD THESE LINES HERE
               
                capture.Set(CapProp.Fps, 15);   // optional but recommended

                int frameCounter = 0;
                Rectangle[] detectedFaces = new Rectangle[0];
                double scaleDown = 0.5;

                using (Mat frame = new Mat())
                {
                    while (isRunning)
                    {
                        if (capture == null || !capture.IsOpened) break;

                        // 1. Read Frame safely
                        if (!capture.Read(frame) || frame.IsEmpty)
                        {
                            System.Threading.Thread.Sleep(5);
                            continue;
                        }

                        // 2. Lock only for the copy operation
                        lock (frameLock)
                        {
                            if (!isRunning || latestFrame == null) break;
                            lock (frameLock)
                            {
                                latestFrame?.Dispose();
                                latestFrame = frame.Clone();
                            }

                        }

                        // 3. UI Processing
                        using (Image<Bgr, byte> img = frame.ToImage<Bgr, byte>())
                        {
                            if (frameCounter % 3 == 0)
                            {
                                using (Image<Gray, byte> gray = img.Convert<Gray, byte>())
                                using (Image<Gray, byte> smallGray = gray.Resize(scaleDown, Inter.Linear))
                                {
                                    detectedFaces = DetectMultiAngleFaces(smallGray);
                                }
                            }
                            frameCounter++;

                            foreach (var r in detectedFaces)
                            {
                                Rectangle fullSizeRect = new Rectangle(
                                    (int)(r.X / scaleDown), (int)(r.Y / scaleDown),
                                    (int)(r.Width / scaleDown), (int)(r.Height / scaleDown));
                                img.Draw(fullSizeRect, new Bgr(Color.Red), 2);
                            }

                            // 4. Thread-safe UI update
                            if (isRunning && !this.IsDisposed && pictureBox1.IsHandleCreated)
                            {
                                Bitmap bmp = img.ToBitmap();
                                try
                                {
                                    pictureBox1.BeginInvoke((MethodInvoker)delegate
                                    {
                                        // Final check inside the UI thread
                                        if (this.IsDisposed || pictureBox1.IsDisposed)
                                        {
                                            bmp.Dispose();
                                            return;
                                        }
                                        var old = pictureBox1.Image;
                                        pictureBox1.Image = bmp;
                                        old?.Dispose();
                                    });
                                }
                                catch { bmp.Dispose(); }
                            }
                        }
                        System.Threading.Thread.Sleep(1);
                    }
                }
            }
            catch (Exception ex)
            {
                if (isRunning) Console.WriteLine("Stream Error: " + ex.Message);
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false; // signal thread to stop

            // wait for streaming loop to exit
            streamingTask?.Wait();

            // now it's safe to release capture
            capture?.Release();
            capture?.Dispose();
            capture = null;

            // recognizer is NOT used by streaming
            recognizer?.Dispose();
        }


        private Rectangle[] DetectMultiAngleFaces(Image<Gray, byte> gray)
        {
            // Use 1.3 or 1.4 for significantly faster detection than 1.1
            var faces = faceClassifier.DetectMultiScale(gray, 1.3, 5, Size.Empty, Size.Empty);

            // If frontal found, return immediately to save CPU
            if (faces.Length > 0) return faces;

            List<Rectangle> allFaces = new List<Rectangle>();

            // Check Left Profile
            allFaces.AddRange(faceLeft.DetectMultiScale(gray, 1.3, 5));

            // Check Right Profile via Flip
            using (var flipped = gray.Flip(FlipType.Horizontal))
            {
                var rightFaces = faceRight.DetectMultiScale(flipped, 1.3, 5);
                foreach (var r in rightFaces)
                {
                    allFaces.Add(new Rectangle(gray.Width - r.Right, r.Top, r.Width, r.Height));
                }
            }

            return allFaces.ToArray();
        }


        private void SaveFilesAndDatabase(Image<Gray, byte> faceImg, string id, int angleIndex = 0, bool insertUser = false)
        {
            string role = radio_student.Checked ? "student" : "lecturer";

            try
            {
                // --- Save files ---
                string facesDir = Path.Combine(Application.StartupPath, "TrainedFaces", role);
                string xamppDir = Path.Combine("C:\\xampp\\htdocs\\Embeddings", role);
                Directory.CreateDirectory(facesDir);
                Directory.CreateDirectory(xamppDir);

                string fileName = $"face_{id}_{Guid.NewGuid():N}.bmp";
                faceImg.Save(Path.Combine(facesDir, fileName));
                faceImg.Save(Path.Combine(xamppDir, fileName));

                // --- Database ---
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            if (insertUser)
                            {
                                string qryInfo = role == "lecturer" ?
                                    "INSERT INTO lect_info (staff_id, lect_name, email, gred, ptj) VALUES (@id,@name,@email,@gred,@ptj)" :
                                    "INSERT INTO stud_info (stud_id, stud_name, email) VALUES (@id,@name,@email)";

                                using (var cmd = new MySqlCommand(qryInfo, con, trans))
                                {
                                    cmd.Parameters.AddWithValue("@id", id);
                                    cmd.Parameters.AddWithValue("@name", txt_Name.Text);
                                    cmd.Parameters.AddWithValue("@email", txt_email.Text);
                                    if (role == "lecturer")
                                    {
                                        cmd.Parameters.AddWithValue("@gred", txt_gred.Text);
                                        cmd.Parameters.AddWithValue("@ptj", txt_ptj.Text);
                                    }
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // Save embedding
                            string tableEmb = role == "lecturer" ? "lect_embeddings" : "stud_embeddings";
                            string idCol = role == "lecturer" ? "staff_id" : "stud_id";
                            string urlPath = $"http://localhost/Embeddings/{role}/{fileName}";

                            string qryEmb = $"INSERT INTO {tableEmb} ({idCol}, angle_index, embedding_path) VALUES (@id, @angle, @path)";
                            using (var cmd = new MySqlCommand(qryEmb, con, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.Parameters.AddWithValue("@angle", angleIndex); // 0
                                cmd.Parameters.AddWithValue("@path", urlPath);
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Error: " + ex.Message);
            }
        }



        private async Task MonitorDatabaseConnection(int intervalMs = 5000)
        {
            // Change while(!this.IsDisposed) to while(isRunning)
            while (isRunning)
            {
                try
                {
                    using var con = new MySqlConnection(connectionString);
                    await con.OpenAsync();

                    // ALWAYS check isRunning before Invoking
                    if (isRunning && !this.IsDisposed && lblDbStatus.IsHandleCreated)
                    {
                        lblDbStatus.BeginInvoke(new Action(() =>
                        {
                            lblDbStatus.Text = "DB Status: Connected ✅";
                            lblDbStatus.ForeColor = Color.Green;
                        }));
                    }
                }
                catch
                {
                    lblDbStatus.Invoke(new Action(() =>
                    {
                        lblDbStatus.Text = "DB Status: Failed ❌";
                        lblDbStatus.ForeColor = Color.Red;
                    }));
                }

                await Task.Delay(intervalMs);
            }
        }

        private void RadioRole_CheckedChanged(object sender, EventArgs e)
        {
            SetLecturerFieldsVisible(radio_staff.Checked);
        }

        private void SetLecturerFieldsVisible(bool visible)
        {
            gred_label.Visible = txt_gred.Visible = ptj_label.Visible = txt_ptj.Visible = visible;
        }
        private void ClearFields()
        {
            txt_ID.Clear(); txt_Name.Clear(); txt_email.Clear();
            txt_gred.Clear(); txt_ptj.Clear();
            
        }

        




        private void btn_register_Click(object sender, EventArgs e)
        {
            label6.Text = ""; // Clear previous message

            if (string.IsNullOrEmpty(txt_ID.Text))
            {
                label6.Text = "Please enter an ID first.";
                label4.ForeColor = Color.Red;
                return;
            }

            lock (frameLock)
            {
                if (latestFrame == null || latestFrame.IsEmpty)
                {
                    label6.Text = "Camera stream not ready.";
                    label6.ForeColor = Color.Red;
                    return;
                }

                // Clone only for local processing
                using (var grayCopy = latestFrame.Clone().ToImage<Bgr, byte>().Convert<Gray, byte>())
                {

                    using (var localClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml"))
                    {
                        var faces = localClassifier.DetectMultiScale(grayCopy, 1.3, 5, Size.Empty, Size.Empty);


                        if (faces.Length == 0)
                        {
                            label6.Text = "No face detected. Please face the camera.";
                            label6.ForeColor = Color.Red;
                            return;
                        }

                        var faceRect = faces[0];
                        using (var croppedFace = grayCopy.Copy(faceRect).Resize(100, 100, Inter.Cubic))
                        {
                            pictureBox2.Image?.Dispose();
                            pictureBox2.Image = croppedFace.ToBitmap();

                            string id = txt_ID.Text.Trim();
                            SaveFilesAndDatabase(croppedFace, id, 0, true);

                            label6.Text = "Registration Successful!";
                            label6.ForeColor = Color.Green;
                            ClearFields();
                        }
                    }
                }
            }

        }



    }

}
