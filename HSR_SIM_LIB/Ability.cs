using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{/// <summary>
/// Ability class
/// </summary>
    public class Ability
    {

        private AbilityTypeEnm abilityType;

        public AbilityTypeEnm AbilityType { get => abilityType; set => abilityType = value; }
        public AbilityParameters AbilityParams { get => abilityParams; set => abilityParams = value; }

        private AbilityParameters abilityParams;

      

        public enum AbilityTypeEnm
        {
            Basic,
            Skill,
            Ultimate,
            Talent,
            Technique
        }
    }
}
