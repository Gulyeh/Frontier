using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Frontier.Methods
{
    class CheckNIP
    {
        public static bool Checker(string data)
        {
            char[] nip = data.ToCharArray();
            int[] multipliers = new int[] { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
            int sumcheck = 0;

            for(int i = 0; i<nip.Length - 1; i++)
            {
                sumcheck += Int32.Parse(nip[i].ToString()) * multipliers[i];
            }

            if ((sumcheck % 11).ToString() == nip[nip.Length-1].ToString())
            {
                return true;
            }

            return false;
        }
    }
}
