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

        static void Main(string[] args)
        {
            ILog log = LogManager.GetCurrentClassLogger();

            IApplicationContext context = ContextRegistry.GetContext();

            watch = new Stopwatch();

            IDictionary cameras = context.GetObjectsOfType(typeof(ICamera));
            
            //Device = (ICamera)context.GetObject("camera");
            //Device.BitmapUpdated += ShowFps;

            Console.WriteLine(Stopwatch.Frequency);

            watch.Start();

            Console.ReadLine();

            log.Info("Stopping camera");
            foreach (ICamera camera in cameras.Values)
            {
                camera.Stop();
            }
        
            //Console.ReadLine();
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

        public void SaveImage(object sender, ImagingEventArgs args)
        {
            Action<object> saveImageAction = (object bm) => SaveImage((ImagingEventArgs)bm);
            Task.Factory.StartNew(saveImageAction, args);
        }

        private void SaveImage(ImagingEventArgs image)
        {
            PngBitmapEncoder Encoder = new PngBitmapEncoder();
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
