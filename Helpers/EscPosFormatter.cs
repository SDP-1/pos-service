using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using pos_service.Models.DTO.Orders;
using pos_service.Models.DTO.OrderItems;

namespace pos_service.Helpers
{
    public static class EscPosFormatter
    {
        // Produce ESC/POS bytes for a receipt from OrderReqDto (kept for compatibility)
        public static byte[] FormatReceipt(OrderReqDto order, int width = 48)
        {
            static string Repeat(char ch, int n) => new string(ch, Math.Max(0, n));
            static string Center(string s, int w)
            {
                if (s == null) s = string.Empty;
                var pad = Math.Max(0, (w - s.Length) / 2);
                return Repeat(' ', pad) + s + Repeat(' ', Math.Max(0, w - s.Length - pad));
            }
            static string PadRight(string s, int n)
            {
                if (s == null) s = string.Empty;
                return s.Length > n ? s.Substring(0, n) : s + Repeat(' ', n - s.Length);
            }
            static string PadLeft(string s, int n)
            {
                if (s == null) s = string.Empty;
                return s.Length > n ? s.Substring(0, n) : Repeat(' ', n - s.Length) + s;
            }

            var lines = new List<string>();

            // Header
            lines.Add(Center("SIRITHUNGA GROCERY", width));
            lines.Add(Center("Nalagasdeniya, Hikkaduwa", width));
            lines.Add(Center($"Tel: {(order?.Description ?? "(+94)912276011")}", width));
            lines.Add(Repeat('-', width));

            // Invoice / Cashier
            var invLeft = $"Invoice No : {order?.Description ?? "-"}"; // OrderReqDto doesn't carry OrderNumber
            var invRight = string.Empty; // no cashier in OrderReqDto
            if (!string.IsNullOrEmpty(invRight))
            {
                var space = Math.Max(1, width - invLeft.Length - invRight.Length);
                lines.Add(invLeft + Repeat(' ', space) + invRight);
            }
            else
            {
                lines.Add(PadRight(invLeft, width));
            }

            var created = DateTime.Now;
            lines.Add(PadRight($"Date       : {created:yyyy-MM-dd}", width));
            lines.Add(PadRight($"Time       : {created:HH:mm:ss}", width));
            lines.Add(Repeat('-', width));

            // Items header
            var qtyHeader = "QTY";
            var priceHeader = "Price";
            var amountHeader = "Amount";
            var headerLine = Repeat(' ', 6) + PadRight(qtyHeader, 12) + PadRight(priceHeader, 12) + PadRight(amountHeader, 18);
            lines.Add(headerLine);

            var items = order?.OrderItems ?? new List<OrderItemReqDto>();
            var idx = 1;
            foreach (var it in items)
            {
                var qty = (it?.Quantity ?? 0m).ToString("0.##");
                var unit = (it?.MarkedPrice ?? it?.SalePrice ?? 0m).ToString("0.00");
                var amt = (it?.LineTotal ?? 0m).ToString("0.00");
                var name = it != null ? (it.PrintName ?? it.ItemUuid ?? "") : ""; // prefer PrintName if provided

                var nameLine = $"{idx}  {name}".Length > width ? $"{idx}  {name}".Substring(0, width) : $"{idx}  {name}";
                lines.Add(PadRight(nameLine, width));

                var valueLine = Repeat(' ', 6) + PadLeft(qty, 12) + PadLeft(unit, 12) + PadLeft(amt, 18);
                lines.Add(valueLine);

                idx++;
            }

            lines.Add(Repeat('-', width));

            // Totals block
            var itemCount = items.Count;
            var qtyDisplay = itemCount.ToString();
            var statusValue = order?.Description ?? string.Empty;

            var rightLabels = new[] { "Net amount", "Discount *", "Cash", "Balance" };
            var maxLabelLen = rightLabels.Max(l => l.Length);

            static string BuildRightBlock(string label, string value, int maxLabelLen)
            {
                var paddedLabel = label.Length > maxLabelLen ? label.Substring(0, maxLabelLen) : label.PadRight(maxLabelLen);
                return $"{paddedLabel} : {value.PadLeft(8)}";
            }

            var qtyLeft = $"QTY      : {qtyDisplay}";
            var netRight = BuildRightBlock("Net amount", (order?.NetAmount ?? 0m).ToString("0.00"), maxLabelLen);
            var spacing1 = Math.Max(1, width - qtyLeft.Length - netRight.Length);
            lines.Add(qtyLeft + Repeat(' ', spacing1) + netRight);

            var statusLeft = $"Status   : {statusValue}";
            var discRight = BuildRightBlock("Discount *", (order?.TotalDiscount ?? 0m).ToString("0.00"), maxLabelLen);
            var spacing2 = Math.Max(1, width - statusLeft.Length - discRight.Length);
            lines.Add(statusLeft + Repeat(' ', spacing2) + discRight);

            var cashRight = BuildRightBlock("Cash", (order?.AmountPaid ?? 0m).ToString("0.00"), maxLabelLen);
            var cashPad = Math.Max(0, width - cashRight.Length);
            lines.Add(Repeat(' ', cashPad) + cashRight);

            var balance = (order?.AmountPaid ?? 0m) - (order?.NetAmount ?? 0m);
            var balRight = BuildRightBlock("Balance", balance.ToString("0.00"), maxLabelLen);
            var balPad = Math.Max(0, width - balRight.Length);
            lines.Add(Repeat(' ', balPad) + balRight);
            lines.Add(" ");

            lines.Add(Center("THANK YOU. PLEASE VISIT AGAIN.", width));
            lines.Add(Repeat('*', width));
            lines.Add(Center("SOLUTION BY : Devinda Panditha", width));
            lines.Add(Center("CONTACT : +94772829780", width));

            // Build byte array with simple ASCII encoding and ESC/POS initialize + cut
            var sb = new List<byte>();
            void Add(params byte[] b) => sb.AddRange(b);
            Add(new byte[] { 0x1B, 0x40 }); // Initialize

            foreach (var l in lines)
            {
                var text = l ?? string.Empty;
                var bytes = Encoding.ASCII.GetBytes(text + "\r\n");
                Add(bytes);
            }

            Add(new byte[] { 0x0A, 0x0A }); // feed
            Add(new byte[] { 0x1D, 0x56, 0x41, 0x10 }); // Full cut

            return sb.ToArray();
        }

        // Produce ESC/POS bytes for a receipt from OrderResDto (used when printing saved orders)
        public static byte[] FormatReceipt(Models.DTO.Orders.OrderResDto order, int width = 48)
        {
            var lines = BuildLinesFromOrderRes(order, width);

            var sb = new List<byte>();
            void Add(params byte[] b) => sb.AddRange(b);
            Add(new byte[] { 0x1B, 0x40 }); // Initialize

            foreach (var l in lines)
            {
                var text = l ?? string.Empty;
                var bytes = Encoding.ASCII.GetBytes(text + "\r\n");
                Add(bytes);
            }

            Add(new byte[] { 0x0A, 0x0A }); // feed
            Add(new byte[] { 0x1D, 0x56, 0x41, 0x10 }); // Full cut

            return sb.ToArray();
        }

        // Return plain text (for preview) for OrderResDto
        public static string FormatReceiptText(Models.DTO.Orders.OrderResDto order, int width = 48)
        {
            var lines = BuildLinesFromOrderRes(order, width);
            return string.Join("\r\n", lines);
        }

        private static List<string> BuildLinesFromOrderRes(Models.DTO.Orders.OrderResDto order, int width)
        {
            static string Repeat(char ch, int n) => new string(ch, Math.Max(0, n));
            static string Center(string s, int w)
            {
                if (s == null) s = string.Empty;
                var pad = Math.Max(0, (w - s.Length) / 2);
                return Repeat(' ', pad) + s + Repeat(' ', Math.Max(0, w - s.Length - pad));
            }
            static string PadRight(string s, int n)
            {
                if (s == null) s = string.Empty;
                return s.Length > n ? s.Substring(0, n) : s + Repeat(' ', n - s.Length);
            }
            static string PadLeft(string s, int n)
            {
                if (s == null) s = string.Empty;
                return s.Length > n ? s.Substring(0, n) : Repeat(' ', n - s.Length) + s;
            }

            var lines = new List<string>();

            lines.Add(Center("SIRITHUNGA GROCERY", width));
            lines.Add(Center("Nalagasdeniya, Hikkaduwa", width));
            lines.Add(Center($"Tel: {order?.CustomerPhone ?? "+94)912276011"}", width));
            lines.Add(Repeat('-', width));

            var invLeft = $"Invoice No : {order?.OrderNumber ?? "-"}";
            var invRight = order?.CashierName ?? string.Empty;
            if (!string.IsNullOrEmpty(invRight))
            {
                var space = Math.Max(1, width - invLeft.Length - invRight.Length);
                lines.Add(invLeft + Repeat(' ', space) + invRight);
            }
            else
            {
                lines.Add(PadRight(invLeft, width));
            }

            var created = order?.CreatedAt ?? DateTime.Now;
            lines.Add(PadRight($"Date       : {created:yyyy-MM-dd}", width));
            lines.Add(PadRight($"Time       : {created:HH:mm:ss}", width));
            lines.Add(Repeat('-', width));

            var qtyHeader = "QTY";
            var priceHeader = "Price";
            var amountHeader = "Amount";
            var headerLine = Repeat(' ', 6) + PadRight(qtyHeader, 12) + PadRight(priceHeader, 12) + PadRight(amountHeader, 18);
            lines.Add(headerLine);

            var items = order?.OrderItems ?? new List<Models.DTO.OrderItems.OrderItemMiniResDto>();
            var idx = 1;
            foreach (var it in items)
            {
                var qty = (it?.Quantity ?? 0m).ToString("0.##");
                var unit = (it?.PriceAtSale ?? 0m).ToString("0.00");
                var amt = (it?.LineTotal ?? 0m).ToString("0.00");
                var name = it != null ? (it.PrintName ?? it.OriginalItemUuid ?? "") : "";

                var nameLine = $"{idx}  {name}".Length > width ? $"{idx}  {name}".Substring(0, width) : $"{idx}  {name}";
                lines.Add(PadRight(nameLine, width));

                var valueLine = Repeat(' ', 6) + PadLeft(qty, 12) + PadLeft(unit, 12) + PadLeft(amt, 18);
                lines.Add(valueLine);

                idx++;
            }

            lines.Add(Repeat('-', width));

            var qtyDisplay = (order?.ItemCount ?? items.Count).ToString();
            var statusValue = order?.Status.ToString() ?? string.Empty;

            var rightLabels = new[] { "Net amount", "Discount *", "Cash", "Balance" };
            var maxLabelLen = rightLabels.Max(l => l.Length);

            static string BuildRightBlock(string label, string value, int maxLabelLen)
            {
                var paddedLabel = label.Length > maxLabelLen ? label.Substring(0, maxLabelLen) : label.PadRight(maxLabelLen);
                return $"{paddedLabel} : {value.PadLeft(8)}";
            }

            var qtyLeft = $"QTY      : {qtyDisplay}";
            var netRight = BuildRightBlock("Net amount", (order?.NetAmount ?? 0m).ToString("0.00"), maxLabelLen);
            var spacing1 = Math.Max(1, width - qtyLeft.Length - netRight.Length);
            lines.Add(qtyLeft + Repeat(' ', spacing1) + netRight);

            var statusLeft = $"Status   : {statusValue}";
            var discRight = BuildRightBlock("Discount *", (order?.TotalDiscount ?? 0m).ToString("0.00"), maxLabelLen);
            var spacing2 = Math.Max(1, width - statusLeft.Length - discRight.Length);
            lines.Add(statusLeft + Repeat(' ', spacing2) + discRight);

            var cashRight = BuildRightBlock("Cash", (order?.AmountPaid ?? 0m).ToString("0.00"), maxLabelLen);
            var cashPad = Math.Max(0, width - cashRight.Length);
            lines.Add(Repeat(' ', cashPad) + cashRight);

            var balance = (order?.Balance ?? (order?.AmountPaid ?? 0m) - (order?.NetAmount ?? 0m));
            var balRight = BuildRightBlock("Balance", balance.ToString("0.00"), maxLabelLen);
            var balPad = Math.Max(0, width - balRight.Length);
            lines.Add(Repeat(' ', balPad) + balRight);
            lines.Add(" ");

            lines.Add(Center("THANK YOU. PLEASE VISIT AGAIN.", width));
            lines.Add(Repeat('*', width));
            lines.Add(Center("SOLUTION BY : Devinda Panditha", width));
            lines.Add(Center("CONTACT : +94772829780", width));

            return lines;
        }
    }
}
