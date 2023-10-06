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
        private SimCls sim = null;

        public CallBackRender CbRend { get => cbRend; set => cbRend = value; }

        public SimCls Sim { get => sim; set => sim = value; }
        /// <summary>
        /// Load and parse xml file with scenario
        /// </summary>
        /// <param name="selectedPath">file path to file</param>
        public void LoadScenarioFromXml(string SelectedPath)
        {
            Sim = XMLLoader.LoadCombatFromXml(SelectedPath);
            LogText("Scenario  " + Sim.CurrentScenario.Name + " was loaded");
            DoSomething();
        }

        /// <summary>
        /// Execute the technique
        /// </summary>
        public void ExecuteTechnique(Step step, Ability ability)
        {
            step.Events.AddRange(ability.Events);
            step.Actor = ability.Parent;
            step.ActorAbility= ability;
            step.StepType = StepTypeEnm.TechniqueUse;
        }

        /// <summary>
        /// Proc all events in step
        /// </summary>
        public void ProcEvents(bool rewind = false)
        {
            //Combat.BeforeStartQueue.Add(ability);
        }

        //Cast all techniques before fights starts
        public void TechniqueWork(Step step)
        {
            List<Ability> abilities = new List<Ability>();
            //gather all abilities from party alive members
            foreach (Unit unit in sim.Party.Where(partyMember => partyMember.IsAlive))
            {
                abilities.AddRange(unit.Abilities.Where(ability => ability.AbilityType==Ability.AbilityTypeEnm.Technique));
            }
            //Use techniques starts from non combat
            foreach (Ability ability in abilities)//TODO: order by non combat
            {
                
                //TODO: choose technique by conditions
                ExecuteTechnique(step,ability);
            }

            return;
         
        }

        /// <summary>
        /// Do next step by logic priority
        /// </summary>
        public void DoSomething()
        {
            Step newStep= new Step();
            //TODO rewind, scheck existed step when next 
            if (Sim.CurrentStep==null)
            {
                Sim.Prepare();
                newStep.StepType = StepTypeEnm.SimInit;
            }
            //buff before fight
            if (newStep.StepType == StepTypeEnm.Iddle)  TechniqueWork(newStep);


            if (newStep.StepType != StepTypeEnm.Iddle)
            {
                Sim.CurrentStep = newStep;
                Sim.Steps.Add(Sim.CurrentStep);
                //TODO: proc the event steps
                LogStepDescription(newStep);
                
            }            
            else 
                LogText(" nothing to do...");

            DrawCombat();

        }

        private void LogStepDescription(Step step)
        {
            string OutText = " Step# " + Sim.Steps.IndexOf(Sim.CurrentStep).ToString() +" ["+ step.StepType.ToString()+ "] ";
            if (step.StepType == StepTypeEnm.SimInit)
                OutText = OutText + "summulation was initialized";
            else if (step.StepType == StepTypeEnm.TechniqueUse)
                OutText = OutText + step.Actor.Name+" used "+step.ActorAbility.Name;
            else
                throw new NotImplementedException();
            LogText(OutText);
            
        }


        /// <summary>
        /// Draw combat in client
        /// </summary>
        private void DrawCombat()
        {
            if (CbRend != null)
                CbRend(GraphicsCls.RenderCombat(Sim));
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
