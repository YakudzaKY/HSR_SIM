using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    internal class Condition
    {
        private List<Check> checks = new List<Check>();
        private string orGroup = "";

        public string OrGroup { get => orGroup; set => orGroup = value; }
        internal List<Check> Checks { get => checks; set => checks = value; }
    }
}
