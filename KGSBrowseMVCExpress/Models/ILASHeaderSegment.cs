using System.Collections.Generic;

namespace Well.Models
{
    public interface ILASHeaderSegment
    {
        string Name { get; set; }
        List<LASHeaderQuadruple> Data { get; set; }
        string OtherInformation { get; set; }
    }
}