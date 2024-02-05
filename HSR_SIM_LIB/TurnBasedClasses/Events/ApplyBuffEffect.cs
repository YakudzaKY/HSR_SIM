using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    /// <summary>
    /// Add effect to existing buff
    /// </summary>
    public class ApplyBuffEffect : Event
    {
        public Effect Eff;
        public Buff BuffToApply { get; set; }
        public ApplyBuffEffect(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            var elmName = (Eff is EffWeaknessImpair wi) ? wi.Element.ToString() : "";
            return $"Apply effect {Eff.GetType().Name} {elmName}{Eff.Value} on {TargetUnit.Name} buff";
        }



        public override void ProcEvent(bool revert)
        {

            Buff currentBuff = TargetUnit.Buffs.FirstOrDefault(x => x.Reference == BuffToApply);
            if (currentBuff != null)
                if (!revert)
                {
                    currentBuff.Effects.Add(Eff);
                }
                else
                {
                    currentBuff.Effects.Remove(Eff);
                }


            base.ProcEvent(revert);
        }
    }

}
