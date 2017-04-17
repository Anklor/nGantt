using System;
using System.Collections.Generic;

namespace nGantt.PeriodSplitter
{
    public abstract class PeriodSplitter
    {
        private List<Period> _result = new List<Period>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public PeriodSplitter(DateTime min, DateTime max)
        {
            MinDate = min;
            MaxDate = max;
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime MinDate { get; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime MaxDate { get; }

        public abstract List<Period> Split();

        protected abstract DateTime Increase(DateTime date, int value);

        protected List<Period> Split(DateTime offsetDate)
        {
            Period firstPeriod = new Period() { Start = MinDate, End = Increase(offsetDate, 1) };
            _result.Add(firstPeriod);

            if (firstPeriod.End >= MaxDate)
            {
                firstPeriod.End = MaxDate;
                return _result;
            }

            int i = 1;
            while (Increase(offsetDate, i) < MaxDate)
            {
                Period period = new Period { Start = Increase(offsetDate, i), End = Increase(offsetDate, i + 1) };
                if (period.End >= MaxDate)
                    period.End = MaxDate;

                _result.Add(period);
                i++;
            }

            return _result;
        }
    }
}
