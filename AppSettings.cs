using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyControl
{
    public class AppSettings
    {
        public int EscF1 { get; set; } = 1;
        public int EscF2 { get; set; } = 2;
        public Keys ToggleMouseMode { get; set; } = Keys.Control | Keys.J;
    }
}
