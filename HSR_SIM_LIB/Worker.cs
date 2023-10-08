using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static HSR_SIM_LIB.CallBacks;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ImageMagick;
using static HSR_SIM_LIB.Constant;
using static HSR_SIM_LIB.Step;
using System.Net.Mail;
using static HSR_SIM_LIB.Event;
using static HSR_SIM_LIB.Resource;
using System.Resources;
using static HSR_SIM_LIB.Check;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Main lib class
    /// </summary>
    public class Worker
    {


        CallBackStr cbLog;
        public CallBackStr CbLog { get => cbLog; set => cbLog = value; }//Calback log procedure. Used for output

        CallBackRender cbRend;
        public CallBackRender CbRend { get => cbRend; set => cbRend = value; }//Callback render procedure. used for graphical output
        private SimCls sim = null;
        public SimCls Sim { get => sim; set => sim = value; }//simulation class( combat ,fights etc in this shit)
        private bool replay = false;//is replay not new gen

        //TODO вообще надо сделать на старте выбор списка сценариев и количество итераций для каждого
        //далее в несколько потоков собрать справочник СЦЕНАРИЙ:Результаты(агрегировать при выполнении каждой итерации)
        //Графическая оболочка только для отладки
        /// <summary>
        /// Load and parse xml file with scenario
        /// </summary>
        /// <param name="selectedPath">file path to file</param>
        public void LoadScenarioFromXml(string SelectedPath)
        {
            Sim = XMLLoader.LoadCombatFromXml(SelectedPath);
            LogText("Scenario  " + Sim.CurrentScenario.Name + " was loaded");
            Rewind();
            DrawCombat();
        }

        /// <summary>
        /// Revert step
        /// </summary>
        public void Rewind(bool goBack = false, int stepcount = 1)
        {
            int stepndx = sim?.steps?.IndexOf(sim.CurrentStep) ?? 0;
            Step oldStep = sim?.CurrentStep;

            if (goBack)
            {
                for (int i = 0; i < stepcount; i++)
                {

                    if (stepndx <= 0)
                    {
                        break;
                    }
                    //revert first
                    replay = true;
                    LogStepDescription(sim.CurrentStep, true);
                    sim.CurrentStep.ProcEvents(true);
                    stepndx -= 1;
                    sim.CurrentStep = sim.steps[stepndx];


                }

            }
            else//go forward
            {
                for (int i = 0; i < stepcount; i++)
                {
                    stepndx += 1;
                    if (sim?.steps.Count >= stepndx + 1)
                    {
                        replay = true;
                        sim.CurrentStep = sim.steps[stepndx];
                        sim.steps[stepndx].ProcEvents();
                        LogStepDescription(sim.steps[stepndx]);
                    }
                    else
                    {
                        replay = false;
                        if (sim == null)
                        {
                            LogText("Load scenario first!!!");
                            return;
                        }
                        Step newStep = sim.WorkIteration();
                        if (newStep.StepType != StepTypeEnm.Iddle)
                        {
                            sim.CurrentStep= newStep; 
                            LogStepDescription(newStep);
                        }

                        //if no changes at step then scenario completed
                        if (stepndx > 0 && sim?.CurrentStep == sim?.steps[stepndx - 1])
                        {
                            break;
                        }
                    }


                }
            }
            if (sim?.CurrentStep != oldStep)
            {
                DrawCombat();
            }
            else
                LogText("step has no changed");

        }



        /// <summary>
        /// output text description of completed step
        /// </summary>
        /// <param name="step">completed step</param>
        /// <param name="revert">we revert this step? </param>
        /// <exception cref="NotImplementedException"></exception>
        private void LogStepDescription(Step step, bool revert = false)
        {
            string OutText = " Step# " + Sim.Steps.IndexOf(Sim.CurrentStep).ToString() + " [" + step.StepType.ToString() + "] " + step.GetStepDescription();
            if (revert)
                OutText = "reverted: " + OutText;
            else if (replay)
                OutText = "reproduced: " + OutText;
            LogText(OutText);

        }


        /// <summary>
        /// Draw combat in client
        /// </summary>
        private void DrawCombat()
        {
            if (CbRend != null)
                CbRend(GraphicsCls.RenderCombat(Sim, replay));
        }


        /// <summary>
        /// wrapper for Text callback using for log output
        /// </summary>
        /// <param name="msg">message to print</param>
        private void LogText(string msg)
        {
            if (CbLog != null)
                CbLog(new KeyValuePair<string, string>(Constant.MsgLog, msg));
        }


        public void Init()
        {
            LogText("lib loaded");
        }
    }
}
