using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Event;

namespace HSR_SIM_LIB
{
    public class Mod :CheckEssence
    {
        public delegate double? CalculateValuePrc(Event ent);
        public delegate IEnumerable<Unit> CalculateTargetPrc(Event ent);

        public CalculateValuePrc CalculateValue { get; init; }
        public CalculateTargetPrc CalculateTargets { get; init; }

        public ModType Type { get; init; }
        public Unit TargetUnit { get; set; }
        public ModifierType Modifier { get; init; }
        public double? Value { get; set; } 
        public int? Duration { get; init; } 
        public bool? Dispellable { get; init; }


        public enum ModType
        {
            Buff,
            Debuff
        }

        public enum ModifierType
        {
            AtkPrc
        }

        public string GetDescription()
        {



            return String.Format(">> {0:s} {1:s} for {2:s} val= {3:D} duration={4:D} dispellable={5:s}", 
                this.Type.ToString()
                ,this.TargetUnit.Name
                ,this.Modifier.ToString()
                ,this.Value.ToString()
                ,this.Duration.ToString()
                ,this.Dispellable.ToString()
                );
        }
    }
}
