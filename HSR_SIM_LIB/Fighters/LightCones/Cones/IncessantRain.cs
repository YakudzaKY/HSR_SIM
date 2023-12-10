using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones
{
    internal class IncessantRain: DefaultLightCone
    {
        private readonly double[]  lcMods =
            {  0.12,  0.14,  0.16 ,  0.18 ,  0.20  };

        private readonly Buff aetherCodeDebuff;
        public IncessantRain(IFighter parent, int rank) : base(parent, rank)
        {
            if (Path == Parent.Path)
            {
                aetherCodeDebuff = new Buff(Parent.Parent, null)
                    { Type = Buff.BuffType.Debuff,AbilityValue = null,BaseDuration = 1,Effects = new List<Effect>(){new EffAllDamageVulnerability(){Value =lcMods[Rank-1] }}};


                Parent.PassiveBuffs.Add(new PassiveBuff(Parent.Parent)
                {
                    AppliedBuff = new Buff(Parent.Parent)
                        { Effects = new List<Effect> { new EffCritPrc() { CalculateValue = calcCrit } } },
                    Target = Parent.Parent,
                    IsTargetCheck = true
                });
            }
        }

        //get 0.2 AllDmg per debuff  on target
        private double? calcCrit(Event ent)
        {
            double debuffs = 0;
            debuffs += ent.TargetUnit.Buffs.Count(x => x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
            if (debuffs >= 3)
                return lcMods[Rank - 1];
            else
                return 0;
            
        }


        public override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Nihility;

        public override void DefaultLightCone_HandleEvent(Event ent)
        {
            
            if (ent is ExecuteAbilityFinish && ent.SourceUnit == Parent.Parent&&ent.AbilityValue.Attack&&ent.AbilityValue.AbilityType!=Ability.AbilityTypeEnm.Technique)
            {
                var targetHits = ent.ParentStep.TargetsHit.Where(x=>x.Buffs.All(y => y.Reference != aetherCodeDebuff));
                if (targetHits.Any())
                {
                    ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, Parent.Parent)
                    {
                        TargetUnit = (Unit)Utl.GetRandomObject(targetHits), BaseChance = 1,
                        BuffToApply = aetherCodeDebuff
                    });
                }


            }
            base.DefaultLightCone_HandleEvent(ent);
        }

    }
}
