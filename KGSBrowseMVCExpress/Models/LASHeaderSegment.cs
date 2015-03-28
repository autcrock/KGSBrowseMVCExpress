using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Well.Models
{

    // LAS Headers are named and are composed of segments composed of four items
    public class LASHeaderSegment : ILASHeaderSegment
    {
        public List<LASHeaderQuadruple> Data { get; set; }
        public string Name { get; set; }
        public string OtherInformation { get; set; }

        public LASHeaderSegment()
        {
            Name = String.Empty;
            Data = new List<LASHeaderQuadruple>();
            OtherInformation = String.Empty;
        }

        public LASHeaderSegment(string inString, Boolean other)
        {
            Name = String.Empty;
            Data = new List<LASHeaderQuadruple>();
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
                    Data.Add(new LASHeaderQuadruple(line));
                }
            }
        }

        public int NumberOfLASData()
        {
            return Data.Count;
        }
    }
}