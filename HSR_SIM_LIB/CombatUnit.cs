using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public class CombatUnit :Unit
    {
       public CombatUnit(): base() { 

       }
        public CombatUnit(Unit unit) : base()
        {
            this.Portrait = unit.Portrait;
            this.Name = unit.Name;
            this.Stats= unit.Stats;
            this.Stats.MaxHp = this.Stats.BaseMaxHp;
            this.Stats.CurrentHp = this.Stats.MaxHp;
        }

    }
}
