using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
