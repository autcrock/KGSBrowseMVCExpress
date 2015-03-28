using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// Load and parse a LAS file, which only contains numerical well logs.
// Go the quick and dirty suck it all into memory and string split approach.
// No error handling.
// Untested for LAS 3.x files or for contractor specific non-standard variations.

namespace Well.Models
{
    public class LAS : ILASHeader
    {
        public List<LASHeaderSegment> Segments { get; set; }
        private Well InputWell { get; set; }

        public LAS(string filename)
        {
            var headerSegments = new List<LASHeaderSegment>();
            var resultHeader = new LASHeader();
            var stringData = new List<LogStringDatum>();
            var resultData = new Logs();
            var logCount = 0;

            using (var fs = File.OpenRead(filename))
            {
                using (var sr = new StreamReader(fs))
                {

                    // The segments of a LAS file are separated by a tilde.
                    // Each type of segment has a label.
                    var data = sr.ReadToEnd();
                    var segments = data.Split('~');

                    foreach (var segment in segments)
                    {
                        if (segment == null || segment.Length == 0) continue;
                        // Preserve the header meta data as strings as the most likely way it will be useful.
                        switch (segment[0])
                        {
                            case 'A':
                                // The ASCII log data.
//                                if (logCount > 0)
                                // Silently bypass if the compulsory log data segment is out of order.
//                                {
                                    resultData = new Logs(logCount, segment);
//                                }
                                break;

                            case 'O':
                                // The Other segment - non-delimited text format - stored as a string.
                                var otherSegment = new LASHeaderSegment(segment, true);
                                break;
                            case 'C':
                                // The Curve names, units, API code, description.
                                // Delimited by '.' and ':' and parsed as one LogDataQuadruple per line
                                var curveSegment = new LASHeaderSegment(segment, false);
                                logCount = curveSegment.NumberOfLASData();
                                headerSegments.Add(curveSegment);
                                break;
                            default:
                                // The Version, Parameter and Well information blocks.
                                // Delimited by '.' and ':' and parsed as one LogDataQuadruple per line
                                headerSegments.Add(new LASHeaderSegment(segment, false));
                                break;
                        }
                    }
                    resultHeader.Segments = headerSegments;
                }
            }

            InputWell = new Well(resultHeader.Segments, resultData);

            InputWell.Name = resultHeader.Name;
            InputWell.Quadruples = resultHeader.Quadruples;
            InputWell.OtherInformation = resultHeader.OtherInformation;
        }

        public Well GetWell()
        {
            return InputWell;
        }
    }
}