using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB;
using static HSR_SIM_GUI.DamageTools.TaskUtils;

namespace HSR_SIM_GUI.DamageTools
{
    internal static class ThreadUtils
    {
        public class ThreadWork
        {

            /// <summary>
            /// child thread func
            /// </summary>
            /// <param name="links"></param>
            public static void ProcWork(Object links)
            {
                RLinksToOjbects rLinks = links as RLinksToOjbects;
                Worker wrk = new Worker();
                wrk.LoadScenarioFromXml(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\" + rLinks.Task.Scenario, AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + rLinks.Task.Profile);
                wrk.ApplyModes(rLinks.Task.StatMods);
                wrk.GetCombatResult(rLinks.Result);

            }


            /// <summary>
            /// main thread work func
            /// </summary>
            /// <param name="taskList"></param>
            public static void DoWork(Object taskList)
            {
                List<Thread> myThreads = new List<Thread>();
                RTaskList myTaskList = taskList as RTaskList;
                for (int i = 0; i < myTaskList.ThreadCount; i++)
                {
                    Thread thd = new Thread(new ParameterizedThreadStart(ThreadWork.ProcWork));
                    myThreads.Add(thd);

                }

                var mainQuery = (from p in myTaskList.Tasks
                                 from c in p.Subtasks
                                 select c).Union(from p in myTaskList.Tasks select p)
                    .Distinct();

                while (mainQuery.Any(x => !x.Fetched))
                {
                    //queue tasks
                    foreach (Thread thr in (List<Thread>)myThreads.Where(x => x.IsAlive == false).ToList())
                    {

                        RTask task = mainQuery.FirstOrDefault(x => x.Results != null && x.Results.Count() < x.Iterations);

                        if (task != null)
                        {
                            Worker.RCombatResult result = new Worker.RCombatResult();
                            task.Results.Add(result);
                            Thread thThread = myThreads[myThreads.IndexOf(thr)];
                            int ndx = myThreads.IndexOf(thr);
                            myThreads[ndx] = null;
                            thThread = new Thread(new ParameterizedThreadStart(ThreadWork.ProcWork));
                            myThreads[ndx] = thThread;
                            RLinksToOjbects links = new RLinksToOjbects();
                            links.Task = task;
                            links.Result = result;
                            thThread.Start(links);
                        }
                    }
                    //fetch data
                    foreach (RTask fetchTask in mainQuery.Where(x =>
                                 !x.Fetched && x.Results.Count(y => y.Success != null) == x.Iterations))
                    {
                        List<Worker.RCombatResult> winResults =
                            fetchTask.Results.Where(x => x.Success ?? false).ToList();
                        List<Worker.RCombatResult> defeatResults =
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
                                    (z.Damages.Sum(x => x.Value) / x.TotalAv
                                    )));
                            fetchTask.Data.minDPAV = winResults.Min(x => x.Combatants
                                .Sum(z =>
                                    (z.Damages.Sum(x => x.Value) / x.TotalAv
                                    )));
                            fetchTask.Data.maxDPAV = winResults.Max(x => x.Combatants
                                .Sum(z =>
                                    (z.Damages.Sum(x => x.Value) / x.TotalAv
                                    )));

                            foreach (string unit in winResults.First()
                                         .Combatants
                                         .Select(y => y.CombatUnit))
                            {
                                PartyUnit prUnit = new PartyUnit();
                                prUnit.CombatUnit = unit;

                                prUnit.avgDPAV = winResults.Average(x => x.Combatants
                                    .Where(y => y.CombatUnit == unit).Sum(z =>
                                        (z.Damages.Sum(x => x.Value) / x.TotalAv
                                        )));
                                prUnit.minDPAV = winResults.Min(x => x.Combatants.Where(y => y.CombatUnit == unit).Sum(z =>
                                    (z.Damages.Sum(x => x.Value) / x.TotalAv
                                    )));
                                prUnit.maxDPAV = winResults.Max(x => x.Combatants.Where(y => y.CombatUnit == unit)
                                    .Sum(z =>
                                        (z.Damages.Sum(x => x.Value) / x.TotalAv
                                        )));

                                Type[] typeArray = new Type[]
                                {
                                typeof(DirectDamage), typeof(DoTDamage), typeof(ToughnessBreakDoTDamage),
                                typeof(ToughnessBreak)
                                };

                                foreach (Type typ in typeArray)
                                {

                                    prUnit.avgByTypeDPAV.Add(typ, winResults.Average(x => x.Combatants
                                        .Where(y => y.CombatUnit == unit).Sum(z =>
                                            z.Damages[typ] / x.TotalAv
                                        )));

                                }

                                fetchTask.Data.PartyUnits.Add(prUnit);

                            }

                        }

                        //clear resources
                        foreach (var sd in fetchTask.Results)
                        {
                            sd.Combatants = null;
                        }
                        System.GC.Collect();
                    }



                    Thread.Sleep(10);
                }

                while (myThreads.Count(x => x.IsAlive == true) > 0)
                {
                    Thread.Sleep(100);
                }

                System.GC.Collect();
            }
        }
    }
}
