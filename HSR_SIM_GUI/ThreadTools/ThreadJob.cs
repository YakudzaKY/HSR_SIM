using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HSR_SIM_GUI.DamageTools;
using HSR_SIM_GUI.TaskTools;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses.Events;


namespace HSR_SIM_GUI.ThreadTools
{
    /// <summary>
    /// The job sended into main sim thread also contains fields for results
    /// </summary>
    /// <param name="pTaskList"></param>
    /// <param name="pIterations"></param>
    internal class ThreadJob(List<SimTask> pTaskList, int pIterations)
    {
        
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
        
            public double WinRate
            {
                get => RunCount>0?(WinCount / RunCount * 100):0;
            }


            public ConcurrentDictionary<String, PartyUnit> PartyUnits = new();
            public int WinCount { get; set; }
            public double TotalAV { get; set; }
            public double Cycles { get; set; }
            public double DefeatCycles { get; set; }
            public int RunCount { get; set; }
        }


        /// <summary>
        /// List of task for sim
        /// </summary>
        public List<SimTask> TaskList { get; } = pTaskList;

        public Dictionary<SimTask, RAggregatedData> CombatData { get; } = new Dictionary<SimTask, RAggregatedData>();

        /// <summary>
        /// Iterations should execute for every task
        /// </summary>
        public int Iterations { get; } = pIterations;

        private Type[] typeArray =
        {
            typeof(DirectDamage), typeof(DoTDamage), typeof(ToughnessBreakDoTDamage),
            typeof(ToughnessBreak)
        };


        public void Aggregate(AggregateThread.rTaskProgress taskProgress,Worker.RCombatResult rCombatResult)
        {
            RAggregatedData Data;
            //load or add new
            if (!CombatData.TryGetValue(taskProgress.STask, out Data))
            {
                Data=new RAggregatedData();
                CombatData[taskProgress.STask] = Data;
            }
 
            //aggregate average value by new value
            double AggAvg(double prevAgg, double newVal, int? newCounter = null)
            {
                return (prevAgg * ((newCounter ?? Data.WinCount) - 1) + newVal) / (newCounter ?? Data.WinCount);
            }

           
            Data.RunCount++; //inc win counter
            if (!rCombatResult.Success)
            {
                //for defeat we aggregate only cycles for sustain test(how much we survive)
                Data.DefeatCycles = AggAvg(Data.DefeatCycles, rCombatResult.Cycles, taskProgress.EndCount - Data.WinCount);
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
                    if (foundPartyUnit.Equals(new KeyValuePair<string, PartyUnit>()))
                    {
                        foundUnit = new PartyUnit();
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

        }
    }
}
