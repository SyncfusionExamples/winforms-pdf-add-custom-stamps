# PDF Stamp Studio

A Windows desktop utility that add stamps every page of an existing PDF with a
branded mark — **logo + company name + date + Reference / Invoice / Document
numbers** — and saves the result as a new file in your `Downloads` folder.

Built with **.NET 9 + Windows Forms** and powered by the
[Syncfusion .NET Core PDF library](https://www.nuget.org/packages/Syncfusion.Pdf.Net.Core)
(`Syncfusion.Pdf.Net.Core` 35.1.37).

---

## ✨ Features

- Apply a consistent stamp to **every page** of a selected PDF.
- Customize the stamp with:
  - **Logo image** (`.png`, `.jpg`, `.jpeg`, `.bmp`) — falls back to a built-in
    static "STAMP" wordmark if no file is picked.
  - **Company name**.
  - **Stamp date and time** (`yyyy-MM-dd HH:mm`).
  - **Reference #**, **Invoice #**, **Document #** — each line is optional and
    only rendered when populated.
  - **Opacity slider** (10% – 100%) so the stamp sits subtly on top of existing
    page content.
- Page numbers (`Page N`) are rendered inside the stamp card on each page.
- Saved automatically to `~/Downloads/{originalName}-stamped.pdf` (with an
  incrementing suffix if the file already exists).
- Modern flat UI with rounded buttons, drop-shadowed cards, and a dark header
  banner — no third-party UI dependencies.
- **Async stamping** so the UI never freezes; marquee progress bar while the
  document is being processed.
- **Reveal-in-Explorer** prompt after a successful save.


## 🚀 Getting started

### Build & run

From the repo root:

````powershell
cd SyncfusionPDFStamp-WinFormsApp
dotnet build
dotnet run --project SyncfusionPDFStamp-WinFormsApp
````

The compiled binary is produced under:

```
SyncfusionPDFStamp-WinFormsApp\bin\Debug\net9.0-windows\SyncfusionPDFStamp-WinFormsApp.exe
```

### Using the app

1. Click **Browse PDF...** and pick the source document.
2. _(Optional)_ Click **Browse...** in the *Stamp Details* card to attach a
   company logo.
3. Fill in the company name, date and any of Reference / Invoice / Document
   numbers (all optional).
4. Adjust the **Opacity** slider if you want a more subtle stamp.
5. Click **Apply Stamp & Save PDF**.
6. The progress bar shows work in progress; the status label reports
   `Done - stamped N page(s).` when finished.
7. Click **Yes** on the success dialog to reveal the file in Explorer. The
   stamped PDF is saved to your `Downloads` folder as
   `{originalName}-stamped.pdf` (a numeric suffix is added if a file with
   the same name already exists).

---