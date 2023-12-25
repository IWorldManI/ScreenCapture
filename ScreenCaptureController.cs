using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    public class ScreenCaptureController
    {
        private ScreenCaptureModel model;
        private MainForm view;

        public ScreenCaptureController(ScreenCaptureModel model, MainForm view)
        {
            this.model = model;
            this.view = view;

            view.MouseDown += MainForm_MouseDown;
            view.MouseMove += MainForm_MouseMove;
            view.MouseUp += MainForm_MouseUp;
            view.Paint += MainForm_Paint;
            view.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            model.StartDrag(e.Location);
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                model.UpdateDrag(e.Location);
                view.Invalidate();
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OnLeftMouseDragComplete(e);
            }
            else if(e.Button == MouseButtons.Right)
            {
                OnRightMouseDragComplete(e);
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
                MessageBox.Show($"Screenshot is null! \n {ex}");
            }

            model.ResetSelection();
            view.Invalidate();
        }

        private void OnRightMouseDragComplete(MouseEventArgs e)
        {
            model.ResetSelection();
            view.Invalidate();
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
    }
}
