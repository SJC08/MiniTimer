using System.Diagnostics;

namespace Asjc.MiniTimer
{
    /// <summary>
    /// Provides a simple timer for general timing tasks.
    /// </summary>
    /// <remarks>
    /// By default, the timer executes in a new thread that is created each time it is started.
    /// </remarks>
    public class MiniTimer
    {
        private bool enabled;
        private Thread? thread;
        private readonly AutoResetEvent are = new(false);
        private readonly Stopwatch stopwatch = new();

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
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/> and <paramref name="enabled"/> status.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <param name="enabled">The enabled status of the timer.</param>
        public MiniTimer(int interval, bool enabled)
        {
            Interval = interval;
            Enabled = enabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/>, <paramref name="enabled"/> status, and <paramref name="elapsed"/> event.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <param name="enabled">The enabled status of the timer.</param>
        /// <param name="elapsed">The event handler for the Elapsed event.</param>
        public MiniTimer(int interval, bool enabled, Action<MiniTimer> elapsed)
        {
            Interval = interval;
            Enabled = enabled;
            Elapsed += elapsed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/> and <paramref name="elapsed"/> event.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <param name="elapsed">The enabled status of the timer.</param>
        public MiniTimer(int interval, Action<MiniTimer> elapsed) : this(interval)
        {
            Interval = interval;
            Elapsed += elapsed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="elapsed"/> event.
        /// </summary>
        /// <param name="elapsed">The enabled status of the timer.</param>
        public MiniTimer(Action<MiniTimer> elapsed)
        {
            Elapsed += elapsed;
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
            get => enabled;
            set
            {
                enabled = value;
                if (enabled)
                    StartTimer();
                else
                    StopTimer();
            }
        }

        /// <summary>
        /// Gets a <see langword="bool"/> indicating whether the <see cref="MiniTimer"/> is running.
        /// </summary>
        public bool IsRunning => thread != null && thread.IsAlive;

        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        public event Action<MiniTimer>? Elapsed;

        /// <summary>
        /// Starts the <see cref="MiniTimer"/> by setting <see cref="Enabled"/> to <see langword="true"/>.
        /// </summary>
        public void Start() => Enabled = true;

        /// <summary>
        /// Starts the <see cref="MiniTimer"/> by setting <see cref="Enabled"/> to <see langword="false"/>.
        /// </summary>
        public void Stop() => Enabled = false;

        /// <summary>
        /// Waits for the thread to end.
        /// </summary>
        /// <remarks>
        /// This is usually waiting for the <see cref="Elapsed"/> event.
        /// </remarks>
        public void Wait() => thread?.Join();

        /// <summary>
        /// Stops and waits for the <see cref="MiniTimer"/>.
        /// </summary>
        public void StopAndWait()
        {
            Stop();
            Wait();
        }

        private void StartTimer()
        {
            if (!IsRunning)
            {
                thread = new Thread(TimerThread);
                thread.Start();
            }
        }

        private void StopTimer()
        {
            if (IsRunning)
            {
                are.Set();
            }
        }

        private void TimerThread()
        {
            while (enabled)
            {
                stopwatch.Restart();
                Elapsed?.Invoke(this);
                are.WaitOne(Math.Max(Interval - (int)stopwatch.ElapsedMilliseconds, 0));
            }
        }
    }
}
