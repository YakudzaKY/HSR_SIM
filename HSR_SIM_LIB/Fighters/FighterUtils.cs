using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.TurnBasedClasses.Event;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Fighters
{
    /// <summary>
    /// ultils
    /// </summary>
    public static class FighterUtils
    {
        private static Dictionary<int, double> _lvlMultiplier;
        /// <summary>
        /// Calc dmg when shield broken
        /// https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
        /// </summary> 
        /// <param name="ent"></param>
        /// <returns></returns>
        public static double? CalculateShieldBrokeDmg( Event ent)
        {
            Unit attacker = ent.ParentStep.Actor;
            Unit defender = ent.TargetUnit;
            Unit.ElementEnm attackElem=ent.ParentStep.ActorAbility.Element??attacker.Fighter.Element;

            ent.ParentStep.Parent.Parent?.LogDebug("=======================");
            ent.ParentStep.Parent.Parent?.LogDebug($"{attacker.Name:s} ({attacker.ParentTeam.Units.IndexOf(attacker)+1:d}) Break shield of {defender.Name} ({defender.ParentTeam.Units.IndexOf(defender)+1:d})");
            double baseDmg;
            //if this is direct shield break
            if (ent.Type == (EventType.ShieldBreak))
            {
                double maxToughnessMult = 0.5 + (double)defender.Stats.MaxToughness / 120;
                
                switch (attackElem)
                {
                    case ElementEnm.Physical:
                        baseDmg = 2 * _lvlMultiplier[attacker.Level] * maxToughnessMult;
                        break;
                    case ElementEnm.Fire:
                        baseDmg = 2 * _lvlMultiplier[attacker.Level] * maxToughnessMult;
                        break;
                    case ElementEnm.Ice:
                        baseDmg = 1 * _lvlMultiplier[attacker.Level] * maxToughnessMult;
                        break;
                    case ElementEnm.Lightning:
                        baseDmg = 1 * _lvlMultiplier[attacker.Level] * maxToughnessMult;
                        break;
                    case ElementEnm.Wind:
                        baseDmg = 1.5 * _lvlMultiplier[attacker.Level] * maxToughnessMult;
                        break;
                    case ElementEnm.Quantum:
                        baseDmg = 0.5 * _lvlMultiplier[attacker.Level] * maxToughnessMult;
                        break;
                    case ElementEnm.Imaginary:
                        baseDmg = 0.5 * _lvlMultiplier[attacker.Level] * maxToughnessMult;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                //shield breake DOT
                Mod dot;
                baseDmg = 0;
            }

            double breakEffect = 1 + attacker.Stats.BreakDmg;
            double def = defender.Stats.Def;
            double defWithIgnore = def*(1-attacker.DefIgnore(ent));
            double defMultiplier=1-(defWithIgnore/(defWithIgnore+200+(10*attacker.Level)));
            double resPen = 1-(defender.GetResists(attackElem,ent)-attacker.ResistsPenetration(attackElem,ent));
            double vulnMult = 1 + defender.GetElemVulnerability(attackElem,ent)+ defender.GetAllDamageVulnerability(ent);
            double brokenMultiplier =  defender.GetBrokenMultiplier();
            double totalDmg = baseDmg * breakEffect  * defMultiplier * resPen * vulnMult * brokenMultiplier;
            ent.ParentStep.Parent.Parent?.LogDebug($"baseDmg({baseDmg:f}) ; breakEffect({breakEffect:f})");
            ent.ParentStep.Parent.Parent?.LogDebug($"Def {def:f} -> ignored to {defWithIgnore:f} ");
            ent.ParentStep.Parent.Parent?.LogDebug($"defMultiplier({defMultiplier:f}) = 1-(defender.Stats.Def({defWithIgnore:f})/(defender.Stats.Def({defWithIgnore:f})+200+(10*attacker.Level({attacker.Level:d}))))");
            ent.ParentStep.Parent.Parent?.LogDebug($"resPen= {resPen:f} ; vulnMult= {vulnMult:f} ; brokenMultiplier= {brokenMultiplier:f}" );
            ent.ParentStep.Parent.Parent?.LogDebug($"TOTAL DAMAGE= {totalDmg:f}");
            return totalDmg;
        }

        /// <summary>
        /// calc real value by stats
        ///  https://honkai-star-rail.fandom.com/wiki/Damage
        /// </summary>
        /// <param name="baseDmg"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static double CalculateBasicDmg(double baseDmg,Event ent)
        {
            Unit attacker = ent.ParentStep.Actor;
            Unit defender = ent.TargetUnit;

            Unit.ElementEnm attackElem=ent.ParentStep.ActorAbility.Element??attacker.Fighter.Element;

            //crit multiplier
            double critMultiplier =1;
            double dotMultiplier=0;
            double dotVulnerability=0;
            if (ent.Type == Event.EventType.DirectDamage  )
            {
                ent.IsCrit = new MersenneTwister().NextDouble()<=attacker.Stats.CritChance;
                if (ent.IsCrit )
                    critMultiplier = 1 + attacker.Stats.CritDmg;
            }
            else
            {
                dotMultiplier = attacker.DotBoost(ent);
                dotVulnerability = defender.GetDotVulnerability(ent);
            }
            
           
            double damageBoost = 1 
                                 + attacker.GetElemBoostValue(attackElem,ent)
                                 + attacker.AllDmgBoost(ent)
                                 + dotMultiplier
                                 ;
            double abilityTypeMultiplier = attacker.GetAbilityTypeMultiplier(ent.AbilityValue,ent);
            double def = defender.Stats.Def;
            double defWithIgnore = def*(1-attacker.DefIgnore(ent));
            double defMultiplier=1-(defWithIgnore/(defWithIgnore+200+(10*attacker.Level)));

            double resPen = 1-(defender.GetResists(attackElem,ent)-attacker.ResistsPenetration(attackElem,ent));

            double vulnMult = 1 + defender.GetElemVulnerability(attackElem,ent)+ defender.GetAllDamageVulnerability(ent)+dotVulnerability;

            double dmgReduction = defender.GetDamageReduction(defender);

            double brokenMultiplier = defender.GetBrokenMultiplier();

            double totalDmg = baseDmg *abilityTypeMultiplier* critMultiplier * damageBoost * defMultiplier * resPen * vulnMult *
                              dmgReduction * brokenMultiplier;
            ent.ParentStep.Parent.Parent?.LogDebug("=======================");
            ent.ParentStep.Parent.Parent?.LogDebug($"baseDmg={baseDmg:f}");
            ent.ParentStep.Parent.Parent?.LogDebug($"{attacker.Name:s} ({attacker.ParentTeam.Units.IndexOf(attacker)+1:d}) Damaging {defender.Name} ({defender.ParentTeam.Units.IndexOf(defender)+1:d})");
            ent.ParentStep.Parent.Parent?.LogDebug($"damageBoost({damageBoost:f}) = 1+ GetElemBoostValue({attacker.GetElemBoostValue(attackElem,ent):f})  +AllDmgBoost({attacker.AllDmgBoost(ent):f}) + dotMultiplier({dotMultiplier:f})");
            ent.ParentStep.Parent.Parent?.LogDebug($"Def {def:f} -> ignored to {defWithIgnore:f} ");
            ent.ParentStep.Parent.Parent?.LogDebug($"defMultiplier({defMultiplier:f}) = 1-(defender.Stats.Def({defWithIgnore:f})/(defender.Stats.Def({defWithIgnore:f})+200+(10*attacker.Level({attacker.Level:d}))))");
            ent.ParentStep.Parent.Parent?.LogDebug($"resPen= {resPen:f} ; vulnMult= {vulnMult:f} ; critMultiplier={critMultiplier:f} ; dmgReduction= {dmgReduction:f} ; brokenMultiplier= {brokenMultiplier:f} ; abilityTypeMultiplier {abilityTypeMultiplier:f}" );
            ent.ParentStep.Parent.Parent?.LogDebug($"TOTAL DAMAGE= {totalDmg:f}");
            return totalDmg;
        }
        /// <summary>
        /// get from table in https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
        /// </summary>
        static FighterUtils()
        {
            _lvlMultiplier = new ()
            {
                {1,54.0000 }
                ,{2,58.0000 }
                ,{3,62.0000 }
                ,{4,67.5264 }
                ,{5,70.5094 }
                ,{6,73.5228 }
                ,{7,76.5660 }
                ,{8,79.6385 }
                ,{9,82.7395 }
                ,{10,85.8684 }
                ,{11,91.4944 }
                ,{12,97.0680 }
                ,{13,102.5892 }
                ,{14,108.0579 }
                ,{15,113.4743 }
                ,{16,118.8383 }
                ,{17,124.1499 }
                ,{18,129.4091 }
                ,{19,134.6159 }
                ,{20,139.7703 }
                ,{21,149.3323 }
                ,{22,158.8011 }
                ,{23,168.1768 }
                ,{24,177.4594 }
                ,{25,186.6489 }
                ,{26,195.7452 }
                ,{27,204.7484 }
                ,{28,213.6585 }
                ,{29,222.4754 }
                ,{30,231.1992 }
                ,{31,246.4276 }
                ,{32,261.1810 }
                ,{33,275.4733 }
                ,{34,289.3179 }
                ,{35,302.7275 }
                ,{36,315.7144 }
                ,{37,328.2905 }
                ,{38,340.4671 }
                ,{39,352.2554 }
                ,{40,363.6658 }
                ,{41,408.1240 }
                ,{42,451.7883 }
                ,{43,494.6798 }
                ,{44,536.8188 }
                ,{45,578.2249 }
                ,{46,618.9172 }
                ,{47,658.9138 }
                ,{48,698.2325 }
                ,{49,736.8905 }
                ,{50,774.9041 }
                ,{51,871.0599 }
                ,{52,964.8705 }
                ,{53,1056.4206 }
                ,{54,1145.7910 }
                ,{55,1233.0585 }
                ,{56,1318.2965 }
                ,{57,1401.5750 }
                ,{58,1482.9608 }
                ,{59,1562.5178 }
                ,{60,1640.3068 }
                ,{61,1752.3215 }
                ,{62,1861.9011 }
                ,{63,1969.1242 }
                ,{64,2074.0659 }
                ,{65,2176.7983 }
                ,{66,2277.3904 }
                ,{67,2375.9085 }
                ,{68,2472.4160 }
                ,{69,2566.9739 }
                ,{70,2659.6406 }
                ,{71,2780.3044 }
                ,{72,2898.6022 }
                ,{73,3014.6029 }
                ,{74,3128.3729 }
                ,{75,3239.9758 }
                ,{76,3349.4730 }
                ,{77,3456.9236 }
                ,{78,3562.3843 }
                ,{79,3665.9099 }
                ,{80,3767.5533 }

            };
        }

        public enum PathType
        {
            Destruction,
            Hunt,
            Erudition,
            Harmony,
            Nihility,
            Preservation,
            Abundance
                
        }

        //debuff is resisted?
        public static bool CalculateDebuffResisted(Event ent)
        {
            Unit attacker = ent.ParentStep.Actor;
            Unit defender = ent.TargetUnit;

            Effect.EffectType mod = ent.Modification.Effects.First().EffType;
            bool isCC = ent.Modification.CrowdControl;
            double baseChance = ent.BaseChance;
            double effectHitRate = attacker.Stats.EffectHit;
            double effectRes = defender.Stats.EffectRes;
            double debuffRes = defender.GetDebuffResists(mod);
            double ccRes = 0;
            if (isCC)
            {
                ccRes = defender.GetDebuffResists(Effect.EffectType.CrowControl);
            }
            double realChance = baseChance  * (1+ effectHitRate)*(1-effectRes)*(1-debuffRes)*(1-ccRes);


            ent.ParentStep.Parent.Parent?.LogDebug("=======================");
            ent.ParentStep.Parent.Parent?.LogDebug($"Debuff realChance {realChance:f} =baseChance {baseChance:f} * (1+ effectHitRate {effectHitRate:f})* (1- effectRes {effectRes:f})  * (1- debuffRes {debuffRes:f})" +(isCC?$"* (1- ccRes {ccRes:f})":"" ));


            return (new MersenneTwister().NextDouble()<=realChance);
        }
    }
}
