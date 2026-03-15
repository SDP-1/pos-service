using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Helpers;
using pos_service.Models.DTO.Bills;
using pos_service.Services;
using System.Drawing;
using System.Runtime.Versioning;

namespace pos_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrintController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IItemService _itemService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IShopService _shopService;
        private readonly IWebHostEnvironment _env;

        public PrintController(
            IOrderService orderService,
            IItemService itemService,
            ICurrentUserService currentUserService,
            IShopService shopService,
            IWebHostEnvironment env)
        {
            _orderService = orderService;
            _itemService = itemService;
            _currentUserService = currentUserService;
            _shopService = shopService;
            _env = env;
        }

        /// <summary>
        /// Prints the bill for the given order number.
        /// </summary>
        /// <param name="orderNumber">The order number to print the bill for.</param>
        /// <returns>A response indicating the result of the print operation.</returns>
        [HttpPost]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> PrintBill([FromQuery] string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                return BadRequest("OrderNumber is required");

            try
            {
                var currentUser = _currentUserService.GetCurrentUser();
                var shop = await _shopService.GetAsync();

                var orderToPrint = await _orderService.GetOrderByOrderNumberAsync(orderNumber, currentUser);
                if (orderToPrint == null)
                    return NotFound($"Order with number '{orderNumber}' not found");

                var printer = FastReportPrintHelper.GetDefaultPrinter();
                if (string.IsNullOrWhiteSpace(printer))
                {
                    return StatusCode(500, "No installed printer found. Set a default printer.");
                }

                var reportPath = Path.Combine(_env.ContentRootPath, "Bills/posbill.frx");
                var success = await FastReportPrintHelper.PrintReceiptAsync(orderToPrint, printer, reportPath, shop);

                if (!success)
                {
                    return StatusCode(500, new PrintResponseDto
                    {
                        Printed = false,
                        Error = $"Failed to send to printer '{printer}'"
                    });
                }

                return Ok(new PrintResponseDto
                { 
                    Printed = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PrintResponseDto
                {
                    Printed = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Prints the barcode for the given item UUID.
        /// </summary>
        /// <param name="itemUuid">The UUID of the item to print the barcode for.</param>
        /// <returns>A response indicating the result of the print operation.</returns>
        [HttpPost("barcode/{itemUuid}")]
        public async Task<IActionResult> PrintBarcode(string itemUuid)
        {
            if (string.IsNullOrWhiteSpace(itemUuid))
                return BadRequest("ItemUuid is required");

            try
            {
                var currentUser = _currentUserService.GetCurrentUser();

                var item = await _itemService.GetItemByUuidAsync(itemUuid, currentUser);
                if (item == null)
                    return NotFound($"Item with UUID '{itemUuid}' not found");

                if (string.IsNullOrWhiteSpace(item.BarCode))
                {
                    return BadRequest(new PrintResponseDto
                    {
                        Printed = false,
                        Error = "This item does not have a barcode assigned"
                    });
                }

                var printer = FastReportPrintHelper.GetDefaultPrinter();
                if (string.IsNullOrWhiteSpace(printer))
                {
                    return StatusCode(500, "No installed printer found. Set a default printer.");
                }

                var reportPath = Path.Combine(_env.ContentRootPath, "Bills/item_barcode.frx");
                var success = await FastReportPrintHelper.PrintBarcodeAsync(item, printer, reportPath);

                if (!success)
                {
                    return StatusCode(500, new PrintResponseDto
                    {
                        Printed = false,
                        Error = $"Failed to send to printer '{printer}'"
                    });
                }

                return Ok(new PrintResponseDto
                { 
                    Printed = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PrintResponseDto
                {
                    Printed = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves the list of available printers and the default printer.
        /// </summary>
        /// <returns>A response containing the list of printers and the default printer.</returns>
        [HttpGet("printers")]
        public IActionResult GetPrinters()
        {
            try
            {
                var printers = FastReportPrintHelper.GetAvailablePrinters();
                var defaultPrinter = FastReportPrintHelper.GetDefaultPrinter();

                return Ok(new PrintersResponseDto
                {
                    Printers = printers,
                    DefaultPrinter = defaultPrinter
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
