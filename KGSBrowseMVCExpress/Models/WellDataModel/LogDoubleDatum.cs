namespace Well.Models.WellDataModel
{
    public class LogDoubleDatum
    {
        // A LogDoubleDatum attaches a depth to a double numerical value from a well log.
        // For example, it could be a resistivity value.
        public double Depth { get; set; }

        public double Datum { get; set; }

        public LogDoubleDatum()
        {
            Depth = 0;
            Datum = 0;
        }
        public LogDoubleDatum(double inDepth, double inDatum)
        {
            Depth = inDepth;
            Datum = inDatum;
        }
    }
}