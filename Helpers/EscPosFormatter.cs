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
        // Produce ESC/POS bytes for a receipt from OrderResDto (used when printing saved orders)
        public static byte[] FormatReceipt(OrderResDto order, int width = 48)
        {
            var lines = BuildLinesFromOrderRes(order, width);

            var sb = new List<byte>();
            void Add(params byte[] b) => sb.AddRange(b);
            Add(new byte[] { 0x1B, 0x40 }); // Initialize

            foreach (var line in lines)
            {
                // Select small font if requested
                if (line.Small)
                {
                    Add(new byte[] { 0x1B, 0x4D, 0x01 }); // Font B
                }

                // Enable bold if requested
                if (line.Bold)
                {
                    Add(new byte[] { 0x1B, 0x45, 0x01 }); // Bold on
                }

                var text = line.Text ?? string.Empty;
                var bytes = Encoding.ASCII.GetBytes(text + "\r\n");
                Add(bytes);

                // Disable bold if it was enabled
                if (line.Bold)
                {
                    Add(new byte[] { 0x1B, 0x45, 0x00 }); // Bold off
                }

                // Restore font A if we switched to Font B
                if (line.Small)
                {
                    Add(new byte[] { 0x1B, 0x4D, 0x00 }); // Font A
                }
            }

            Add(new byte[] { 0x0A, 0x0A }); // feed
            Add(new byte[] { 0x1D, 0x56, 0x41, 0x10 }); // Full cut

            return sb.ToArray();
        }

        // Return plain text (for preview) for OrderResDto
        public static string FormatReceiptText(OrderResDto order, int width = 48)
        {
            var lines = BuildLinesFromOrderRes(order, width);
            return string.Join("\r\n", lines.Select(l => l.Text));
        }

        private sealed record PrintLine(string Text, bool Bold = false, bool Small = false);

        private static List<PrintLine> BuildLinesFromOrderRes(OrderResDto order, int width)
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

            var lines = new List<PrintLine>();

            lines.Add(new PrintLine(Center("SIRITHUNGA GROCERY", width), Bold: true));
            lines.Add(new PrintLine(Center("Nalagasdeniya, Hikkaduwa", width), Bold: true));
            lines.Add(new PrintLine(Center($"Tel: {order?.CustomerPhone ?? "(+94)912276011"}", width), Bold: true));
            lines.Add(new PrintLine(Repeat('-', width)));

            var invLeft = $"Invoice No : {order?.OrderNumber ?? "-"}";
            var invRight = order?.CashierName ?? string.Empty;
            if (!string.IsNullOrEmpty(invRight))
            {
                var space = Math.Max(1, width - invLeft.Length - invRight.Length);
                lines.Add(new PrintLine(invLeft + Repeat(' ', space) + invRight));
            }
            else
            {
                lines.Add(new PrintLine(PadRight(invLeft, width)));
            }

            var created = order?.CreatedAt ?? DateTime.Now;
            // Date and Time on single row (left: Date, right: Time) with AM/PM
            var leftDate = $"Date : {created:yyyy-MM-dd}";
            var rightTime = $"Time : {created:hh:mm:ss tt}";
            var spaceDt = Math.Max(1, width - leftDate.Length - rightTime.Length);
            lines.Add(new PrintLine(leftDate + Repeat(' ', spaceDt) + rightTime));
            // Print description if available
            if (!string.IsNullOrWhiteSpace(order?.Description))
            {
                lines.Add(new PrintLine(PadRight($"Desc : {order.Description}", width)));
            }
            lines.Add(new PrintLine(Repeat('-', width)));

            // Prepare columns for numeric values: MarkedPrice, OurPrice, QTY, Amount
            // Column widths (sum should be <= width). We'll right-align numbers in each column and center headers.
            var colMarked = 12;
            var colOur = 12;
            var colQty = 6;
            var colAmount = 12;
            var colsTotal = colMarked + colOur + colQty + colAmount;
            var leftPad = Math.Max(0, width - colsTotal);

            static string CenterInWidth(string s, int w)
            {
                if (s == null) s = string.Empty;
                var pad = Math.Max(0, (w - s.Length) / 2);
                return new string(' ', pad) + (s.Length > w ? s.Substring(0, w) : s) + new string(' ', Math.Max(0, w - s.Length - pad));
            }

            var qtyHeader = "QTY";
            var priceHeader = "OurPrice";
            var markedHeader = "M.Price";
            var amountHeader = "Amount";
            // Right-align column headers so labels sit above numeric columns
            var headerLine = Repeat(' ', leftPad) + PadLeft(markedHeader, colMarked) + PadLeft(priceHeader, colOur) + PadLeft(qtyHeader, colQty) + PadLeft(amountHeader, colAmount);
            lines.Add(new PrintLine(headerLine));
            // Empty spacer line to visually separate header from items
            lines.Add(new PrintLine(string.Empty));

            var items = order?.OrderItems ?? new List<Models.DTO.OrderItems.OrderItemMiniResDto>();
            var idx = 1;
            foreach (var it in items)
            {
                var qty = (it?.Quantity ?? 0m).ToString("0.##");
                var marked = (it?.MarkedPriceAtSale ?? 0m).ToString("0.00");
                var unit = (it?.PriceAtSale ?? 0m).ToString("0.00");
                var amt = (it?.LineTotal ?? 0m).ToString("0.00");
                var name = it != null ? (it.PrintName ?? it.OriginalItemUuid ?? "") : string.Empty;

                // First line: item index and name
                var nameLine = $"{idx}. {name}";
                if (nameLine.Length > width) nameLine = nameLine.Substring(0, width);
                lines.Add(new PrintLine(PadRight(nameLine, width)));

                // Second line: numeric columns right-aligned under headers (MarkedPrice, OurPrice, QTY, Amount)
                var valueLine = Repeat(' ', leftPad) + PadLeft(marked, colMarked) + PadLeft(unit, colOur) + PadLeft(qty, colQty) + PadLeft(amt, colAmount);
                lines.Add(new PrintLine(valueLine));

                idx++;
            }

            lines.Add(new PrintLine(Repeat('-', width)));

            // Totals row: Gros | Discount | Net
            var gross = (order?.GrossAmount ?? 0m);
            var discount = (order?.TotalDiscount ?? 0m);
            var net = (order?.NetAmount ?? 0m);

            // Totals: align Gross under M.Price column, Discount under OurPrice column,
            // Net under Amount column (right-aligned with item amounts).
            // We'll reuse the item column widths to position totals.
            var headerTotals = Repeat(' ', leftPad) + PadLeft("Gros", colMarked) + PadLeft("Discount", colOur) + PadLeft(string.Empty, colQty) + PadLeft("Net Amount", colAmount);
            lines.Add(new PrintLine(headerTotals));

            var valuesTotals = Repeat(' ', leftPad) + PadLeft(gross.ToString("0.00"), colMarked) + PadLeft(discount.ToString("0.00"), colOur) + PadLeft(string.Empty, colQty) + PadLeft(net.ToString("0.00"), colAmount);
            lines.Add(new PrintLine(valuesTotals));

            // Cash and Balance on the right
            var cashValue = (order?.AmountPaid ?? 0m).ToString("0.00");
            var balanceValue = (order?.Balance ?? (order?.AmountPaid ?? 0m) - (order?.NetAmount ?? 0m)).ToString("0.00");

            var cashLabel = "Cash";
            var valueWidth = 12;
            var cashLabelSep = cashLabel + " : ";
            var cashPrefix = Math.Max(0, width - (cashLabelSep.Length + valueWidth));
            var cashLine = Repeat(' ', cashPrefix) + cashLabelSep + cashValue.PadLeft(valueWidth);
            lines.Add(new PrintLine(cashLine));

            var balLabel = "Balance";
            var balLabelSep = balLabel + " : ";
            var balPrefix = Math.Max(0, width - (balLabelSep.Length + valueWidth));
            var balLine = Repeat(' ', balPrefix) + balLabelSep + balanceValue.PadLeft(valueWidth);
            lines.Add(new PrintLine(balLine));
            //lines.Add(new PrintLine(" "));

            // If discount > 0 show the highlighted discount block before Thank you
            var totalDiscount = order?.TotalDiscount ?? 0m;
            if (totalDiscount > 0)
            {
                lines.Add(new PrintLine(Repeat('_', width)));
                lines.Add(new PrintLine(Center("** Your Total Discount **", width), Bold: true));
                lines.Add(new PrintLine(Repeat(' ', Math.Max(0, (width - 8) / 2)) + totalDiscount.ToString("0.00")));
            }

            lines.Add(new PrintLine(Repeat('_', width)));

            lines.Add(new PrintLine(Center("THANK YOU. PLEASE VISIT AGAIN.", width)));

            //lines.Add(new PrintLine(Repeat('*', width)));
            //// Footer in small font
            //lines.Add(new PrintLine(Center("SOLUTION BY : Devinda Panditha", width), Small: true));
            //lines.Add(new PrintLine(Center("CONTACT : +94772829780", width), Small: true));

            return lines;
        }
    }
}
