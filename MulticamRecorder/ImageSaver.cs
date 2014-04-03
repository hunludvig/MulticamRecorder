using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace MulticamRecorder
{
    public abstract class ImageSaver
    {
        public String Filename { get; set; }
        
        protected abstract BitmapEncoder newEncoder();

        public void SaveImage(object sender, ImagingEventArgs args)
        {
            Action<object> saveImageAction = (object bm) => SaveImage((ImagingEventArgs)bm);
            Task.Factory.StartNew(saveImageAction, args, TaskCreationOptions.AttachedToParent);
        }

        private void SaveImage(ImagingEventArgs image)
        {
            BitmapEncoder Encoder = newEncoder();
            Encoder.Frames.Add(image.Bitmap);
            String filename = generateFilename(image);
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            FileStream file = new FileStream(filename, FileMode.Create);
            Encoder.Save(file);
            file.Close();
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