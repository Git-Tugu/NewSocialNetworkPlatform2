using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ImageFitting
{
    /// <summary>
    /// Helper methods for loading and fitting images into a PictureBox.
    /// </summary>
    internal static class ImageHelper
    {
        public static System.Drawing.Image LoadImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required", nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("Image file not found", path);
            return System.Drawing.Image.FromFile(path);
        }

        public static Bitmap ResizeToFit(System.Drawing.Image src, Size target)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (target.Width <= 0 || target.Height <= 0) throw new ArgumentException("Invalid target size", nameof(target));

            var srcW = src.Width;
            var srcH = src.Height;
            var ratio = Math.Min((double)target.Width / srcW, (double)target.Height / srcH);
            var newW = Math.Max(1, (int)Math.Round(srcW * ratio));
            var newH = Math.Max(1, (int)Math.Round(srcH * ratio));

            var bmp = new Bitmap(newW, newH);
            using var g = Graphics.FromImage(bmp);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(src, 0, 0, newW, newH);
            return bmp;
        }

        public static void LoadAndFitToPictureBox(PictureBox pictureBox, string path)
        {
            if (pictureBox == null) throw new ArgumentNullException(nameof(pictureBox));
            using var src = LoadImage(path);
            var target = pictureBox.ClientSize;
            System.Drawing.Image toAssign;
            if (target.Width <= 1 || target.Height <= 1)
            {
                toAssign = new Bitmap(src);
            }
            else
            {
                toAssign = ResizeToFit(src, target);
            }

            if (pictureBox.Image != null)
            {
                try { pictureBox.Image.Dispose(); } catch { }
            }
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox.Image = toAssign;
        }
    }
}
