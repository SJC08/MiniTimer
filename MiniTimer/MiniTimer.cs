using System.Diagnostics;

namespace Asjc.MiniTimer
{
    /// <summary>
    /// Specifies the operational status of a <see cref="MiniTimer"/> instance.
    /// </summary>
    public enum TimerStatus
    {
        /// <summary>
        /// The timer is not running.
        /// </summary>
        Stopped,

        /// <summary>
        /// The timer is executing the <see cref="MiniTimer.Elapsed"/> event handler.
        /// </summary>
        Executing,

        /// <summary>
        /// The timer is waiting for the interval.
        /// </summary>
        Waiting
    }

    /// <summary>
    /// Provides a simple timer for general timing tasks.
    /// </summary>
    /// <remarks>
    /// By default, the timer executes in a new thread that is created each time it is started.
    /// </remarks>
    public class MiniTimer : IDisposable
    {
        private volatile bool enabled;
        private volatile Thread? thread;
        private volatile TimerStatus status = TimerStatus.Stopped;
        private readonly AutoResetEvent are = new(false);
        private readonly Stopwatch stopwatch = new();
        private readonly object syncLock = new();
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class.
        /// </summary>
        public MiniTimer() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        public MiniTimer(int interval)
        {
            Interval = interval;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="elapsed"/> event.
        /// </summary>
        /// <param name="elapsed">The event handler for the Elapsed event.</param>
        public MiniTimer(Action<MiniTimer> elapsed)
        {
            Elapsed += elapsed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/> and <paramref name="elapsed"/> event.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <param name="elapsed">The event handler for the Elapsed event.</param>
        public MiniTimer(int interval, Action<MiniTimer> elapsed)
        {
            Interval = interval;
            Elapsed += elapsed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/>, <paramref name="enabled"/> status, and <paramref name="elapsed"/> event.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <param name="enabled">The enabled status of the timer.</param>
        /// <param name="elapsed">The event handler for the Elapsed event.</param>
        public MiniTimer(int interval, Action<MiniTimer> elapsed, bool enabled)
        {
            Interval = interval;
            Elapsed += elapsed;
            Enabled = enabled;
        }

        /// <summary>
        /// Gets or sets the interval, expressed in milliseconds, at which to raise the <see cref="Elapsed"/> event.
        /// </summary>
        /// <remarks>
        /// This is measured from the time before each <see cref="Elapsed"/> call.
        /// In fact, you can even change this value at any time except when waiting.
        /// </remarks>
        public int Interval { get; set; }

        /// <summary>
        /// Gets or sets a <see langword="bool"/> indicating whether the <see cref="MiniTimer"/> needs to be enabled.
        /// </summary>
        /// <remarks>
        /// Even if the value is <see langword="false"/>, the timer may keep running.
        /// </remarks>
        public bool Enabled
        {
            get
            {
                lock (syncLock) return enabled;
            }
            set
            {
                lock (syncLock)
                {
                    if (enabled == value) return;
                    enabled = value;
                    if (enabled)
                        StartTimer();
                    else
                        StopTimer();
                }
            }
        }

        /// <summary>
        /// Gets a <see langword="bool"/> indicating whether the <see cref="MiniTimer"/> is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (syncLock) return thread != null && thread.IsAlive;
            }
        }

        /// <summary>
        /// Gets the current operational status of the <see cref="MiniTimer"/>.
        /// </summary>
        public TimerStatus Status
        {
            get
            {
                lock (syncLock) return status;
            }
        }

        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        public event Action<MiniTimer>? Elapsed;

        /// <summary>
        /// Starts the <see cref="MiniTimer"/> by setting <see cref="Enabled"/> to <see langword="true"/>.
        /// </summary>
        public void Start() => Enabled = true;

        /// <summary>
        /// Stops the <see cref="MiniTimer"/> by setting <see cref="Enabled"/> to <see langword="false"/>.
        /// </summary>
        public void Stop() => Enabled = false;

        /// <summary>
        /// Waits for the thread to end.
        /// </summary>
        public void Wait() => thread?.Join();

        /// <summary>
        /// Stops and waits for the <see cref="MiniTimer"/>.
        /// </summary>
        public void StopAndWait() { Stop(); Wait(); }

        private void StartTimer()
        {
            if (thread == null || !thread.IsAlive)
            {
                thread = new Thread(TimerThread);
                thread.Start();
            }
        }

        private void StopTimer()
        {
            if (thread != null && thread.IsAlive)
            {
                are.Set();
            }
        }

        private void TimerThread()
        {
            try
            {
                while (enabled)
                {
                    stopwatch.Restart();
                    status = TimerStatus.Executing;
                    Elapsed?.Invoke(this);
                    status = TimerStatus.Waiting;
                    are.WaitOne(Math.Max(Interval - (int)stopwatch.ElapsedMilliseconds, 0));
                }
            }
            finally
            {
                status = TimerStatus.Stopped;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopAndWait();
                    are.Close();
                }
                disposed = true;
            }
        }

        ~MiniTimer() => Dispose(false);
    }
}
