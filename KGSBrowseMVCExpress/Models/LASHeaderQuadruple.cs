using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Well.Models
{
    
    // A LAS Log header is composed of lines each with at most four items of non-formatting information. 
    public class LASHeaderQuadruple : ILASHeaderQuadruple
    {
        public string Mnemonic { get; set; }
        public string Unit { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }

        // LAS file specific parsing algorithm
        public LASHeaderQuadruple(string lasHeaderQuadrupleLine)
        {
            var dotSplit = lasHeaderQuadrupleLine.Split(new char[] { '.' }, 2);
            var colonSplit = dotSplit[1].Split(new char[] { ':' }, 2);
            var spaceSplit = colonSplit[0].Split(new char[] { ' ' }, 2);
            var firstField = dotSplit[0].Trim();
            var secondField = spaceSplit[0].Trim();
            var thirdField = String.Empty;
            var fourthField = String.Empty;
            if (spaceSplit.Length > 1) thirdField = spaceSplit[1].Trim();
            if (colonSplit.Length > 1) fourthField = colonSplit[1].Trim();

            Mnemonic = firstField;
            Unit = secondField;
            Value = thirdField;
            Name = fourthField;
        }
    }

}