using System;
using System.Timers;

namespace LlamaLibrary.Helpers.Classes
{
    public class RandomMinutePulseTimer : PulseTimer
    {
        private readonly int _maxMs;
        private readonly int _minMs;
        private readonly Random _random = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomMinutePulseTimer"/> class.
        ///     Creates a Pulse Timer whose interval is a random number of milliseconds between the specified minimum and maximum (inclusive) and changes each restart
        /// </summary>
        /// <param name="minMinute">The minimum number of minutes to use for the interval.</param>
        /// <param name="maxMinutes">The maximum number of minutes to use for the interval.</param>
        public RandomMinutePulseTimer(int minMinute, int maxMinutes)
        {
            if (maxMinutes < minMinute)
            {
                throw new ArgumentException("maxMinutes must be greater than minMinutes");
            }

            _minMs = minMinute * 60_000;
            _maxMs = (maxMinutes + 1) * 60_000;
            Timer = new Timer(RandomMilliseconds);
            Timer.Elapsed += _timer_Elapsed;
            Timer.AutoReset = false;
        }

        private double RandomMilliseconds => _random.Next(_minMs, _maxMs);

        /// <inheritdoc/>
        public override void Reset()
        {
            Timer.Stop();
            Completed = false;
            Timer.Interval = RandomMilliseconds;
        }

        /// <summary>
        ///     Creates a Pulse Timer whose interval is a random number of milliseconds between the specified minimum and maximum (inclusive) and changes each restart. Starts the timer.
        /// </summary>
        /// <param name="minMinute">The minimum number of minutes to use for the interval.</param>
        /// <param name="maxMinutes">The maximum number of minutes to use for the interval.</param>
        public static RandomMinutePulseTimer StartNew(int minMinute, int maxMinutes)
        {
            var result = new RandomMinutePulseTimer(minMinute, maxMinutes);
            result.Start();
            return result;
        }
    }
}