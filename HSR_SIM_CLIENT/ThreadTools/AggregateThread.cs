using System.Collections.Concurrent;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_LIB;

namespace HSR_SIM_CLIENT.ThreadTools;

/// <summary>
///     Main thread class for sim job
///     It is System.Threading wrapper
/// </summary>
internal class AggregateThread
{
    private const int QueueSlotMultiplier = 100;
    private readonly int childThreadCount;
    private readonly ThreadJob? job;
    private readonly Thread mainThread;
    private readonly List<TaskProgress> taskProgress = new();
    private readonly ConcurrentQueue<SimTask> taskQueue = new();
    private readonly ConcurrentQueue<KeyValuePair<SimTask, Worker.RCombatResult>> taskResultQueue = new();
    private readonly List<SimThread> threads = new();

    public AggregateThread(ThreadJob? pJob, int pChildThreadCount)
    {
        job = pJob;
        childThreadCount = pChildThreadCount;
        mainThread = new Thread(DoWork);
    }

    public bool IsAlive => mainThread.IsAlive;

    private bool HaveTaskToWait()
    {
        return taskProgress.Any(x => x.EndCount < job.Iterations);
    }

    private void StopChildThreads()
    {
        foreach (var thread in threads) thread.Interrupt();

        threads.Clear();
    }

    private void CreateChildThreads()
    {
        for (var i = 0; i < childThreadCount; i++)
        {
            var newThread = new SimThread(taskQueue, taskResultQueue);
            threads.Add(newThread);
        }
    }

    private bool QueueTasks(bool hasTasksToRun)
    {
        var freeQueueSlots = childThreadCount * QueueSlotMultiplier - taskQueue.Count;
        while (freeQueueSlots > 0)
        {
            var taskToQueue = taskProgress.FirstOrDefault(x => x.StartCount < job.Iterations);
            if (taskToQueue == null) return false;

            var insertCount = Math.Min(freeQueueSlots, job.Iterations - taskToQueue.StartCount);
            freeQueueSlots -= insertCount;
            for (var i = 0; i < insertCount; i++) taskQueue.Enqueue(taskToQueue.STask);

            taskToQueue.StartCount += insertCount;
        }

        return true;
    }

    private void ProcessTaskResults()
    {
        while (taskResultQueue.TryDequeue(out var result))
        {
            var sProgress = taskProgress.First(x => x.STask == result.Key);
            job.Aggregate(sProgress, result.Value);
            sProgress.EndCount++;
        }
    }

    private void DoWork(object taskList)
    {
        var hasTasksToRun = true;
        var threadJob = taskList as ThreadJob;

        foreach (var task in threadJob.TaskList) taskProgress.Add(new TaskProgress { STask = task });

        CreateChildThreads();

        while (hasTasksToRun || HaveTaskToWait())
        {
            if (hasTasksToRun) hasTasksToRun = QueueTasks(hasTasksToRun);
            ProcessTaskResults();
            try
            {
                Thread.Sleep(10);
            }
            catch (ThreadInterruptedException e)
            {
                StopChildThreads();
                break;
            }
        }

        StopChildThreads();
    }

    public void Start()
    {
        mainThread.Start(job);
    }

    public void Interrupt()
    {
        mainThread.Interrupt();
    }

    public int Progress()
    {
        return taskProgress.Sum(x => x.EndCount);
    }

    internal record TaskProgress
    {
        public SimTask STask { get; init; }
        public int StartCount { get; set; }
        public int EndCount { get; set; }
    }
}