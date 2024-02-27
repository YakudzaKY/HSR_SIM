using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HSR_SIM_GUI.TaskTools;
using HSR_SIM_LIB;

namespace HSR_SIM_GUI.ThreadTools;

/// <summary>
///     Main thread class for sim job
///     It is System.Threading wrapper
/// </summary>
internal class AggregateThread
{
    private readonly int childThdCount; //child thread count
    private readonly ConcurrentQueue<SimTask> cq = new(); //Task queue. Child threads will grab task from here

    private readonly ConcurrentQueue<KeyValuePair<SimTask, Worker.RCombatResult>>
        cResq = new(); //Child thread will insert results into this queue. Main thread will consume it

    private readonly ThreadJob job; // ref to job( job= SimTask+ Results
    private readonly Thread mainThread; //ref to thread
    private readonly List<rTaskProgress> taskProgress = new(); //task progression tracker

    private readonly List<SimThread> threads = new();


    public AggregateThread(ThreadJob pJob, int pChildThdCount)
    {
        job = pJob;
        childThdCount = pChildThdCount;
        mainThread = new Thread(DoWork);
    }

    public bool IsAlive => mainThread.IsAlive;

    private bool HaveTaskToWait()
    {
        return taskProgress.Any(x => x.EndCount < job.Iterations);
    }

    /// <summary>
    ///     Interrupt all child threads
    /// </summary>
    private void StopChild()
    {
        foreach (var thread in threads) thread.Interrupt();

        threads.Clear();
    }

    private void DoWork(object taskList)
    {
        var haveTaskToRun = true; //flag that we have some task to queue
        var thd = taskList as ThreadJob;
        //fill progress tracker
        foreach (var task in thd.TaskList) taskProgress.Add(new rTaskProgress { STask = task });

        //create child threads
        for (var i = 0; i < childThdCount; i++)
        {
            var sim = new SimThread(cq, cResq);
            threads.Add(sim);
        }

        //while we have tasks that are not running, or tasks that we are waiting for completion
        while (haveTaskToRun || HaveTaskToWait())
        {
            //start child jobs
            if (haveTaskToRun)
            {
                //have slots to insert queue
                var freeQSlots = childThdCount * 100 - cq.Count;
                while (freeQSlots > 0)
                {
                    //get first uncompleted task
                    var taskToQ = taskProgress.FirstOrDefault(x => x.StartCount < job.Iterations);
                    // if no task then break
                    if (taskToQ == null)
                    {
                        haveTaskToRun = false; //nothing to run
                        break;
                    }

                    //how much recs should be inserted into queue
                    var insCnt = Math.Min(freeQSlots, job.Iterations - taskToQ.StartCount);
                    freeQSlots -= insCnt;
                    for (var i = 0; i < insCnt; i++) cq.Enqueue(taskToQ.STask);

                    taskToQ.StartCount += insCnt;
                }
            }

            //aggregate res
            while (cResq.TryDequeue(out var result))
            {
                var sProgress = taskProgress.First(x => x.STask == result.Key);
                job.Aggregate(sProgress, result.Value);
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

    public record rTaskProgress
    {
        public SimTask STask { get; init; }
        public int StartCount { get; set; }
        public int EndCount { get; set; }
    }
}