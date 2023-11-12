using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using ImageMagick;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Luocha : DefaultFighter
    {

        private readonly Dictionary<int, double> DeathWishMods = new()
        {
            { 1, 1.20 }, { 2, 1.28 }, { 3, 1.36}, { 4, 1.44 }, { 5, 1.52 }
            ,{ 6, 1.60 }, {7, 1.70 }, { 8, 1.80 }, { 9, 1.90}, { 10, 2 }
            ,{ 11, 2.08 }, { 12, 2.16 }
        };

        private readonly Dictionary<int, double> PoAFAtkMods = new()
        {
            { 1, 0.40 }, { 2, 0.425 }, { 3, 0.45 }, { 4, 0.475 }, { 5, 0.50 }
            ,{ 6, 0.52 }, {7, 0.54 }, { 8, 0.56 }, { 9, 0.58}, { 10, 0.60 }
            ,{ 11, 0.62 }, { 12, 0.64 }
        };
        private readonly Dictionary<int, double> PoAFFix = new()
        {
            { 1, 200 }, { 2, 320 }, { 3, 410 }, { 4, 500 }, { 5, 560 }, { 6, 620 }, { 7, 665 }, { 8, 710 },
            { 9, 755 }, { 10, 800 }  ,{ 11, 845 }, { 12, 890 }
        };

        private readonly Dictionary<int, double> CoLFAtkMods = new()
        {
            { 1, 0.12 }, { 2, 0.128 }, { 3, 0.135 }, { 4, 0.143 }, { 5, 0.15 }
            ,{ 6, 0.156 }, {7, 0.162 }, { 8, 0.168 }, { 9, 0.174}, { 10, 0.18 }
            ,{ 11, 0.186 }, { 12, 0.192}
        };
        private readonly Dictionary<int, double> CoLFFix = new()
        {
            { 1, 60 }, { 2, 96 }, { 3, 123 }, { 4, 150 }, { 5, 168 }, { 6, 186 }, { 7, 200 }, { 8, 213 },
            { 9, 227 }, { 10, 240 }  ,{ 11, 254 }, { 12, 267 }
        };

        private List<Unit> trackedUnits = new List<Unit>();


        public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Abundance;
        public sealed override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Imaginary;
        private readonly Buff uniqueBuff = null;
        private readonly Ability cycleOfLife;
        private readonly double cycleOfLifeMaxCnt = 2;
        private readonly Ability prayerOfAbyssFlowerAuto;
        private readonly Ability prayerOfAbyssFlower;
        private readonly Ability ultimateAbility;
        private readonly Buff triggerCdBuff = null;

        //If unit hp<=50% for Luocha follow up heals
        private bool UnitAtLowHpForAuto(Unit unit)
        {
            return unit.GetRes(Resource.ResourceType.HP).ResVal / unit.GetMaxHp(null) <= 0.5;
        }

        public double? CalcCoLHealing(Event ent)
        {
            int skillLvl = Parent.Skills.First(x => x.Name == "Cycle of Life")!.Level;
            return FighterUtils.CalculateHealByBasicVal((Parent.GetAttack(null) * CoLFAtkMods[skillLvl]) + CoLFFix[skillLvl], ent);
        }
        public double? CalcCoLHealingParty(Event ent)
        {
            return FighterUtils.CalculateHealByBasicVal((Parent.GetAttack(null) *0.07) + 93, ent);
        }



        public override void DefaultFighter_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff

            if (ent is FinishCombat)
            {
                trackedUnits = new List<Unit>();
            }
            else if (ent is ResourceDrain or DamageEventTemplate or Healing or ResourceGain)
            {
                CheckAndAddTarget(ent.TargetUnit);
            }
            else if (ent is ExecuteAbilityFinish &&Parent.Friends.Any(x=>x==ent.SourceUnit)&&ent.AbilityValue.Attack&&ent.SourceUnit.IsAlive&&ColBuffAvailable())
            {
                ent.ChildEvents.Add(new Healing(ent.Parent, this, ent.SourceUnit)//will put source unit coz Output healing calc will be calculated by target unit
                {
                    AbilityValue = ent.AbilityValue, TargetUnit = ent.SourceUnit, CalculateValue = CalcCoLHealing
                });

                if (Atraces.HasFlag(ATracesEnm.A4))
                {
                    //for all friends except attacker unit
                    foreach (Unit unit in ent.SourceUnit.Friends.Where(x => x.IsAlive &&x!=ent.SourceUnit))
                    {
                  
                        ent.ChildEvents.Add(new Healing(ent.Parent, this, unit)//will put source unit coz Output healing calc will be calculated by target unit
                        { 
                            AbilityValue = ent.AbilityValue, TargetUnit = unit, CalculateValue = CalcCoLHealingParty
                        });
                    }
                }
            }


            base.DefaultFighter_HandleEvent(ent);
        }


        //check unit alive and hp status and add/or remove
        private void CheckAndAddTarget(Unit entTargetUnit)
        {
            //if friend have low hp then add to track
            if (Parent.Friends.Any(x => x == entTargetUnit))
            {
                if (trackedUnits.All(x => x != entTargetUnit) && entTargetUnit.IsAlive && UnitAtLowHpForAuto(entTargetUnit))
                {
                    trackedUnits.Add(entTargetUnit);
                    Parent.ParentTeam.ParentSim.Parent.LogDebug($"{Parent.Name} add {entTargetUnit.Name} to track list");
                }
                else if (trackedUnits.Any(x => x == entTargetUnit) && (!UnitAtLowHpForAuto(entTargetUnit) || !entTargetUnit.IsAlive))
                {
                    //remove high hp unit from track
                    trackedUnits.Remove(entTargetUnit);
                    Parent.ParentTeam.ParentSim.Parent.LogDebug($"{Parent.Name} remove {entTargetUnit.Name} from track list");
                }
            }


        }


        public override void DefaultFighter_HandleStep(Step step)
        {
            //check if friendly unit do action
            if (step.StepType is Step.StepTypeEnm.ExecuteAbilityFromQueue or Step.StepTypeEnm.UnitFollowUpAction
                or Step.StepTypeEnm.UnitTurnContinued or Step.StepTypeEnm.UnitTurnStarted)
            {
                CheckAndAddTarget(step.Actor);
                //also check self  stacks
                if (!ColBuffAvailable() & step.Actor == Parent && (step.ActorAbility == ultimateAbility || step.ActorAbility == prayerOfAbyssFlowerAuto || step.ActorAbility == prayerOfAbyssFlower))
                {
                    step.Events.Add(new MechanicValChg(step, this, Parent) { TargetUnit = Parent, Val = 1, AbilityValue = cycleOfLife });
                }
            }


            base.DefaultFighter_HandleStep(step);
        }


        public override string GetSpecialText()
        {
            return $"CoL: {(int)Mechanics.Values[cycleOfLife]:d}\\{(int)cycleOfLifeMaxCnt:d}";
        }

        public bool ColAvailable()
        {
            return (Mechanics.Values[cycleOfLife] == cycleOfLifeMaxCnt);
        }

        public bool PoAFAvailable()
        {
            return Parent.Buffs.All(x => x.Reference != triggerCdBuff) && (trackedUnits?.Any(x => x.IsAlive) ?? false);
        }

        public bool ColBuffAvailable()
        {
            return Parent.Buffs.Any(x => x.Reference == uniqueBuff);
        }

        public double? CalculateBasicDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(null) * (0.4 + (Parent.Skills.First(x => x.Name == "Thorns of the Abyss").Level * 0.1)), ent);
        }


        //50-110
        public double? CalculatePrayerOfAbyssFlower(Event ent)
        {
            int skillLvl = Parent.Skills.First(x => x.Name == "Prayer of Abyss Flower")!.Level;
            return FighterUtils.CalculateHealByBasicVal((Parent.GetAttack(null) * PoAFAtkMods[skillLvl]) + PoAFFix[skillLvl], ent);
        }


        public double? CalculateUltimateDmg(Event ent)
        {
            int skillLvl = Parent.Skills.First(x => x.Name == "Death Wish")!.Level;
            return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(null) * DeathWishMods[skillLvl], ent);
        }
        //get targets for auto heal. One target for Luocha
        public IEnumerable<Unit> CalcFollowPoAFTarget()
        {
            IEnumerable<Unit> targets = new List<Unit>() { trackedUnits.First() };

            return targets;
        }
        public Luocha(Unit parent) : base(parent)
        {
            Parent.Stats.BaseMaxEnergy = 100;

            uniqueBuff = new Buff(Parent)
            {
                Type = Buff.ModType.Buff,
                BaseDuration = 2,
                MaxStack = 1,
                CustomIconName = "Icon_Abyss_Flower"
            };

            triggerCdBuff = new Buff(Parent)
            {
                Type = Buff.ModType.Buff,
                BaseDuration = 2,
                MaxStack = 1,
                CustomIconName = "Abyss_Flower_CD"
            };

            //=====================
            //Abilities
            //=====================
            //Cycle of life
            cycleOfLife = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.FollowUpAction
                ,
                Name = "Cycle of life"
                ,
                Element = Element
                ,
                Available = ColAvailable
                ,
                Priority = Ability.PriorityEnm.Low
                ,
                TargetType = Ability.TargetTypeEnm.Self
            };
            cycleOfLife.Events.Add(new MechanicValChg(null, this, this.Parent) { TargetUnit = Parent, AbilityValue = cycleOfLife, Val = -cycleOfLifeMaxCnt });
            cycleOfLife.Events.Add(new ApplyBuff(null, this, Parent)
            {
                AbilityValue = cycleOfLife,
                TargetUnit = Parent,
                BuffToApply = uniqueBuff
            });
            Mechanics.AddVal(cycleOfLife);
            Abilities.Add(cycleOfLife);

            //Prayer of Abyss Flower
            prayerOfAbyssFlower = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Ability
                ,
                Name = "Prayer of Abyss Flower"
                ,
                Element = Element
                ,
                TargetType = Ability.TargetTypeEnm.Friend
                ,
                CostType = Resource.ResourceType.SP
                ,
                Cost = 1
                ,
                EnergyGain = 30
            };
            if (Atraces.HasFlag(ATracesEnm.A2))
                prayerOfAbyssFlower.Events.Add(new DispelShit(null, this, this.Parent)
                { AbilityValue = prayerOfAbyssFlower });

            prayerOfAbyssFlower.Events.Add(new Healing(null, this, this.Parent) { CalculateValue = CalculatePrayerOfAbyssFlower, AbilityValue = prayerOfAbyssFlower });


            Abilities.Add(prayerOfAbyssFlower);


            //Prayer of Abyss Flower(auto)
            prayerOfAbyssFlowerAuto = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.FollowUpAction
                ,
                Name = "Prayer of Abyss Flower (auto)"
                ,
                Element = Element
                ,
                Available = PoAFAvailable
                ,
                Priority = Ability.PriorityEnm.High
                ,
                TargetType = Ability.TargetTypeEnm.Friend
                ,
                EnergyGain = 30
            };
            if (Atraces.HasFlag(ATracesEnm.A2))
                prayerOfAbyssFlowerAuto.Events.Add(new DispelShit(null, this, this.Parent) { CalculateTargets = CalcFollowPoAFTarget, AbilityValue = prayerOfAbyssFlowerAuto });
            prayerOfAbyssFlowerAuto.Events.Add(new Healing(null, this, this.Parent) { CalculateTargets = CalcFollowPoAFTarget, CalculateValue = CalculatePrayerOfAbyssFlower, AbilityValue = prayerOfAbyssFlowerAuto });
            prayerOfAbyssFlowerAuto.Events.Add(new ApplyBuff(null, this, Parent)
            {
                AbilityValue = prayerOfAbyssFlowerAuto,
                TargetUnit = Parent,
                BuffToApply = triggerCdBuff
            });
            Abilities.Add(prayerOfAbyssFlowerAuto);





            //basic attack
            Ability ThornsoftheAbyss;
            ThornsoftheAbyss = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Basic
                ,
                Name = "Thorns of the Abyss"
                ,
                Element = Element
                ,
                AdjacentTargets = Ability.AdjacentTargetsEnm.None
                ,
                Attack = true
                ,
                EnergyGain = 20
                ,
                SpGain = 1
            };

            for (int i = 0; i < 2; i++)
            {
                ThornsoftheAbyss.Events.Add(new DirectDamage(null, this, this.Parent)
                { CalculateValue = CalculateBasicDmg, AbilityValue = ThornsoftheAbyss, CalculateProportion = 0.3 });
                ThornsoftheAbyss.Events.Add(new ToughnessShred(null, this, this.Parent)
                { Val = 30 * 0.3, AbilityValue = ThornsoftheAbyss });
            }
            ThornsoftheAbyss.Events.Add(new DirectDamage(null, this, this.Parent)
            { CalculateValue = CalculateBasicDmg, AbilityValue = ThornsoftheAbyss, CalculateProportion = 0.4 });
            ThornsoftheAbyss.Events.Add(new ToughnessShred(null, this, this.Parent)
            { Val = 30 * 0.4, AbilityValue = ThornsoftheAbyss });
            Abilities.Add(ThornsoftheAbyss);


            // ULTIMATE
            ultimateAbility = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Ultimate
                ,
                Name = "Death Wish"
                ,
                AdjacentTargets = Ability.AdjacentTargetsEnm.All
                ,
                Attack = true
                ,
                EnergyGain = 5
                ,
                Available = UltimateAvailable
                ,
                Priority = PriorityEnm.Ultimate
                ,
                EndTheTurn = false
                ,
                CostType = Resource.ResourceType.Energy
                ,
                Cost = Parent.Stats.BaseMaxEnergy

            };
            //dmg events
            ultimateAbility.Events.Add(new DispelGood(null, this, this.Parent) { AbilityValue = ultimateAbility, CurentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
            ultimateAbility.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateUltimateDmg, AbilityValue = ultimateAbility, CurentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
            ultimateAbility.Events.Add(new ToughnessShred(null, this, this.Parent) { Val = 60, AbilityValue = ultimateAbility, CurentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
            Abilities.Add(ultimateAbility);


            //Mercy of a Fool
            var ability = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Technique
                ,
                Name = "Mercy of a Fool"
                ,
                Cost = 1
                ,
                CostType = Resource.ResourceType.TP
                ,
                Element = Element
            };
            ability.Events.Add(new MechanicValChg(null, this, Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue, TargetUnit = Parent, Val = cycleOfLifeMaxCnt, AbilityValue = cycleOfLife });//AbilityValue for Cycle of life
            Abilities.Add(ability);

            //CoL buffs
            ConditionMods.Add(new ConditionMod(Parent)
            {

                Mod = new Buff(Parent) { Effects = new List<Effect>() { new EffAtkPrc() { Value = 0.2 } }, CustomIconName = uniqueBuff.CustomIconName }
                ,
                Target = Parent.ParentTeam
                ,
                Condition = new ConditionMod.ConditionRec() { ConditionAvailable = ColBuffAvailable }

            });



            //A6
            if (Atraces.HasFlag(ATracesEnm.A6))
                DebuffResists.Add(new DebuffResist() { Debuff = typeof(EffCrowControl), ResistVal = 0.7 });

        }
    }
}
