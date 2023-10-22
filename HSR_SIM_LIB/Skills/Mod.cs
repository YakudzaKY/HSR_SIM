using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Effect;

namespace HSR_SIM_LIB.Skills
{
    public class Mod : CloneClass
    {

        public ModType Type { get; init; }

        public string CustomIconName { get; set; }
        public List<Effect> Effects { get; init; } = new List<Effect>();
        //debuf is CC? 
        public bool CrowdControl
        {
            get
            {
                return Effects.Any(x => 
                    x.EffType is EffectType.Entanglement 
                    or EffectType.Frozen 
                    or EffectType.Imprisonment 
                    or EffectType.Dominated 
                    or EffectType.Outrage);
            }
        }

        public int Stack { get; set; } = 1;
      
        public int MaxStack { get; set; } = 1;
        public int? BaseDuration { get; set; }
        public int? DurationLeft { get; set; }

        public string UniqueStr { get; set; }

        public Mod RefMod { get; set; }

        public bool Dispellable { get; init; }
        public Unit UniqueUnit { get; set; }


        public enum ModType
        {
            Buff,
            Debuff,
            Dot
        }




        public Mod(Mod reference=null)
        {
            RefMod = reference;
        }
        public string GetDescription()
        {

            string modsStr = "";
            foreach (var eff in Effects)
            {
                modsStr += $"{eff.EffType.ToString():s} val= {eff.Value:f} ; ";
            }

            return
                $">> {Type.ToString():s} for {modsStr:s} duration={BaseDuration.ToString():D} dispellable={Dispellable.ToString():s}";
        }
    }
}
