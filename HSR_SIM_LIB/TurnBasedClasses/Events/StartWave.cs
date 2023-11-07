using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    // starting the wave
    public class StartWave : Event
    {
        public List<Unit> StartingUnits { get; set; }

        public StartWave(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return "next wave";
        }

        public override void ProcEvent(bool revert)
        {
            
            //Loading wave

            if (!revert)
            {
                Parent.Parent.CurrentFight.CurrentWaveCnt += 1;
                Parent.Parent.CurrentFight.CurrentWave = Parent.Parent.CurrentFight.ReferenceFight.Waves[Parent.Parent.CurrentFight.CurrentWaveCnt - 1];
                StartingUnits ??= SimCls.GetCombatUnits(Parent.Parent.CurrentFight.CurrentWave.Units);
                Parent.Parent.HostileTeam.BindUnits(StartingUnits);
                //set start action value
                foreach (Unit unit in Parent.Parent.AllUnits)
                {
                    unit.Stats.ResetAV();
                }

            }
            else
            {
                Parent.Parent.HostileTeam.UnBindUnits();
                Parent.Parent.CurrentFight.CurrentWaveCnt -= 1;
                Parent.Parent.CurrentFight.CurrentWave = null;
                

            }
            base.ProcEvent(revert);

        }
    }
}