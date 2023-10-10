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
        public ModType Type { get; init; }
        public ModTarget Target { get; init; }
        public ModifierType Modifier { get; init; }
        public int? Value { get; init; } 
        public int? Duration { get; init; } 
        public bool? Dispellable { get; init; }

        public enum ModTarget
        {
            Party
        }
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
                ,this.Target.ToString()
                ,this.Modifier.ToString()
                ,this.Value.ToString()
                ,this.Duration.ToString()
                ,this.Dispellable.ToString()
                );
        }
    }
}
