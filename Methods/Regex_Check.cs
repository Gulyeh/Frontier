using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Frontier.Methods
{
    class Regex_Check
    {
        public static bool CheckPrice(string data)
        {
            Regex regex = new Regex("[^0-9,]+");
            return regex.IsMatch(data);
        }
        public static bool CheckNumbers(string data)
        {
            Regex regex = new Regex(@"\s*[^0-9]+\s*");
            return regex.IsMatch(data);
        }
        public static bool CheckPostCode(string data)
        {
            Regex regex = new Regex(@"\s*[^0-9-]+\s*");
            return regex.IsMatch(data);
        }
    }
}
