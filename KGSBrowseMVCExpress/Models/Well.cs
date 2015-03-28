using System;
using System.Collections.Generic;
using System.Linq;

namespace Well.Models
{
    // A well log internal representation holding metadata in the header member
    // and the log data in the data member.
    public class Well : LASHeader
    {
        private Logs DataLogs { get; set; }

        public Well()
        {
            DataLogs = new Logs();
        }

        public Well(List<LASHeaderSegment> segments, Logs dataLogs)
        {
            Segments = segments;
            DataLogs = dataLogs;
        }

        // Do this rather than a serialisation library so we can choose to thin data for the display
        public string WellToJson(int maxlogs, int thin)
        {
            var jsonString = "{" + Environment.NewLine;
            var curveInfo = GetCurveInfo();

            var numberOfSamples = DataLogs.NumberOfSamples();
            var numberOfLogs = DataLogs.NumberOfLogs();

            // Thin the data out, ensuring the thinning inputs are sensible
            if (numberOfSamples < thin) thin = numberOfSamples;
            if (maxlogs > numberOfLogs) maxlogs = numberOfLogs;
            var maxsamples = numberOfSamples - numberOfSamples % thin;
            
            for (var i = 0; i < maxlogs; i++)
            {
                jsonString += "\"" + curveInfo.Data[i].Mnemonic + "\": [";
                
                for (var j = 0; j < maxsamples; j += thin)
                {
                    if (j == maxsamples - thin)
                    {
                        jsonString += DataLogs.GetDoubleDatum(i,j);
                    }
                    else
                    {
                        jsonString += DataLogs.GetDoubleDatum(i, j) + ", ";
                    }
                }
                if (i == maxlogs - 1)
                    jsonString += "]" + Environment.NewLine + Environment.NewLine;
                else
                    jsonString += "]," + Environment.NewLine + Environment.NewLine;
            }
            jsonString += "}" + Environment.NewLine;
            return jsonString;
        }

        public void SetData ( Logs data)
        {
            data = DataLogs;
        }

        public string GetDepthsAsJSON(int thin)
        {

            // The depth log should always be the first column in a valid LAS file ASCII data section.
            const int depthIndex = 0;

            var curveInfo = GetCurveInfo();
            var depthCurveData = curveInfo.Data[depthIndex];

            // Thin the data out, ensuring the thinning inputs are sensible
            var numberOfSamples = DataLogs.NumberOfSamples();
            if (numberOfSamples < thin) thin = numberOfSamples;
            int maxsamples = numberOfSamples - numberOfSamples % thin;

            var depthString = "\"" + depthCurveData.Mnemonic + "\": [";
            for (var j = 0; j < maxsamples; j += thin)
            {
                if (j == maxsamples - thin)
                {
                    depthString += DataLogs.GetDoubleDatum(depthIndex, j);
                }
                else
                {
                    depthString += DataLogs.GetDoubleDatum(depthIndex, j) + ", ";
                }
            }
            depthString += "]" + Environment.NewLine;
            return depthString;
        }

        // Relies on LAS convention that the segment names begin with a unique capital letter,
        // In the case of curve data, the letter capital 'C'.
        public LASHeaderSegment GetCurveInfo()
        {
            return Segments.Where(n => n.Name[0] == 'C').First();
        }

        public LASHeaderQuadruple GetDepths()
        {
            return GetCurveInfo().Data[0];
        }
    }
}