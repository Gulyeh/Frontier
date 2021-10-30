using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Methods
{
    class Calculate
    {
        public static double GetBrutto(double vat, double netto)
        {
            var amount = netto + (netto * (vat / 100));
            return Math.Round(amount, 2);
        }
    }
}
