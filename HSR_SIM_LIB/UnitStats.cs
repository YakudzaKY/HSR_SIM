using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{/// <summary>
/// Class for unit stats
/// </summary>
    public class UnitStats
    {
        int baseMaxHp = 0;
        int maxHp = 0;
        int baseAttack = 0;
        int currentHp = 0;
        public int BaseMaxHp { get => baseMaxHp; set => baseMaxHp = value; }
        public int BaseAttack { get => baseAttack; set => baseAttack = value; }
        public int CurrentHp { get => currentHp; set => currentHp = value; }
        public int MaxHp { get => maxHp; set => maxHp = value; }

        public UnitStats() { }
    }
}
