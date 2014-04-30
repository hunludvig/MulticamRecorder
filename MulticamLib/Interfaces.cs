using System;
using System.Windows.Media.Imaging;
using System.Threading;


namespace MulticamRecorder
{
    public abstract class ImagingEventArgs : EventArgs
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
        int FramesGrabbed { get; }
        void Wait();
        void Wait(CancellationToken cancellationToken);
        bool Wait(int millisecondsTimeout);
        bool Wait(TimeSpan timeout);
        bool Wait(int millisecondsTimeout, CancellationToken cancellationToken);
    }
}
