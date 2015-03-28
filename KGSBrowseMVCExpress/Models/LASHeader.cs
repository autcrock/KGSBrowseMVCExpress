using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Well.Models
{
    // A LAS file has a header describing metadata and data layout and meaning.
    public class LASHeader : ILASHeader
    {
        public List<LASHeaderSegment> Segments { get; set; }
        public string Name { get; set; }
        public List<LASHeaderQuadruple> Quadruples { get; set; }
        public string OtherInformation { get; set; }

        public LASHeader()
        {
            Segments = new List<LASHeaderSegment>();
        }

        public LASHeader(List<LASHeaderSegment> insegments)
        {
            Segments = insegments;
        }
    }
}