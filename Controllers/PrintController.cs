using System.Net.Sockets;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using pos_service.Models.DTO.Orders;
using pos_service.Services;
using pos_service.Models;

namespace pos_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICurrentUserService _currentUserService;

        public PrintController(IOrderService orderService, ICurrentUserService currentUserService)
        {
            _orderService = orderService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Print([FromBody] PrintRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var currentUser = _currentUserService.GetCurrentUser();

                pos_service.Models.DTO.Orders.OrderResDto? orderToPrint = null;
                if (string.IsNullOrWhiteSpace(req.OrderNumber))
                    return BadRequest("OrderNumber is required");

                orderToPrint = await _orderService.GetOrderByOrderNumberAsync(req.OrderNumber!, currentUser);
                if (orderToPrint == null)
                    return NotFound($"Order with number '{req.OrderNumber}' not found");

                var bytes = Helpers.EscPosFormatter.FormatReceipt(orderToPrint!);

                if (req.UseNetwork && !string.IsNullOrWhiteSpace(req.PrinterIp))
                {
                    await Helpers.NetworkPrinter.SendAsync(req.PrinterIp, req.Port ?? 9100, bytes);
                }
                else
                {
                    var printerName = req.PrinterName;
                    if (string.IsNullOrWhiteSpace(printerName))
                    {
                        try
                        {
                            printerName = new PrinterSettings().PrinterName;
                        }
                        catch
                        {
                            printerName = "POS Receipt Printer";
                        }
                    }

                    var ok = Helpers.RawPrinterHelper.SendBytesToPrinter(printerName, bytes);
                    if (!ok) return StatusCode(500, $"Failed to send to printer '{printerName}'");
                }

                return Ok(new { printed = true, orderNumber = orderToPrint!.OrderNumber, status = orderToPrint.Status.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { printed = false, error = ex.Message });
            }
        }
    }

    public record PrintRequest
    {
        [Required]
        public string OrderNumber { get; init; }
        // Whether to use network printing (TCP 9100). Defaults to false when not provided.
        public bool UseNetwork { get; init; } = false;
        public string? PrinterName { get; init; }
        public string? PrinterIp { get; init; }
        public int? Port { get; init; }
    }
}
