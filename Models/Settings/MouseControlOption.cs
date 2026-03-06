using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyControl.Models.Settings
{
    public class MouseControlOption
    {
        public string Label { get; set; }
        public MouseControlMode Value { get; set; }

        public override string ToString() => Label;
    }
}