﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static HSR_SIM_LIB.Resource;
using static HSR_SIM_LIB.Unit;

namespace HSR_SIM_LIB
{/// <summary>
/// Ability class
/// </summary>
    public class Ability: CheckEssence
    {

        private AbilityTypeEnm abilityType;//Technique, ultimate etc..
        private Unit parent;//caster
        private short cost=0;
        private ResourceType costType= ResourceType.nil;
        private ElementEnm? element;//element of skill
        public ElementEnm? Element { get => element; set => element = value; }
        public AbilityTypeEnm AbilityType { get => abilityType; set => abilityType = value; }
        public Unit Parent { get => parent; set => parent = value; }

        public string Name { get; internal set; }
        public List<Event> Events { get => events; set => events = value; }
        public short Cost { get => cost; set => cost = value; }
        public ResourceType CostType { get => costType; set => costType = value; }


        private List<Event> events = new List<Event>();
        

        public Ability(Unit parent) 
        { 
            Parent= parent;
        }
      

        public enum AbilityTypeEnm
        {
            Basic,
            Skill,
            Ultimate,
            Talent,
            Technique,
            Trigger
        }

        public enum TargetTypeEnm
        {
            Self,
            Target,
            Hostiles,
            Party,
            AOE,
            Blast
        }
            
    }
}
