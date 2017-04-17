using System;
using System.Collections.Generic;

namespace nGantt.PeriodSplitter
{
    public class PeriodHourSplitter : PeriodSplitter
    {
        public PeriodHourSplitter(DateTime min, DateTime max)
            : base(min, max)
        {

        }

        public override List<Period> Split() => Split(new DateTime(MinDate.Year, MinDate.Month, MinDate.Day, MinDate.Hour, 0, 0));
        
        protected override DateTime Increase(DateTime date, int value) => date.AddHours(value);
        
    }
}
