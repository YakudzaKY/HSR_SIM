using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public class Step
    {
        private StepTypeEnm stepType;

        public StepTypeEnm StepType { get => stepType; set => stepType = value; }

        public Step()
        {
            StepType = StepTypeEnm.Iddle;
        }
        public enum StepTypeEnm
        {
            CombatInit,//on scenario load and combat init
            Iddle//on Iddle, nothing to_do
        }
    }
}
