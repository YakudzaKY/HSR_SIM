﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    /// <summary>
    /// lvl up
    /// </summary>
    public class IncreaseLevel : Event
    {
        public IncreaseLevel(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"{TargetUnit.Name} level up!";
        }

        public override void ProcEvent(bool revert)
        {
            
            //IncreaseLevel

            TargetUnit.Level += (int)(revert ? -Val : Val);
            base.ProcEvent(revert);

        }
    }
}