using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{/// <summary>
/// Combat simulation class
/// </summary>
    public class Combat
    {
        Scenario scenario;
        int currentStep=0;

        public int CurrentStep { get => currentStep; set => currentStep = value; }
        internal Scenario Scenario { get => scenario; set => scenario = value; }
        
        public Combat()
        {
            CurrentStep = 0;
        }
    }
}
