using System.Collections.Generic;

namespace HSR_SIM_GUI.Data;

internal static class StatData
{
    public static Dictionary<string, string> subStatsUpgrades = new()
    {
        { "spd_fix", "2,3" }, { "hp_fix", "38,103755" }, { "atk_fix", "19,051877" }, { "def", "19,051877" },
        { "hp_prc", "0,03888" }, { "atk_prc", "0,03888" }, { "def_prc", "0,0486" }, { "break_dmg_prc", "0,05832" },
        { "effect_hit_prc", "0,03888" }, { "effect_res_prc", "0,03888" }, { "crit_rate_prc", "0,02916" },
        { "crit_dmg_prc", "0,05832" }
    };

    public static Dictionary<string, string> mainStatsUpgrades = new()
    {
        { "spd_fix", "1,4" }, { "hp_fix", "39,5136" }, { "atk_fix", "19,7568" }, { "hp_prc", "0,024192" },
        { "atk_prc", "0,024192" }, { "def_prc", "0,03024" }, { "break_dmg_prc", "0,036277" },
        { "effect_hit_prc", "0,024192" }, { "sp_rate_prc", "0,010886" }, { "heal_rate_prc", "0,019354" },
        { "crit_rate_prc", "0,018144" }, { "crit_dmg_prc", "0,036288" }
        //elements
        ,
        { "wind_dmg_prc", "0,021773" }, { "physical_dmg_prc", "0,021773" }, { "fire_dmg_prc", "0,021773" },
        { "ice_dmg_prc", "0,021773" }, { "lightning_dmg_prc", "0,021773" }, { "quantum_dmg_prc", "0,021773" },
        { "imaginary_dmg_prc", "0,021773" }
    };

    public static Dictionary<string, string> mainStatsBase = new()
    {
        { "spd_fix", "4,032" }, { "hp_fix", "112,896" }, { "atk_fix", "56,448" }, { "hp_prc", "0,069120" },
        { "atk_prc", "0,069120" }, { "def_prc", "0,0864" }, { "break_dmg_prc", "0,103680" },
        { "effect_hit_prc", "0,069120" }, { "sp_rate_prc", "0,031104" }, { "heal_rate_prc", "0,055296" },
        { "crit_rate_prc", "0,05184" }, { "crit_dmg_prc", "0,10368" }
        //elements
        ,
        { "wind_dmg_prc", "0,062208" }, { "physical_dmg_prc", "0,062208" }, { "fire_dmg_prc", "0,062208" },
        { "ice_dmg_prc", "0,062208" }, { "lightning_dmg_prc", "0,062208" }, { "quantum_dmg_prc", "0,062208" },
        { "imaginary_dmg_prc", "0,062208" }
    };
}