using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MulticamRecorder
{
    public abstract class ImageSaver
    {
        public String Filename { get; set; }
        private int framesProcessed = 0;
        private Action<object> saveImageAction;

        public int FramesProcessed { get { return Thread.VolatileRead(ref framesProcessed); } }

        protected abstract BitmapEncoder newEncoder();

        public ImageSaver()
        {
            saveImageAction = (object bm) => SaveImage((ImagingEventArgs)bm);
        }

        public void SaveImage(object sender, ImagingEventArgs args)
        {
            Task.Factory.StartNew(saveImageAction, args, TaskCreationOptions.AttachedToParent);
        }

        private void SaveImage(ImagingEventArgs image)
        {
            BitmapEncoder encoder = encodeImage(image);
            FileStream file = createFile(image);
            encoder.Save(file);
            file.Close();
            Interlocked.Increment(ref framesProcessed);
        }

        private BitmapEncoder encodeImage(ImagingEventArgs image)
        {
            BitmapEncoder encoder = newEncoder();
            encoder.Frames.Add(image.Bitmap);
            return encoder;
        }

        private FileStream createFile(ImagingEventArgs image)
        {
            String filename = generateFilename(image);
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            FileStream file = new FileStream(filename, FileMode.Create);
            return file;
        }

        private String generateFilename(ImagingEventArgs image)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(Filename, image.Frame, image.Timestamp);
            return builder.ToString();
        }
    }

    public class PngSaver: ImageSaver {
        protected override BitmapEncoder newEncoder()
        {
            return new PngBitmapEncoder();
        }
    }

    public class JpegSaver : ImageSaver
    {
        protected override BitmapEncoder newEncoder()
        {
            return new JpegBitmapEncoder();
        }
    }
}