namespace LazyControl
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label2 = new Label();
            label1 = new Label();
            btn_save = new Button();
            cbb_esc_f1 = new ComboBox();
            cbb_esc_f2 = new ComboBox();
            btn_uninstall_app = new Button();
            cbb_toggle_mouse_mode = new ComboBox();
            label3 = new Label();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(26, 61);
            label2.Name = "label2";
            label2.Size = new Size(53, 15);
            label2.TabIndex = 7;
            label2.Text = "ESC + F2";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(26, 28);
            label1.Name = "label1";
            label1.Size = new Size(53, 15);
            label1.TabIndex = 5;
            label1.Text = "ESC + F1";
            // 
            // btn_save
            // 
            btn_save.Location = new Point(143, 100);
            btn_save.Name = "btn_save";
            btn_save.Size = new Size(75, 23);
            btn_save.TabIndex = 8;
            btn_save.Text = "Save";
            btn_save.UseVisualStyleBackColor = true;
            btn_save.Click += btn_save_Click;
            // 
            // cbb_esc_f1
            // 
            cbb_esc_f1.FormattingEnabled = true;
            cbb_esc_f1.Location = new Point(97, 28);
            cbb_esc_f1.Name = "cbb_esc_f1";
            cbb_esc_f1.Size = new Size(121, 23);
            cbb_esc_f1.TabIndex = 9;
            // 
            // cbb_esc_f2
            // 
            cbb_esc_f2.FormattingEnabled = true;
            cbb_esc_f2.Location = new Point(97, 61);
            cbb_esc_f2.Name = "cbb_esc_f2";
            cbb_esc_f2.Size = new Size(121, 23);
            cbb_esc_f2.TabIndex = 10;
            // 
            // btn_uninstall_app
            // 
            btn_uninstall_app.Location = new Point(666, 415);
            btn_uninstall_app.Name = "btn_uninstall_app";
            btn_uninstall_app.Size = new Size(110, 23);
            btn_uninstall_app.TabIndex = 11;
            btn_uninstall_app.Text = "Uninstall App";
            btn_uninstall_app.UseVisualStyleBackColor = true;
            btn_uninstall_app.Click += btn_uninstall_app_Click;
            // 
            // cbb_toggle_mouse_mode
            // 
            cbb_toggle_mouse_mode.FormattingEnabled = true;
            cbb_toggle_mouse_mode.Location = new Point(659, 28);
            cbb_toggle_mouse_mode.Name = "cbb_toggle_mouse_mode";
            cbb_toggle_mouse_mode.Size = new Size(121, 23);
            cbb_toggle_mouse_mode.TabIndex = 13;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(522, 31);
            label3.Name = "label3";
            label3.Size = new Size(118, 15);
            label3.TabIndex = 12;
            label3.Text = "On/Off Mouse Mode";
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(cbb_toggle_mouse_mode);
            Controls.Add(label3);
            Controls.Add(btn_uninstall_app);
            Controls.Add(cbb_esc_f2);
            Controls.Add(cbb_esc_f1);
            Controls.Add(btn_save);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "SettingsForm";
            Text = "Settings";
            Load += SettingsForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label2;
        private Label label1;
        private Button btn_save;
        private ComboBox cbb_esc_f1;
        private ComboBox cbb_esc_f2;
        private Button btn_uninstall_app;
        private ComboBox cbb_toggle_mouse_mode;
        private Label label3;
    }
}