using System;

namespace Frontier.Methods.Numerics
{
    class Checkers
    {
        public static bool CheckNIP(string data)
        {
            char[] nip = data.ToCharArray();
            int[] multipliers = new int[] { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
            int sumcheck = 0;

            for (int i = 0; i < nip.Length - 1; i++)
            {
                sumcheck += Int32.Parse(nip[i].ToString()) * multipliers[i];
            }

            if ((sumcheck % 11).ToString() == nip[nip.Length - 1].ToString())
            {
                return true;
            }

            return false;
        }
        public static bool CheckREGON(string data)
        {
            int sumcheck = 0;
            char[] regon = data.ToCharArray();
            int[] multipliers;
            if (data.Length == 9)
            {
                multipliers = new int[] { 8, 9, 2, 3, 4, 5, 6, 7 };
            }
            else
            {
                multipliers = new int[] { 2, 4, 8, 5, 0, 9, 7, 3, 6, 1, 2, 4, 8 };
            }

            for (int i = 0; i < regon.Length - 1; i++)
            {
                sumcheck += Int32.Parse(regon[i].ToString()) * multipliers[i];
            }

            if ((sumcheck % 11).ToString() == regon[regon.Length - 1].ToString())
            {
                return true;
            }
            return false;
        }
    }
}
