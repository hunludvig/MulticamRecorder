using System;
using System.Collections;
using System.Diagnostics;
using System.Configuration;

using Spring.Context;
using Spring.Context.Support;

using Common.Logging;

namespace MulticamRecorder
{
    class Program
    {
        private static int UPDATE_INTERVAL = 1000; //ms
        
        private static ILog log = LogManager.GetCurrentClassLogger();

        private static void Init() 
        {
            try
            {
                UPDATE_INTERVAL = Int32.Parse(ConfigurationManager.AppSettings["update_interval"]);
                if (UPDATE_INTERVAL < -1)
                {
                    throw new ArgumentOutOfRangeException("Update interval should be non negative");
                }
            }
            catch (Exception e)
            {
                log.Error("Update Interval not recognized from config file", e);
            }
        }

        static void Main(string[] args)
        {
            Init();

            IApplicationContext context = ContextRegistry.GetContext();

            IDictionary cameras = context.GetObjectsOfType(typeof(ICamera));
            IDictionary toStop = context.GetObjectsOfType(typeof(IStoppableAndWaitable));
            IDictionary consumers = context.GetObjectsOfType(typeof(ImageSaver));

            log.Info("Stopwatch frequency: " + Stopwatch.Frequency + " ticks/sec");

            Boolean run = cameras.Count > 0;
            while (run)
            {
                try
                {
                    Reader.ReadLine(UPDATE_INTERVAL);

                    log.Info("Stopping cameras");
                    foreach (IStoppable device in toStop.Values)
                    {
                        device.Stop();
                    }
                    run = false;
                }
                catch (TimeoutException)
                {
                    printDetails(cameras, consumers);
                }
            }

            log.Info("Waiting for cameras");
            foreach (IWaitable device in toStop.Values)
            {
                 while(!device.Wait(UPDATE_INTERVAL))
                     printDetails(cameras, consumers);
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
