using System;
using System.Linq;

namespace Well.Models.WellDataModel
{
    public class LinearDoubleWell
    {
        private LinearDoubleLog[] LinearStringLogs { get; set; }

        private Int64 LogCount { get; set; }

        public LinearDoubleWell(LinearDoubleLog[] linearStringLogs)
        {
            LinearStringLogs = linearStringLogs;
            LogCount = LinearStringLogs.LongCount();
        }

    }
}