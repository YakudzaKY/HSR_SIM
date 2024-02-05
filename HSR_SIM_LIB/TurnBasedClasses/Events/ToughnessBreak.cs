using System;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.ReadyBuffs;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Break the shield
public class ToughnessBreak : DamageEventTemplate
{
    public ToughnessBreak(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return TargetUnit.Name + " shield broken " +
               $" overall={Val:f} to_hp={RealVal:f}";
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
        {
            ModActionValue delayAV = new(this.ParentStep, this.Source, this.SourceUnit)
            {
                TargetUnit = this.TargetUnit,
                Val = -this.TargetUnit.GetActionValue(this) * 0.25
            }; //default delay
            this.ChildEvents.Add(delayAV);
            // https://honkai-star-rail.fandom.com/wiki/Toughness
            switch (this.ParentStep.ActorAbility.Element)
            {
                case ElementEnm.Physical:
                    this.TryDebuff(new BuffBleedWB(this.SourceUnit, this.SourceUnit.Fighter.ShieldBreakDebuff), 1.5);
                    break;
                case ElementEnm.Fire:
                    this.TryDebuff(new BuffBurnWB(this.SourceUnit, this.SourceUnit.Fighter.ShieldBreakDebuff), 1.5);
                    break;
                case ElementEnm.Ice:
                    this.TryDebuff(new BuffFreezeWB(this.SourceUnit, this.SourceUnit.Fighter.ShieldBreakDebuff), 1.5);
                    break;
                case ElementEnm.Lightning:
                    this.TryDebuff(new BuffShockWB(this.SourceUnit, this.SourceUnit.Fighter.ShieldBreakDebuff), 1.5);
                    break;
                case ElementEnm.Wind:
                    this.TryDebuff(
                        new BuffWindShearWB(this.SourceUnit, this.SourceUnit.Fighter.ShieldBreakDebuff)
                        { Stack = this.TargetUnit.Fighter.IsEliteUnit ? 3 : 1 }, 1.5);
                    break;
                case ElementEnm.Quantum:
                    this.TryDebuff(
                        new BuffEntanglementWB(this.SourceUnit, this.SourceUnit.Fighter.ShieldBreakDebuff, this),
                        1.5);

                    break;
                case ElementEnm.Imaginary:
                    this.TryDebuff(
                        new BuffImprisonmentWB(this.SourceUnit, this.SourceUnit.Fighter.ShieldBreakDebuff, this),
                        1.5);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        DamageWorks(revert);
        base.ProcEvent(revert);
    }
}