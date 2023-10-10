﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Class for resources. It can be party (like SP) or character HP
    /// </summary>
    public class Resource: CheckEssence
    {
        private ResourceType resType;
        private double? resVal;

        public ResourceType ResType { get => resType; set => resType = value; }
        public double? ResVal { get => resVal; set => resVal = value; }

        public enum ResourceType
        {
            nil,//dont use type
            TP,
            SP,
            HP,
            Toughness
        }
    }
}
