using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Well.Models
{
    // Class Logs encapsulates an arbitrary number of logs as a 2D array.
    // A Log has a data section containing well log data in arbitrary quantity,
    // set out as described in the log header.  Each log can be string or doubles represented as strings.
    // This implementation assumes no string logs for the time being.

    public class Logs
    {
        private int LogCount;
        private int SampleCount;
        private double[][] DoubleData;
        private LogStringDatum[][] StringData;

        public Logs()
        {
            LogCount = 0;
            SampleCount = 0;
            DoubleData = null;
            StringData = null;
        }
        public Logs(int lC, int sC, double[][] dd, LogStringDatum[][] sd)
        {
            LogCount = lC;
            SampleCount = sC;
            DoubleData = dd;
            StringData = sd;
        }
        public Logs(int lC, string inString)
        {
            int wordCount = 0;

            if (String.IsNullOrEmpty(inString))
            {
                LogCount = 0;
                SampleCount = 0;
                DoubleData = null;
                StringData = null;
                return;
            }

            // Remove the first line containing the ~ASCII identifier
            var index = inString.IndexOf(System.Environment.NewLine);
            var inString1 = inString.Substring(index + System.Environment.NewLine.Length).Trim();
            // Split into words and convert to raw log data
            var words = Regex.Split(inString1, @"\s+");
            var lines = Regex.Split(inString1, System.Environment.NewLine.ToString());

            LogCount = lC;
            wordCount = (int)words.Length;
            SampleCount = wordCount / LogCount;
            StringData = null;
            DoubleData = new double[LogCount][];
            for (var logIndex = 0; logIndex < LogCount; logIndex++)
            {
                DoubleData[logIndex] = new double[SampleCount];
                for (var wordIndex = 0; wordIndex < wordCount; wordIndex += LogCount)
                {
                    var sampleIndex = wordIndex / LogCount;
                    var word = words[wordIndex + logIndex];
                    if (!string.IsNullOrEmpty(word))
                    {
                        DoubleData[logIndex][sampleIndex] = Convert.ToDouble(word);
                    }
                }
            }
        }
        public int NumberOfSamples()
        {
            return SampleCount;
        }
        public int NumberOfLogs()
        {
            return LogCount;
        }
        public double[][] GetDoubleData () { return DoubleData; }
        public double GetDoubleDatum (int i, int j) { return DoubleData[i][j]; }
        public LogStringDatum[][] GetStringData() { return StringData; }
    }


    // A LogDoubleDatum attaches a depth to a datum from a well log, as doubles.
    // For example, it could represent a resistivity datum.
    public class LogDoubleDatum
    {
        public double Depth;
        public double Datum;

        public LogDoubleDatum()
        {
            Depth = 0;
            Datum = 0;
        }
        public LogDoubleDatum(string inDepth, string inDatum)
        {
            Depth = Convert.ToDouble(inDepth);
            Datum = Convert.ToDouble(inDatum);
        }
    }

    // A LogStringDatum is a string attached to a depth from a well log.
    // Could be a rock chip logging descriptor for example.
    public class LogStringDatum
    {
        public double Depth;
        public string Datum;

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