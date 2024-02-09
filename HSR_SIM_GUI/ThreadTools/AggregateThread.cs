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

namespace HSR_SIM_GUI.ThreadTools
{
    /// <summary>
    /// Main thread class for sim job
    /// Its Thread wrapper
    /// </summary>
    internal class AggregateThread
    {
        private Thread mainThread;
        private ThreadJob job;
        private int childThdCount;
        private ConcurrentQueue<SimTask> cq = new ConcurrentQueue<SimTask>();
        private ConcurrentQueue<KeyValuePair<SimTask, Worker.RCombatResult>> cResq = new();

        public record rTaskProgress
        {
            public SimTask STask { get; init; }
            public int StartCount { get; set; } = 0;
            public int EndCount { get; set; } = 0;

        }
        private List<rTaskProgress> taskProgress = new();

        List<SimThread> threads = new List<SimThread>();

        public bool IsAlive
        {
            get => mainThread.IsAlive;
        }

        private bool HaveTaskToWait()
        {
            return taskProgress.Any(x => x.EndCount < job.Iterations);
        }

        /// <summary>
        /// Interrupt all childs
        /// </summary>
        private void StopChilds()
        {
            foreach (SimThread thread in threads)
            {
                thread.Interrupt();
                
            }

            threads.Clear();
        }
        private void DoWork(object taskList)
        {
            bool haveTaskToRun = true;//flag that we have some task to queue
            bool haveTaskToWait = true;//flag that we have uncompleted sims
            bool haveTaskToAggregate = true;//flag that we have some task to aggregate data
            ThreadJob thd = taskList as ThreadJob;
            foreach (var task in thd.TaskList)
            {
                taskProgress.Add(new rTaskProgress() { STask = task });
            }

            for (var i = 0; i < childThdCount; i++)
            {
                SimThread sim = new SimThread(cq, cResq);
                threads.Add(sim);
            }


            while (haveTaskToRun || HaveTaskToWait() )
            {
                //start child jobs
                if (haveTaskToRun)
                {
                    //have slots to insert queue
                    int freeQSlots = (childThdCount * 100) - cq.Count;
                    while (freeQSlots > 0)
                    {
                        //get first uncompleted task
                        rTaskProgress taskToQ = taskProgress.FirstOrDefault(x => x.StartCount < job.Iterations);
                        // if no task then break
                        if (taskToQ == null)
                        {
                            haveTaskToRun = false; //nothing to run
                            break;
                        }

                        //how much recs should be inserted into queue
                        int insCnt = Math.Min(freeQSlots, job.Iterations - taskToQ.StartCount);
                        freeQSlots -= insCnt;
                        for (int i = 0; i < insCnt; i++)
                        {
                            cq.Enqueue(taskToQ.STask);
                        }

                        taskToQ.StartCount += insCnt;

                    }
                }
                //aggregate res
                while (cResq.TryDequeue(out var result))
                {
                    rTaskProgress sProgress = taskProgress.First(x => x.STask == result.Key);
                    job.Aggregate(sProgress,result.Value);
                    sProgress.EndCount++;
                }
               
                try
                {
                    Thread.Sleep(10);
                }
                catch (ThreadInterruptedException e)
                {
                    StopChilds();
                    Console.WriteLine("thread interrupted");
                    break;

                }
            }



            StopChilds();


        }


        public AggregateThread(ThreadJob pJob, int pChildThdCount)
        {
            job = pJob;
            childThdCount = pChildThdCount;
            mainThread = new Thread(DoWork);
        }

        public void Start()
        {
            mainThread.Start(job);
        }

        public void Interrupt()
        {
            mainThread.Interrupt();
        }

        //return number of completed sims
        public int Progress()
        {
            return taskProgress.Sum(x => x.EndCount);
        }
    }
}
