using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Effect;

namespace HSR_SIM_LIB.Fighters.NPC
{
    public class AutomatonBeetle : DefaultNPCFighter
    {
        private Buff barierBuff = null;
        public double? CalcMyAttack(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(ent) * 3, ent);
        }

        public void MyBarrierEventHandler(Event ent)
        {
            if (ent is DirectDamage && ent.TargetUnit == Parent&& ent.TargetUnit.Buffs.Any(x=>x.Effects.Any(y=>y is EffBarrier)))
            {
                RemoveBuff newEvent = new(ent.Parent, this, Parent)
                {
                    TargetUnit = Parent,
                    BuffToApply = barierBuff
                };
                ent.Parent.Events.Add(newEvent);
            }

        }

        public AutomatonBeetle(Unit parent) : base(parent)
        {
            barierBuff = new Buff(Parent, null) { EventHandlerProc = MyBarrierEventHandler, Effects = new List<Effect>() { new EffBarrier() } };
            //Elemenet
            Element = Unit.ElementEnm.Physical;

            Weaknesses.Add(Unit.ElementEnm.Wind);
            Weaknesses.Add(Unit.ElementEnm.Lightning);
            Weaknesses.Add(Unit.ElementEnm.Imaginary);
            Resists.Add(new Resist() { ResistType = Unit.ElementEnm.Lightning, ResistVal = 0.20 });
            Resists.Add(new Resist() { ResistType = Unit.ElementEnm.Physical, ResistVal = 0.20 });
            Resists.Add(new Resist() { ResistType = Unit.ElementEnm.Ice, ResistVal = 0.20 });
            Resists.Add(new Resist() { ResistType = Unit.ElementEnm.Quantum, ResistVal = 0.20 });

            Ability myAttackAbility;
            //Deals Physical DMG (300% ATK) to a single target, and grants a Barrier to self. The Barrier nullifies all DMG received except for DoT until after being attacked.
            myAttackAbility = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Basic
                ,
                Name = "Unstable Forcefield"
                ,
                Element = Element
                ,
                AdjacentTargets = Ability.AdjacentTargetsEnm.None
                ,
                Attack = true
                ,
                SpGain = 1
            };
            //dmg events
            myAttackAbility.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalcMyAttack, AbilityValue = myAttackAbility });
            myAttackAbility.Events.Add(new EnergyGain(null, this, this.Parent) { Val = 15, AbilityValue = myAttackAbility });

            myAttackAbility.Events.Add(new ApplyBuff(null, this, this.Parent) { TargetUnit = Parent, AbilityValue = myAttackAbility, BuffToApply = barierBuff });
            Abilities.Add(myAttackAbility);


        }
    }
}
