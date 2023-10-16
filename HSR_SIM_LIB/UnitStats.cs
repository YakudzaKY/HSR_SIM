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
        public Unit Parent { get; set; }
        public double BaseMaxHp { get; set; } = 0;

        public double BreakDmg
        {
            get
            {
                return BaseBreakDmg;//todo with mods?
            }
        }

        public double BaseBreakDmg { get; set; } = 0;
        public double BaseAttack { get; set; } = 0;
        private double? baseDef;
        public double? BaseDef
        {
            get
            {
                if (baseDef == null)
                    baseDef = 200 + (10 * Parent.Level);
                return baseDef;
            }
            set => baseDef=value;
        }

        public double Def => BaseDef??0;//todo mods?


        public double MaxHp { get; set; } = 0;//TODO baseMaxHP * mod
        public int CurrentEnergy { get; set; } = 0;
        public int BaseMaxEnergy { get; set; } = 0;

        public int MaxToughness { get; set; } = 0;

        private double? baseActionValue ;//TODO при выборе кто ходит,  отсортировать . мб сделать минусовые значения
        public double BaseActionValue
        {
            get => baseActionValue?? 10000 / Speed; 
            set => baseActionValue=value;
        }
        
        public double BaseCritChance { get; set; } = 0;
        public double CritChance
        {
            get => BaseCritChance; //TODO need calc with modifications
        }

        public double ActionValue { get; set; } = 0;
        public double FlatSpeed { get; internal set; }
        public double BaseSpeed { get; internal set; }
        public double SpdPrc { get; internal set; } = 0;
        public double Speed//speed for calcing
        {
            get
            {
               return BaseSpeed*(1+ SpdPrc)+ FlatSpeed;
            }
            internal set { throw new NotImplementedException(); }
        }

        public double BaseCritDmg { get; set; }

        public double CritDmg
        {
            get => BaseCritChance; //TODO need calc with modifications
        }


        public UnitStats(Unit unit) { }

        public void ResetAV()
        {
            ActionValue = BaseActionValue;
        }
    }
}
