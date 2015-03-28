using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Well.Models.WellDataModel
{
    // A well log internal representation holding metadata in the header member
    // and the log data in the data member.
    public class Well
    {
        public WellHeaderSegments Header { get; set; }

        public LogData Data { get; set; }

        public string JsonHolder { get; set; }

        public Well()
        {
            Header = new WellHeaderSegments();
            Data = new LogData();
            JsonHolder = "";
        }
        public Well(WellHeaderSegments header, LogData data)
        {
            Header = header;
            Data = data;
            JsonHolder = "";
        }
        /// <summary>
        /// Load and parse a LAS file, which only contains numerical well logs.
        /// Go the quick and dirty suck it all into memory and string split approach.
        /// No error handling.
        /// Untested for LAS 3.x files or for contractor specific non-standard variations.
        /// </summary>
        /// <param name="sr">The streamreader</param>
        public Well(StreamReader sr)
        {
            var headerSegments = new List<LogHeaderSegment>();
            var resultHeader = new WellHeaderSegments();
            var resultData = new LogData();
            var logCount = 0;

            // The segments of a LAS file are separated by a tilde.
            // Each type of segment has a label.
            // Start at 1, not 0 index as the first list segment is empty
            var segments = sr.ReadToEnd().Split('~').ToList().Skip(1);

            foreach (var segment in segments)
            {
                // Preserve the header meta data as strings as the most likely way it will be useful.
                switch (segment[0])
                {
                    case 'A':
                        // The ASCII log data.
                        // Silently bypass if the compulsory log data segment is out of order.
                        if (logCount > 0)
                        {
                            resultData = new LogData(logCount, segment);
                        }
                        break;

                    case 'O':
                        // The Other segment - non-delimited text format - stored as a string.
                        // var logOHeaderSegment = new LogHeaderSegment(segment, true);
                        break;

                    case 'C':
                        // The Curve names, units, API code, description.
                        // Delimited by '.' and ':' and parsed as one LogDataQuadruple per line
                        var newCHeaderSegment = new LogHeaderSegment(segment, false);
                        headerSegments.Add(newCHeaderSegment);
                        logCount = newCHeaderSegment.Data.Count;
                        break;

                    default:
                        // The Version, Parameter and Well information blocks.
                        // Delimited by '.' and ':' and parsed as one LogDataQuadruple per line
                        var newDefaultHeaderSegment = new LogHeaderSegment(segment, false);
                        headerSegments.Add(newDefaultHeaderSegment);
                        break;
                }
            }
            resultHeader.Segments = headerSegments;
            Header = resultHeader;
            Data = resultData;
            JsonHolder = "";
        }

        public string WellToJson(int maxlogs, int thin)
        {
            string jSonString = "{" + Environment.NewLine;
            int curveInfoIndex = 0;
            while (Header.Segments[curveInfoIndex].Name[0] != 'C') curveInfoIndex++;

            // Thin the data out, ensuring the thinning inputs are sensible
            if (Data.SampleCount < thin) thin = Data.SampleCount;
            if (maxlogs > Data.LogCount) maxlogs = Data.LogCount;
            int maxsamples = Data.SampleCount - Data.SampleCount % thin;
            for (int i = 0; i < maxlogs; i++)
            {
                jSonString += "'" + Header.Segments[curveInfoIndex].Data[i].Mnemonic + "': [";
                for (int j = 0; j < maxsamples; j += thin)
                {
                    if (j == maxsamples - thin)
                    {
                        jSonString += Data.DoubleData[i][j];
                    }
                    else
                    {
                        jSonString += Data.DoubleData[i][j] + ", ";
                    }
                }
                if (i == maxlogs - 1)
                    jSonString += "]" + Environment.NewLine + Environment.NewLine;
                else
                    jSonString += "]," + Environment.NewLine + Environment.NewLine;
            }
            jSonString += "}" + Environment.NewLine;
            return jSonString;
        }

        // Do this rather than a serialisation library so we can choose to thin data for the display
        public LinearDoubleWell WellToLinearDoubleWell()
        {
            const Int64 depthInfoIndex = 0;
            var depths = Data.DoubleData[depthInfoIndex];


            var curveInfoIndex = 0;

            // Find Curves segment.
            while (Header.Segments[curveInfoIndex].Name[0] != 'C') curveInfoIndex++;


            // Get the Depth Mnemonic string (varies from contractor to contractor)
            // var depthString = Header.Segments[curveInfoIndex].Data[depthIndex].Mnemonic;
            var curves = Header.Segments[curveInfoIndex];
            var logs = new LinearDoubleLog[Data.LogCount];
            var i = 0;
            foreach (var log in Data.DoubleData)
            {
                logs[i] = new LinearDoubleLog(curves.Data[i].Mnemonic, Data.SampleCount);

                //logs[i].SetMnemonic(curves.Data[i].Mnemonic);
                //logs[i].SetSampleCount(Data.SampleCount);

                var j = 0;
                foreach (var datum in log)
                {
                    logs[i].SetDatumDepthPair(j, depths[j], datum);
                    j++;
                }
                i++;
            }
            
            return new LinearDoubleWell(logs);

        }
        // Do this rather than a serialisation library so we can choose to thin data for the display
        public string LinearWellToLinearJsonWell(LinearDoubleWell well)
        {
            
            return "";
        }

        public string GetDepths(int thin)
        {
            var curveInfoIndex = 0;
            while (curveInfoIndex < Data.LogCount && (Header.Segments[curveInfoIndex].Name[0] != 'C'))
            {
                curveInfoIndex++;
            }

            const int depthIndex = 0;
            // The depth log should always be the first column in a LAS file ASCII data section.

            // Thin the data out, ensuring the thinning inputs are sensible
            if (Data.SampleCount < thin) thin = Data.SampleCount;
            int maxsamples = Data.SampleCount - Data.SampleCount % thin;

            var depthString = "'" + Header.Segments[curveInfoIndex].Data[depthIndex].Mnemonic + "': [";
            for (var j = 0; j < maxsamples; j += thin)
            {
                if (j == maxsamples - thin)
                {
                    depthString += Data.DoubleData[depthIndex][j];
                }
                else
                {
                    depthString += Data.DoubleData[depthIndex][j] + ", ";
                }
            }
            depthString += "]" + Environment.NewLine;
            return depthString;
        }
    }

    // A LAS Log has a header describing metadata and data layout and meaning.
    public class WellHeaderSegments
    {
        private List<LogHeaderSegment> _segments;

        public List<LogHeaderSegment> Segments
        {
            get { return _segments; }
            set { _segments = value; }
        }

        public WellHeaderSegments()
        {
            Segments = new List<LogHeaderSegment>();
        }
        public WellHeaderSegments(List<LogHeaderSegment> insegments)
        {
            Segments = insegments;
        }
    }

    // A LAS Log has a data section containing well log data in arbitrary quantity,
    // set out as described in the log header.  Each log can be string or doubles represented as strings.
    // This implementation assumes no string logs for the time being.
    public class LogData
    {
        private int _logCount;
        private int _sampleCount;
        private double[][] _doubleData;

        public int LogCount
        {
            get { return _logCount; }
            set { _logCount = value; }
        }

        public int SampleCount
        {
            get { return _sampleCount; }
            set { _sampleCount = value; }
        }

        public double[][] DoubleData
        {
            get { return _doubleData; }
            set { _doubleData = value; }
        }

        public LogStringDatum[][] StringData { get; set; }

        public LogData()
        {
            LogCount = 0;
            SampleCount = 0;
            DoubleData = null;
            StringData = null;
        }
        public LogData(int lC, int sC, double[][] dd, LogStringDatum[][] sd)
        {
            LogCount = lC;
            SampleCount = sC;
            DoubleData = dd;
            StringData = sd;
        }
        public LogData(int lC, string inString)
        {
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
            LogCount = lC;
            var wordCount = words.Length;
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
    }

    // A LAS Log header is composed of lines each with at most four items of non-formatting information. 
    public class LogHeaderQuadruple
    {
        private string _mnemonic;
        private string _unit;
        private string _value;
        private string _name;

        public string Mnemonic
        {
            get { return _mnemonic; }
            set { _mnemonic = value; }
        }

        public string Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }

        public string Value
        {
            set { _value = value; }
            get { return _value; }
        }

        public string Name
        {
            set { _name = value; }
            get { return _name; }
        }

        public LogHeaderQuadruple(string incoming)
        {
            var dotSplit = incoming.Split(new char[] { '.' }, 2);
            var colonSplit = dotSplit[1].Split(new char[] { ':' }, 2);
            var spaceSplit = colonSplit[0].Split(new char[] { ' ' }, 2);
            var firstField = dotSplit[0].Trim();
            var secondField = spaceSplit[0].Trim();
            var thirdField = String.Empty;
            var fourthField = String.Empty;
            if (spaceSplit.Length > 1) thirdField = spaceSplit[1].Trim();
            if (colonSplit.Length > 1) fourthField = colonSplit[1].Trim();

            Mnemonic = firstField;
            Unit = secondField;
            Value = thirdField;
            Name = fourthField;
        }
    }

    // Log Headers are named and are composed of segments composed of four items
    public class LogHeaderSegment
    {
        private List<LogHeaderQuadruple> _data;
        private string _name;
        private string _otherInformation;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<LogHeaderQuadruple> Data
        {
            get { return _data; }
        }

        public string OtherInformation
        {
            get { return _otherInformation; }
            set { _otherInformation = value; }
        }

        public LogHeaderSegment()
        {
            Name = String.Empty;
            _data = new List<LogHeaderQuadruple>();
            OtherInformation = String.Empty;
        }

        public LogHeaderSegment(string inString, Boolean other)
        {
            Name = String.Empty;
            _data = new List<LogHeaderQuadruple>();
            OtherInformation = String.Empty;

            if (String.IsNullOrEmpty(inString))
            {
                return;
            }

            if (other)
            {
                OtherInformation = inString;
                return;
            }

            string[] lines = Regex.Split(inString, "\r\n|\r|\n");
            Name = lines[0];
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                if (!string.IsNullOrEmpty(line) && (line[0] != '#'))
                {
                    Data.Add(new LogHeaderQuadruple(line));
                }
            }
        }
    }

    // A LogDoubleDatum attaches a depth to a double numerical value from a well log.
    // For example, it could be a resistivity value.
    public class LogDoubleDatum
    {
        private double _depth;
        private double _datum;

        public double Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        public double Datum
        {
            get { return _datum; }
            set { _datum = value; }
        }

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

    // A LogStringDatum is a string attached to a depth.
    // Could be a rock chip logging descriptor for example.
    public class LogStringDatum
    {
        private double _depth;
        private string _datum;

        public double Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        public string Datum
        {
            get { return _datum; }
            set { _datum = value; }
        }

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

    public class LogHeader
    {
        private string _mnemonic;

        public string Mnemonic
        {
            get { return _mnemonic; }
            set { _mnemonic = value; }
        }

        public LogHeader()
        {
        }

        public LogHeader(string mnemonic)
        {
            Mnemonic = mnemonic;
        }
    }

    // For arranging Each well log with its own meta-data
    // Initially for easy conversion to JSON input to plotting libraries
    public class LinearDoubleLog : LogHeader
    {
        private LogDoubleDatum[] _datumDepthPairs;
        private long _sampleCount;

        public long SampleCount
        {
            get { return _sampleCount; }
            set { _sampleCount = value; }
        }

        public LogDoubleDatum[] DatumDepthPairs
        {
            get { return _datumDepthPairs; }
            set { _datumDepthPairs = value; }
        }
        public LinearDoubleLog(string mnemonic, Int64 dataCount)
        {
            Mnemonic = mnemonic;
            _datumDepthPairs = new LogDoubleDatum[dataCount];
            for (var i = 0; i < dataCount; i++)
            {
                _datumDepthPairs[i] = new LogDoubleDatum();
            }
            SampleCount = _datumDepthPairs.LongCount();
        }
        public LinearDoubleLog(string mnemonic, LogDoubleDatum[] datumDepthPairs)
        {
            Mnemonic = mnemonic;
            _datumDepthPairs = datumDepthPairs;
            SampleCount = _datumDepthPairs.LongCount();
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
            _datumDepthPairs[i].Depth = depth;
            _datumDepthPairs[i].Datum = measurement;
        }
        public LogDoubleDatum GetDatumDepthPair(Int64 i)
        {
            return new LogDoubleDatum( _datumDepthPairs[i].Depth, _datumDepthPairs[i].Datum);
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

    public class LinearDoubleWell
    {
        private LinearDoubleLog[] _linearStringLogs;
        private long _logCount;

        private LinearDoubleLog[] LinearStringLogs
        {
            get { return _linearStringLogs; }
            set { _linearStringLogs = value; }
        }

        private Int64 LogCount
        {
            get { return _logCount; }
            set { _logCount = value; }
        }

        public LinearDoubleWell(LinearDoubleLog[] linearStringLogs)
        {
            LinearStringLogs = linearStringLogs;
            LogCount = LinearStringLogs.LongCount();
        }

    }
}