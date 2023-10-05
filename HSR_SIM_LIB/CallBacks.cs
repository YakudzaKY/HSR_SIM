using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public static class CallBacks
    {

        public delegate void CallBackStr(KeyValuePair<String, String> kv);
        public delegate void CallBackRender(Bitmap combatImg);
    }
}
