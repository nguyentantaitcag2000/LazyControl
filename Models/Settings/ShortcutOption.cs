using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LazyControl.Models.Settings
{
    public class ShortcutOption
    {
        public string Label { get; set; }
        public Keys Value { get; set; }

        public override string ToString() => Label;
    }
}
