using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using ImageMagick;
using static HSR_SIM_GUI.DamageTools.TaskUtils;

namespace HSR_SIM_GUI.DamageTools;

internal static class ThreadUtils
{
    public class ThreadWork
    {
        public static int GetDecision(string[] items, string description)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        ///     child thread func
        /// </summary>
        /// <param name="links"></param>
        public static void ProcWork(object links)
        {
            var rLinks = links as RLinksToOjbects;
            var wrk = new Worker();
            wrk.DevMode = rLinks.Task.DevMode;
            if (wrk.DevMode)
                wrk.CbGetDecision += GetDecision;
            wrk.LoadScenarioFromXml(rLinks.Task.Scenario, rLinks.Task.Profile);
            wrk.ApplyModes(rLinks.Task.StatMods);
            wrk.GetCombatResult(rLinks.Result);
            rLinks.Task.Aggregate(rLinks.Result);

        }


        /// <summary>
        ///     main thread work func
        /// </summary>
        /// <param name="taskList">list of task to do</param>
        /// 
        public static void DoWork(object taskList)
        {
            var myThreads = new List<Thread>();
            var myTaskList = taskList as RTaskList;
            for (var i = 0; i < myTaskList.ThreadCount; i++)
            {
                var thd = new Thread(ProcWork);
                myThreads.Add(thd);
            }

            var mainQuery = (from p in myTaskList.Tasks select p).Union(from p in myTaskList.Tasks.Where(x => x.Subtasks is not null)
                             from c in p.Subtasks
                             select c)
                .Distinct();

            while (mainQuery.Any(x => !x.Fetched))
            {
                //queue tasks
                foreach (var thr in myThreads.Where(x => x.IsAlive == false).ToList())
                {
                    //get first task where result count < target iterations
                    var task = mainQuery.FirstOrDefault(x => x.StartCount < x.Iterations);

                    if (task != null)
                    {
                        task.StartCount++;
                        //result will be filled in worker class
                        var result = new Worker.RCombatResult();
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

                try
                {
                    Thread.Sleep(10);
                }
                catch(ThreadInterruptedException e)
                {
                    Console.WriteLine("thread interrupted");
                    break;

                }

               
            }

            try
            {
                while (myThreads.Count(x => x.IsAlive) > 0) Thread.Sleep(100);
            }
            catch(ThreadInterruptedException e)
            {
                Console.WriteLine("thread interrupted");
            }
         

            GC.Collect();
        }
    }
}