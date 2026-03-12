namespace ImageFitting
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        private PictureBox pictureBox1;
        private Button buttonLoad;

        /// <summary>
        /// Initializes UI components. Kept minimal to avoid designer conflicts when editing in Visual Studio.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            buttonLoad = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Location = new Point(205, 49);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(512, 512);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // buttonLoad
            // 
            buttonLoad.Location = new Point(21, 531);
            buttonLoad.Name = "buttonLoad";
            buttonLoad.Size = new Size(100, 30);
            buttonLoad.TabIndex = 1;
            buttonLoad.Text = "Load image...";
            buttonLoad.UseVisualStyleBackColor = true;
            buttonLoad.Click += buttonLoad_Click;
            // 
            // Form1
            // 
            ClientSize = new Size(769, 592);
            Controls.Add(buttonLoad);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "ImageFitting";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }
    }
}
