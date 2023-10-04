using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    internal class Unit
    {
        int maxHp = 0;
        string name = string.Empty;
        public Unit() { }

        public int MaxHp { get => maxHp; set => maxHp = value; }
        public string Name { get => name; set => name = value; }
    }
}
