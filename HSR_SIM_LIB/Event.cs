using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public class Event
    {
        private EventType type;
        private Ability abilityValue;

        public EventType Type { get => type; set => type = value; }
        public Ability AbilityValue { get => abilityValue; set => abilityValue = value; }

        public enum EventType
        {
            CombatStartSkillQueue,
            ResourceDrain,
            EnterCombat
        }

        public enum ResourceType
        {
            TP,
            SP
        }
    }
}
