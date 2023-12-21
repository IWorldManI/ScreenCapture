using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    public class ScreenCaptureModel
    {
        public Rectangle SelectedRegion { get; private set; }

        public void StartDrag(Point startPoint)
        {
            SelectedRegion = new Rectangle(startPoint.X, startPoint.Y, 0, 0);
        }

        public void UpdateDrag(Point currentPoint)
        {
            SelectedRegion = new Rectangle(SelectedRegion.Left, SelectedRegion.Top,
                                          currentPoint.X - SelectedRegion.Left, currentPoint.Y - SelectedRegion.Top);
        }

        public Bitmap GetScreenshot()
        {
            Bitmap screenBmp = new Bitmap(SelectedRegion.Width, SelectedRegion.Height, PixelFormat.Format32bppArgb);

            using (Graphics bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(SelectedRegion.Left, SelectedRegion.Top, 0, 0, new Size(SelectedRegion.Width, SelectedRegion.Height));
            }

            return screenBmp;
        }

        public void ResetSelection()
        {
            SelectedRegion = Rectangle.Empty;
        }
    }
}
