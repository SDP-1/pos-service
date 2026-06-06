using FastReport;
using FastReport.Export.Image;
using pos_service.Models.DTO.Orders;
using pos_service.Models.DTO.Items;
using pos_service.Services;
using System.Drawing.Printing;
using System.Drawing;
using System.Runtime.Versioning;
using pos_service.Models.DTO.Settings;
using pos_service.Models.DTO.Inventory;

namespace pos_service.Helpers
{
    public static class FastReportPrintHelper
    {
        [SupportedOSPlatform("windows")]
        public static async Task<bool> PrintReceiptAsync(
            OrderResDto order,
            string printerName,
            string reportTemplatePath,
            ShopResDto? shop)
        {
            return await PrintReportAsync(reportTemplatePath, printerName, async report =>
            {
                // Prepare report items data
                var items = order.OrderItems.Select((item, index) => new
                {
                    no          = index + 1,
                    description = item.PrintName,
                    mprice      = item.MarkedPriceAtSale.ToString("F2"),
                    ourprice    = item.PriceAtSale.ToString("F2"),
                    qty         = item.Quantity.ToString(item.AllowsDecimalQuantities ? "F2" : "F0"),
                    nextAmount  = item.LineTotal.ToString("F2")
                }).ToList();

                // Shop details, set "-" when value missing.
                var storeName  = shop?.Name.ToUpper().ToString() ?? "-";
                var storeAddr  = shop?.Address?.ToString() ?? "-";
                var storePhone = shop?.PhoneNumber?.ToString() ?? "-";

                report.SetParameterValue("StoreName", storeName);
                report.SetParameterValue("StoreAddress", storeAddr);
                report.SetParameterValue("StorePhoneNumber", storePhone);

                // Register data source
                report.RegisterData(items, "Items");

                // Set report parameters for the order
                report.SetParameterValue("date", order.CreatedAt.ToString("yyyy/MM/dd"));
                report.SetParameterValue("time", order.CreatedAt.ToString("h:mm:ss tt"));
                report.SetParameterValue("DateTime", order.CreatedAt.ToString());
                report.SetParameterValue("invoceNo", order.OrderNumber);
                report.SetParameterValue("TotalAmount", order.NetAmount.ToString("F2"));
                report.SetParameterValue("Discount", order.TotalDiscount.ToString("F2"));
                report.SetParameterValue("Cash", order.AmountPaid.ToString("F2"));
                report.SetParameterValue("Balance", order.Balance.ToString("F2"));
                report.SetParameterValue("GrosAmount", order.GrossAmount.ToString("F2"));
                report.SetParameterValue("Barcode", order.OrderNumber.ToString());

            });
        }

        public static async Task<bool> PrintRequiredItemListAsync(
            List<RequiredItemDto> items,
            string supplierName,
            string printerName,
            string reportTemplatePath,
            ShopResDto? shop)
        {
            return await PrintReportAsync(reportTemplatePath, printerName, async report =>
            {
                // Register items
                report.RegisterData(items, "Items");

                // Shop details
                report.SetParameterValue("StoreName", shop?.Name?.ToUpper() ?? "-");
                report.SetParameterValue("StoreAddress", shop?.Address ?? "-");
                report.SetParameterValue("StorePhoneNumber", shop?.PhoneNumber ?? "-");

                report.SetParameterValue("SupplierName", supplierName ?? "-");
                report.SetParameterValue("PrintDate", DateTime.Now.ToString("yyyy/MM/dd"));
                report.SetParameterValue("PrintTime", DateTime.Now.ToString("h:mm:ss tt"));
            });
        }

        public static async Task<bool> PrintBarcodeAsync(
            ItemResDto item,
            string printerName,
            string reportTemplatePath)
        {
            return await PrintReportAsync(reportTemplatePath, printerName, async report =>
            {
                // Set barcode parameters
                report.SetParameterValue("Name", item.Name.ToString());
                report.SetParameterValue("PrintName", item.PrintName.ToString());
                report.SetParameterValue("BarCode", item.BarCode.ToString());
                //report.SetParameterValue("Price", item.RetailPrice.ToString("F2"));
                report.SetParameterValue("ItemId", $"{item.Id}-{item.SubId}");
                await Task.CompletedTask;
            });
        }

        private static async Task<bool> PrintReportAsync(
            string reportTemplatePath,
            string printerName,
            Func<Report, Task> configureReport)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var report = await LoadAndPrepareReport(reportTemplatePath, configureReport);
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

        private static async Task<Report?> LoadAndPrepareReport(string reportTemplatePath, Func<Report, Task> configureReport)
        {
            try
            {
                if (!File.Exists(reportTemplatePath))
                {
                    throw new FileNotFoundException($"Report template not found at {reportTemplatePath}");
                }

                var report = new Report();
                report.Load(reportTemplatePath);

                // Configure report with specific parameters (async)
                if (configureReport != null)
                {
                    await configureReport(report);
                }

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
                using var image = Image.FromFile(tempPath);
                
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.PrinterSettings.Copies = 1;
                printDocument.OriginAtMargins = false;
                
                // Set all margins to 0 - template already has its own margins
                printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                var currentY = 0;

                printDocument.PrintPage += (sender, e) =>
                {
                    if (e.Graphics != null)
                    {
                        // Draw in slices to support long thermal receipts.
                        var printableWidth = e.PageBounds.Width;
                        var printableHeight = e.PageBounds.Height;

                        var scale = (float)printableWidth / image.Width;
                        var sourceSliceHeight = (int)Math.Floor(printableHeight / scale);

                        if (sourceSliceHeight <= 0)
                        {
                            e.HasMorePages = false;
                            return;
                        }

                        var remainingSourceHeight = image.Height - currentY;
                        var drawSourceHeight = Math.Min(sourceSliceHeight, remainingSourceHeight);

                        var srcRect = new Rectangle(0, currentY, image.Width, drawSourceHeight);
                        var destHeight = (int)Math.Ceiling(drawSourceHeight * scale);
                        var destRect = new Rectangle(0, 0, printableWidth, destHeight);

                        e.Graphics.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);

                        currentY += drawSourceHeight;
                        e.HasMorePages = currentY < image.Height;
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
