using System;
using System.IO;
using System.Windows.Forms;

namespace ImageFitting
{
    public partial class Form1 : Form
    {
        private string? _currentImagePath;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.Resize += Form1_Resize;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redline.webp");
            if (File.Exists(defaultPath))
            {
                _currentImagePath = defaultPath;
                ImageHelper.LoadAndFitToPictureBox(pictureBox1, _currentImagePath);
            }
        }

        private void buttonLoad_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|All files|*.*";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _currentImagePath = dlg.FileName;
                ImageHelper.LoadAndFitToPictureBox(pictureBox1, _currentImagePath);
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentImagePath) && File.Exists(_currentImagePath))
            {
                ImageHelper.LoadAndFitToPictureBox(pictureBox1, _currentImagePath);
            }
        }
    }
}
