using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public class Unit
    {
        
        string name = string.Empty;
        Bitmap portrait = null;
        UnitStats stats = null;
        public Unit() {
            stats=new UnitStats();
        }


        public string Name { get => name; set => name = value; }
        public UnitStats Stats { get => stats; set => stats = value; }

        public Bitmap Portrait { get => portrait; set => portrait = value; }
    }
}
