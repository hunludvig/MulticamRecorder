using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace MulticamRecorder
{
    /*
     * Waits for one enter if the program has its own console otherwise doesn't do anything as here
     * http://stackoverflow.com/questions/9009333/how-to-check-if-the-program-is-run-from-a-console 
     */
    public class ConsoleHelper
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hwnd, out IntPtr lpdwProcessId);

        public static void waitForKey() {
            IntPtr processId = new IntPtr(Process.GetCurrentProcess().Id);
            IntPtr windowThreadProcId;
            GetWindowThreadProcessId(GetConsoleWindow(), out windowThreadProcId);
            if ( processId == windowThreadProcId)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
