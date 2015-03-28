using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Well.Models
{
    
    public interface ILASHeader
    {
        List<LASHeaderSegment> Segments { get; set; }
    }

}