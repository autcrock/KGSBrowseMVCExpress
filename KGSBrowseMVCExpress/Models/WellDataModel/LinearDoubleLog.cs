using System;
using System.Linq;

namespace Well.Models.WellDataModel
{
    // For arranging Each well log with its own meta-data
    // Initially for easy conversion to JSON input to plotting libraries
    public class LinearDoubleLog : LogHeader
    {
        public long SampleCount { get; set; }

        public LogDoubleDatum[] DatumDepthPairs { get; set; }

        public LinearDoubleLog(string mnemonic, Int64 dataCount)
        {
            Mnemonic = mnemonic;
            DatumDepthPairs = new LogDoubleDatum[dataCount];
            for (var i = 0; i < dataCount; i++)
            {
                DatumDepthPairs[i] = new LogDoubleDatum();
            }
            SampleCount = DatumDepthPairs.LongCount();
        }
        public LinearDoubleLog(string mnemonic, LogDoubleDatum[] datumDepthPairs)
        {
            Mnemonic = mnemonic;
            DatumDepthPairs = datumDepthPairs;
            SampleCount = DatumDepthPairs.LongCount();
        }
        public Int64 GetSampleCount()
        {
            return SampleCount;
        }
        public void SetSampleCount(Int64 sampleCount)
        {
            SampleCount = sampleCount;
        }

        public void SetDatumDepthPair(Int64 i, double depth, double measurement)
        {
            DatumDepthPairs[i].Depth = depth;
            DatumDepthPairs[i].Datum = measurement;
        }
        public LogDoubleDatum GetDatumDepthPair(Int64 i)
        {
            return new LogDoubleDatum( DatumDepthPairs[i].Depth, DatumDepthPairs[i].Datum);
        }

        public void SetMnemonic(string mnemonic)
        {
            Mnemonic = mnemonic;
        }

        public string GetMnemonic()
        {
            return Mnemonic;
        }
    }
}