using System;
using System.IO;
using System.Windows.Forms;
using SyncfusionPDFStamp_WinFormsApp.Services;
using SyncfusionPDFStamp_WinFormsApp.UI;

namespace SyncfusionPDFStamp_WinFormsApp
{
    public partial class Form1 : Form
    {
        private TextBox  _txtPdfPath     = null!;
        private TextBox  _txtLogoPath    = null!;
        private TextBox  _txtCompany     = null!;
        private DateTimePicker _datePicker = null!;
        private TextBox  _txtReference   = null!;
        private TextBox  _txtInvoice     = null!;
        private TextBox  _txtDocument    = null!;
        private TrackBar _trackOpacity   = null!;
        private Label    _lblOpacity     = null!;
        private Label    _lblStatus      = null!;
        private ProgressBar _progressBar = null!;
        private StyledButton _btnBrowsePdf      = null!;
        private StyledButton _btnBrowseLogo     = null!;
        private StyledButton _btnApplyStamp     = null!;

        public Form1()
        {
            InitializeComponent();
            BuildUi();
        }

        private void BuildUi()
        {
            // Shared card geometry so every card lines up vertically.
            const int cardWidth       = 1000;
            const int horizontalGap   = 24;   // padding inside the content shell
            const int verticalGap     = 20;   // gap between cards

            // ===== Header strip =====
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = Color.FromArgb(33, 37, 41),
            };

            Label title = new Label
            {
                Text = "PDF Stamp Studio",
                Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(28, 16),
            };
            Label subtitle = new Label
            {
                Text = "Apply a branded stamp (logo + date + numbers) to every page of a PDF",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(200, 210, 220),
                AutoSize = true,
                Location = new Point(28, 52),
            };
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            Controls.Add(header);

            // ===== Content shell (holds the scrollable card stack) =====
            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(horizontalGap, 28, horizontalGap, 16),
                BackColor = Color.FromArgb(245, 247, 250),
                AutoScroll = true,
            };

            // Use a TableLayoutPanel for the cards so WinForms handles
            // vertical stacking, spacing, and form-resizing for us.
            TableLayoutPanel cardStack = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount    = 5, // card, spacer, card, spacer, card
                BackColor   = Color.Transparent,
            };
            cardStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            cardStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // card 1
            cardStack.RowStyles.Add(new RowStyle(SizeType.Absolute, verticalGap)); // spacer
            cardStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // card 2
            cardStack.RowStyles.Add(new RowStyle(SizeType.Absolute, verticalGap)); // spacer
            cardStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // card 3

            // Spacer panels used between cards (transparent, fixed height).
            Panel spacer1 = new Panel { Height = verticalGap, BackColor = Color.Transparent, Margin = new Padding(0) };
            Panel spacer2 = new Panel { Height = verticalGap, BackColor = Color.Transparent, Margin = new Padding(0) };

            // ----- Card 1: Source PDF (sits just below the header banner) -----
            CardPanel cardSource = new CardPanel
            {
                Title = "Source PDF",
                Subtitle = "Choose the PDF you want to stamp",
                IconGlyph = "\uE8E5",
                AccentColor = Color.FromArgb(52, 120, 246),
                Width = cardWidth,
                Height = 150,
                Margin = new Padding(0),
            };

            // "PDF file" label (matches the label-column used by the other cards)
            AddLabel(cardSource, 24, 96, "PDF file");

            _txtPdfPath = new TextBox
            {
                Width = 640,
                Height = 32,
                Location = new Point(180, 92),
                PlaceholderText = "Select a PDF file using Browse...",
                Font = new Font("Segoe UI", 9.5f),
            };
            _btnBrowsePdf = new StyledButton
            {
                Text = "Browse PDF...",
                BaseColor = Color.FromArgb(52, 120, 246),
                HoverColor = Color.FromArgb(72, 140, 255),
                Location = new Point(836, 88),
                Width = 140,
                Height = 40,
            };
            _btnBrowsePdf.Click += OnBrowsePdf;

            cardSource.Controls.Add(_txtPdfPath);
            cardSource.Controls.Add(_btnBrowsePdf);

            // ----- Card 2: Stamp details -----
            CardPanel cardDetails = new CardPanel
            {
                Title = "Stamp Details",
                Subtitle = "Logo, company name, date and custom numbers",
                IconGlyph = "✎",
                AccentColor = Color.FromArgb(102, 16, 242),
                Width = cardWidth,
                Height = 380,
                Margin = new Padding(0),
            };

            // ----- Layout coordinates inside cardDetails -----
            // Card body inner width = cardWidth - 2*CardPanel.PaddingInternal
            // (980 - 8 = 972).  Children are placed using a 24-px outer margin.
            int labelX  = 24;
            int fieldX  = 180;       // textbox column start
            int rowY    = 76;        // first row top
            int rowGap  = 44;        // vertical gap between simple rows

            // --- Row 1: Logo file + Browse ---
            AddLabeledField(cardDetails, labelX, rowY, "Logo file", out _txtLogoPath, fieldX, 600);
            _btnBrowseLogo = new StyledButton
            {
                Text = "Browse...",
                BaseColor = Color.FromArgb(108, 117, 125),
                HoverColor = Color.FromArgb(134, 142, 151),
                Location = new Point(fieldX + 615, rowY - 2),
                Width = 140,
                Height = 32,
            };
            _btnBrowseLogo.Click += OnBrowseLogo;
            cardDetails.Controls.Add(_btnBrowseLogo);
            rowY += rowGap;

            // --- Row 2: Company name ---
            AddLabeledField(cardDetails, labelX, rowY, "Company name", out _txtCompany, fieldX, 750);
            rowY += rowGap;

            // --- Row 3: Stamp date ---
            AddLabel(cardDetails, labelX, rowY + 3, "Stamp date");
            _datePicker = new DateTimePicker
            {
                Location = new Point(fieldX, rowY),
                Width = 220,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Font = new Font("Segoe UI", 9.5f),
            };
            cardDetails.Controls.Add(_datePicker);
            rowY += rowGap;

            // --- Row 4: Reference / Invoice / Document # ---
            // Use a TableLayoutPanel so the three label-above-field columns
            // align cleanly regardless of label text width.
            int numbersWidth = 770;   // 750 fields + 20 padding buffer
            TableLayoutPanel numbersTable = new TableLayoutPanel
            {
                Location = new Point(fieldX, rowY),
                Width    = numbersWidth,
                Height   = 60,
                ColumnCount = 3,
                RowCount    = 2,
                BackColor   = Color.Transparent,
                Margin      = new Padding(0),
                Padding     = new Padding(0),
            };
            numbersTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            numbersTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            numbersTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            numbersTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f)); // label row
            numbersTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f)); // input row

            _txtReference = new TextBox { Font = new Font("Segoe UI", 9.5f), Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0) };
            _txtInvoice   = new TextBox { Font = new Font("Segoe UI", 9.5f), Dock = DockStyle.Fill, Margin = new Padding(4, 0, 4, 0) };
            _txtDocument  = new TextBox { Font = new Font("Segoe UI", 9.5f), Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0) };

            Label lblRef = MakeFieldLabel("Reference #");
            Label lblInvoice = MakeFieldLabel("Invoice #");
            Label lblDocument = MakeFieldLabel("Document #");

            numbersTable.Controls.Add(lblRef,      0, 0);
            numbersTable.Controls.Add(_txtReference, 0, 1);
            numbersTable.Controls.Add(lblInvoice,   1, 0);
            numbersTable.Controls.Add(_txtInvoice,   1, 1);
            numbersTable.Controls.Add(lblDocument,  2, 0);
            numbersTable.Controls.Add(_txtDocument,  2, 1);

            // A single section label so the row still reads as "Reference #"
            // when the form is scanned top-to-bottom.
            AddLabel(cardDetails, labelX, rowY + 8, "Reference #");

            cardDetails.Controls.Add(numbersTable);
            rowY += 70;  // table is 60 tall plus a 10-px breathing gap

            // --- Row 5: Opacity ---
            AddLabel(cardDetails, labelX, rowY + 3, "Opacity");
            _trackOpacity = new TrackBar
            {
                Minimum = 10,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10,
                Location = new Point(fieldX, rowY - 2),
                Width = 320,
            };
            _lblOpacity = new Label
            {
                Text = "100%",
                Location = new Point(fieldX + 335, rowY + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            };
            _trackOpacity.ValueChanged += (_, _) =>
            {
                if (_lblOpacity != null) _lblOpacity.Text = $"{_trackOpacity.Value}%";
            };
            cardDetails.Controls.Add(_trackOpacity);
            cardDetails.Controls.Add(_lblOpacity);

            // ----- Card 3: Actions -----
            CardPanel cardActions = new CardPanel
            {
                Title = "Apply Stamp",
                Subtitle = "Pick a save location and start the stamping",
                IconGlyph = "▶",
                AccentColor = Color.FromArgb(40, 167, 69),
                Width = cardWidth,
                Height = 150,
                Margin = new Padding(0),
            };

            _btnApplyStamp = new StyledButton
            {
                Text = "Apply Stamp & Save PDF",
                BaseColor = Color.FromArgb(40, 167, 69),
                HoverColor = Color.FromArgb(60, 187, 89),
                Location = new Point(24, 92),
                Width = 260,
                Height = 42,
            };
            _btnApplyStamp.Click += OnApplyStamp;

            _progressBar = new ProgressBar
            {
                Location = new Point(304, 100),
                Width = 360,
                Height = 26,
                Style = ProgressBarStyle.Continuous,
                Visible = false,
            };
            _lblStatus = new Label
            {
                Text = "Ready.",
                Location = new Point(680, 104),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 117, 125),
            };

            cardActions.Controls.Add(_btnApplyStamp);
            cardActions.Controls.Add(_progressBar);
            cardActions.Controls.Add(_lblStatus);

            // Stack the three cards in a TableLayoutPanel so vertical spacing
            // and width stay consistent regardless of card content height.
            cardStack.Controls.Add(cardSource,  0, 0);
            cardStack.Controls.Add(spacer1,     0, 1);
            cardStack.Controls.Add(cardDetails, 0, 2);
            cardStack.Controls.Add(spacer2,     0, 3);
            cardStack.Controls.Add(cardActions, 0, 4);

            content.Controls.Add(cardStack);
            Controls.Add(content);
        }

        /// <summary>
        /// Creates the small bold label used above each form field.
        /// </summary>
        private static Label MakeFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9f),
                ForeColor = Color.FromArgb(73, 80, 87),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
            };
        }

        private static void AddLabel(Control parent, int x, int y, string text)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9f),
                ForeColor = Color.FromArgb(73, 80, 87),
            });
        }

        private static void AddLabeledField(Control parent, int labelX, int fieldY, string label,
            out TextBox textBox, int fieldX, int width)
        {
            AddLabel(parent, labelX, fieldY + 3, label);
            textBox = new TextBox
            {
                Location = new Point(fieldX, fieldY),
                Width = width,
                Font = new Font("Segoe UI", 9.5f),
            };
            parent.Controls.Add(textBox);
        }

        // ---------------- Event handlers ----------------

        private void OnBrowsePdf(object? sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "PDF Documents (*.pdf)|*.pdf",
                Title  = "Select a PDF to stamp",
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _txtPdfPath.Text = dlg.FileName;
                SetStatus($"Selected PDF: {Path.GetFileName(dlg.FileName)}");
            }
        }

        private void OnBrowseLogo(object? sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title  = "Select a company logo",
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _txtLogoPath.Text = dlg.FileName;
            }
        }

        private async void OnApplyStamp(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtPdfPath.Text) || !File.Exists(_txtPdfPath.Text))
            {
                MessageBox.Show(this, "Please select a valid source PDF first.",
                    "Missing PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StampOptions options = new StampOptions
            {
                LogoPath        = (_txtLogoPath.Text ?? string.Empty).Trim(),
                CompanyName     = (_txtCompany.Text   ?? string.Empty).Trim(),
                StampDate       = _datePicker.Value,
                ReferenceNumber = (_txtReference.Text ?? string.Empty).Trim(),
                InvoiceNumber   = (_txtInvoice.Text   ?? string.Empty).Trim(),
                DocumentNumber  = (_txtDocument.Text  ?? string.Empty).Trim(),
                Opacity         = _trackOpacity.Value / 100f,
            };

            string inputPath  = _txtPdfPath.Text;
            string outputPath = BuildOutputPath(inputPath);

            _btnApplyStamp.Enabled = false;
            _progressBar.Visible   = true;
            _progressBar.Style     = ProgressBarStyle.Marquee;
            SetStatus("Stamping pages...");

            try
            {
                PdfStampingService service = new PdfStampingService();
                int stamped = await System.Threading.Tasks.Task.Run(() =>
                    service.ApplyStamp(inputPath, outputPath, options));

                _progressBar.Style = ProgressBarStyle.Continuous;
                _progressBar.Value = _progressBar.Maximum;
                SetStatus($"Done - stamped {stamped} page(s).");

                if (MessageBox.Show(this,
                    $"Stamped PDF saved to:\n{outputPath}\n\nOpen the folder?",
                    "Stamp complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
                    == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
                }
            }
            catch (Exception ex)
            {
                SetStatus("Failed.");
                MessageBox.Show(this, ex.Message, "Stamping error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnApplyStamp.Enabled = true;
                _progressBar.Visible   = false;
                _progressBar.Value     = 0;
            }
        }

        /// <summary>
        /// Builds a unique output path under the user's Downloads folder:
        /// <c>{baseName}-stamped.pdf</c>, or <c>{baseName}-stamped (n).pdf</c>
        /// if a file with that name already exists.
        /// </summary>
        private static string BuildOutputPath(string inputPath)
        {
            string downloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloads = Path.Combine(downloads, "Downloads");

            if (!Directory.Exists(downloads))
                Directory.CreateDirectory(downloads);

            string baseName = Path.GetFileNameWithoutExtension(inputPath);
            string candidate = Path.Combine(downloads, baseName + "-stamped.pdf");

            int n = 2;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(downloads, $"{baseName}-stamped ({n}).pdf");
                n++;
            }

            return candidate;
        }

        private void SetStatus(string text)
        {
            if (_lblStatus != null) _lblStatus.Text = text;
        }
    }
}
