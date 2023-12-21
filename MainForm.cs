using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ScreenCapture
{
    public partial class MainForm : Form
    {
        private Point startPoint;
        private Rectangle selectedRegion;

        public MainForm()
        {
            InitializeComponent();
            InitializeEventHanders();
            SetupWindowParameters();
        }

        private void InitializeEventHanders()
        {
            MouseDown += MainForm_MouseDown;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;
        }
        private void SetupWindowParameters()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Red;
            TransparencyKey = BackColor;
            DoubleBuffered = true;
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            startPoint = e.Location;
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OnDragSterted(sender, e);
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OnDragComplete(sender, e);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Color.White, 2))
            {
                e.Graphics.DrawRectangle(pen, selectedRegion);
            }
        }

        protected void OnDragSterted(object sender, MouseEventArgs e)
        {
            int wigth = e.X - startPoint.X;
            int height = e.Y - startPoint.Y;

            selectedRegion = new Rectangle(startPoint.X, startPoint.Y, wigth, height);
            Invalidate();
        }

        protected void OnDragComplete(object sender, MouseEventArgs e)
        {
            Bitmap screenshot = GetScreenshot(selectedRegion);
            Clipboard.SetImage(screenshot);

            selectedRegion = Rectangle.Empty;
            Invalidate();
        }

        private static Bitmap GetScreenshot(Rectangle coordinates)
        {
            Bitmap screenBmp = new Bitmap(coordinates.Width, coordinates.Height, PixelFormat.Format32bppArgb);

            using (Graphics bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(coordinates.Left, coordinates.Top, 0, 0, new Size(coordinates.Width, coordinates.Height));
            }

            return screenBmp;
        }
    }
}