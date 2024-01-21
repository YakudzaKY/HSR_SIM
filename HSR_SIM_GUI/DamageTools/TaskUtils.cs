using System;
using System.Collections.Generic;
using HSR_SIM_LIB;

namespace HSR_SIM_GUI.DamageTools;

internal static class TaskUtils
{
    public record RTask
    {
        public RAggregatedData Data = new();
        public bool Fetched;
        public string Profile;
        public List<Worker.RCombatResult> Results = new();
        public string Scenario;
        public int Iterations { get; set; }
        public List<RTask> Subtasks { get; set; }
        public List<Worker.RStatMod> StatMods { get; set; }
        public bool DevMode { get; set; }
    }

    public record RLinksToOjbects
    {
        public Worker.RCombatResult Result;
        public RTask Task;
    }

    public record PartyUnit
    {
        public Dictionary<Type, double> avgByTypeDPAV = new();
        public double avgDPAV;
        public string CombatUnit;
        public double maxDPAV;
        public double minDPAV;
    }

    public record RAggregatedData
    {
        public double avgDPAV;
        public double maxDPAV;
        public double minDPAV;

        public List<PartyUnit> PartyUnits = new();
        public double WinRate { get; set; }
        public double TotalAV { get; set; }
        public double Cycles { get; set; }
        public double DefeatCycles { get; set; }
    }

    public record RTaskList
    {
        public List<RTask> Tasks;
        public int ThreadCount = 0;
        public bool FetchAndAggregateData = true;//Aggregate result data
    }
}