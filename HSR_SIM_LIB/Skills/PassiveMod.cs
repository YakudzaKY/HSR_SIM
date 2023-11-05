using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills
{
    public class PassiveMod
    {
        public Buff Mod { get; set; }
        public CloneClass Target  { get; set; }//in most cases target==parent, but when target is full team then not
        public Unit Parent { get; init; }
        public  bool IsTargetCheck{ get; set; }
        public PassiveMod(Unit parentUnit)
        {
            Parent=parentUnit;
        }
    }
}
