using System;
using System.Timers;

namespace LlamaLibrary.Helpers.Classes
{
    public class PulseTimer
    {
        private DateTime _startTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="PulseTimer"/> class.
        ///     Creates a Pulse Timer.
        /// </summary>
        /// <param name="interval">The TimeSpan interval in which the timer triggers.</param>
        public PulseTimer(TimeSpan interval)
        {
            Timer = new Timer(interval.TotalMilliseconds);
            Timer.Elapsed += _timer_Elapsed;
            Timer.AutoReset = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PulseTimer"/> class.
        ///     Creates a Pulse Timer.
        /// </summary>
        /// <param name="interval">The number of milliseconds in which the timer triggers.</param>
        public PulseTimer(int interval)
        {
            Timer = new Timer(interval);
            Timer.Elapsed += _timer_Elapsed;
            Timer.AutoReset = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PulseTimer"/> class.
        ///     Creates a Pulse Timer with a default interval of 1 second.
        /// </summary>
        protected PulseTimer()
        {
            Timer = new Timer(1000);
            Timer.Elapsed += _timer_Elapsed;
            Timer.AutoReset = false;
        }

        protected Timer Timer { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the timer has elapsed the interval.
        /// </summary>
        public bool Completed { get; protected set; }

        public bool Enabled
        {
            get => Timer.Enabled;
            set => Timer.Enabled = value;
        }

        /// <summary>
        ///     Gets or sets timer interval in milliseconds.
        /// </summary>
        public double Interval
        {
            get => Timer.Interval;
            set => Timer.Interval = value;
        }

        /// <summary>
        ///     Gets or sets timer interval as a TimeSpan.
        /// </summary>
        public TimeSpan IntervalTimeSpan
        {
            get => TimeSpan.FromMilliseconds(Timer.Interval);
            set => Timer.Interval = value.TotalMilliseconds;
        }

        /// <summary>
        ///     Resets the timer to 0.
        /// </summary>
        public virtual void Reset()
        {
            var interval = Timer.Interval;
            Timer.Stop();
            Completed = false;
            Timer.Interval = interval;
        }

        /// <summary>
        ///     Restarts the timer.
        /// </summary>
        public virtual void Restart()
        {
            Reset();
            Start();
        }

        protected void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Completed = true;
            Timer.Stop();
        }

        /// <summary>
        ///     Starts the timer.
        /// </summary>
        public void Start()
        {
            Timer.Start();
            _startTime = DateTime.Now;
        }

        /// <summary>
        ///     Stops the timer.
        /// </summary>
        public void Stop()
        {
            Timer.Stop();
        }

        public DateTime EndTime => _startTime.AddMilliseconds(Interval);

        public TimeSpan RemainingTime => EndTime - DateTime.Now;

        /// <summary>
        ///     Sets the timer interval.
        /// </summary>
        /// <param name="interval">The TimeSpan interval in which the timer triggers.</param>
        public void SetInterval(TimeSpan interval)
        {
            Timer.Interval = interval.TotalMilliseconds;
        }

        /// <summary>
        ///     Creates a Pulse Timer and starts it.
        /// </summary>
        /// <param name="interval">The TimeSpan interval in which the timer triggers.</param>
        /// <returns>New Pulse Timer.</returns>
        public static PulseTimer StartNew(TimeSpan interval)
        {
            var timer = new PulseTimer(interval);

            timer.Start();
            return timer;
        }

        /// <summary>
        ///     Creates a Pulse Timer.
        /// </summary>
        /// <param name="interval">The number of milliseconds in which the timer triggers.</param>
        /// <returns>New Pulse Timer.</returns>
        public static PulseTimer StartNew(int interval)
        {
            var timer = new PulseTimer(interval);
            timer.Start();
            return timer;
        }
    }
}