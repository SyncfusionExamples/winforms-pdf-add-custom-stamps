using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using SizeF      = Syncfusion.Drawing.SizeF;
using PointF     = Syncfusion.Drawing.PointF;
using RectangleF = Syncfusion.Drawing.RectangleF;

namespace SyncfusionPDFStamp_WinFormsApp.Services
{
    /// <summary>
    /// Options that drive the stamp appearance.
    /// All string fields are optional; only non-empty ones are drawn.
    /// If <see cref="LogoPath"/> is empty or the file can't be found,
    /// a bundled default logo is used automatically.
    /// </summary>
    public sealed class StampOptions
    {
        public string LogoPath { get; init; } = string.Empty;
        public string CompanyName { get; init; } = string.Empty;
        public DateTime? StampDate { get; init; } = DateTime.Now;
        public string ReferenceNumber { get; init; } = string.Empty;
        public string InvoiceNumber { get; init; } = string.Empty;
        public string DocumentNumber { get; init; } = string.Empty;
        public float Opacity { get; init; } = 1f;
        public int MarginPoints { get; init; } = 24;
    }

    /// <summary>
    /// Encapsulates the Syncfusion PDF logic for opening an existing PDF,
    /// adding a logo + date + numbers stamp on every page (top-right corner)
    /// and saving the result to disk. Pure-function style: no UI dependencies.
    /// </summary>
    public sealed class PdfStampingService
    {
        // Fonts are reused across every page of a single stamping pass.
        private PdfFont? _companyFont;
        private PdfFont? _dateFont;
        private PdfFont? _numberFont;
        private PdfFont? _pageFont;

// The default static company logo image is created once and cached
        // for the lifetime of the service.
        private PdfImage? _defaultLogoImage;

        /// <summary>
        /// Apply the stamp to every page of <paramref name="inputPdfPath"/>
        /// and write the result to <paramref name="outputPdfPath"/>.
        /// Returns the number of pages that were stamped.
        /// </summary>
        public int ApplyStamp(string inputPdfPath, string outputPdfPath, StampOptions options)
        {
            if (string.IsNullOrWhiteSpace(inputPdfPath))
                throw new ArgumentException("Input PDF path is required.", nameof(inputPdfPath));
            if (string.IsNullOrWhiteSpace(outputPdfPath))
                throw new ArgumentException("Output PDF path is required.", nameof(outputPdfPath));
            if (!File.Exists(inputPdfPath))
                throw new FileNotFoundException("Source PDF could not be located.", inputPdfPath);

            using PdfLoadedDocument loaded = new PdfLoadedDocument(inputPdfPath);
            float opacity = Math.Clamp(options.Opacity, 0f, 1f);

            int stamped = 0;
            for (int i = 0; i < loaded.Pages.Count; i++)
            {
                PdfPageBase page = loaded.Pages[i];
                PdfGraphics graphics = page.Graphics;

                // Isolate stamp styling from any page content.
                PdfGraphicsState state = graphics.Save();
                graphics.SetTransparency(opacity);

                DrawStamp(graphics, page.Size, options, i + 1);

                graphics.Restore(state);
                stamped++;
            }

            using FileStream output = File.Create(outputPdfPath);
            loaded.Save(output);
            loaded.Close(true);

            return stamped;
        }

        private void DrawStamp(PdfGraphics graphics, SizeF pageSize, StampOptions options, int pageNumber)
        {
            // Render the stamp in the TOP-RIGHT corner of every page.
            float width  = Math.Min(260f, pageSize.Width - (options.MarginPoints * 2));
            float height = 110f;
            float x      = pageSize.Width - width - options.MarginPoints;
            float y      = options.MarginPoints;

            PdfTemplate template    = new PdfTemplate(width, height);
            PdfGraphics tplGraphics = template.Graphics;

            PdfImage logoImage = TryLoadLogo(options.LogoPath);
            EnsureFonts();

            float cursorX    = 0f;
            float cursorY    = 0f;
            float logoHeight = 0f;

            // --- 1. Logo (user-supplied, falls back to a static "STAMP" mark) ---
            // logos are always rendered square.
            {
                logoHeight = Math.Min(28f, height);
                float logoWidth = logoHeight;
                if (logoWidth > width * 0.5f)
                {
                    logoWidth  = width * 0.5f;
                    logoHeight = logoWidth;
                }

                tplGraphics.DrawImage(logoImage, new RectangleF(cursorX, cursorY, logoWidth, logoHeight));
                cursorX += logoWidth + 6f;
            }

            // --- 2. Company name (optional) ---
            if (!string.IsNullOrWhiteSpace(options.CompanyName))
            {
                tplGraphics.DrawString(
                    options.CompanyName,
                    _companyFont!,
                    PdfBrushes.Black,
                    new PointF(cursorX, cursorY + 2f));
            }

            cursorY += Math.Max(logoHeight, 14f);

            // --- 3. Current date (optional) ---
            if (options.StampDate.HasValue)
            {
                string dateText = $"Date: {options.StampDate.Value:yyyy-MM-dd HH:mm}";
                tplGraphics.DrawString(dateText, _dateFont!, PdfBrushes.DimGray, new PointF(0f, cursorY));
                cursorY += 13f;
            }

            // --- 4. Custom numbers (optional, inline helper to avoid duplication) ---
            static void WriteLine(PdfGraphics g, PdfFont font, ref float y, string label, string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                g.DrawString($"{label}: {value}", font, PdfBrushes.DimGray, new PointF(0f, y));
                y += 13f;
            }

            WriteLine(tplGraphics, _numberFont!, ref cursorY, "Reference", options.ReferenceNumber);
            WriteLine(tplGraphics, _numberFont!, ref cursorY, "Invoice",   options.InvoiceNumber);
            WriteLine(tplGraphics, _numberFont!, ref cursorY, "Document",  options.DocumentNumber);

            // --- 5. Page number footer inside the stamp card ---
            if (cursorY < height)
            {
                tplGraphics.DrawString(
                    $"Page {pageNumber}",
                    _pageFont!,
                    PdfBrushes.Gray,
                    new PointF(width - 45f, height - 11f));
            }

            // --- 6. Place the rendered stamp on the actual page ---
            graphics.DrawPdfTemplate(template, new PointF(x, y));
        }

        /// <summary>
        /// Initializes each font lazily. Each PdfFont is created exactly once
        /// per <see cref="ApplyStamp"/> invocation and reused for every page.
        /// </summary>
        private void EnsureFonts()
        {
            _companyFont ??= new PdfStandardFont(PdfFontFamily.Helvetica, 11f, PdfFontStyle.Bold);
            _dateFont    ??= new PdfStandardFont(PdfFontFamily.Helvetica, 9f);
            _numberFont  ??= new PdfStandardFont(PdfFontFamily.Helvetica, 9f);
            _pageFont    ??= new PdfStandardFont(PdfFontFamily.Helvetica, 8f, PdfFontStyle.Italic);
        }

        /// <summary>
        /// Loads the user-supplied logo, falling back to a built-in static
        /// "STAMP" wordmark if the user didn't pick anything (or the file is
        /// invalid). Always returns a non-null image.
        /// </summary>
        private PdfImage TryLoadLogo(string? logoPath)
        {
            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                try
                {
                    using FileStream stream = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
                    return new PdfBitmap(stream);
                }
                catch
                {
                    // Corrupt/unsupported file - fall through to default.
                }
            }

            _defaultLogoImage ??= CreateStaticLogoImage();
            return _defaultLogoImage;
        }

        /// <summary>
        /// Renders a deterministic, dependency-free static company logo:
        /// a solid indigo swatch with the white wordmark "STAMP" centered.
        /// Always identical between runs - not a procedurally generated
        /// gradient raster.
        /// </summary>
        private static PdfBitmap CreateStaticLogoImage()
        {
            const int size = 96;
            using Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                // 1. Solid brand swatch background (no gradients).
                using SolidBrush bg = new SolidBrush(Color.FromArgb(102, 16, 242));
                g.FillRectangle(bg, 0, 0, size, size);

                // 2. White inset border.
                using Pen border = new Pen(Color.White, 2f);
                g.DrawRectangle(border, 6, 6, size - 12, size - 12);

                // 3. Static wordmark "STAMP" centered inside the swatch.
                using Font font = new Font("Segoe UI", 22f, FontStyle.Bold, GraphicsUnit.Pixel);
                using SolidBrush fg = new SolidBrush(Color.White);
                System.Drawing.SizeF textSize = g.MeasureString("STAMP", font);
                g.DrawString("STAMP", font, fg,
                    (size - textSize.Width) / 2f,
                    (size - textSize.Height) / 2f);
            }

            using MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            return new PdfBitmap(ms);
        }
    }
}