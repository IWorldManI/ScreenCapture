using ScreenCapture.Model;

namespace ScreenCapture.Controller
{
    /// <summary>
    /// Controller for managing screen capture functionality in the ScreenCapture application.
    /// Handles user interactions, recording, and GIF creation.
    /// </summary>
    public class ScreenCaptureController
    {
        private ScreenCaptureModel model;
        private MainForm view;
        private ToolTip mouseTooltip;
        private MousePressedEnum mousePressed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenCaptureController"/> class.
        /// </summary>
        public ScreenCaptureController(ScreenCaptureModel model, MainForm view)
        {
            this.model = model;
            this.view = view;
            this.mouseTooltip = new ToolTip();

            view.MouseDown += MainForm_MouseDown;
            view.MouseMove += MainForm_MouseMove;
            view.MouseUp += MainForm_MouseUp;
            view.Paint += MainForm_Paint;
            view.KeyDown += MainForm_KeyDown;
            view.stopButton.Click += MainForm_StopRecording;

            mousePressed = MousePressedEnum.None;
        }

        private void MainForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (mousePressed == MousePressedEnum.None && !model.IsRecording)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        model.StartDrag(e.Location);
                        mousePressed = MousePressedEnum.Left;
                        break;

                    case MouseButtons.Right:
                        model.StartDrag(e.Location);
                        mousePressed = MousePressedEnum.Right;
                        break;

                    default:
                        break;
                }
            }
        }

        private void MainForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!model.IsRecording)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                    case MouseButtons.Right:
                        model.UpdateDrag(e.Location);
                        view.Invalidate();
                        break;

                    default:
                        break;
                }
            }

            bool showTooltip = !model.IsRecording && mousePressed == MousePressedEnum.None;

            if (showTooltip)
            {
                mouseTooltip.ShowAlways = true;
                mouseTooltip.Show(model.ToolTipMessage, view, model.CalculateToolTipLocation(view, e));
            }
            else
            {
                mouseTooltip.Hide(view);
            }
        }

        private void MainForm_MouseUp(object? sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if(mousePressed == MousePressedEnum.Left)
                        OnLeftMouseDragComplete(e);
                    break;

                case MouseButtons.Right:
                    if (mousePressed == MousePressedEnum.Right && !model.IsRecording)
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
            finally
            {
                CleanUp();
            }
        }

        private async void OnRightMouseDragComplete(MouseEventArgs e)
        {
            try
            {
                if(model.DeleteExistingFile)
                    model.DeleteFile();

                view.stopButton.Visible = true;
                view.stopButton.Location = model.CalculateStopButtonLocation(view, model.ButtonOffset);

                await model.RecordGifAsync(model.DefaultFrameDelay);
            }
            catch
            {
                // Log the exception for further analysis
                // Optionally, display a user-friendly message or handle the exception as needed
                // MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                CleanUp();
            }
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
            model.DisposeRecorder();
            CleanUp();
        }

        private void CleanUp()
        {
            mousePressed = MousePressedEnum.None;
            view.stopButton.Visible = false;
            model.ResetSelection();
            view.Invalidate();
            view.Focus();
        }
    }
}