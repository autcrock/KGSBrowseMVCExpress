using System.Collections.Generic;

namespace Well.Models.WellDataModel
{
    public class WellHeaderSegments
    {
        public List<LogHeaderSegment> Segments { get; set; }

        public WellHeaderSegments()
        {
            Segments = new List<LogHeaderSegment>();
        }
        public WellHeaderSegments(List<LogHeaderSegment> insegments)
        {
            Segments = insegments;
        }
    }
}