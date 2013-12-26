///////////////////////////////////////////////////////////////////////////////
// CapDevice v1.1
//
// This software is released into the public domain.  You are free to use it
// in any way you like, except that you may not sell this source code.
//
// This software is provided "as is" with no expressed or implied warranty.
// I accept no liability for any damage or loss of business that this software
// may cause.
// 
// This source code is originally written by Tamir Khason (see http://blogs.microsoft.co.il/blogs/tamir
// or http://www.codeplex.com/wpfcap).
// 
// Modifications are made by Geert van Horrik (CatenaLogic, see http://blog.catenalogic.com) 
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

using Common.Logging;

using MulticamRecorder;

namespace CatenaLogic
{
    public class CapDevice : DependencyObject, IDisposable, ICamera
    {
        #region Win32
        static readonly Guid FilterGraph = new Guid(0xE436EBB3, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

        static readonly Guid SampleGrabber = new Guid(0xC1F400A0, 0x3F08, 0x11D3, 0x9F, 0x0B, 0x00, 0x60, 0x08, 0x03, 0x9E, 0x37);

        public static readonly Guid SystemDeviceEnum = new Guid(0x62BE5D10, 0x60EB, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

        public static readonly Guid VideoInputDevice = new Guid(0x860BB310, 0x5D01, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

        [ComVisible(false)]
        internal class MediaTypes
        {
            public static readonly Guid Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid Interleaved = new Guid(0x73766169, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid Audio = new Guid(0x73647561, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid Text = new Guid(0x73747874, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid Stream = new Guid(0xE436EB83, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
        }

        [ComVisible(false)]
        internal class MediaSubTypes
        {
            public static readonly Guid YUYV = new Guid(0x56595559, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid IYUV = new Guid(0x56555949, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid DVSD = new Guid(0x44535644, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid RGB1 = new Guid(0xE436EB78, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB4 = new Guid(0xE436EB79, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB8 = new Guid(0xE436EB7A, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB565 = new Guid(0xE436EB7B, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB555 = new Guid(0xE436EB7C, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB24 = new Guid(0xE436Eb7D, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB32 = new Guid(0xE436EB7E, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid Avi = new Guid(0xE436EB88, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid Asf = new Guid(0x3DB80F90, 0x9412, 0x11D1, 0xAD, 0xED, 0x00, 0x00, 0xF8, 0x75, 0x4B, 0x99);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
        #endregion

        #region Variables
        private Thread _grabThread = null;
        private BlockingCollection<ThreadStart> tasks = new BlockingCollection<ThreadStart>();
        private CancellationTokenSource stopSignal = new CancellationTokenSource();
        private IGraphBuilder _graph = null;
        private ISampleGrabber _grabber = null;
        private IBaseFilter _sourceObject = null;
        private IBaseFilter _grabberObject = null;
        private IMediaControl _control = null;
        private CapGrabber _capGrabber = null;
        private IntPtr _map = IntPtr.Zero;
        private IntPtr _section = IntPtr.Zero;
        private int frames = 0;

        private static ILog log = LogManager.GetCurrentClassLogger();

        private string _monikerString = "";
        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes the default capture device
        /// </summary>
        public CapDevice()
            : this("") {}

        /// <summary>
        /// Initializes a specific capture device
        /// </summary>
        /// <param name="moniker">Moniker string that represents a specific device</param>
        public CapDevice(string moniker)
        {

            // Store moniker string
            InitializeDeviceForMoniker(moniker);

            // Check if this code is invoked by an application or as a user control
            if (Application.Current != null)
            {
                // Application, subscribe to exit event so we can shut down
                Application.Current.Exit += new ExitEventHandler(CurrentApplication_Exit);
            }

        }

        private void InitGrabThread() {
            _grabThread = new Thread(ProcessTasks);
            _grabThread.Start();
            log.Trace("Grap Thread started");
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
        #endregion

        #region Events
        /// <summary>
        /// Event that is invoked when a new bitmap is ready
        /// </summary>
        public event EventHandler NewBitmapReady;
        public event EventHandler<ImagingEventArgs> NewFrameArrived;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the device monikers
        /// </summary>
        public static FilterInfo[] DeviceMonikers
        {
            get
            {
                List<FilterInfo> filters = new List<FilterInfo>();
                IMoniker[] ms = new IMoniker[1];
                ICreateDevEnum enumD = Activator.CreateInstance(Type.GetTypeFromCLSID(SystemDeviceEnum)) as ICreateDevEnum;
                IEnumMoniker moniker;
                Guid g = VideoInputDevice;
                if (enumD.CreateClassEnumerator(ref g, out moniker, 0) == 0)
                {
                    while (true)
                    {
                        int r = moniker.Next(1, ms, IntPtr.Zero);
                        if (r != 0 || ms[0] == null)
                            break;
                        filters.Add(new FilterInfo(ms[0]));
                        Marshal.ReleaseComObject(ms[0]);
                        ms[0] = null;
                    }
                }

                return filters.ToArray();
            }
        }

        /// <summary>
        /// Gets the available devices
        /// </summary>
        public static CapDevice[] Devices
        {
            get
            {
                // Declare variables
                List<CapDevice> devices = new List<CapDevice>();

                // Loop all monikers
                foreach (FilterInfo moniker in DeviceMonikers)
                {
                    devices.Add(new CapDevice(moniker.MonikerString));
                }

                // Return result
                return devices.ToArray();
            }
        }

        /// <summary>
        /// Wrapper for the BitmapSource dependency property
        /// </summary>
        public InteropBitmap BitmapSource
        {
            get; private set;
        }

        /// <summary>
        /// Wrapper for the Name dependency property
        /// </summary>
        public string Name
        {
            get; private set;
        }

        /// <summary>
        /// Wrapper for the MonikerString dependency property
        /// </summary>
        public string MonikerString
        {
            get {return _monikerString;}
            set { _monikerString = value; }
        }

        /// <summary>
        /// Wrapper for the Framerate dependency property
        /// </summary>
        public float Framerate
        {
            get ; set;
        }

        /// <summary>
        /// Gets whether the capture device is currently running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                // Check if we have a worker thread
                if (_grabThread == null) return false;

                // Check if we can join the thread
                if (_grabThread.Join(0) == false && !tasks.IsAddingCompleted) return true;

                // Release
                Release();

                // Not running
                return false;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Invoked when the application exits
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        private void CurrentApplication_Exit(object sender, ExitEventArgs e)
        {
            log.Debug("exit application");
            // Dispose
            Dispose();
        }

        /// <summary>
        /// Invoked when a new frame arrived
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        private void capGrabber_NewFrameArrived(object sender, EventArgs e)
        {
            log.Trace("capGrabber new frame arrived");
            // Make sure to be thread safe
            frames++;
                addTask(delegate
                {
                    if (BitmapSource != null)
                    {
                        BitmapFrame bitmap = BitmapFrame.Create(BitmapSource);
                        bitmap.Freeze();
                        if (NewFrameArrived != null)
                            NewFrameArrived(this, new ImagingEventArgs(bitmap, frames, Stopwatch.GetTimestamp()));
                        BitmapSource.Invalidate();
                    }
                });
        }

        private void InitCapGrabber()
        {
            log.Trace("capGrabber property changed");
            addTask(delegate
            {
                try
                {
                    if ((_capGrabber.Width != default(int)) && (_capGrabber.Height != default(int)))
                    {
                        // Get the pixel count
                        uint pcount = (uint)(_capGrabber.Width * _capGrabber.Height * PixelFormats.Bgr32.BitsPerPixel / 8);

                        // Create a file mapping
                        _section = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, pcount, null);
                        _map = MapViewOfFile(_section, 0xF001F, 0, 0, pcount);

                        // Get the bitmap
                        BitmapSource = Imaging.CreateBitmapSourceFromMemorySection(_section, _capGrabber.Width,
                            _capGrabber.Height, PixelFormats.Bgr32, _capGrabber.Width * PixelFormats.Bgr32.BitsPerPixel / 8, 0) as InteropBitmap;
                        _capGrabber.Map = _map;

                        // Invoke event
                        if (NewBitmapReady != null)
                        {
                            NewBitmapReady(this, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Trace
                    log.Error(ex);
                }
            });
        }

        /// <summary>
        /// Initialize the device for a specific moniker
        /// </summary>
        /// <param name="moniker">Moniker to initialize the device for</param>
        private void InitializeDeviceForMoniker(string moniker)
        {
            log.Trace("init device for new moniker");
            // Store moniker (since dependency properties are not thread-safe, store it locally as well)
            _monikerString = moniker;

            // Find the name
            foreach (FilterInfo filterInfo in DeviceMonikers)
            {
                if (filterInfo.MonikerString == moniker)
                {
                    Name = filterInfo.Name;
                    break;
                }
            }
        }

        private void addTask(Action task) {
            if (!tasks.IsAddingCompleted)
                tasks.Add(new ThreadStart(task));
        }

        /// <summary>;
        /// Starts grabbing images from the capture device
        /// </summary>
        public void Start()
        {
            log.Debug("start CapDevice");

            // First check if we have a valid moniker string
            if (string.IsNullOrEmpty(_monikerString)) return;

            InitGrabThread();

            // Create new grabber
            _capGrabber = new CapGrabber();
            _capGrabber.NewFrameArrived += new EventHandler(capGrabber_NewFrameArrived);

            // Start the thread
            //_serviceThread = new Thread(RunWorker);
            //_serviceThread.Start();
            addTask(Init);
            InitCapGrabber();
        }

 

        /// <summary>
        /// Worker thread that captures the images
        /// </summary>
        private void Init()
        {
            try
            {
                log.Trace("Start worker thread");
                // Create the main graph
                _graph = Activator.CreateInstance(Type.GetTypeFromCLSID(FilterGraph)) as IGraphBuilder;

                // Create the webcam source
                _sourceObject = FilterInfo.CreateFilter(_monikerString);

                // Create the grabber
                _grabber = Activator.CreateInstance(Type.GetTypeFromCLSID(SampleGrabber)) as ISampleGrabber;
                _grabberObject = _grabber as IBaseFilter;

                // Add the source and grabber to the main graph
                _graph.AddFilter(_sourceObject, "source");
                _graph.AddFilter(_grabberObject, "grabber");

                using (AMMediaType mediaType = new AMMediaType())
                {
                    mediaType.MajorType = MediaTypes.Video;
                    mediaType.SubType = MediaSubTypes.RGB32;
                    _grabber.SetMediaType(mediaType);

                    if (_graph.Connect(_sourceObject.GetPin(PinDirection.Output, 0), _grabberObject.GetPin(PinDirection.Input, 0)) >= 0)
                    {
                        if (_grabber.GetConnectedMediaType(mediaType) == 0)
                        {
                            // During startup, this code can be too fast, so try at least 3 times
                            int retryCount = 0;
                            bool succeeded = false;
                            while ((retryCount < 3) && !succeeded)
                            {
                                // Tried again
                                retryCount++;

                                try
                                {
                                    // Retrieve the grabber information
                                    VideoInfoHeader header = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                                    _capGrabber.Width = header.BmiHeader.Width;
                                    _capGrabber.Height = header.BmiHeader.Height;

                                    // Succeeded
                                    succeeded = true;
                                }
                                catch (Exception retryException)
                                {
                                    // Trace
                                    log.InfoFormat("Failed to retrieve the grabber information, tried {0} time(s)", retryCount);

                                    // Sleep
                                    Thread.Sleep(50);
                                }
                            }
                        }
                    }
                    _graph.Render(_grabberObject.GetPin(PinDirection.Output, 0));
                    _grabber.SetBufferSamples(false);
                    _grabber.SetOneShot(false);
                    _grabber.SetCallback(_capGrabber, 1);
                    log.Trace("_grabber set up");
                    
                    // Get the video window
                    IVideoWindow wnd = (IVideoWindow)_graph;
                    wnd.put_AutoShow(false);
                    wnd = null;

                    // Create the control and run
                    _control = (IMediaControl)_graph;
                    _control.Run();
                    log.Trace("control runs");

                    // Wait for the stop signal
                    //while (!_stopSignal.WaitOne(0, true))
                    //{
                    //    Thread.Sleep(10);
                    //}
                }
            }catch (Exception ex)
            {
                // Trace
                log.Debug(ex);
                Release();
            }
        }


        /// <summary>
        /// Stops grabbing images from the capture device
        /// </summary>
        public void Stop()
        {
            log.Debug("stop CapDevice");
            // Check if the capture device is even running
            if (IsRunning)
            {
                // Yes, stop via the event
                tasks.CompleteAdding();
                if(!_grabThread.Join(100))
                     stopSignal.Cancel();
                StopDevice();
            }

        }

        /// <summary>
        /// Releases the capture device
        /// </summary>
        private void Release()
        {
            log.Trace("Release all references");

            // Clean up
            _graph = null;
            _sourceObject = null;
            _grabberObject = null;
            _grabber = null;
            _capGrabber = null;
            _control = null;
        }

       private void StopDevice() {
           try {
                    log.Debug("stop device");
                    tasks.CompleteAdding();
                    //_control.StopWhenReady();
                    _control.StopWhenReady();
                
            }  catch (Exception ex)
            {
                // Trace
                log.Debug(ex);
            }
            finally
            {
                // Clean up
                Release();
            }
        }

        private void ProcessTasks()
        {
            log.Trace("TaskProcessor started");
            try
            {
                foreach (ThreadStart task in tasks.GetConsumingEnumerable(stopSignal.Token))
                    task.Invoke();
            }
            catch (OperationCanceledException ex)
            {
                log.Debug(ex);
            }
        }

        #endregion
    }

}
