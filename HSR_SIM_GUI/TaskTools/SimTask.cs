using HSR_SIM_LIB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses;

namespace HSR_SIM_GUI.TaskTools
{
    /// <summary>
    /// class for task some sim job
    /// </summary>
    internal class SimTask
    {
        public SimCls SimScenario { get; init; }
        public string DevLogPath { get; init; }

        public bool DevMode { get; init; } = false;
        public int Step { get; set; } = 0;
        
        // Parent need for group results
        public SimTask Parent { get; init; }

        public SimTask()
        {

        }
        /// <summary>
        /// modifiers to profile
        /// </summary>
        public List<Worker.RStatMod> StatMods { get; set; }
    }
}
