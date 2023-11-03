using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    public class ToughnessBreakDoTDamage:DoTDamage
    {
        
        public override string GetDescription()
        {
            return $"DoT(Shield Break) tick from {SourceUnit.Name}" +
                   $" overall={Val:f} to_barier={RealBarrierVal:f} to_hp={RealVal:f}";
            
        }
     
        public ToughnessBreakDoTDamage(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }
    }

}
