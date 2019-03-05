using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace KGSBrowseMVC.Models
{
    public class Model
    {
        public string ReturnedValue;

        public Model()
        {
            ReturnedValue = "Empty Model";
        }
        public Model( string message)
        {
            ReturnedValue = message;
        }
    }

    // A well log internal representation holding metadata in the header member
    // and the log data in the data member.
    public class Well
    {
        public LogHeader Header;
        public LogData Data;
        public string JsonHolder { set; get;}

        public Well()
        {
            Header = new LogHeader();
            Data = new LogData();
            JsonHolder = "";
        }
        public Well(LogHeader header, LogData data)
        {
            Header = header;
            Data = data;
            JsonHolder = "";
        }
        // Load and parse a LAS file, which only contains numerical well logs.
        // Go the quick and dirty suck it all into memory and string split approach.
        // No error handling.
        // Untested for LAS 3.x files or for contractor specific non-standard variations.

        public Well(string filename)
        {
//            var headerSegments = new List<LogHeaderSegment>();
//            var logCount = 0;

            using (var fs = File.OpenRead(filename))
            {
                using (var sr = new StreamReader(fs))
                {

                    // The segments of a LAS file are separated by a tilde.
                    // Each type of segment has a label.
                    var data = sr.ReadToEnd();
                    List<String> segments = new List<String> (data.Split('~').Where(seg => !String.IsNullOrEmpty(seg)));

                    var headerSegments = new List<LogHeaderSegment>();
                    headerSegments.Add(new LogHeaderSegment(segments.Where(segment => segment[0] == 'O').Single(), true));
                    headerSegments.Add(new LogHeaderSegment(segments.Where(segment => segment[0] == 'V').Single(), false));
                    headerSegments.Add(new LogHeaderSegment(segments.Where(segment => segment[0] == 'P').Single(), false));
                    headerSegments.Add(new LogHeaderSegment(segments.Where(segment => segment[0] == 'W').Single(), false));

                    var curveHeaderSegment = new LogHeaderSegment(segments.Where(segment => segment[0] == 'C').Single(), false);
                    headerSegments.Add(curveHeaderSegment);

                    Data = new LogData(
                          curveHeaderSegment.Data.Count
                        , segments.Where(segment => segment[0] == 'A').Single()
                        );

                    Header = new LogHeader(headerSegments);

                }
            }
            JsonHolder = "";
        }

        // Do this rather than a serialisation library so we can choose to thin data for the display
        public string WellToJson(int maxlogs, int thin)
        {
            var jsonString = new StringBuilder("{" + Environment.NewLine);
            var curveData = Header.Segments.ToList().Where(s => s.Name[0] == 'C').Single().Data;

            var i = 0;
            foreach (var curve in Data.DoubleData)
            {
                jsonString.Append("\"" + curveData[i].Mnemonic + "\": [");
                var j = 0;
                foreach (var datum in Data.DoubleData[i]) { 
                    if (j == Data.SampleCount)
                    {
                        jsonString.Append(datum);
                    }
                    else
                    {
                        jsonString.Append(datum + ", ");
                    }
                    j++;
                }
                if (i == Data.LogCount)
                    jsonString.Append("]" + Environment.NewLine + Environment.NewLine);
                else
                    jsonString.Append("]," + Environment.NewLine + Environment.NewLine);
                i++;
            }
            jsonString.Append("}" + Environment.NewLine);
            return jsonString.ToString();
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

            if (Data.SampleCount < thin) thin = Data.SampleCount;
            int maxsamples = Data.SampleCount - Data.SampleCount % thin;

            var depthString = new StringBuilder("\"" + Header.Segments[curveInfoIndex].Data[depthIndex].Mnemonic + "\": [");
            for (var j = 0; j < maxsamples; j += thin)
            {
                if (j == maxsamples - thin)
                {
                    depthString.Append(Data.DoubleData[depthIndex][j]);
                }
                else
                {
                    depthString.Append(Data.DoubleData[depthIndex][j] + ", ");
                }
            }
            depthString.Append("]" + Environment.NewLine);
            return depthString.ToString();
        }
    }

    // A LAS Log has a header describing metadata and data layout and meaning.
    public class LogHeader
    {
        public List<LogHeaderSegment> Segments;
        public LogHeader()
        {
            Segments = new List<LogHeaderSegment>();
        }
        public LogHeader(List<LogHeaderSegment> insegments)
        {
            Segments = insegments;
        }

    }

    // A LAS Log has a data section containing well log data in arbitrary quantity,
    // set out as described in the log header.  Each log can be string or doubles represented as strings.
    // This implementation assumes no string logs for the time being.
    public class LogData
    {
        public int LogCount;
        public int SampleCount;
        public double[][] DoubleData;
        public LogStringDatum[][] StringData;

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
            int wordCount = 0;

            if (String.IsNullOrEmpty(inString) )
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
    }

    // A LAS Log header is composed of lines each with at most four items of non-formatting information. 
    public class LogHeaderQuadruple
    {
        public string Mnemonic;
        public string Unit;
        public string Value;
        public string Name;

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
        public string Name;
        public List<LogHeaderQuadruple> Data;
        public string OtherInformation;

        public LogHeaderSegment()
        {
            Name = String.Empty;
            Data = new List<LogHeaderQuadruple>();
            OtherInformation = String.Empty;
        }

        public LogHeaderSegment(string inString, Boolean other)
        {
            Name = String.Empty;
            Data = new List<LogHeaderQuadruple>();
            OtherInformation = String.Empty;


            if ( String.IsNullOrEmpty(inString) )
            {
                throw new Exception("LogHeaderSegment: No Input string.");
            }

            string[] lines = Regex.Split(inString, "\r\n|\r|\n");
            Name = lines[0];

            if (other)
            {
                OtherInformation = inString;
                return;
            }

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

    // A LogStringDatum is a string attached to a depth.
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