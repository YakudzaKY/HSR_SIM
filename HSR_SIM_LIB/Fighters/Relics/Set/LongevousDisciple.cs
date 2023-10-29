using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    internal class LongevousDisciple : DefaultRelicSet
    {
        private readonly Mod uniqueBuff = null;

        public LongevousDisciple(IFighter parent,int num) : base(parent,num)
        {
            uniqueBuff = new Mod(Parent.Parent)
            {
                Type = Mod.ModType.Buff, BaseDuration = 2, MaxStack = 2, 
                Effects= new (){new (){EffType = Effect.EffectType.CritPrc, Value = 0.08}}
            };
        }

        public override  void DefaultRelicSet_HandleEvent(Event ent)
        {
            //if friend unit consume our hp or got attack then apply buff
            if (Num >= 4)
            {
                if (( ent is ResourceDrain
                      &&ent.AbilityValue?.Parent.Parent.ParentTeam==Parent.Parent.ParentTeam
                      && ent.TargetUnit == Parent.Parent
                      && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal != 0)
                    || (ent.TargetUnit == Parent.Parent && ent is DirectDamage))
                {
                    ApplyMod newEvent = new (ent.ParentStep, this, Parent.Parent)
                    {
                        TargetUnit = Parent.Parent,
                        Modification = uniqueBuff
                    };
                    ent.ChildEvents.Add(newEvent);
                }
            }


            base.DefaultRelicSet_HandleEvent(ent);
           
        }
        public override  void DefaultRelicSet_HandleStep(Step step)
        {
            
            base.DefaultRelicSet_HandleStep(step);
        }
    }
}
