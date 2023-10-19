using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    internal class FleetoftheAgeless:DefaultRelicSet
    {
        public FleetoftheAgeless(IFighter parent,int num) : base(parent,num)
        {
            if (num >= 2)
            {
                ConditionMods.Add(new ConditionMod(parent.Parent)
                {
                    Mod=new Mod(null){Modifiers = new List<Mod.ModifierType>(){Mod.ModifierType.AtkPrc},Value = 0.08,CustomIconName = "gear\\"+GetType().ToString().Split('.').Last()}
                    , Target=parent.Parent.ParentTeam
                    ,Condition= new ConditionMod.ConditionRec(){CondtionParam = ConditionMod.ConditionCheckParam.SPD,CondtionExpression = ConditionMod.ConditionCheckExpression.EqualOrMore,Value = 120}
                    
                });
            }
        }
    }
}
