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

    public class ImageTripleEventArgs : EventArgs 
    {
        public ImagingEventArgs LeftEye { get; private set; }
        public ImagingEventArgs RightEye { get; private set; }
        public ImagingEventArgs Scene { get; private set; }

        public ImageTripleEventArgs(ImagingEventArgs left, ImagingEventArgs right, ImagingEventArgs scene)
        {
            LeftEye = left;
            RightEye = right;
            Scene = scene;
        }
    }
   
    public interface ICamera : IFrameProducer, IStartable, IStoppableAndWaitable
    {
        int FramesGrabbed { get; }
    }

    public interface IStartable
    {
        void Start();
    }

    public interface IStoppable
    {
        void Stop();
    }

    public interface IStoppableAndWaitable : IStoppable, IWaitable { }

    public interface IFrameProducer {
        event EventHandler<ImagingEventArgs> NewFrame;
    }

    public interface IWaitable {
        void Wait();
        void Wait(CancellationToken cancellationToken);
        bool Wait(int millisecondsTimeout);
        bool Wait(TimeSpan timeout);
        bool Wait(int millisecondsTimeout, CancellationToken cancellationToken);
    }

    public interface IImageHandler {
        void HandleImage(ImagingEventArgs args);
    }
}
