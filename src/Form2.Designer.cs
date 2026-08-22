
using System.Drawing;
using System.Drawing.Drawing2D;

namespace workshop2_w
{
    partial class Form2
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
            pictureBox2 = new PictureBox();
            statusLabel = new Label();
            lblDbStatus = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            gred_label = new Label();
            ptj_label = new Label();
            txt_ID = new TextBox();
            txt_Name = new TextBox();
            txt_email = new TextBox();
            txt_gred = new TextBox();
            txt_ptj = new TextBox();
            radio_student = new RadioButton();
            radio_staff = new RadioButton();
            btn_register = new Button();
            camCard = new Panel();
            formCard = new Panel();
            label6 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            camCard.SuspendLayout();
            formCard.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(598, 348);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.LightGray;
            pictureBox2.Location = new Point(40, 420);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(120, 120);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            // 
            // statusLabel
            // 
            statusLabel.BackColor = Color.FromArgb(33, 150, 243);
            statusLabel.Font = new Font("Segoe UI Semibold", 10F);
            statusLabel.ForeColor = Color.White;
            statusLabel.Location = new Point(20, 10);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(580, 30);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "Camera Connecting...";
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDbStatus
            // 
            lblDbStatus.ForeColor = Color.Gray;
            lblDbStatus.Location = new Point(40, 550);
            lblDbStatus.Name = "lblDbStatus";
            lblDbStatus.Size = new Size(100, 23);
            lblDbStatus.TabIndex = 0;
            // 
            // label1
            // 
            label1.Font = new Font("Segoe UI Semibold", 14F);
            label1.Location = new Point(16, 10);
            label1.Name = "label1";
            label1.Size = new Size(114, 35);
            label1.TabIndex = 0;
            label1.Text = "Register User";
            // 
            // label2
            // 
            label2.Location = new Point(20, 77);
            label2.Name = "label2";
            label2.Size = new Size(81, 23);
            label2.TabIndex = 1;
            label2.Text = "ID";
            // 
            // label3
            // 
            label3.Location = new Point(20, 120);
            label3.Name = "label3";
            label3.Size = new Size(81, 23);
            label3.TabIndex = 3;
            label3.Text = "Name";
            // 
            // label4
            // 
            label4.Location = new Point(20, 163);
            label4.Name = "label4";
            label4.Size = new Size(96, 23);
            label4.TabIndex = 5;
            label4.Text = "Email";
            // 
            // label5
            // 
            label5.Location = new Point(20, 201);
            label5.Name = "label5";
            label5.Size = new Size(96, 23);
            label5.TabIndex = 7;
            label5.Text = "Position";
            // 
            // gred_label
            // 
            gred_label.Location = new Point(16, 240);
            gred_label.Name = "gred_label";
            gred_label.Size = new Size(100, 23);
            gred_label.TabIndex = 10;
            gred_label.Text = "Grade";
            // 
            // ptj_label
            // 
            ptj_label.Location = new Point(20, 277);
            ptj_label.Name = "ptj_label";
            ptj_label.Size = new Size(96, 23);
            ptj_label.TabIndex = 12;
            ptj_label.Text = "PTJ";
            // 
            // txt_ID
            // 
            txt_ID.Location = new Point(126, 77);
            txt_ID.Name = "txt_ID";
            txt_ID.Size = new Size(380, 30);
            txt_ID.TabIndex = 2;
            // 
            // txt_Name
            // 
            txt_Name.Location = new Point(126, 120);
            txt_Name.Name = "txt_Name";
            txt_Name.Size = new Size(380, 30);
            txt_Name.TabIndex = 4;
            // 
            // txt_email
            // 
            txt_email.Location = new Point(126, 160);
            txt_email.Name = "txt_email";
            txt_email.Size = new Size(380, 30);
            txt_email.TabIndex = 6;
            // 
            // txt_gred
            // 
            txt_gred.Location = new Point(126, 240);
            txt_gred.Name = "txt_gred";
            txt_gred.Size = new Size(380, 30);
            txt_gred.TabIndex = 11;
            // 
            // txt_ptj
            // 
            txt_ptj.Location = new Point(126, 276);
            txt_ptj.Name = "txt_ptj";
            txt_ptj.Size = new Size(380, 30);
            txt_ptj.TabIndex = 13;
            // 
            // radio_student
            // 
            radio_student.Location = new Point(126, 200);
            radio_student.Name = "radio_student";
            radio_student.Size = new Size(104, 24);
            radio_student.TabIndex = 8;
            radio_student.Text = "Student";
            // 
            // radio_staff
            // 
            radio_staff.Location = new Point(255, 201);
            radio_staff.Name = "radio_staff";
            radio_staff.Size = new Size(104, 24);
            radio_staff.TabIndex = 9;
            radio_staff.Text = "Staff";
            // 
            // btn_register
            // 
            btn_register.BackColor = Color.FromArgb(76, 175, 80);
            btn_register.FlatAppearance.BorderSize = 0;
            btn_register.FlatStyle = FlatStyle.Flat;
            btn_register.Font = new Font("Segoe UI Semibold", 11F);
            btn_register.ForeColor = Color.White;
            btn_register.Location = new Point(180, 350);
            btn_register.Name = "btn_register";
            btn_register.Size = new Size(200, 45);
            btn_register.TabIndex = 14;
            btn_register.Text = "✔ Register";
            btn_register.UseVisualStyleBackColor = false;
            btn_register.Click += btn_register_Click;
            // 
            // camCard
            // 
            camCard.BackColor = Color.White;
            camCard.BorderStyle = BorderStyle.FixedSingle;
            camCard.Controls.Add(pictureBox1);
            camCard.Location = new Point(20, 50);
            camCard.Name = "camCard";
            camCard.Size = new Size(600, 350);
            camCard.TabIndex = 1;
            // 
            // formCard
            // 
            formCard.BackColor = Color.White;
            formCard.BorderStyle = BorderStyle.FixedSingle;
            formCard.Controls.Add(label1);
            formCard.Controls.Add(label2);
            formCard.Controls.Add(txt_ID);
            formCard.Controls.Add(label3);
            formCard.Controls.Add(txt_Name);
            formCard.Controls.Add(label4);
            formCard.Controls.Add(txt_email);
            formCard.Controls.Add(label5);
            formCard.Controls.Add(radio_student);
            formCard.Controls.Add(radio_staff);
            formCard.Controls.Add(gred_label);
            formCard.Controls.Add(txt_gred);
            formCard.Controls.Add(ptj_label);
            formCard.Controls.Add(txt_ptj);
            formCard.Controls.Add(btn_register);
            formCard.Location = new Point(644, 51);
            formCard.Name = "formCard";
            formCard.Size = new Size(560, 420);
            formCard.TabIndex = 3;
            // 
            // label6
            // 
            label6.Location = new Point(644, 474);
            label6.Name = "label6";
            label6.Size = new Size(381, 23);
            label6.TabIndex = 15;
            // 
            // Form2
            // 
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1247, 550);
            Controls.Add(label6);
            Controls.Add(statusLabel);
            Controls.Add(camCard);
            Controls.Add(pictureBox2);
            Controls.Add(formCard);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form2";
            Text = "User Registration";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            camCard.ResumeLayout(false);
            formCard.ResumeLayout(false);
            formCard.PerformLayout();
            ResumeLayout(false);
        }

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label gred_label;
        private Label ptj_label;
        private TextBox txt_ID;
        private TextBox txt_Name;
        private TextBox txt_email;
        private TextBox txt_gred;
        private TextBox txt_ptj;
        private RadioButton radio_student;
        private RadioButton radio_staff;
        private Button btn_register;
        private PictureBox pictureBox2;
        private Label lblDbStatus;
        private Panel camCard;
        private Panel formCard;
        private Label label6;
    }
}
