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
        private ToolTip mainToolTipMessage;

        public MainForm()
        {
            InitializeComponent();
            SetupWindowParameters();

            model = new ScreenCaptureModel();
            mainToolTipMessage = new ToolTip();
            screenCaptureController = new ScreenCaptureController(model, this);
        }

        private void SetupWindowParameters()
        {
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Blue;
            TransparencyKey = BackColor;
            DoubleBuffered = true;
            ShowInTaskbar = true;

            Screen[] screens = Screen.AllScreens;

            if (screens.Length > 1)
            {
                int totalWidth = Math.Abs(screens[0].Bounds.Left) + screens[0].Bounds.Width + screens[1].Bounds.Width;
                int totalHeight = Math.Max(screens[0].Bounds.Height, screens[1].Bounds.Height);

                this.Width = totalWidth;
                this.Height = totalHeight;

                this.StartPosition = FormStartPosition.Manual;
                this.Location = new System.Drawing.Point(Math.Min(screens[0].Bounds.Left, screens[1].Bounds.Left), screens[0].Bounds.Top);
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }
        }
    }
}