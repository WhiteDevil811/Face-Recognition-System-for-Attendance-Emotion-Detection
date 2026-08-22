using System.Drawing;

namespace RTSP_Tester
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            statusLabel = new Label();
            button1 = new Button();
            groupBox1 = new GroupBox();
            label2 = new Label();
            listBox1 = new ListBox();
            label1 = new Label();
            label3 = new Label();
            label4 = new Label();
            txt_matric = new TextBox();
            txt_name = new TextBox();
            txt_ptj = new TextBox();
            panel1 = new Panel();
            label_time = new Label();
            label5 = new Label();
            label6 = new Label();
            txt_in = new TextBox();
            txt_out = new TextBox();
            label7 = new Label();
            label8 = new Label();
            cameraCard = new Panel();
            label9 = new Label();
            panel2 = new Panel();
            textBox3 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            cameraCard.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(618, 328);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // statusLabel
            // 
            statusLabel.BackColor = Color.FromArgb(33, 150, 243);
            statusLabel.Font = new Font("Segoe UI Semibold", 10F);
            statusLabel.ForeColor = Color.White;
            statusLabel.Location = new Point(950, 10);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(270, 30);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "Connecting...";
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(76, 175, 80);
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI Semibold", 11F);
            button1.ForeColor = Color.White;
            button1.Location = new Point(950, 390);
            button1.Name = "button1";
            button1.Size = new Size(280, 45);
            button1.TabIndex = 2;
            button1.Text = "➕ Add Record";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.White;
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(listBox1);
            groupBox1.Font = new Font("Segoe UI Semibold", 10F);
            groupBox1.Location = new Point(20, 50);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(430, 180);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Detected Persons";
            // 
            // label2
            // 
            label2.Location = new Point(20, 35);
            label2.Name = "label2";
            label2.Size = new Size(100, 23);
            label2.TabIndex = 0;
            label2.Text = "Person(s) detected:";
            // 
            // listBox1
            // 
            listBox1.BorderStyle = BorderStyle.None;
            listBox1.ItemHeight = 23;
            listBox1.Location = new Point(20, 60);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(390, 92);
            listBox1.TabIndex = 1;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.Location = new Point(20, 25);
            label1.Name = "label1";
            label1.Size = new Size(100, 23);
            label1.TabIndex = 0;
            label1.Text = "Matric No";
            // 
            // label3
            // 
            label3.Location = new Point(20, 70);
            label3.Name = "label3";
            label3.Size = new Size(100, 23);
            label3.TabIndex = 2;
            label3.Text = "Name";
            // 
            // label4
            // 
            label4.Location = new Point(20, 115);
            label4.Name = "label4";
            label4.Size = new Size(100, 23);
            label4.TabIndex = 4;
            label4.Text = "PTJ";
            // 
            // txt_matric
            // 
            txt_matric.Location = new Point(120, 22);
            txt_matric.Name = "txt_matric";
            txt_matric.Size = new Size(280, 30);
            txt_matric.TabIndex = 1;
            // 
            // txt_name
            // 
            txt_name.Location = new Point(120, 67);
            txt_name.Name = "txt_name";
            txt_name.Size = new Size(280, 30);
            txt_name.TabIndex = 3;
            // 
            // txt_ptj
            // 
            txt_ptj.Location = new Point(120, 112);
            txt_ptj.Name = "txt_ptj";
            txt_ptj.Size = new Size(280, 30);
            txt_ptj.TabIndex = 5;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(label1);
            panel1.Controls.Add(txt_matric);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(txt_name);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(txt_ptj);
            panel1.Location = new Point(20, 245);
            panel1.Name = "panel1";
            panel1.Size = new Size(430, 190);
            panel1.TabIndex = 4;
            // 
            // label_time
            // 
            label_time.Font = new Font("Segoe UI Semibold", 11F);
            label_time.ForeColor = Color.FromArgb(76, 175, 80);
            label_time.Location = new Point(80, 15);
            label_time.Name = "label_time";
            label_time.Size = new Size(370, 23);
            label_time.TabIndex = 6;
            // 
            // label5
            // 
            label5.Location = new Point(20, 15);
            label5.Name = "label5";
            label5.Size = new Size(54, 23);
            label5.TabIndex = 5;
            label5.Text = "Time:";
            // 
            // label6
            // 
            label6.Location = new Point(20, 460);
            label6.Name = "label6";
            label6.Size = new Size(100, 23);
            label6.TabIndex = 7;
            label6.Text = "Check In";
            // 
            // txt_in
            // 
            txt_in.Location = new Point(120, 457);
            txt_in.Name = "txt_in";
            txt_in.ReadOnly = true;
            txt_in.Size = new Size(280, 30);
            txt_in.TabIndex = 8;
            // 
            // txt_out
            // 
            txt_out.Location = new Point(120, 492);
            txt_out.Name = "txt_out";
            txt_out.ReadOnly = true;
            txt_out.Size = new Size(280, 30);
            txt_out.TabIndex = 10;
            // 
            // label7
            // 
            label7.Location = new Point(20, 495);
            label7.Name = "label7";
            label7.Size = new Size(100, 23);
            label7.TabIndex = 9;
            label7.Text = "Check Out";
            // 
            // label8
            // 
            label8.ForeColor = Color.Gray;
            label8.Location = new Point(400, 457);
            label8.Name = "label8";
            label8.Size = new Size(280, 30);
            label8.TabIndex = 0;
            label8.Text = "Waiting...";
            label8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cameraCard
            // 
            cameraCard.BackColor = Color.White;
            cameraCard.BorderStyle = BorderStyle.FixedSingle;
            cameraCard.Controls.Add(pictureBox1);
            cameraCard.Location = new Point(610, 50);
            cameraCard.Name = "cameraCard";
            cameraCard.Size = new Size(620, 330);
            cameraCard.TabIndex = 1;
            // 
            // label9
            // 
            label9.Location = new Point(791, 434);
            label9.Name = "label9";
            label9.Size = new Size(100, 23);
            label9.TabIndex = 11;
            label9.Text = "Motivation";
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(textBox3);
            panel2.Location = new Point(791, 460);
            panel2.Name = "panel2";
            panel2.Size = new Size(447, 72);
            panel2.TabIndex = 6;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(18, 18);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(410, 30);
            textBox3.TabIndex = 5;
            // 
            // Form1
            // 
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1250, 540);
            Controls.Add(panel2);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(statusLabel);
            Controls.Add(cameraCard);
            Controls.Add(button1);
            Controls.Add(groupBox1);
            Controls.Add(panel1);
            Controls.Add(label5);
            Controls.Add(label_time);
            Controls.Add(label6);
            Controls.Add(txt_in);
            Controls.Add(label7);
            Controls.Add(txt_out);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "RTSP Attendance System";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupBox1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            cameraCard.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private Button button1;
        private GroupBox groupBox1;
        private Label label2;
        private ListBox listBox1;
        private Label label1;
        private Label label3;
        private Label label4;
        private TextBox txt_matric;
        private TextBox txt_name;
        private TextBox txt_ptj;
        private Panel panel1;
        private Label label_time;
        private Label label5;
        private Label label6;
        private TextBox txt_in;
        private TextBox txt_out;
        private Label label7;
        private Label label8;
        private Panel cameraCard;
        private Label label9;
        private Panel panel2;
        private TextBox textBox3;
    }
}
