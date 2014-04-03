using System;
using System.Collections;
using System.Diagnostics;

using Spring.Context;
using Spring.Context.Support;

using Common.Logging;

namespace MulticamRecorder
{
    class Program
    {
        static private Stopwatch watch;

        private static ILog log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            ILog log = LogManager.GetCurrentClassLogger();

            IApplicationContext context = ContextRegistry.GetContext();

            watch = new Stopwatch();

            IDictionary cameras = context.GetObjectsOfType(typeof(ICamera));
            
            //Device = (ICamera)context.GetObject("camera");
            //Device.BitmapUpdated += ShowFps;

            log.Info("Stopwatch frequency: " + Stopwatch.Frequency + " ticks/sec");

            watch.Start();

            Console.ReadLine();

            log.Info("Stopping camera");
            foreach (ICamera camera in cameras.Values)
            {
                camera.Stop();
            }

            log.Info("Waiting for camera");
            foreach (ICamera camera in cameras.Values)
            {
                camera.Wait();
            }
            
            log.Info("Recording finished");
        }

        static void ShowEvent(object sender, EventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString());
        }

        static void ShowFps(object sender, ImagingEventArgs args)
        {
            Console.WriteLine(args.Frame * 1000f / watch.ElapsedMilliseconds);
        }
    }
}
