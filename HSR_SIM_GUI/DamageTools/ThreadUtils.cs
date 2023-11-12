using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using static HSR_SIM_GUI.DamageTools.TaskUtils;

namespace HSR_SIM_GUI.DamageTools;

internal static class ThreadUtils
{
    public class ThreadWork
    {
        /// <summary>
        ///     child thread func
        /// </summary>
        /// <param name="links"></param>
        public static void ProcWork(object links)
        {
            var rLinks = links as RLinksToOjbects;
            var wrk = new Worker();
            wrk.LoadScenarioFromXml(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\" + rLinks.Task.Scenario,
                AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + rLinks.Task.Profile);
            wrk.ApplyModes(rLinks.Task.StatMods);
            wrk.GetCombatResult(rLinks.Result);
        }


        /// <summary>
        ///     main thread work func
        /// </summary>
        /// <param name="taskList"></param>
        public static void DoWork(object taskList)
        {
            var myThreads = new List<Thread>();
            var myTaskList = taskList as RTaskList;
            for (var i = 0; i < myTaskList.ThreadCount; i++)
            {
                var thd = new Thread(ProcWork);
                myThreads.Add(thd);
            }

            var mainQuery = (from p in myTaskList.Tasks
                    from c in p.Subtasks
                    select c).Union(from p in myTaskList.Tasks select p)
                .Distinct();

            while (mainQuery.Any(x => !x.Fetched))
            {
                //queue tasks
                foreach (var thr in myThreads.Where(x => x.IsAlive == false).ToList())
                {
                    var task = mainQuery.FirstOrDefault(x => x.Results != null && x.Results.Count() < x.Iterations);

                    if (task != null)
                    {
                        var result = new Worker.RCombatResult();
                        task.Results.Add(result);
                        var thThread = myThreads[myThreads.IndexOf(thr)];
                        var ndx = myThreads.IndexOf(thr);
                        myThreads[ndx] = null;
                        thThread = new Thread(ProcWork);
                        myThreads[ndx] = thThread;
                        var links = new RLinksToOjbects();
                        links.Task = task;
                        links.Result = result;
                        thThread.Start(links);
                    }
                }

                //fetch data
                foreach (var fetchTask in mainQuery.Where(x =>
                             !x.Fetched && x.Results.Count(y => y.Success != null) == x.Iterations))
                {
                    var winResults =
                        fetchTask.Results.Where(x => x.Success ?? false).ToList();
                    var defeatResults =
                        fetchTask.Results.Where(x => !(x.Success ?? false)).ToList();
                    fetchTask.Fetched = true;
                    fetchTask.Data.WinRate = fetchTask.Results.Average(x => x.Success ?? false ? 100 : 0);
                    if (defeatResults.Any())
                        fetchTask.Data.DefeatCycles = defeatResults.DefaultIfEmpty().Average(x => x.Cycles);
                    if (winResults.Any())
                    {
                        fetchTask.Data.TotalAV = winResults.Average(x => x.TotalAv);
                        fetchTask.Data.Cycles = winResults.Average(x => x.Cycles);
                        fetchTask.Data.avgDPAV = winResults.Average(x => x.Combatants
                            .Sum(z =>
                                z.Damages.Sum(x => x.Value) / x.TotalAv));
                        fetchTask.Data.minDPAV = winResults.Min(x => x.Combatants
                            .Sum(z =>
                                z.Damages.Sum(x => x.Value) / x.TotalAv));
                        fetchTask.Data.maxDPAV = winResults.Max(x => x.Combatants
                            .Sum(z =>
                                z.Damages.Sum(x => x.Value) / x.TotalAv));

                        foreach (var unit in winResults.First()
                                     .Combatants
                                     .Select(y => y.CombatUnit))
                        {
                            var prUnit = new PartyUnit();
                            prUnit.CombatUnit = unit;

                            prUnit.avgDPAV = winResults.Average(x => x.Combatants
                                .Where(y => y.CombatUnit == unit).Sum(z =>
                                    z.Damages.Sum(x => x.Value) / x.TotalAv));
                            prUnit.minDPAV = winResults.Min(x => x.Combatants.Where(y => y.CombatUnit == unit).Sum(z =>
                                z.Damages.Sum(x => x.Value) / x.TotalAv));
                            prUnit.maxDPAV = winResults.Max(x => x.Combatants.Where(y => y.CombatUnit == unit)
                                .Sum(z =>
                                    z.Damages.Sum(x => x.Value) / x.TotalAv));

                            Type[] typeArray =
                            {
                                typeof(DirectDamage), typeof(DoTDamage), typeof(ToughnessBreakDoTDamage),
                                typeof(ToughnessBreak)
                            };

                            foreach (var typ in typeArray)
                                prUnit.avgByTypeDPAV.Add(typ, winResults.Average(x => x.Combatants
                                    .Where(y => y.CombatUnit == unit).Sum(z =>
                                        z.Damages[typ] / x.TotalAv
                                    )));

                            fetchTask.Data.PartyUnits.Add(prUnit);
                        }
                    }

                    //clear resources
                    foreach (var sd in fetchTask.Results) sd.Combatants = null;
                    GC.Collect();
                }


                Thread.Sleep(10);
            }

            while (myThreads.Count(x => x.IsAlive) > 0) Thread.Sleep(100);

            GC.Collect();
        }
    }
}