using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB;

namespace HSR_SIM_GUI.DamageTools
{
    internal static class TaskUtils
    {
        public  record RTask
        {
            public string Scenario;
            public string Profile;
            public List<Worker.RCombatResult> Results = new List<Worker.RCombatResult>();
            public bool Fetched;
            public RAggregatedData Data = new RAggregatedData();
            public int Iterations { get; set; }
            public List<RTask> Subtasks { get; set; }
            public List<Worker.RStatMod> StatMods { get; set; }
        }

        public record RLinksToOjbects
        {
            public RTask Task;
            public Worker.RCombatResult Result;
        }

        public record PartyUnit
        {
            public string CombatUnit;
            public Dictionary<Type, double> avgByTypeDPAV = new Dictionary<Type, double>();
            public double avgDPAV;
            public double maxDPAV;
            public double minDPAV;
        }

        public record RAggregatedData
        {
            public double WinRate { get; set; }
            public double TotalAV { get; set; }
            public double Cycles { get; set; }
            public double DefeatCycles { get; set; }
            public double avgDPAV;
            public double maxDPAV;
            public double minDPAV;

            public List<PartyUnit> PartyUnits = new List<PartyUnit>();
        }

        public record RTaskList
        {
            public List<RTask> Tasks;
            public int ThreadCount = 0;

        }
    }
}
