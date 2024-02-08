using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HSR_SIM_GUI.TaskTools;
using HSR_SIM_LIB;

namespace HSR_SIM_GUI.ThreadTools
{
    internal class SimThread
    {
        private ConcurrentQueue<SimTask> qRef;
        private  ConcurrentQueue<KeyValuePair<SimTask, Worker.RCombatResult>> rRef;
        private Thread thread;
        public SimThread(ConcurrentQueue<SimTask> cq,
            ConcurrentQueue<KeyValuePair<SimTask, Worker.RCombatResult>> keyValuePairs)
        {
            qRef = cq;
            rRef=keyValuePairs;
            thread=new Thread(DoWork);
            thread.Start();
        }
        public static int GetDecision(string[] items, string description)
        {
            throw new NotImplementedException();
        }
        public void DoWork()
        {
            while (true)
            {
                if (qRef.TryDequeue(out var task))
                {
                    var wrk = new Worker();
                    wrk.DevMode = task.DevMode;
                    if (wrk.DevMode)
                        wrk.CbGetDecision += GetDecision;
                    wrk.LoadScenarioFromXml(task.Scenario, task.Profile);
                    wrk.ApplyModes(task.StatMods);
                    Worker.RCombatResult combatRes=new Worker.RCombatResult();
                    wrk.GetCombatResult(combatRes);
                    rRef.Enqueue(new KeyValuePair<SimTask, Worker.RCombatResult>(task,combatRes));
                }

                try
                {
                    Thread.Sleep(10);
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
}
