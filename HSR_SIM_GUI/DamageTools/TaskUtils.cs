using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HSR_SIM_GUI.DamageTools;

internal static class TaskUtils
{
    public record RTask
    {
        public RAggregatedData Data = new();
        public bool Fetched
        {
            get => ResultCount == Iterations;
        }
        public string Profile;
        public int ResultCount { get; set; } = 0;
        public int StartCount { get; set; } = 0;
        public string Scenario;
        public int Iterations { get; set; }
        public List<RTask> Subtasks { get; set; }
        public List<Worker.RStatMod> StatMods { get; set; }
        public bool DevMode { get; set; }

        private Type[] typeArray =
        {
            typeof(DirectDamage), typeof(DoTDamage), typeof(ToughnessBreakDoTDamage),
            typeof(ToughnessBreak)
        };

        public double WinRate
        {
            get => ResultCount>0?(Data.WinCount / ResultCount * 100):0;
        }
        //aggregate average value by new value
        private double AggAvg(double prevAgg, double newVal, int? newCounter = null)
        {

            return (prevAgg * ((newCounter ?? Data.WinCount) - 1) + newVal) / (newCounter ?? Data.WinCount);
        }


        public void Aggregate(Worker.RCombatResult rCombatResult)
        {
            ResultCount++;//increment result counter
            if (!rCombatResult.Success)
            {
                //for defeat we aggregate only cycles for sustain test(how much we survive)
                Data.DefeatCycles = AggAvg(Data.DefeatCycles, rCombatResult.Cycles, ResultCount - Data.WinCount);
            }
            else
            {
                Data.WinCount++; //inc win counter
                //calc team values
                Data.TotalAV = AggAvg(Data.TotalAV, rCombatResult.TotalAv, Data.WinCount);
                Data.Cycles = AggAvg(Data.Cycles, rCombatResult.Cycles, Data.WinCount);
                double teamDpav =
                    rCombatResult.Combatants.Sum(z => z.Damages.Sum(x => x.Value) / rCombatResult.TotalAv);
                Data.avgDPAV = AggAvg(Data.avgDPAV, teamDpav, Data.WinCount);
                Data.minDPAV = Math.Min(Data.minDPAV, teamDpav);
                Data.maxDPAV = Math.Max(Data.maxDPAV, teamDpav);
                //calc personal stats
                foreach (var unit in rCombatResult.Combatants)
                {
                    //try find existing unit
                    var foundPartyUnit = Data.PartyUnits.FirstOrDefault(x => x.Key == unit.CombatUnit);
                    PartyUnit foundUnit;
                    if (foundPartyUnit.Equals(new KeyValuePair<string,PartyUnit>()))
                    {
                        foundUnit=new PartyUnit();
                        Data.PartyUnits[unit.CombatUnit] = foundUnit;

                    }
                    else
                    {
                        foundUnit = foundPartyUnit.Value;
                    }
        
                    foundUnit.avgDPAV = AggAvg(foundUnit.avgDPAV, unit.Damages.Sum(x => x.Value) / rCombatResult.TotalAv);
                    foundUnit.minDPAV = Math.Min(foundUnit.minDPAV, unit.Damages.Sum(x => x.Value) / rCombatResult.TotalAv);
                    foundUnit.maxDPAV = Math.Max(foundUnit.maxDPAV, unit.Damages.Sum(x => x.Value) / rCombatResult.TotalAv);

                    foreach (var typ in typeArray)
                    {
                        var fndAvgByTypeDpav = foundUnit.avgByTypeDPAV.FirstOrDefault(x => x.Key == typ);
                        double val = AggAvg(fndAvgByTypeDpav.Value, unit.Damages[typ] / rCombatResult.TotalAv);


                        foundUnit.avgByTypeDPAV[typ] = val;


                    }


                }


            }


            // GC.Collect();
        }

    }

    public record RLinksToOjbects
    {
        public Worker.RCombatResult Result;
        public RTask Task;
    }

    public record PartyUnit
    {
        public ConcurrentDictionary<Type, Double> avgByTypeDPAV = new();
        public double avgDPAV;
        public double maxDPAV;
        public double minDPAV;
    }

    public record RAggregatedData
    {
        public double avgDPAV;
        public double maxDPAV;
        public double minDPAV;

        public ConcurrentDictionary<String, PartyUnit> PartyUnits = new();
        public int WinCount { get; set; }
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