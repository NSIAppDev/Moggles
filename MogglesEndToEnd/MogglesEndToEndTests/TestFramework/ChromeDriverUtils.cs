using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace MogglesEndToEndTests.TestFramework
{
    public class ChromeDriverUtils
    {
        public static void KillChromeDriverProcesses()
        {
            try
            {
                var chromeDriverProcesses = Process.GetProcessesByName("chromedriver");

                if (!chromeDriverProcesses.Any()) return;
                foreach (var process in chromeDriverProcesses)
                {
                    process.Kill();
                }
            }
            catch (Win32Exception ex)
            {
                Trace.WriteLine($"WARNING: Failed killing chrome driver processes: {ex.Message}");
            }
        }        
    }
}
