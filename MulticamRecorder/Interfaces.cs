using System;
using System.Windows.Media.Imaging;


namespace MulticamRecorder
{
    public class ImagingEventArgs : EventArgs
    {
        public long Timestamp { get; private set; }
        public int Frame { get; private set; }
        public BitmapFrame Bitmap { get; private set; }

        public ImagingEventArgs(BitmapFrame bitmap, int frame, long timestamp)
        {
            Bitmap = bitmap;
            Frame = frame;
            Timestamp = timestamp;
        }
    }
   
    public interface ICamera
    {
        void Start();
        void Stop();
        event EventHandler<ImagingEventArgs> NewFrameArrived;
    }
}
