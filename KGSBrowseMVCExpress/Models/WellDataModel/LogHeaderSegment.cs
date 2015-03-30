using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Well.Models.WellDataModel
{
    public class LogHeaderSegment
    {
        // Log Headers are named and are composed of segments composed of four items
        private readonly List<LogHeaderQuadruple> _data;

        public string Name { get; set; }

        public List<LogHeaderQuadruple> Data
        {
            get { return _data; }
        }

        public string OtherInformation { get; set; }

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
}