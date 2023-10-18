using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class SilverWolf:DefaultFighter
    {
        public  override FighterUtils.PathType? Path { get; set; } = FighterUtils.PathType.Nihility;
        public SilverWolf(Unit parent) : base(parent)
        {
        }
    }
}
