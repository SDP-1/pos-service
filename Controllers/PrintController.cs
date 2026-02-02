using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Helpers;
using pos_service.Models;
using pos_service.Models.DTO.Bills;
using pos_service.Services;

namespace pos_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrintController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWebHostEnvironment _env;

        public PrintController(
            IOrderService orderService, 
            ICurrentUserService currentUserService,
            IWebHostEnvironment env)
        {
            _orderService = orderService;
            _currentUserService = currentUserService;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Print([FromBody] PrintRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var currentUser = _currentUserService.GetCurrentUser();

                if (string.IsNullOrWhiteSpace(req.OrderNumber))
                    return BadRequest("OrderNumber is required");

                var orderToPrint = await _orderService.GetOrderByOrderNumberAsync(req.OrderNumber!, currentUser);
                if (orderToPrint == null)
                    return NotFound($"Order with number '{req.OrderNumber}' not found");

                var printerName = req.PrinterName ?? FastReportPrintHelper.GetDefaultPrinter();
                if (string.IsNullOrWhiteSpace(printerName))
                {
                    return StatusCode(500, "No installed printer found. Set a default printer or provide PrinterName.");
                }

                var reportPath = Path.Combine(_env.ContentRootPath, "posbill.frx");
                var success = await FastReportPrintHelper.PrintReceiptAsync(orderToPrint, printerName, reportPath);

                if (!success)
                {
                    return StatusCode(500, new PrintResponseDto
                    {
                        Printed = false,
                        OrderNumber = orderToPrint.OrderNumber,
                        Status = orderToPrint.Status.ToString(),
                        Printer = printerName,
                        Error = $"Failed to send to printer '{printerName}'"
                    });
                }

                return Ok(new PrintResponseDto
                { 
                    Printed = true, 
                    OrderNumber = orderToPrint.OrderNumber, 
                    Status = orderToPrint.Status.ToString(),
                    Printer = printerName
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
