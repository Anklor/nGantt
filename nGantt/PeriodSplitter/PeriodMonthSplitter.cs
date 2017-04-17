using System;
using System.Collections.Generic;

namespace nGantt.PeriodSplitter
{
    public class PeriodMonthSplitter : PeriodSplitter
    {
        public PeriodMonthSplitter(DateTime min, DateTime max)
            : base(min, max)
        { }

        public override List<Period> Split() => Split(new DateTime(MinDate.Year, MinDate.Month, 1));
        
        protected override DateTime Increase(DateTime date, int value) => date.AddMonths(value);        
    }
}
