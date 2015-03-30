using System;

namespace Well.Models.WellDataModel
{
    // A LogStringDatum is a string attached to a depth.
    // Could be a rock chip logging descriptor for example.
    public class LogStringDatum
    {
        public double Depth { get; set; }

        public string Datum { get; set; }

        public LogStringDatum()
        {
            Depth = 0;
            Datum = null;
        }
        public LogStringDatum(string inDepth, string inDatum)
        {
            Depth = Convert.ToDouble(inDepth);
            Datum = inDatum;
        }
    }
}