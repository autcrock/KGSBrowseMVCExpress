using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Well.Models.Model
{
    public class Model
    {
        public string ReturnedValue;

        public Model()
        {
            ReturnedValue = "Empty Model";
        }
        public Model(string message)
        {
            ReturnedValue = message;
        }
    }
}