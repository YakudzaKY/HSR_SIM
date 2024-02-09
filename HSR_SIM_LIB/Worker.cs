using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Utils.CallBacks;
using static HSR_SIM_LIB.Utils.Constant;
using static HSR_SIM_LIB.TurnBasedClasses.Step;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB;

/// <summary>
///     Main lib class
/// </summary>
public class Worker
{
    private bool replay; //is replay not new gen
    public CallBackStr CbLog { get; set; } //Calback log procedure. Used for output
    public CallBackRender CbRend { get; set; } //Callback render procedure. used for graphical output
    public CallBackGetDecision CbGetDecision { get; set; } //Callback get decision
    public SimCls Sim { get; set; } //simulation class( combat ,fights etc in this shit)
    public bool Completed { get; set; }
    public bool DevMode { get; set; } = false;//Developer mode TODO: write documentation about it
    public DevModeLogger DevModeLog { get; set; }


    /// <summary>
    ///     Load and parse xml file with scenario
    /// </summary>
    /// <param name="selectedPath">file path to file</param>
    public void LoadScenarioFromXml(string scenarioPath, string profilePath)
    {
        Init();
        Sim = XMLLoader.LoadCombatFromXml(scenarioPath, profilePath);
        Sim.Parent = this;
        LogText("Scenario  " + Sim.CurrentScenario.Name + " was loaded");
        if (DevMode)
            DevModeLog = new DevModeLogger(DevModeUtils.GetDevLogPath(scenarioPath, profilePath), this);
    }

    
    
    public void LoadScenarioFromSim(SimCls sim,string devLogPath)
    {
        Init();
        Sim = (SimCls)sim.Clone();
        Sim.Parent = this;
        LogText("Scenario  " + Sim.CurrentScenario.Name + " was loaded");
        if (DevMode)
            DevModeLog = new DevModeLogger(devLogPath, this);
    }



    public RCombatResult GetCombatResult(RCombatResult inRslt = null)
    {
        var res = inRslt ?? new RCombatResult();
        MoveStep(false, -1);
        if (Sim.PartyTeam.Units.Any(x => x.IsAlive))
        {
            //fill combatants
            foreach (var unit in Sim.PartyTeam.Units)
            {
                var combatant = new RCombatant();
                combatant.CombatUnit = unit.Name;
                combatant.Damages = new Dictionary<Type, double>
                {
                    {
                        typeof(DirectDamage),
                        Sim.Steps.Sum(x =>
                            x.Events.Where(y =>
                                y is DirectDamage && y.SourceUnit == unit &&
                                unit.Friends.All(j => j != y.TargetUnit)).Sum(y => y.Val ?? 0))
                    },
                    {
                        typeof(ToughnessBreak),
                        Sim.Steps.Sum(x =>
                            x.Events.Where(y =>
                                y is ToughnessBreak && y.SourceUnit == unit &&
                                unit.Friends.All(j => j != y.TargetUnit)).Sum(y => y.Val ?? 0))
                    },
                    {
                        typeof(DoTDamage),
                        Sim.Steps.Sum(x =>
                            x.Events.Where(y =>
                                y is DoTDamage and not ToughnessBreakDoTDamage && y.SourceUnit == unit &&
                                unit.Friends.All(j => j != y.TargetUnit)).Sum(y => y.Val ?? 0))
                    },
                    {
                        typeof(ToughnessBreakDoTDamage),
                        Sim.Steps.Sum(x =>
                            x.Events.Where(y =>
                                y is ToughnessBreakDoTDamage && y.SourceUnit == unit &&
                                unit.Friends.All(j => j != y.TargetUnit)).Sum(y => y.Val ?? 0))
                    }
                };

                res.Combatants.Add(combatant);
            }

            res.TotalAv = Sim.TotalAv;
            res.Cycles = Sim.SpecialTeam.Units.FirstOrDefault(x => x.Name == "Forgotten Hall").Level;
            res.Success = true;
        }
        else
        {
            res.Cycles = Sim.SpecialTeam.Units.FirstOrDefault(x => x.Name == "Forgotten Hall").Level;
            res.Success = false;
        }

        return res;
    }

    /// <summary>
    ///     GoNextStep or go back
    /// </summary>
    public void MoveStep(bool goBack = false, int stepcount = 1, bool forceNewSteps = false)
    {
        var stepndx = Sim?.steps?.IndexOf(Sim.CurrentStep) ?? 0;
        var oldStep = Sim?.CurrentStep;
        //delete future steps
        if (forceNewSteps && Sim != null && Sim.steps?.Count() > 1)
            for (var i = Sim?.steps.Count() ?? 0; i > stepndx + 1; i--)
            {
                Sim.steps[i - 1] = null;
                Sim.steps.Remove(Sim.steps[i - 1]);
            }

        if (goBack)
            for (var i = 0; i < stepcount || stepcount == -1; i++)
            {
                if (stepndx <= 0) break;
                //revert first
                replay = true;
                LogStepDescription(Sim.CurrentStep, true);
                Sim.CurrentStep.ProcEvents(true, true);
                stepndx -= 1;
                Sim.CurrentStep = Sim.steps[stepndx];
            }
        else //go forward
            for (var i = 0; i < stepcount || stepcount == -1; i++)
            {
                stepndx += 1;
                if (Sim?.steps.Count >= stepndx + 1)
                {
                    replay = true;
                    Sim.CurrentStep = Sim.steps[stepndx];
                    Sim.steps[stepndx].ProcEvents(false, true);
                    LogStepDescription(Sim.steps[stepndx]);
                }
                else
                {
                    if (!Completed)
                    {
                        replay = false;
                        if (Sim == null)
                        {
                            LogText("Load scenario first!!!");
                            return;
                        }

                        var newStep = Sim.WorkIteration();


                        Sim.CurrentStep = newStep;
                        LogStepDescription(newStep);

                        if (newStep.StepType == StepTypeEnm.Idle)
                        {
                            Completed = true;
                            LogText("scenario complete.");
                            DevModeLog?.WriteToFile();//write dev log 
                        }


                        //if no changes at step then scenario completed
                        if (stepndx > 0 && Sim?.CurrentStep == Sim?.steps[stepndx - 1]) break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

        if (Sim?.CurrentStep != oldStep) DrawCombat();
    }


    /// <summary>
    ///     output text description of completed step
    /// </summary>
    /// <param name="step">completed step</param>
    /// <param name="revert">we revert this step? </param>
    /// <exception cref="NotImplementedException"></exception>
    private void LogStepDescription(Step step, bool revert = false)
    {
        var OutText = "===================================\n";
        OutText = OutText + " Step# " + Sim.Steps.IndexOf(Sim.CurrentStep) + " [" + step.StepType + "] " +
                  step.GetDescription();
        if (revert)
            OutText = "reverted: " + OutText;
        else if (replay)
            OutText = "reproduced: " + OutText;
        LogText(OutText);

        foreach (var ent in step.Events)
        {
            if (string.IsNullOrEmpty(ent.GetDescription()))
                continue;
            OutText = " * " + ent.GetDescription();
            if (revert)
                OutText = "reverted: " + OutText;
            else if (replay)
                OutText = "reproduced: " + OutText;
            LogText(OutText);
            if (ent is BuffEventTemplate)
                if (((BuffEventTemplate)ent).BuffToApply != null)
                {
                    OutText = " * " + ((BuffEventTemplate)ent).BuffToApply.GetDescription();
                    LogText(OutText);
                }
        }
    }


    /// <summary>
    ///     Draw combat in client
    /// </summary>
    public void DrawCombat()
    {
        if (CbRend != null)
        {
            var render = GraphicsCls.RenderCombat(Sim, replay);
            CbRend(render);
            render.Dispose();
        }
    }


    /// <summary>
    ///     wrapper for Text callback using for log output
    /// </summary>
    /// <param name="msg">message to print</param>
    private void LogText(string msg)
    {
        CbLog?.Invoke(new KeyValuePair<string, string>(MsgLog, msg));
    }

    /// <summary>
    ///     wrapper for Text callback using for log output
    /// </summary>
    /// <param name="msg">message to print</param>
    public void LogDebug(string msg)
    {
        CbLog?.Invoke(new KeyValuePair<string, string>(MsgDebug, msg));
    }


    public void Init()
    {
        Completed = false;
        LogText("lib loaded");
    }

    //Apply modes(stats??) to sim elements
    public void ApplyModes(List<RStatMod> taskStatMods)
    {
        if (taskStatMods == null)
            return;

        foreach (var mod in taskStatMods)
        {
            var targetUnit = Sim.CurrentScenario.Party.FirstOrDefault(x => string.Equals(x.Name, mod.Character));
            if (targetUnit != null)
            {
                if (mod.Stat == "spd_fix")
                {
                    targetUnit.Stats.SpeedFix += mod.Val;
                }
                else if (mod.Stat == "hp_fix")
                {
                    targetUnit.Stats.MaxHpFix += mod.Val;
                }
                else if (mod.Stat == "atk_fix")
                {
                    targetUnit.Stats.AttackFix += mod.Val;
                }
                else if (mod.Stat == "def")
                {
                    targetUnit.Stats.BaseDef += mod.Val;
                }
                else if (mod.Stat == "hp_prc")
                {
                    targetUnit.Stats.MaxHpPrc += mod.Val;
                }
                else if (mod.Stat == "atk_prc")
                {
                    targetUnit.Stats.AttackPrc += mod.Val;
                }
                else if (mod.Stat == "def_prc")
                {
                    targetUnit.Stats.DefPrc += mod.Val;
                }
                else if (mod.Stat == "break_dmg_prc")
                {
                    targetUnit.Stats.BreakDmgPrc += mod.Val;
                }
                else if (mod.Stat == "effect_hit_prc")
                {
                    targetUnit.Stats.EffectHitPrc += mod.Val;
                }
                else if (mod.Stat == "effect_res_prc")
                {
                    targetUnit.Stats.EffectResPrc += mod.Val;
                }
                else if (mod.Stat == "crit_rate_prc")
                {
                    targetUnit.Stats.BaseCritChance += mod.Val;
                }
                else if (mod.Stat == "crit_dmg_prc")
                {
                    targetUnit.Stats.BaseCritDmg += mod.Val;
                }
                else if (mod.Stat == "sp_rate_prc")
                {
                    targetUnit.Stats.BaseEnergyResPrc += mod.Val;
                }
                else if (mod.Stat.EndsWith("_dmg_prc"))
                {
                    var elem = mod.Stat.Split("_").First();
                    targetUnit.GetElemBoost((ElementEnm)Enum.Parse(typeof(ElementEnm), elem, true)).Value += mod.Val;
                }
            }
        }
    }

    //stat modififer

    public record RStatMod
    {
        public string Character { get; set; }
        public string Stat { get; set; }
        public double Val { get; set; }
  
    }

    public record RCombatResult
    {
        public List<RCombatant> Combatants = new();
        public int Cycles;
        public bool Success;
        public double TotalAv;
    }

    public record RCombatant
    {
        public string CombatUnit;
        public Dictionary<Type, double> Damages;
    }
}