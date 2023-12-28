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
            WindowState = FormWindowState.Maximized;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Blue;
            TransparencyKey = BackColor;
            DoubleBuffered = true;
            ShowInTaskbar = true;
        }
    }
}