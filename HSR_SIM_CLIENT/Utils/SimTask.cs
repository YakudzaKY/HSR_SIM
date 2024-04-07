using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses;

namespace HSR_SIM_CLIENT.Utils;

/// <summary>
///     Task for simulator.
/// </summary>
public class SimTask
{
    //loaded scenario. will be cloned into every sim iteration
    public SimCls SimScenario { get; init; }

    //path to dev mode script. will be used in auto tests
    public string DevLogPath { get; init; }

    //this flag will be used by auto-tests for scenario reproducing
    public bool DevMode { get; init; } = false;

    //this value need for display the result in a child chart
    public int UpgradesCount { get; set; } = 0;

    // Parent need for group results
    public SimTask Parent { get; init; }

    /// <summary>
    ///     modifiers to profile
    /// </summary>
    public List<Worker.RStatMod> StatMods { get; set; }
}