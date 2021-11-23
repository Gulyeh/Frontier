using System;
using System.Windows;

namespace Frontier.Methods
{
    class Calculate
    {
        public static decimal GetNetto(decimal vat, decimal brutto)
        {
            var amount = brutto / (1+(vat / 100));
            return decimal.Parse(String.Format("{0:0.00}", Math.Round(amount, 2)));
        }
        public static decimal GetBrutto(decimal vat, decimal netto)
        {
            var amount = netto * (1 + (vat / 100));
            return decimal.Parse(String.Format("{0:0.00}", Math.Round(amount, 2)));
        }
        public static decimal GetMarginVAT(decimal vat, decimal BruttoBuy, decimal BruttoSell)
        {
            decimal amount;
            if (BruttoSell <= BruttoBuy)
            {
                amount = 0;
            }
            else
            {
                decimal BruttoMargin = BruttoSell - BruttoBuy;
                decimal NettoMargin = BruttoMargin / (1 + (vat / 100));
                amount = BruttoMargin - NettoMargin;
            }

            return decimal.Parse(String.Format("{0:0.00}", Math.Round(amount, 2)));
        }
    }
}
