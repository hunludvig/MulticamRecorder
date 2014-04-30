/*
 * http://stackoverflow.com/questions/57615/how-to-add-a-timeout-to-console-readline
 * author: JSQuareD
 */

using System;
using System.Threading;

namespace MulticamRecorder
{
    class Reader
    {
        private static Thread inputThread;
        private static AutoResetEvent getInput, gotInput;
        private static string input;

        static Reader()
        {
            inputThread = new Thread(reader);
            inputThread.IsBackground = true;
            inputThread.Start();
            getInput = new AutoResetEvent(false);
            gotInput = new AutoResetEvent(false);
        }

        private static void reader()
        {
            while (true)
            {
                getInput.WaitOne();
                input = Console.ReadLine();
                gotInput.Set();
            }
        }

        public static string ReadLine(int timeOutMillisecs)
        {
            getInput.Set();
            bool success = gotInput.WaitOne(timeOutMillisecs);
            if (success)
                return input;
            else
                throw new TimeoutException("User did not provide input within the timelimit.");
        }
    }
}
