using System.Collections.Concurrent;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_LIB;

namespace HSR_SIM_CLIENT.ThreadTools;

internal class SimThread
{
    /// <summary>
    ///     wrapper for System.Threading.
    ///     Child thread for Sim run by task and return results
    /// </summary>
    private readonly ConcurrentQueue<SimTask> qRef; //ref to task queue

    private readonly ConcurrentQueue<KeyValuePair<SimTask, Worker.RCombatResult>> rRef; //ref to results queue
    private readonly Thread thread;

    public SimThread(ConcurrentQueue<SimTask> cq,
        ConcurrentQueue<KeyValuePair<SimTask, Worker.RCombatResult>> keyValuePairs)
    {
        qRef = cq;
        rRef = keyValuePairs;
        thread = new Thread(DoWork);
        thread.Start();
    }

    //handler for DevMode
    public static int GetDecision(string[] items, string description)
    {
        return 0;
    }

    public void DoWork()
    {
        while (true)
        {
            //get some thing from queue and start sim/
            if (qRef.TryDequeue(out var task))
            {
                var wrk = new Worker();
                wrk.DevMode = task.DevMode;
                if (wrk.DevMode)
                    wrk.CbGetDecision += GetDecision;
                wrk.LoadScenarioFromSim(task.SimScenario, task.DevLogPath);
                wrk.ApplyModes(task.StatMods);
                var combatRes = new Worker.RCombatResult();
                wrk.GetCombatResult(combatRes);
                rRef.Enqueue(new KeyValuePair<SimTask, Worker.RCombatResult>(task, combatRes));
            }


            try
            {
                Thread.Sleep(0);
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine("thread interrupted");
                break;
            }
        }
    }


    public void Interrupt()
    {
        thread.Interrupt();
    }
}