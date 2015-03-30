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


    public class LogData
    {
        // A Log has a data section containing well log data in arbitrary quantity,
        // set out as described in the log header.  Each log can be string or doubles represented as strings.
        // This implementation assumes no string logs for the time being.

        public int LogCount { get; set; }

        public int SampleCount { get; set; }

        public double[][] DoubleData { get; set; }

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
            var index = inString.IndexOf(Environment.NewLine);
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
        public string Mnemonic { get; set; }

        public string Unit { get; set; }

        public string Value { set; get; }

        public string Name { set; get; }

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
}