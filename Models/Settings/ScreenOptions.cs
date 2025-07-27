using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LazyControl.Models.Settings
{
    public class ScreenOptions
    {
        public int Value { get; set; }
        public string Label { get; set; }

        public ScreenOptions(int value)
        {
            this.Label = "Screen " + value;
            this.Value = value;
        }
    }
}
