using ScreenCapture.Model;

namespace ScreenCapture.Controller
{
    /// <summary>
    /// Controller for managing screen capture functionality in the ScreenCapture application.
    /// Handles user interactions, recording, and GIF creation.
    /// </summary>
    public class ScreenCaptureController
    {
        #region Fields
        private int defaultFrameDelay = 100;
        private string filePath = "captured_screen.gif";
        private int buttonOffset = 10;
        #endregion

        private ScreenCaptureModel model;
        private MainForm view;
        private GifWriter gifWriter;
        private CancellationTokenSource recordingCancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenCaptureController"/> class.
        /// </summary>
        public ScreenCaptureController(ScreenCaptureModel model, MainForm view)
        {
            this.model = model;
            this.view = view;

            view.MouseDown += MainForm_MouseDown;
            view.MouseMove += MainForm_MouseMove;
            view.MouseUp += MainForm_MouseUp;
            view.Paint += MainForm_Paint;
            view.KeyDown += MainForm_KeyDown;
            view.stopButton.Click += MainForm_StopRecording;

            gifWriter = new GifWriter(filePath, defaultFrameDelay, 0); // Repeat enable
            recordingCancellationTokenSource = new CancellationTokenSource();
        }

        private void MainForm_MouseDown(object? sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    model.StartDrag(e.Location);
                    break;

                case MouseButtons.Right:
                    model.StartDrag(e.Location);
                    break;

                default:
                    break;
            }
        }

        private void MainForm_MouseMove(object? sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    model.UpdateDrag(e.Location);
                    view.Invalidate();
                    break;

                case MouseButtons.Right:
                    model.UpdateDrag(e.Location);
                    view.Invalidate();
                    break;

                default:
                    break;
            }
        }

        private void MainForm_MouseUp(object? sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    OnLeftMouseDragComplete(e);
                    break;

                case MouseButtons.Right:
                    OnRightMouseDragComplete(e);
                    break;

                default:
                    break;
            }
        }

        private void OnLeftMouseDragComplete(MouseEventArgs e)
        {
            Bitmap? screenshot = model.GetScreenshot();

            try
            {
                Clipboard.SetImage(screenshot);
            }
            catch
            {
                // Log the exception for further analysis
                // Optionally, display a user-friendly message or handle the exception as needed
                // MessageBox.Show($"Error: {ex.Message}");
            }

            model.ResetSelection();
            view.Invalidate();
        }

        private async void OnRightMouseDragComplete(MouseEventArgs e)
        {
            view.stopButton.Visible = true;
            view.stopButton.Location = model.CalculateStopButtonLocation(view, buttonOffset);
            await model.RecordGifAsync(gifWriter, recordingCancellationTokenSource.Token, defaultFrameDelay);
        }

        private void MainForm_Paint(object? sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.White, 2))
            {
                e.Graphics.DrawRectangle(pen, model.SelectedRegion);
            }
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Handles the click event of the stop button to stop recording.
        /// </summary>
        private void MainForm_StopRecording(object? sender, EventArgs e)
        {
            recordingCancellationTokenSource.Cancel();

            view.stopButton.Visible = false;
            model.ResetSelection();
            view.Invalidate();
            view.Focus();
        }
    }
}