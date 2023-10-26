using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones
{
    internal class BeforetheTutorialMissionStarts:DefaultLightCone
    {
        public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Nihility;
        public BeforetheTutorialMissionStarts(IFighter parent, int rank) : base(parent, rank)
        {
            if (Path == Parent.Path)
            {

            }
        }
    }
}
