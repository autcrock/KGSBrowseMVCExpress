using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Well.Models
{
    // A LAS Log header is composed of lines each with at most four items of non-formatting information. 
    public interface ILASHeaderQuadruple : IMnemonic
    {
    }
}