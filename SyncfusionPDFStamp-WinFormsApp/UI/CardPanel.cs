using System.Drawing;
using System.Windows.Forms;

namespace SyncfusionPDFStamp_WinFormsApp.UI
{
    /// <summary>
    /// A lightweight, dependency-free "card" container that draws a rounded,
    /// drop-shadowed panel without requiring any third-party UI library.
    /// </summary>
    public sealed class CardPanel : Panel
    {
        private const int PaddingInternal = 4;
        private const int Radius = 12;
        private const int ShadowOffset = 4;
        private const int ShadowLayers = 5;

        public Color ShadowColor { get; set; } = Color.FromArgb(28, 0, 0, 0);

        public CardPanel()
        {
            DoubleBuffered   = true;
            BackColor        = Color.White;
            Padding          = new Padding(20, 18, 20, 18);
            Margin           = new Padding(0, 0, 0, 18);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        /// <summary>Title rendered inside the card header.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Subtitle / helper text rendered under the title.</summary>
        public string Subtitle { get; set; } = string.Empty;

        /// <summary>Accent color of the small left bar in the card header.</summary>
        public Color AccentColor { get; set; } = Color.FromArgb(52, 120, 246);

        /// <summary>Optional icon glyph (single character) shown next to the title.</summary>
        public string IconGlyph { get; set; } = string.Empty;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int headerHeight = string.IsNullOrEmpty(Title) ? 0 : 48;
            Rectangle contentRect = new Rectangle(
                PaddingInternal,
                PaddingInternal + headerHeight,
                Width - (PaddingInternal * 2),
                Height - (PaddingInternal * 2) - headerHeight);

            // 1. Drop shadow (multiple low-alpha layers for a soft, modern shadow)
            for (int i = ShadowLayers; i >= 1; i--)
            {
                int offset = (ShadowOffset * i) / ShadowLayers;
                int alpha  = (ShadowColor.A / ShadowLayers) + (i * 2);
                using SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(Math.Min(alpha, 60), ShadowColor));
                FillRoundedRectangle(g, shadowBrush,
                    PaddingInternal + offset,
                    PaddingInternal + offset + headerHeight,
                    contentRect.Width,
                    contentRect.Height,
                    Radius);
            }

            // 2. Card body
            using (SolidBrush bodyBrush = new SolidBrush(BackColor))
            {
                FillRoundedRectangle(g, bodyBrush, contentRect, Radius);
            }

            // 3. Header (accent bar + title + subtitle + icon)
            if (headerHeight > 0)
            {
                Rectangle headerRect = new Rectangle(PaddingInternal, PaddingInternal, contentRect.Width, headerHeight);
                using (SolidBrush headerBrush = new SolidBrush(BackColor))
                {
                    FillRoundedRectangle(g, headerBrush, headerRect, Radius);
                }

                // Accent bar
                using (SolidBrush accentBrush = new SolidBrush(AccentColor))
                {
                    g.FillRectangle(accentBrush,
                        headerRect.Left + 2,
                        headerRect.Top + 12,
                        4,
                        headerHeight - 24);
                }

                // Icon + Title + Subtitle
                int textX = headerRect.Left + 16;
                using Font titleFont    = new Font("Segoe UI Semibold", 11.5f, FontStyle.Bold);
                using Font subtitleFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);

                if (!string.IsNullOrEmpty(IconGlyph))
                {
                    using Font iconFont = new Font("Segoe UI Symbol", 14f, FontStyle.Bold);
                    using SolidBrush iconBrush = new SolidBrush(AccentColor);
                    g.DrawString(IconGlyph, iconFont, iconBrush, textX, headerRect.Top + 8);
                    textX += 24;
                }

                using SolidBrush textBrush = new SolidBrush(Color.FromArgb(33, 37, 41));
                g.DrawString(Title, titleFont, textBrush, textX, headerRect.Top + 6);
                g.DrawString(Subtitle, subtitleFont, new SolidBrush(Color.FromArgb(108, 117, 125)),
                    textX, headerRect.Top + 26);
            }

            // 4. Card border (subtle)
            using Pen borderPen = new Pen(Color.FromArgb(228, 232, 238), 1f);
            // DrawRectangle is too noisy against the rounded body, so we omit a
            // hard border — the drop shadow already gives a clear edge.
            _ = borderPen;
        }

        private static void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using System.Drawing.Drawing2D.GraphicsPath path = CreateRoundedPath(rect, radius);
            g.FillPath(brush, path);
        }

        private static void FillRoundedRectangle(Graphics g, Brush brush, int x, int y, int width, int height, int radius)
        {
            FillRoundedRectangle(g, brush, new Rectangle(x, y, width, height), radius);
        }

        private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}