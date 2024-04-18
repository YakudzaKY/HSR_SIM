using System.Collections.Concurrent;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_CLIENT.ThreadTools;

/// <summary>
///     The job sended into main sim thread also contains fields for results
/// </summary>
/// <param name="pTaskList"></param>
/// <param name="pIterations"></param>
public class ThreadJob(List<SimTask>? pTaskList, int pIterations)
{
    private readonly Type[] typeArray =
    {
        typeof(DirectDamage), typeof(DoTDamage), typeof(ToughnessBreakDoTDamage),
        typeof(ToughnessBreak)
    };


    /// <summary>
    ///     List of task for sim
    /// </summary>
    public List<SimTask>? TaskList { get; } = pTaskList;

    public Dictionary<SimTask, RAggregatedData> CombatData { get; } = new();

    /// <summary>
    ///     Iterations should execute for every task
    /// </summary>
    public int Iterations { get; } = pIterations;

    /// <summary>
    ///     calculate average values. Also recal values by new result
    /// </summary>
    /// <param name="taskProgress"></param>
    /// <param name="rCombatResult"></param>
    public void Aggregate(AggregateThread.TaskProgress taskProgress, Worker.RCombatResult rCombatResult)
    {
        RAggregatedData Data;
        //load or add new
        if (!CombatData.TryGetValue(taskProgress.STask, out Data))
        {
            Data = new RAggregatedData();
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
            var teamDpav =
                rCombatResult.Combatants.Sum(z => z.Damages.Sum(x => x.Value) / rCombatResult.TotalAv);
            Data.avgDPAV = AggAvg(Data.avgDPAV, teamDpav, Data.WinCount);
            Data.minDPAV = Data.minDPAV.HasValue ? Math.Min((double)Data.minDPAV, teamDpav) : teamDpav;
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
                var fndUnitDpav = unit.Damages.Sum(x => x.Value) / rCombatResult.TotalAv;
                foundUnit.minDPAV =  foundUnit.minDPAV.HasValue ? Math.Min((double)foundUnit.minDPAV,fndUnitDpav ):fndUnitDpav;
                foundUnit.maxDPAV = Math.Max(foundUnit.maxDPAV, unit.Damages.Sum(x => x.Value) / rCombatResult.TotalAv);

                foreach (var typ in typeArray)
                {
                    var fndAvgByTypeDpav = foundUnit.avgByTypeDPAV.FirstOrDefault(x => x.Key == typ);
                    var val = AggAvg(fndAvgByTypeDpav.Value, unit.Damages[typ] / rCombatResult.TotalAv);


                    foundUnit.avgByTypeDPAV[typ] = val;
                }
            }
        }
    }

    public record PartyUnit
    {
        public ConcurrentDictionary<Type, double> avgByTypeDPAV = new();
        public double avgDPAV;
        public double maxDPAV;
        public double? minDPAV;
    }

    public record RAggregatedData
    {
        public double avgDPAV;
        public double maxDPAV;
        public double? minDPAV;


        public ConcurrentDictionary<string, PartyUnit> PartyUnits = new();

        public double WinRate => RunCount > 0 ? WinCount / RunCount * 100 : 0;

        public int WinCount { get; set; }
        public double TotalAV { get; set; }
        public double Cycles { get; set; }
        public double DefeatCycles { get; set; }
        public int RunCount { get; set; }
    }
}