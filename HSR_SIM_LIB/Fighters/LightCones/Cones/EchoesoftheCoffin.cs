using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones
{
    internal class EchoesoftheCoffin:DefaultLightCone
    {
        public override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Abundance;
        public EchoesoftheCoffin(IFighter parent, int rank) : base(parent, rank)
        {
            if (Path == Parent.Path)
            {

            }
        }
    }
}
