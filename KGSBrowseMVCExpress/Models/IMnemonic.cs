using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Well.Models
{
    public interface IMnemonic
    {
        string Mnemonic { get; set; }
        string Unit { get; set; }
        string Value { get; set; }
        string Name { get; set; }
    }
}