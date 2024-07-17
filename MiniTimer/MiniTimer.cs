using System.Diagnostics;

namespace Asjc.MiniTimer
{
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
        public MiniTimer(int interval) => Interval = interval;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/> and <paramref name="enabled"/> status.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <param name="enabled">The enabled status of the timer.</param>
        public MiniTimer(int interval, bool enabled) : this(interval) => Enabled = enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/>, <paramref name="enabled"/> status, and <paramref name="elapsed"/> event handler.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <param name="enabled">The enabled status of the timer.</param>
        /// <param name="elapsed">The event handler for the Elapsed event.</param>
        public MiniTimer(int interval, bool enabled, Action<MiniTimer> elapsed) : this(interval, enabled) => Elapsed += elapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="interval"/> and <paramref name="elapsed"/> event handler.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <param name="elapsed">The enabled status of the timer.</param>
        public MiniTimer(int interval, Action<MiniTimer> elapsed) : this(interval) => Elapsed += elapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTimer"/> class with the specified <paramref name="elapsed"/> event handler.
        /// </summary>
        /// <param name="elapsed">The enabled status of the timer.</param>
        public MiniTimer(Action<MiniTimer> elapsed) => Elapsed += elapsed;

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

        public int Interval { get; set; }

        public bool IsRunning => thread != null && thread.IsAlive;

        public event Action<MiniTimer>? Elapsed;

        public void Start() => Enabled = true;

        public void Stop() => Enabled = false;

        public void Wait() => thread?.Join();

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
