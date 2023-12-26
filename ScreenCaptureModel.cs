using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;

namespace ScreenCapture
{
    /// <summary>
    /// Model class for managing screen capture operations in the ScreenCapture application.
    /// </summary>
    public class ScreenCaptureModel
    {
        private Point dragStartPoint { get; set; }
        public Rectangle SelectedRegion { get; private set; }

        public void StartDrag(Point startPoint)
        {
            dragStartPoint = startPoint;
            SelectedRegion = new Rectangle(startPoint.X, startPoint.Y, 0, 0);
        }

        /// <summary>
        /// Updates the drag operation based on the current mouse position.
        /// </summary>
        public void UpdateDrag(Point currentPoint)
        {
            int x = Math.Min(dragStartPoint.X, currentPoint.X);
            int y = Math.Min(dragStartPoint.Y, currentPoint.Y);
            int width = Math.Abs(currentPoint.X - dragStartPoint.X);
            int height = Math.Abs(currentPoint.Y - dragStartPoint.Y);

            SelectedRegion = new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Captures and returns a screenshot of the selected region.
        /// </summary>
        public Bitmap? GetScreenshot()
        {
            try
            {
                Bitmap screenBmp = new Bitmap(SelectedRegion.Width, SelectedRegion.Height, PixelFormat.Format32bppArgb);

                using (Graphics bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(SelectedRegion.Left, SelectedRegion.Top, 0, 0, new Size(SelectedRegion.Width, SelectedRegion.Height));
                }

                return screenBmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ArgumentException: {ex.Message}");
                return null;
            }
        }

        public async Task RecordGifAsync(GifWriter gifWriter, CancellationToken recordingCancellationTokenSource, int defaultFrameDelay)
        {
            try
            {
                while (!recordingCancellationTokenSource.IsCancellationRequested)
                {
                    using (Image? screenshot = GetScreenshot())
                    {
                        if (screenshot != null)
                        {
                            gifWriter.WriteFrame(screenshot);
                            gifWriter.Flush();
                        }
                    }

                    await Task.Delay(defaultFrameDelay);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка записи GIF: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the location for the stop button relative to the selected region.
        /// </summary>
        /// <param name="view">The MainForm instance containing the stop button.</param>
        /// <param name="offset">The offset from the selected region.</param>
        /// <returns>The calculated location for the stop button.</returns>
        public Point CalculateStopButtonLocation(MainForm view,int offset)
        {
            int buttonX = SelectedRegion.Right + offset;
            int buttonY = SelectedRegion.Top;

            if (buttonX + view.stopButton.Width > Screen.PrimaryScreen.Bounds.Width)
            {
                buttonX = SelectedRegion.Left - view.stopButton.Width - offset;
            }

            if (buttonY + view.stopButton.Height > Screen.PrimaryScreen.Bounds.Height)
            {
                buttonY = Screen.PrimaryScreen.Bounds.Height - view.stopButton.Height;
            }

            if (buttonY < 0)
            {
                buttonY = 0;
            }

            return new Point(buttonX, buttonY);
        }

        /// <summary>
        /// Resets the selected region to an empty rectangle.
        /// </summary>
        public void ResetSelection()
        {
            SelectedRegion = Rectangle.Empty;
        }
    }
}
