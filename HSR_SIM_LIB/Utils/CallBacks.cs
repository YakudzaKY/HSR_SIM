using System.Collections.Generic;
using System.Drawing;

namespace HSR_SIM_LIB.Utils;

public static class CallBacks
{
    public delegate int CallBackGetDecision(string[] items, string description);

    public delegate void CallBackRender(Bitmap combatImg);

    public delegate void CallBackStr(KeyValuePair<string, string> kv);
}