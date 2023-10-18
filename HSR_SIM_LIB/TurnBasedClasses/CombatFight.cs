namespace HSR_SIM_LIB.TurnBasedClasses
{
    internal class CombatFight
    {
        private Fight referenceFight;
        private short currentWaveCnt = 0;
        private Wave currentWave;
        public short CurrentWaveCnt { get => currentWaveCnt; set => currentWaveCnt = value; }
        public Fight ReferenceFight { get => referenceFight; set => referenceFight = value; }
        internal Wave CurrentWave { get => currentWave; set => currentWave = value; }

        public CombatFight(Fight refFight)
        {
            ReferenceFight = refFight;
        }


    }
}
