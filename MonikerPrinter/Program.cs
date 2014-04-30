using System;
using System.Xml.Linq;
using CatenaLogic;

namespace MulticamRecorder
{
    public class MonikerPrinter
    {
        public static void Main()
        {
            FilterInfo[] filters = CapDevice.DeviceMonikers;
            foreach (FilterInfo filter in filters)
            {
                Console.WriteLine(filter.Name + ":");
                Console.WriteLine(new XText(filter.MonikerString).ToString());
            }
            ConsoleHelper.waitForKey();
        }
    }
}