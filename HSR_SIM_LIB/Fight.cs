using System.Collections.Generic;

namespace HSR_SIM_LIB
{
    public class Fight
    {
        List<Wave> waves;

        public string Name { get; internal set; }
        internal List<Wave> Waves { get => waves; set => waves = value; }
    }
}
