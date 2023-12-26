using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScreenCapture
{
    public class ScreenCaptureController
    {
        private ScreenCaptureModel model;
        private MainForm view;

        #region Fields
        private int defaultFrameDelay = 100;
        private string filePath = "captured_screen.gif";
        #endregion

        private GifWriter gifWriter { get; set; }
        private CancellationTokenSource recordingCancellationTokenSource { get; set; }

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

            gifWriter = new GifWriter(filePath, defaultFrameDelay, 0);

            recordingCancellationTokenSource = new CancellationTokenSource();
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
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

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
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

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
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
            catch (Exception ex)
            {
                // MessageBox.Show($"Screenshot is null! \n {ex}"); 
            }

            model.ResetSelection();
            view.Invalidate();
        }

        private async void OnRightMouseDragComplete(MouseEventArgs e)
        {
            int offset = 10;
            view.stopButton.Visible = true;
            view.stopButton.Location = model.CalculateStopButtonLocation(view, offset);
            await model.RecordGifAsync(gifWriter,recordingCancellationTokenSource.Token);
        }
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.White, 2))
            {
                e.Graphics.DrawRectangle(pen, model.SelectedRegion);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

        private void MainForm_StopRecording(object sender, EventArgs e)
        {
            recordingCancellationTokenSource.Cancel();

            view.stopButton.Visible = false;
            model.ResetSelection();
            view.Invalidate();
            view.Focus();
        }
    }
}
