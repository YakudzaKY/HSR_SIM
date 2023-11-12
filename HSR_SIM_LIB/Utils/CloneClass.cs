using System;

namespace HSR_SIM_LIB;

public class CloneClass : ICloneable
{
    public virtual object Clone()
    {
        return MemberwiseClone();
    }
}