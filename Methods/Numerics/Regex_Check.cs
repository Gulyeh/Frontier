using System.Text.RegularExpressions;

namespace Frontier.Methods.Numerics
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
