using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using ImageMagick;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Luocha : DefaultFighter
    {
        private readonly Dictionary<int, double> PoAFAtkMods = new()
        {
            { 1, 0.40 }, { 2, 0.425 }, { 3, 0.45 }, { 4, 0.475 }, { 5, 0.50 }
            ,{ 6, 0.52 }, {7, 0.54 }, { 8, 0.56 }, { 9, 0.58}, { 10, 0.60 }
            ,{ 11, 0.62 }, { 12, 0.64 }
        };

        private List<Unit> trackedUnits = new List<Unit>();

        private readonly Dictionary<int, double> PoAFFix = new()
        {
            { 1, 200 }, { 2, 320 }, { 3, 410 }, { 4, 500 }, { 5, 560 }, { 6, 620 }, { 7, 665 }, { 8, 710 },
            { 9, 755 }, { 10, 800 }  ,{ 11, 845 }, { 12, 890 }
        };
        public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Abundance;
        public sealed override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Imaginary;
        private readonly Buff uniqueBuff = null;
        private Ability cycleOfLife;
        private readonly double cycleOfLifeMaxCnt = 2;
        private Ability PrayerOfAbyssFlowerAuto;
        private Ability PrayerOfAbyssFlower;
        private readonly Buff triggerCDBuff = null;

        //If unit hp<=50% for Luocha follow up heals
        private bool UnitAtLowHpForAuto(Unit unit)
        {
            return unit.GetRes(Resource.ResourceType.HP).ResVal / unit.GetMaxHp(null) <= 0.5;
        }

        //Handler for healing
        public override void DefaultFighter_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff

            if (ent is FinishCombat )
            {
                trackedUnits = new List<Unit>();
            }
            else if (ent is ResourceDrain or DamageEventTemplate or Healing or ResourceGain)
            {
                CheckAndAddTarget(ent.TargetUnit);
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
                else if (trackedUnits.Any(x => x == entTargetUnit) && !UnitAtLowHpForAuto(entTargetUnit))
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
            if (step.StepType is Step.StepTypeEnm.ExecuteAbility or Step.StepTypeEnm.UnitFollowUpAction
                or Step.StepTypeEnm.UnitTurnContinued or Step.StepTypeEnm.UnitTurnStarted)
            {
                CheckAndAddTarget(step.Actor);
                //also check self  stacks
                if (!ColBuffAvailable() &step.Actor ==Parent && (step.ActorAbility == PrayerOfAbyssFlowerAuto || step.ActorAbility == PrayerOfAbyssFlower))
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
            return  Parent.Mods.All(x => x.RefMod != triggerCDBuff) && (trackedUnits?.Any(x => x.IsAlive)??false);
        }

        public bool ColBuffAvailable()
        {
            return Parent.Mods.Any(x => x.RefMod == uniqueBuff);
        }

        public double? CalculateBasicDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(null) *(0.4 + (Parent.Skills.FirstOrDefault(x=>x.Name=="Thorns of the Abyss").Level*0.1)), ent);
        }


        //50-110
        public double? CalculatePrayerOfAbyssFlower(Event ent)
        {
            int skillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Prayer of Abyss Flower")!.Level;
            return FighterUtils.CalculateHealByBasicVal((Parent.GetAttack(null) * PoAFAtkMods[skillLvl]) + PoAFFix[skillLvl], ent);
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

            triggerCDBuff = new Buff(Parent)
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
            cycleOfLife.Events.Add(new ApplyMod(null, this, Parent)
            {
                AbilityValue = cycleOfLife,
                TargetUnit = Parent,
                Modification = uniqueBuff
            });
            Mechanics.AddVal(cycleOfLife);
            Abilities.Add(cycleOfLife);

            //Prayer of Abyss Flower(auto)
            PrayerOfAbyssFlowerAuto = new Ability(this)
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
            PrayerOfAbyssFlowerAuto.Events.Add(new Healing(null, this, this.Parent) { CalculateTargets = CalcFollowPoAFTarget, CalculateValue = CalculatePrayerOfAbyssFlower, AbilityValue = PrayerOfAbyssFlowerAuto });
            PrayerOfAbyssFlowerAuto.Events.Add(new ApplyMod(null, this, Parent)
            {
                AbilityValue = PrayerOfAbyssFlowerAuto,
                TargetUnit = Parent,
                Modification = triggerCDBuff
            });
            //TODO DISPELL BULLSHIT
            Abilities.Add(PrayerOfAbyssFlowerAuto);

            //Prayer of Abyss Flower(auto)
            PrayerOfAbyssFlower = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Ability
                ,
                Name = "Prayer of Abyss Flower"
                ,
                Element = Element
                ,
                TargetType = Ability.TargetTypeEnm.Friend
                ,Cost = 300
                ,
                EnergyGain = 30
            };

            //todo fullfill events
            Abilities.Add(PrayerOfAbyssFlower);

        
            Ability SystemWarning;
            //System Warning
            SystemWarning = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Basic
                , Name = "FIX THOIS FUCKING ATTACK"
                , Element = Element
                , AdjacentTargets = Ability.AdjacentTargetsEnm.None
                , Attack=true
                , EnergyGain = 20
                , SpGain = 1
            };
            //dmg events
            SystemWarning.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateBasicDmg,  AbilityValue = SystemWarning });
            SystemWarning.Events.Add(new ToughnessShred(null, this, this.Parent) { Val=30,  AbilityValue = SystemWarning });
            Abilities.Add(SystemWarning);
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
            ability.Events.Add(new MechanicValChg(null, this, Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, TargetUnit = Parent, Val = cycleOfLifeMaxCnt, AbilityValue = cycleOfLife });//AbilityValue for Cycle of life
            Abilities.Add(ability);

            //CoL buffs
            ConditionMods.Add(new ConditionMod(Parent)
            {

                Mod = new Buff(Parent) { Effects = new List<Effect>() { new EffAtkPrc() { Value = 0.08 } }, CustomIconName = uniqueBuff.CustomIconName }
                ,
                Target = Parent.ParentTeam
                ,
                Condition = new ConditionMod.ConditionRec() { ConditionAvailable = ColBuffAvailable }

            });

        }
    }
}
