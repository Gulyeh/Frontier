using System.Diagnostics;
using System.Threading.Tasks;

namespace Frontier.Methods.Invoices
{
    class PermaDelete
    {
        public static Task Delete(string path, string filename)
        {
            Process p = new Process();

            p.StartInfo = new ProcessStartInfo("cmd.exe", "/c cd " + path + " && del " + filename)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p.Start();
            p.WaitForExit();

            return Task.CompletedTask;
        }
    }
}
