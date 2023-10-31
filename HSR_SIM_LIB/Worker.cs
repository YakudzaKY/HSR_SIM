﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static HSR_SIM_LIB.Utils.CallBacks;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ImageMagick;
using static HSR_SIM_LIB.Utils.Constant;
using static HSR_SIM_LIB.TurnBasedClasses.Step;
using System.Net.Mail;
using static HSR_SIM_LIB.TurnBasedClasses.Events.Event;
using static HSR_SIM_LIB.UnitStuff.Resource;
using System.Resources;
using System.Drawing;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Worker;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Main lib class
    /// </summary>
    public class Worker
    {


        CallBacks.CallBackStr cbLog;
        public CallBacks.CallBackStr CbLog { get => cbLog; set => cbLog = value; }//Calback log procedure. Used for output

        CallBacks.CallBackRender cbRend;
        public CallBacks.CallBackRender CbRend { get => cbRend; set => cbRend = value; }//Callback render procedure. used for graphical output
        private SimCls sim = null;
        public SimCls Sim { get => sim; set => sim = value; }//simulation class( combat ,fights etc in this shit)
        public bool Completed { get => completed; set => completed = value; }
        private bool replay = false;//is replay not new gen
        private bool completed = false;


        //TODO вообще надо сделать на старте выбор списка сценариев и количество итераций для каждого
        //далее в несколько потоков собрать справочник СЦЕНАРИЙ:Результаты(агрегировать при выполнении каждой итерации)
        //Графическая оболочка только для отладки
        /// <summary> 
        /// Load and parse xml file with scenario
        /// </summary>
        /// <param name="selectedPath">file path to file</param>
        public void LoadScenarioFromXml(string scenarioPath, string profilePath)
        {
            Init();
            Sim = XMLLoader.LoadCombatFromXml(scenarioPath, profilePath);
            Sim.Parent = this;
            LogText("Scenario  " + Sim.CurrentScenario.Name + " was loaded");
        }

        
        public record RCombatResult
        {
            public bool? Success;
            public int Cycles = 0;
            public double TotalAv = 0;
            public List<RCombatant> Combatants =new List<RCombatant>();

        }

        public record RCombatant
        {
            public string CombatUnit;
            public Dictionary<Type, double> Damages;
        }
        public RCombatResult GetCombatResult(RCombatResult inRslt=null)
        {
            RCombatResult res = inRslt??new RCombatResult();
            MoveStep(false, -1);
            if (sim.PartyTeam.Units.Any(x => x.IsAlive))
            {
                //fill combatants
                foreach (Unit unit in sim.PartyTeam.Units)
                {
                    RCombatant combatant=new RCombatant();
                    combatant.CombatUnit = unit.Name;
                    combatant.Damages= new Dictionary<Type, double>
                    {
                        {
                            typeof(DirectDamage),
                            sim.Steps.Sum(x =>
                                x.Events.Where(y =>
                                    y is DirectDamage && y.SourceUnit == unit &&
                                    unit.Friends.All(j => j != y.TargetUnit)).Sum(y => y.Val ?? 0))
                        },
                        {
                            typeof(ShieldBreak),
                            sim.Steps.Sum(x =>
                                x.Events.Where(y =>
                                    y is ShieldBreak && y.SourceUnit == unit &&
                                    unit.Friends.All(j => j != y.TargetUnit)).Sum(y => y.Val ?? 0))
                        },
                        { typeof(DoTDamage), sim.Steps.Sum(x => x.Events.Where(y=>y is DoTDamage and not  BreakShieldDoTDamage && y.SourceUnit==unit && unit.Friends.All(j => j != y.TargetUnit)).Sum(y=>y.Val??0)) },
                        { typeof(BreakShieldDoTDamage), sim.Steps.Sum(x => x.Events.Where(y=>y is BreakShieldDoTDamage&&y.SourceUnit==unit&& unit.Friends.All(j => j != y.TargetUnit)).Sum(y=>y.Val??0)) }
                    };

                    res.Combatants.Add(combatant);
                }

                res.TotalAv = sim.TotalAv;
                res.Cycles = sim.SpecialTeam.Units.FirstOrDefault(x => x.Name == "Forgotten Hall").Level;
                res.Success = true;
            }
            else
            {
                res.Success = false;
            }

            return res;
        }
        /// <summary>
        /// GoNextStep or go back
        /// </summary>
        public void MoveStep(bool goBack = false, int stepcount = 1, bool forceNewSteps = false)
        {
            int stepndx = sim?.steps?.IndexOf(sim.CurrentStep) ?? 0;
            Step oldStep = sim?.CurrentStep;
            //delete future steps
            if (forceNewSteps && sim != null && sim.steps?.Count()>1)
            {
                for (int i = sim?.steps.Count()??0 ; i > stepndx+1; i--)
                {
                    sim.steps[i-1] = null;
                    sim.steps.Remove(sim.steps[i-1]);
                    
                }
            }
            if (goBack)
            {
                for (int i = 0; i < stepcount || stepcount == -1; i++)
                {

                    if (stepndx <= 0)
                    {
                        break;
                    }
                    //revert first
                    replay = true;
                    LogStepDescription(sim.CurrentStep, true);
                    sim.CurrentStep.ProcEvents(true, true);
                    stepndx -= 1;
                    sim.CurrentStep = sim.steps[stepndx];


                }

            }
            else//go forward
            {
                for (int i = 0; i < stepcount || stepcount == -1; i++)
                {
                    stepndx += 1;
                    if (sim?.steps.Count >= stepndx + 1)
                    {
                        replay = true;
                        sim.CurrentStep = sim.steps[stepndx];
                        sim.steps[stepndx].ProcEvents(false, true);
                        LogStepDescription(sim.steps[stepndx]);
                    }
                    else
                    {
                        if (!Completed)
                        {
                            replay = false;
                            if (sim == null)
                            {
                                LogText("Load scenario first!!!");
                                return;
                            }
                            Step newStep = sim.WorkIteration();





                            sim.CurrentStep = newStep;
                            LogStepDescription(newStep);

                            if (newStep.StepType == StepTypeEnm.Idle)
                            {
                                Completed = true;
                                LogText("scenario complete.");
                            }


                            //if no changes at step then scenario completed
                            if (stepndx > 0 && sim?.CurrentStep == sim?.steps[stepndx - 1])
                            {
                                break;
                            }
                        }
                        else
                            break;
                    }


                }
            }
            if (sim?.CurrentStep != oldStep)
            {
                DrawCombat();
            }
        }



        /// <summary>
        /// output text description of completed step
        /// </summary>
        /// <param name="step">completed step</param>
        /// <param name="revert">we revert this step? </param>
        /// <exception cref="NotImplementedException"></exception>
        private void LogStepDescription(Step step, bool revert = false)
        {
            string OutText = "===================================\n";
             OutText =OutText+ " Step# " + Sim.Steps.IndexOf(Sim.CurrentStep).ToString() + " [" + step.StepType.ToString() + "] " + step.GetDescription();
            if (revert)
                OutText = "reverted: " + OutText;
            else if (replay)
                OutText = "reproduced: " + OutText;
            LogText(OutText);

            foreach (Event ent in step.Events)
            {
                if (String.IsNullOrEmpty(ent.GetDescription()))
                    continue;
                OutText = " * " + ent.GetDescription();
                if (revert)
                    OutText = "reverted: " + OutText;
                else if (replay)
                    OutText = "reproduced: " + OutText;
                LogText(OutText);
                if (ent is ModEventTemplate)
                if (((ModEventTemplate)ent).Modification != null)
                {
                    OutText = " * " + ((ModEventTemplate)ent).Modification.GetDescription();
                    LogText(OutText);
                }


            }

        }


        /// <summary>
        /// Draw combat in client
        /// </summary>
        public void DrawCombat()
        {
            if (CbRend != null)
            {
                Bitmap render = GraphicsCls.RenderCombat(Sim, replay);
                CbRend(render);
                render.Dispose();

            }
        }


        /// <summary>
        /// wrapper for Text callback using for log output
        /// </summary>
        /// <param name="msg">message to print</param>
        private void LogText(string msg)
        {
            CbLog?.Invoke(new KeyValuePair<string, string>(Constant.MsgLog, msg));
        }

        /// <summary>
        /// wrapper for Text callback using for log output
        /// </summary>
        /// <param name="msg">message to print</param>
        public void LogDebug(string msg)
        {
            CbLog?.Invoke(new KeyValuePair<string, string>(Constant.MsgDebug, msg));
        }



        public void Init()
        {
            Completed = false;
            LogText("lib loaded");
        }


    }
}
