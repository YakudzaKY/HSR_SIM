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
    /// It is System.Threading wrapper
    /// </summary>
    internal class AggregateThread
    {
        private Thread mainThread;//ref to thread
        private ThreadJob job;// ref to job( job= SimTask+ Results
        private int childThdCount;//child thread count
        private ConcurrentQueue<SimTask> cq = new ConcurrentQueue<SimTask>();//Task queue. Child threads will grab task from here
        private ConcurrentQueue<KeyValuePair<SimTask, Worker.RCombatResult>> cResq = new();//Child thread will insert results into this queue. Main thread will consume it

        public record rTaskProgress
        {
            public SimTask STask { get; init; }
            public int StartCount { get; set; } = 0;
            public int EndCount { get; set; } = 0;

        }
        private List<rTaskProgress> taskProgress = new();//task progression tracker

        List<SimThread> threads = new List<SimThread>();

        public bool IsAlive => mainThread.IsAlive;

        private bool HaveTaskToWait()
        {
            return taskProgress.Any(x => x.EndCount < job.Iterations);
        }

        /// <summary>
        /// Interrupt all child threads
        /// </summary>
        private void StopChild()
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
            ThreadJob thd = taskList as ThreadJob;
            //fill progress tracker
            foreach (var task in thd.TaskList)
            {
                taskProgress.Add(new rTaskProgress() { STask = task });
            }

            //create child threads
            for (var i = 0; i < childThdCount; i++)
            {
                SimThread sim = new SimThread(cq, cResq);
                threads.Add(sim);
            }

            //while we have tasks that are not running, or tasks that we are waiting for completion
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
                    StopChild();
                    Console.WriteLine("thread interrupted");
                    break;

                }
            }
            StopChild();
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

        //return number of completed sims. Can use it for progress bar or other visual control
        public int Progress()
        {
            return taskProgress.Sum(x => x.EndCount);
        }
    }
}
