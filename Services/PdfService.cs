using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.IO;
using System.Threading.Tasks;

namespace pos_service.Services
{
    /// <summary>
    /// Service implementation for converting HTML strings to PDF files using a headless Chromium browser instance.
    /// </summary>
    public class PdfService : IPdfService
    {
        /// <summary>
        /// Attempts to locate a local Google Chrome installation executable path to avoid downloading Chromium.
        /// </summary>
        /// <returns>The string path of Chrome's executable if found; otherwise, null.</returns>
        private string? GetChromeExecutablePath()
        {
            var paths = new[]
            {
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
            };

            // Return the first path that exists on disk
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        /// <summary>
        /// Converts a raw HTML string into a PDF document file stream.
        /// </summary>
        /// <param name="htmlContent">The raw HTML content to convert.</param>
        /// <returns>A byte array containing the generated PDF document binary data.</returns>
        public async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
        {
            // Configure headless mode launch options using the local Chrome path
            var launchOptions = new LaunchOptions
            {
                Headless       = true,
                ExecutablePath = GetChromeExecutablePath()
            };

            // If no local Chrome installation is found, download a Chromium bundle dynamically
            if (string.IsNullOrEmpty(launchOptions.ExecutablePath))
            {
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
            }

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page    = await browser.NewPageAsync();
            
            // Build style overrides to standardize margins and completely hide any static/legacy footer elements
            string overrideStyles = @"
                                        <style>
                                            @media print {
                                                @page {
                                                    margin: 1.0cm 1.0cm 1.2cm 1.0cm !important;
                                                }
                                                .footer, #footer, .page-footer {
                                                    display: none !important;
                                                    opacity: 0 !important;
                                                    visibility: hidden !important;
                                                    height: 0 !important;
                                                    overflow: hidden !important;
                                                }
                                            }
                                            .footer, #footer, .page-footer {
                                                display: none !important;
                                                opacity: 0 !important;
                                                visibility: hidden !important;
                                                height: 0 !important;
                                                overflow: hidden !important;
                                            }
                                        </style>";

            // Inject the style rules inside the <head> tag of the document to ensure correct parsing
            string styledHtml;
            if (htmlContent.Contains("</head>", StringComparison.OrdinalIgnoreCase))
            {
                styledHtml = htmlContent.Replace("</head>", overrideStyles + "</head>", StringComparison.OrdinalIgnoreCase);
            }
            else if (htmlContent.Contains("<body>", StringComparison.OrdinalIgnoreCase))
            {
                styledHtml = htmlContent.Replace("<body>", "<body>" + overrideStyles, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                styledHtml = overrideStyles + htmlContent;
            }

            await page.SetContentAsync(styledHtml);

            // Configure PDF margins, layouts, and header/footer templates
            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                MarginOptions = new MarginOptions
                {
                    Top    = "1cm",
                    Bottom = "1.2cm", // slightly larger bottom margin to clear the footer
                    Left   = "1cm",
                    Right  = "1cm"
                },
                DisplayHeaderFooter = true,
                FooterTemplate = @"
                    <style>
                        .pdf-footer {
                            width: 100%;
                            font-size: 8.5px;
                            font-family: 'Segoe UI', Arial, sans-serif;
                            color: #6b7280;
                            text-align: center;
                            padding: 0 1cm;
                            box-sizing: border-box;
                        }
                        .pdf-footer-line {
                            border-top: 1.5px solid #cbd5e1;
                            margin-bottom: 6px;
                            width: 100%;
                        }
                    </style>
                    <div class='pdf-footer'>
                        <div class='pdf-footer-line'></div>
                        <span>page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
                    </div>",
                HeaderTemplate = "<div></div>" // Empty container hides default browser headers (page title & date stamp)
            };

            // Generate and return PDF binary array
            return await page.PdfDataAsync(pdfOptions);
        }
    }
}
