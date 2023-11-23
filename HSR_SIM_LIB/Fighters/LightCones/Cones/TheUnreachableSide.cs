using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones;

internal class TheUnreachableSide : DefaultLightCone
{
    private readonly Dictionary<int, double> modifiers = new()
        { { 1, 0.24 }, { 2, 0.28 }, { 3, 0.32 }, { 4, 0.36 }, { 5, 0.40 } };

    private readonly Buff uniqueBuff;

    public TheUnreachableSide(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
            uniqueBuff = new Buff(Parent.Parent)
            {
                Type = Buff.BuffType.Buff,
                BaseDuration = null,
                MaxStack = 1,
                Effects = new List<Effect> { new EffAllDamageBoost { Value = modifiers[rank] } }
            };
    }

    public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Destruction;

    //add buff when attacked or loose hp
    public override void DefaultLightCone_HandleEvent(Event ent)
    {
        //if unit consume hp or got attack then apply buff
        if (((ent.AbilityValue?.Parent == Parent && ent.TargetUnit == Parent.Parent && ent is ResourceDrain &&
              ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal != 0)
             || (ent.TargetUnit == Parent.Parent && ent is DirectDamage)) && uniqueBuff != null)
        {
            ApplyBuff newEvent = new(ent.ParentStep, this, Parent.Parent)
            {
                TargetUnit = Parent.Parent,
                BuffToApply = uniqueBuff
            };
            ent.ChildEvents.Add(newEvent);
        }

        //remove buff when attack completed
        if (ent.SourceUnit == Parent.Parent && ent is ExecuteAbilityFinish && ent.AbilityValue.Attack)
        {
            RemoveBuff newEvent = new(ent.ParentStep, this, Parent.Parent)
            {
                TargetUnit = Parent.Parent,
                BuffToApply = uniqueBuff
            };
            ent.ChildEvents.Add(newEvent);
        }

        base.DefaultLightCone_HandleEvent(ent);
    }
}