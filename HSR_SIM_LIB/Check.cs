using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Check for condition
    /// </summary>
    internal class Check
    {
        private List<Check> innerChecks;
        private CheckTypeEnm checkType;

        public string Value { get; internal set; }
        public bool Clause { get; internal set; }

        internal List<Check> InnerChecks { get
            {
                if (innerChecks == null)
                    innerChecks = new List<Check>();
                return innerChecks; 
            }
            set => innerChecks = value; }
        internal CheckTypeEnm CheckType { get => checkType; set => checkType = value; }

        public enum CheckTypeEnm
        {
            FindTarget,
            Alive,
            Weakness,
            HaveSkill,
            AbilityType,
            AbilityCost,
            HaveEvent,
            ResourceCheck,
            FindBuff,
            Buff,
            ResourceQuantity,
            ResourceType
        }
    }
}
