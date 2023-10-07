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
            step.Events.AddRange(ability.Events);//copy events from ability reference
            if (ability.CostType != ResourceType.nil)
                step.Events.Add(new Event() { Type = EventType.PartyResourceDrain, ResType= ability.CostType, ResourceValue= ability.Cost }) ;//energy drain
            step.Actor = ability.Parent;//WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.Parent but can change in future)
            step.ActorAbility= ability;//WAT ABILITY is casting
            step.StepType = StepTypeEnm.TechniqueUse;
        }

        /// <summary>
        /// Proc all events in step
        /// </summary>
        public void ProcEvents(Step step,bool rewind = false)
        {
            //for all events saved in step
            foreach(Event ent in step.Events)
            {
                if (ent.Type == EventType.PartyResourceDrain)//SP or technical points
                {
                    Sim.GetRes(ent.ResType).ResVal -= 1;
                }
                else if (ent.Type == EventType.CombatStartSkillQueue)//party buffs or opening
                    sim.BeforeStartQueue.Add(step.ActorAbility);
                else if (ent.Type == EventType.EnterCombat)//entering combat
                    sim.DoEnterCombat = true;
                else
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Party have res to cast abilitiy
        /// if res not found then false
        /// </summary>
        /// <returns></returns>
        private bool PartyHaveRes(Ability ability)
        {
            foreach (Resource res in sim.Resources)
            {
                if (res.ResType==ability.CostType)                
                    return res.ResVal>=ability.Cost;                
            }
            return false;
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
            
            var orderedAbilities = from ability in abilities
                                  where PartyHaveRes(ability)
                                  orderby ability.Events.Exists(ent=> ent.Type== EventType.EnterCombat) ascending//non combat first
                                        //TODO add element order
                                        //TODO priority order
                                      , ability.Cost descending//start from Hight cost abilities
                                 select ability;            
            foreach (Ability ability in orderedAbilities)
            {

                //TODO: choose technique by conditions
                if (FullFiledConditions(ability))
                {
                    ExecuteTechnique(step, ability);
                    return;
                }
            }

            return;
         
        }
        /// <summary>
        /// Check conditions are ok for use ability
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool FullFiledConditions(Ability ability)
        {
            bool res = true;
            foreach (Condition condition in ability.ExecuteWhen)
            {

            }
            return res;
        }

        /// <summary>
        /// Do next step by logic priority
        /// </summary>
        public void DoSomething()
        {
            if (sim==null)
            {
                LogText("Load scenario first!!!");
                return;
            }    
            Step newStep= new Step();
            //TODO rewind, scheck existed step when next 
            if (Sim.CurrentStep==null)
            {
                //simulation preparations
                Sim.Prepare();
                newStep.StepType = StepTypeEnm.SimInit;
            }
            //buff before fight
            if (newStep.StepType == StepTypeEnm.Iddle&& sim.DoEnterCombat==false)  TechniqueWork(newStep);

            //enter the combat
            if (newStep.StepType == StepTypeEnm.Iddle && sim.DoEnterCombat == true) throw new NotImplementedException();

            //if we doing somethings then need proced the events
            if (newStep.StepType != StepTypeEnm.Iddle)
            {
                Sim.CurrentStep = newStep;
                Sim.Steps.Add(Sim.CurrentStep);
                LogStepDescription(newStep);
                ProcEvents(newStep);
            }            
            else 
                LogText(" nothing to do...");

            //render combat situitatuiom
            DrawCombat();

        }

        /// <summary>
        /// output text description of completed step
        /// </summary>
        /// <param name="step">completed step</param>
        /// <exception cref="NotImplementedException"></exception>
        private void LogStepDescription(Step step)
        {
            string OutText = " Step# " + Sim.Steps.IndexOf(Sim.CurrentStep).ToString() +" ["+ step.StepType.ToString()+ "] " + step.GetStepDescription();          
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


        public void Init()
        {
            LogText("lib loaded");
        }
    }
}
