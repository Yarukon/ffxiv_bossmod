﻿using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.HuntA.Petalodus
{
    public enum OID : uint
    {
        Boss = 0x35FB, // R5.400, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        MarineMayhem = 27063,
        Waterga = 27067, // Boss->players, 5.0s cast, range 6 circle
        TidalGuillotine = 27068, // Boss->self, 4.0s cast, range 13 circle
        AncientBlizzard = 27069, // Boss->self, 4.0s cast, range 40 45-degree cone
    }

    class MarineMayhem : Components.CastHint // TODO: find out what this spell does...
    {
        public MarineMayhem() : base(ActionID.MakeSpell(AID.MarineMayhem), "Interruptible raidwide") { }
    }

    // TODO: generalize (outdoor spread)
    class Waterga : Components.CastHint
    {
        private static float _radius = 6;

        public Waterga() : base(ActionID.MakeSpell(AID.Waterga), "Spread") { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Targets(module).Any(t => t != actor && actor.Position.InCircle(t.Position, _radius)))
                hints.Add("GTFO from spread marker!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var t in Targets(module))
                arena.AddCircle(t.Position, _radius, ArenaColor.Danger);
        }

        private IEnumerable<Actor> Targets(BossModule module)
        {
            foreach (var c in Casters)
                if (module.WorldState.Actors.Find(c.CastInfo!.TargetID) is var t && t != null)
                    yield return t;
        }
    }

    class TidalGuillotine : Components.SelfTargetedAOEs
    {
        public TidalGuillotine() : base(ActionID.MakeSpell(AID.TidalGuillotine), new AOEShapeCircle(13)) { }
    }

    class AncientBlizzard : Components.SelfTargetedAOEs
    {
        public AncientBlizzard() : base(ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeCone(40, 22.5f.Degrees())) { }
    }

    class PetalodusStates : StateMachineBuilder
    {
        public PetalodusStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<MarineMayhem>()
                .ActivateOnEnter<Waterga>()
                .ActivateOnEnter<TidalGuillotine>()
                .ActivateOnEnter<AncientBlizzard>();
        }
    }

    public class Petalodus : SimpleBossModule
    {
        public Petalodus(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
