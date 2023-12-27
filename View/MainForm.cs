using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ScreenCapture.Controller;
using ScreenCapture.Model;

namespace ScreenCapture
{
    /// <summary>
    /// Main form for the ScreenCapture application, allowing users to capture a selected region of the screen.
    /// Utilizes the ScreenCaptureModel and ScreenCaptureController for managing the capture process.
    /// </summary>
    public partial class MainForm : Form
    {
        private ScreenCaptureModel model;
        private ScreenCaptureController screenCaptureController;

        public MainForm()
        {
            InitializeComponent();
            SetupWindowParameters();

            model = new ScreenCaptureModel();
            screenCaptureController = new ScreenCaptureController(model, this);
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.White, 2))
            {
                e.Graphics.DrawRectangle(pen, model.SelectedRegion);
            }
        }

        private void SetupWindowParameters()
        {
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Blue;
            TransparencyKey = BackColor;
            DoubleBuffered = true;
        }
    }
}