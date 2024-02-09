using System;

namespace HSR_SIM_LIB.Skills;

public class Skill:ICloneable
{
    public string Name { get; set; }
    public int Level { get; set; }
    public object Clone()
    {
        return MemberwiseClone();
    }
}