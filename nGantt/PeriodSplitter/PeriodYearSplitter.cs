using System;
using System.Collections.Generic;

namespace nGantt.PeriodSplitter
{
    public class PeriodYearSplitter : PeriodSplitter
    {
        public PeriodYearSplitter(DateTime min, DateTime max)
            : base(min, max)
        { }

        public override List<Period> Split() => Split(new DateTime(MinDate.Year, 1, 1));
        
        protected override DateTime Increase(DateTime date, int value) => date.AddYears(value);
        
    }
}
