﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public class Trigger
    {
        public TriggerType TrType { get; init; }

        public enum TriggerType {
            ShieldBreakeTrigger
        }

     
    }
}