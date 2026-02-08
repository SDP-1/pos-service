using FastReport;
using FastReport.Export.Image;
using pos_service.Models.DTO.Orders;
using pos_service.Models.DTO.Items;
using System.Drawing.Printing;

namespace pos_service.Helpers
{
    public static class FastReportPrintHelper
    {
        public static async Task<bool> PrintReceiptAsync(
            OrderResDto order, 
            string printerName, 
            string reportTemplatePath)
        {
            return await PrintReportAsync(reportTemplatePath, printerName, report =>
            {
                // Prepare report items data
                var items = order.OrderItems.Select((item, index) => new
                {
                    no = index + 1,
                    description = item.PrintName,
                    mprice = item.MarkedPriceAtSale.ToString("F2"),
                    ourprice = item.PriceAtSale.ToString("F2"),
                    qty = item.Quantity.ToString(item.AllowsDecimalQuantities ? "F2" : "F0"),
                    nextAmount = item.LineTotal.ToString("F2")
                }).ToList();

                // Register data source
                report.RegisterData(items, "Items");

                // Set report parameters
                report.SetParameterValue("date", order.CreatedAt.ToString("yyyy/MM/dd"));
                report.SetParameterValue("time", order.CreatedAt.ToString("h:mm:ss tt"));
                report.SetParameterValue("DateTime", order.CreatedAt.ToString());
                report.SetParameterValue("invoceNo", order.OrderNumber);
                report.SetParameterValue("TotalAmount", order.NetAmount.ToString("F2"));
                report.SetParameterValue("Discount", order.TotalDiscount.ToString("F2"));
                report.SetParameterValue("Cash", order.AmountPaid.ToString("F2"));
                report.SetParameterValue("Balance", order.Balance.ToString("F2"));
                report.SetParameterValue("GrosAmount", order.GrossAmount.ToString("F2"));
            });
        }

        public static async Task<bool> PrintBarcodeAsync(
            ItemResDto item, 
            string printerName, 
            string reportTemplatePath)
        {
            return await PrintReportAsync(reportTemplatePath, printerName, report =>
            {
                // Set barcode parameters
                report.SetParameterValue("Name", item.Name.ToString());
                report.SetParameterValue("PrintName", item.PrintName.ToString());
                report.SetParameterValue("BarCode", item.BarCode.ToString());
                //report.SetParameterValue("Price", item.RetailPrice.ToString("F2"));
                report.SetParameterValue("ItemId", $"{item.Id}-{item.SubId}");
            });
        }

        private static async Task<bool> PrintReportAsync(
            string reportTemplatePath, 
            string printerName, 
            Action<Report> configureReport)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var report = LoadAndPrepareReport(reportTemplatePath, configureReport);
                    if (report == null)
                        return false;

                    PrintReport(report, printerName);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FastReport printing error: {ex.Message}");
                    return false;
                }
            });
        }

        private static Report? LoadAndPrepareReport(string reportTemplatePath, Action<Report> configureReport)
        {
            try
            {
                if (!File.Exists(reportTemplatePath))
                {
                    throw new FileNotFoundException($"Report template not found at {reportTemplatePath}");
                }

                var report = new Report();
                report.Load(reportTemplatePath);

                // Configure report with specific parameters
                configureReport(report);

                // Prepare the report
                if (!report.Prepare())
                {
                    return null;
                }

                return report;
            }
            catch
            {
                return null;
            }
        }

        private static void PrintReport(Report report, string printerName)
        {
            // Export to image
            var imageExport = new ImageExport
            {
                ImageFormat = ImageExportFormat.Png,
                Resolution = 203 // Thermal printer resolution (203 DPI)
            };

            var tempPath = Path.Combine(Path.GetTempPath(), $"receipt_{Guid.NewGuid()}.png");

            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    report.Export(imageExport, stream);
                }

                // Print the exported image
                using var printDocument = new PrintDocument();
                using var image = System.Drawing.Image.FromFile(tempPath);
                
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.PrinterSettings.Copies = 1;
                
                // Set all margins to 0 - template already has its own margins
                printDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);

                printDocument.PrintPage += (sender, e) =>
                {
                    if (e.Graphics != null)
                    {
                        // Draw at position (0,0) using full page bounds - no additional margins
                        var pageWidth = e.PageBounds.Width;
                        
                        // Scale image to fit printer width while maintaining aspect ratio
                        var scale = (float)pageWidth / image.Width;
                        var newHeight = (int)(image.Height * scale);
                        
                        // Draw from (0,0) - template margins are already included in the image
                        e.Graphics.DrawImage(image, 0, 0, pageWidth, newHeight);
                    }
                };

                printDocument.Print();
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        public static List<string> GetAvailablePrinters()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToList();
        }

        public static string? GetDefaultPrinter()
        {
            try
            {
                var printerName = new PrinterSettings().PrinterName;
                if (!string.Equals(printerName, "Default printer is not set.", StringComparison.OrdinalIgnoreCase))
                {
                    return printerName;
                }
            }
            catch
            {
                // Ignore
            }

            return PrinterSettings.InstalledPrinters.Cast<string>().FirstOrDefault();
        }
    }
}
