using System;
using System.Diagnostics;
using System.Threading;

namespace Asjc.MiniTimer
{
    public class MiniTimer
    {
        private bool enabled;
        private Thread thread;
        private readonly AutoResetEvent are = new AutoResetEvent(false);
        private readonly Stopwatch stopwatch = new Stopwatch();

        public event Action<MiniTimer> Elapsed;

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
