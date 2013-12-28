using System;
using System.Collections;
using System.Xml.Linq;
using AForge.Video.DirectShow;

namespace MulticamRecorder
{


    public class MonikerPrinter
    {
        public static void Main()
        {
            ICollection filters = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (AForge.Video.DirectShow.FilterInfo filter in filters)
            {
                Console.WriteLine(filter.Name + ":");
                Console.WriteLine(new XText(filter.MonikerString).ToString());
            }
            Console.ReadLine();
        }
    }
}