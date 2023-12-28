using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;

namespace ScreenCapture.Model
{
    #region State
    public enum MousePressedEnum
    {
        None,
        Left,
        Right,
        Middle,
    }
    #endregion

    /// <summary>
    /// Model class for managing screen capture operations in the ScreenCapture application.
    /// </summary>
    public class ScreenCaptureModel
    {
        #region Editable fields
        public int DefaultFrameDelay = 100;
        public string FilePath = "captured_screen.gif";
        public int ButtonOffset = 10;
        public int Repeat = 0;
        public bool DeleteExistingFile = true;
        public string ToolTipMessage = "Select area";
        #endregion

        private GifWriter? gifWriter { get; set; }
        public CancellationTokenSource? RecordingCancellationTokenSource { get; private set; }
        private Point dragStartPoint { get; set; }
        public Rectangle SelectedRegion { get; private set; }
        public bool IsRecording { get; set; }

        /// <summary>
        /// Starts the drag operation for selecting a region on the screen.
        /// </summary>
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

        /// <summary>
        /// Asynchronously records the selected region as a GIF using the provided GifWriter.
        /// </summary>
        public async Task RecordGifAsync(int defaultFrameDelay)
        {
            try
            {
                var gifWriter = InitializeGifWriter();
                RecordingCancellationTokenSource = new CancellationTokenSource();

                while (!RecordingCancellationTokenSource.Token.IsCancellationRequested)
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
        /// Initializes the GifWriter for recording based on the model's properties.
        /// </summary>
        private GifWriter InitializeGifWriter()
        {
            gifWriter = new GifWriter(FilePath, DefaultFrameDelay, Repeat);
            IsRecording = true;
            return gifWriter;
        }

        /// <summary>
        /// Calculates the location of the stop button relative to the middle of the selected area.
        /// </summary>
        /// <param name="view">The MainForm instance containing the stop button.</param>
        /// <param name="offset">The offset from the selected region.</param>
        /// <returns>The calculated location for the stop button.</returns>
        public Point CalculateStopButtonLocation(MainForm view, int offset)
        {
            int buttonX;
            int buttonY;

            int middleOfBoundary = SelectedRegion.Top + (SelectedRegion.Height / 2);

            if (SelectedRegion.Right + offset + view.stopButton.Width > Screen.PrimaryScreen.Bounds.Width)
            {
                buttonX = SelectedRegion.Left - view.stopButton.Width - offset;
            }
            else
            {
                buttonX = SelectedRegion.Right + offset;
            }

            buttonY = middleOfBoundary - (view.stopButton.Height / 2);

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
        /// Calculates the location for the tooltip based on the mouse position.
        /// </summary>
        /// <param name="form">The MainForm instance containing the tooltip.</param>
        /// <param name="e">Mouse event arguments containing the mouse position.</param>
        /// <param name="offsetX">Horizontal offset from the mouse position.</param>
        /// <param name="offsetY">Vertical offset from the mouse position.</param>
        /// <returns>The calculated location for the tooltip.</returns>
        public Point CalculateToolTipLocation(MainForm form, MouseEventArgs e, int offsetX = 5, int offsetY = 20)
        {
            return new Point(e.Location.X + offsetX, e.Location.Y + offsetY);
        }

        /// <summary>
        /// Deletes the existing file at the specified file path.
        /// </summary>
        public void DeleteFile()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }

        /// <summary>
        /// Resets the selected region to an empty rectangle.
        /// </summary>
        public void ResetSelection()
        {
            SelectedRegion = Rectangle.Empty;
            IsRecording = false;
        }

        /// <summary>
        /// Disposes of the GifWriter instance used for recording.
        /// </summary>
        public void DisposeRecorder()
        {
            RecordingCancellationTokenSource?.Cancel();
            gifWriter?.Dispose();
        }
    }
}
