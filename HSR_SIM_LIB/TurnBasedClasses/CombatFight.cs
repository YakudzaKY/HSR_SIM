using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.TurnBasedClasses.Step;

namespace HSR_SIM_LIB.TurnBasedClasses
{
    internal class CombatFight
    {
        public SimCls Parent { get; set; }
        private Fight referenceFight;
        private short currentWaveCnt = 0;
        private Wave currentWave;
        public short CurrentWaveCnt { get => currentWaveCnt; set => currentWaveCnt = value; }
        public Fight ReferenceFight { get => referenceFight; set => referenceFight = value; }
        internal Wave CurrentWave { get => currentWave; set => currentWave = value; }

        public TurnR Turn { get; set; } = null;

        public record TurnR
        {
            public Unit Actor { get; set; } = null;
            public StepTypeEnm? TurnStage { get; set; }= null;

        }



        public IEnumerable<Unit> AllAliveUnits
        {
            get
            {
                return Parent?.PartyTeam.Units.Where(x => x.IsAlive)
                    .Concat(Parent.HostileTeam.Units.Where(x => x.IsAlive)).Concat(Parent.SpecialTeam.Units);

            }

        }

        public CombatFight(Fight refFight, SimCls parent)
        {

            ReferenceFight = refFight;
            Parent = parent;
        }


    }
}
