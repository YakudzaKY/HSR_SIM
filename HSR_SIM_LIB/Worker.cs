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
using System.Runtime.Remoting.Contexts;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ImageMagick;
using static HSR_SIM_LIB.Constant;
using static HSR_SIM_LIB.Step;
using System.Net.Mail;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Main lib class
    /// </summary>
    public class Worker
    {


        CallBackStr cbLog;
        public CallBackStr CbLog { get => cbLog; set => cbLog = value; }

        CallBackRender cbRend;
        private CombatCls combat = null;

        public CallBackRender CbRend { get => cbRend; set => cbRend = value; }

        public CombatCls Combat { get => combat; set => combat = value; }
        /// <summary>
        /// Load and parse xml file with scenario
        /// </summary>
        /// <param name="selectedPath">file path to file</param>
        public void LoadScenarioFromXml(string SelectedPath)
        {
            Combat = XMLLoader.LoadCombatFromXml(SelectedPath);
            LogText("Scenario name: " + Combat.CurrentScenario.Name + " loaded");
            DoSomething();
        }

        public void BuffTeam(Step step)
        {

        }

        /// <summary>
        /// Do next step by logic priority
        /// </summary>
        public void DoSomething()
        {
            Step newStep= new Step();
            //TODO rewind, scheck existed step when next 
            if (Combat.CurrentStep==null)
            {
                Combat.Prepare();
                newStep.StepType = StepTypeEnm.CombatInit;
            }
            //buff before fight
            if (newStep.StepType != StepTypeEnm.Iddle)  BuffTeam(newStep);


            if (newStep.StepType != StepTypeEnm.Iddle)
            {
                Combat.CurrentStep = newStep;
                Combat.Steps.Add(Combat.CurrentStep);
                LogText(" Step# " + Combat.Steps.IndexOf(Combat.CurrentStep).ToString() + " executed");
                
            }            
                LogText(" nothing to do...");

            DrawCombat();

        }


        /// <summary>
        /// Draw combat in client
        /// </summary>
        private void DrawCombat()
        {
            if (CbRend != null)
                CbRend(GraphicsCls.RenderCombat(Combat));
        }        
             

        /// <summary>
        /// wrapper for Text callback using for log output
        /// </summary>
        /// <param name="msg">message to print</param>
        private void LogText(string msg)
        {
            if (CbLog!= null)
                CbLog  (new KeyValuePair<string, string>(Constant.MsgLog, msg));
        }


        public  Worker()
        {            
         

        }

        public void Init()
        {
            LogText("lib loaded");
        }
    }
}
