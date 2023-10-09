﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{/// <summary>
/// Class for unit stats
/// </summary>
    public class UnitStats
    {
        public int BaseMaxHp { get; set; } = 0;
        public int BaseAttack { get; set; } = 0;
        public int CurrentHp { get; set; } = 0;
        public int MaxHp { get; set; } = 0;
        public int CurrentEnergy { get; set; } = 0;
        public int BaseMaxEnergy { get; set; } = 0;
        public int BaseActionValue
        {
            get => 10000 / Speed;
            set => throw new NotImplementedException();
        }

        public int ActionValue { get; set; } = 0;
        public int FlatSpeed { get; internal set; }
        public int BaseSpeed { get; internal set; }
        public int SpdPrc { get; internal set; } = 0;
        public int Speed//speed for calcing
        {
            get
            {
               return BaseSpeed*(1+ SpdPrc)+ FlatSpeed;
            }
            internal set { throw new NotImplementedException(); }
        }

        public UnitStats() { }

        public void ResetAV()
        {
            ActionValue = BaseActionValue;
        }
    }
}