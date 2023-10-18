using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones
{
    internal class PlanetaryRendezvous:DefaultLightCone
    {
        public PlanetaryRendezvous(IFighter parent, int rank) : base(parent, rank)
        {
        }
    }
}
