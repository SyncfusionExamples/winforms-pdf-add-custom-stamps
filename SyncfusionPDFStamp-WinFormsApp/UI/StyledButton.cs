using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SyncfusionPDFStamp_WinFormsApp.UI
{
    /// <summary>
    /// Flat-style rounded button with hover/press visual feedback.
    /// Avoids the default WinForms beveled look so the app feels modern.
    /// </summary>
    public sealed class StyledButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public Color BaseColor { get; set; } = Color.FromArgb(52, 120, 246);
        public Color HoverColor { get; set; } = Color.FromArgb(72, 140, 255);

        public StyledButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = BaseColor;
            ForeColor = Color.White;
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            Cursor = Cursors.Hand;
            Size = new Size(150, 38);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _pressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int radius = 8;

            Color fill = _pressed ? Darken(BaseColor, 0.85f) : (_hover ? HoverColor : BaseColor);
            if (!Enabled) fill = Color.FromArgb(180, fill.R, fill.G, fill.B);

            using (GraphicsPath path = CreateRoundedPath(rect, radius))
            using (SolidBrush brush = new SolidBrush(fill))
            {
                g.FillPath(brush, path);
            }

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                rect,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static Color Darken(Color c, float factor)
        {
            return Color.FromArgb(
                c.A,
                (int)(c.R * factor),
                (int)(c.G * factor),
                (int)(c.B * factor));
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}