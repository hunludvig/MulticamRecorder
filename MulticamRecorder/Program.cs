using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;

using Spring.Context;
using Spring.Context.Support;

using Common.Logging;

namespace MulticamRecorder
{
    class Program
    {
        static private Stopwatch watch;
        static ICamera Device;

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

    class PngSaver
    {
        public String Filename { get; set; }
        public String EncoderClass { get; set;}
            //set {
            //    _EncoderClass = Type.GetType(value);
            //    if (_EncoderClass.IsSubclassOf(typeof(BitmapEncoder)))
            //        EncoderClass = value;
            //    else
            //        throw new Exception("Not valid subclass of BitmapEncoder: "+value);
            //} }

        private Type _EncoderClass;

        public void SaveImage(object sender, ImagingEventArgs args)
        {
            Action<object> saveImageAction = (object bm) => SaveImage((ImagingEventArgs)bm);
            Task.Factory.StartNew(saveImageAction, args, TaskCreationOptions.AttachedToParent);
        }

        private void SaveImage(ImagingEventArgs image)
        {
            //if (_EncoderClass == default(Type))
            //    return;
            BitmapEncoder Encoder = new PngBitmapEncoder();
            Encoder.Frames.Add(image.Bitmap);
            String filename = generateFilename(image);
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            FileStream file = new FileStream(filename, FileMode.Create);
            Encoder.Save(file);
            file.Close();
        }

        private String generateFilename(ImagingEventArgs image) {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(Filename, image.Frame, image.Timestamp);
            return builder.ToString();
        }
    }
}
