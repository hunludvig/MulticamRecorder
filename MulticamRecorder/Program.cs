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
        private const int UPDATE_INTERVAL = 1000; //ms
       
        private static ILog log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            ILog log = LogManager.GetCurrentClassLogger();

            IApplicationContext context = ContextRegistry.GetContext();

            IDictionary cameras = context.GetObjectsOfType(typeof(ICamera));
            IDictionary consumers = context.GetObjectsOfType(typeof(ImageSaver));

            log.Info("Stopwatch frequency: " + Stopwatch.Frequency + " ticks/sec");

            Boolean run = cameras.Count > 0;
            Boolean stopped = false;
            while (run)
            {
                try
                {
                    if (!stopped)
                    {
                        Reader.ReadLine(UPDATE_INTERVAL);

                        log.Info("Stopping camera");
                        foreach (ICamera camera in cameras.Values)
                        {
                            camera.Stop();
                        }
                        stopped = true;
                        log.Info("Waiting for camera");
                    }
                    else
                    {
                        foreach (ICamera camera in cameras.Values)
                        {
                            camera.Wait(UPDATE_INTERVAL);
                        }
                        run = false;
                    }
                }
                catch (TimeoutException)
                {
                    printDetails(cameras, consumers);
                }
            }
            
            log.Info("Recording finished");
            ConsoleHelper.waitForKey();
        }

        private static void printDetails(IDictionary cameras, IDictionary consumers)
        {
            int framesGrabbed = 0;
            int framesProcessed = 0;
            foreach (ICamera camera in cameras.Values)
            {
                framesGrabbed += camera.FramesGrabbed;
            }
            foreach (ImageSaver consumer in consumers.Values)
            {
                framesProcessed += consumer.FramesProcessed;
            }
            Console.WriteLine("Frames grabbed: " + framesGrabbed +
                " Frames saved: " + framesProcessed);
        }
    }
}
