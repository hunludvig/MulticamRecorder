using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Threading;
using CatenaLogic.Windows.Presentation.WebcamPlayer;

using Common.Logging;

public class Test
{
    private BlockingCollection<ThreadStart> tasks = new BlockingCollection<ThreadStart>();
    private CancellationTokenSource _stopSignal = new CancellationTokenSource();
    private CancellationToken _stopToken;

    private static ILog log = LogManager.GetCurrentClassLogger();
    private void ProcessTasks()
    {
        foreach (ThreadStart task in tasks.GetConsumingEnumerable(_stopToken))
        {
            task.Invoke();
            Console.WriteLine("Loop akarmi");
        }
    }

    private void TestExecutor() {
        _stopToken = _stopSignal.Token;
        new Thread(ProcessTasks).Start();
        tasks.Add(new ThreadStart(delegate { Console.WriteLine("egy"); }));
        tasks.Add(new ThreadStart(delegate { Console.WriteLine("ket"); }));
        tasks.Add(new ThreadStart(delegate { Console.WriteLine("ha"); }));
        tasks.Add(new ThreadStart(delegate { Console.WriteLine("negy"); }));
        tasks.CompleteAdding();
    }


    //private void DiyDispatcher() {
    //    Dispatcher.Run();
    //}
    //new Thread(DiyDispatcher).Start();     


    public Test() {
        //TestExecutor();
          
        WpfCam();
    }


    public static void Main(string[] args)
    {
        new Test();
    }



    public void WpfCam() {
        String moniker = "@device:pnp:\\\\?\\usb#vid_0bda&pid_5727&mi_00#6&29acc6bd&0&0000#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\\global";
        CapDevice device = new CapDevice(moniker);

        device.Framerate = 15;

        device.NewFrameArrived += showFps;
        device.NewFrameArrived += saveImage;
        
        device.Start();

        Console.WriteLine(device.Name + " running : " + device.IsRunning);
        
        Console.WriteLine("Recording");
        Console.ReadLine();

        Console.WriteLine("Stop");
        device.Stop();

        Console.ReadLine();
    }

    private long numOfFrames = 0;
    private Stopwatch stopwatch = new Stopwatch();

    void showFps(object sender, EventArgs e) {
        if (!stopwatch.IsRunning)
            stopwatch.Start();
        else {
            numOfFrames++;
            long time = stopwatch.ElapsedMilliseconds;
            float fps = 1000f * numOfFrames / time;
            log.DebugFormat("FPS: {0}", fps);
        }
    }


    void saveImage(object sender, EventArgs e)
    {
        log.Trace("Save Image");
        // Get the sender
        CapDevice typedSender = sender as CapDevice;
        if (typedSender != null)
        {
            // Set the source of the image
            PngBitmapEncoder Encoder = new PngBitmapEncoder();
            BitmapFrame bitmap = BitmapFrame.Create(typedSender.BitmapSource);
            bitmap.Freeze();
            Encoder.Frames.Add(bitmap);
            FileStream file = new FileStream("pics/img" + Stopwatch.GetTimestamp() + ".png", FileMode.Create);
            Encoder.Save(file);
            file.Close();
        }
    }
}
