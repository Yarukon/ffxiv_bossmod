﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.HuntS.Sphatika
{
    public enum OID : uint
    {
        Boss = 0x3670, // R8.750, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Gnaw = 27617, // Boss->player, 5.0s cast, single-target tankbuster
        Infusion = 27618, // Boss->players, no cast, width 10 rect charge
        Caterwaul = 27619, // Boss->self, 5.0s cast, range 40 circle

        Brace1 = 27620, // Boss->self, 3.0s cast, single-target (??? forward->backward, lickwhip 624->626->715->627->715)
        Brace2 = 27621, // ???
        Brace3 = 27622, // Boss->self, 3.0s cast, single-target (??? backward->rightward, whiplick 625->631->714->633->714)
        Brace4 = 27623, // Boss->self, 3.0s cast, single-target (??? left->right, whiplick 625->632->714->633->714)
        LickwhipStance = 27624, // Boss->self, 5.0s cast, single-target, visual (lick -> whip using first bearing buff)
        WhiplickStance = 27625, // Boss->self, 5.0s cast, single-target, visual (whip -> lick using first bearing buff)
        LongLickForward = 27626, // Boss->self, 1.0s cast, range 40 180-degree cone aimed forward (with forward bearing)
        LongLickBackward = 27627, // Boss->self, 1.0s cast, range 40 180-degree cone aimed backward (with backward bearing)
        LongLickLeftward = 27628, // Boss->self, 1.0s cast, range 40 180-degree cone aimed left (with leftward bearing)
        LongLickRightward = 27629, // Boss->self, 1.0s cast, range 40 180-degree cone aimed right (with rightward bearing)
        HindWhipForward = 27630, // Boss->self, 1.0s cast, range 40 180-degree cone aimed backward (with forward bearing)
        HindWhipBackward = 27631, // Boss->self, 1.0s cast, range 40 180-degree cone aimed forward (with backward bearing)
        HindWhipLeftward = 27632, // Boss->self, 1.0s cast, range 40 180-degree cone aimed right (with leftward bearing)
        HindWhipRightward = 27633, // Boss->self, 1.0s cast, range 40 180-degree cone aimed left (with rightward bearing)
        LongLickSecond = 27714, // Boss->self, 1.0s cast, range 40 180-degree cone (after hind whip)
        HindWhipSecond = 27715, // Boss->self, 1.0s cast, range 40 180-degree cone (after long lick)
    };

    public enum SID : uint
    {
        ForwardBearing = 2835, // Boss->Boss, extra=0x0
        BackwardBearing = 2836, // Boss->Boss, extra=0x0
        LeftwardBearing = 2837, // Boss->Boss, extra=0x0
        RightwardBearing = 2838, // Boss->Boss, extra=0x0
    };

    class Gnaw : Components.SingleTargetCast
    {
        public Gnaw() : base(ActionID.MakeSpell(AID.Gnaw)) { }
    }

    class Caterwaul : Components.RaidwideCast
    {
        public Caterwaul() : base(ActionID.MakeSpell(AID.Caterwaul)) { }
    }

    class Stance : Components.GenericAOEs
    {
        private SID _secondBearing; // TODO: this is a hack, find a way to properly determine which buff is 'first'
        private List<Angle> _pendingCleaves = new();
        private static AOEShapeCone _shape = new(40, 90.Degrees());

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_pendingCleaves.Count > 0)
                yield return (_shape, module.PrimaryActor.Position, _pendingCleaves.First(), new());
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;
            var hint = (AID)module.PrimaryActor.CastInfo!.Action.ID switch
            {
                AID.Brace1 or AID.Brace2 or AID.Brace3 or AID.Brace4 => "Select directions",
                AID.LickwhipStance => "Cleave sides literal",
                AID.WhiplickStance => "Cleave sides inverted",
                _ => ""
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster != module.PrimaryActor)
                return;

            switch ((AID)spell.Action.ID)
            {
                case AID.LickwhipStance:
                    InitCleaves(module, spell.Rotation, false);
                    break;
                case AID.WhiplickStance:
                    InitCleaves(module, spell.Rotation, true);
                    break;
                case AID.LongLickForward:
                case AID.HindWhipForward:
                    FixupOrder(SID.ForwardBearing);
                    break;
                case AID.LongLickBackward:
                case AID.HindWhipBackward:
                    FixupOrder(SID.BackwardBearing);
                    break;
                case AID.LongLickLeftward:
                case AID.HindWhipLeftward:
                    FixupOrder(SID.LeftwardBearing);
                    break;
                case AID.LongLickRightward:
                case AID.HindWhipRightward:
                    FixupOrder(SID.RightwardBearing);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster == module.PrimaryActor && _pendingCleaves.Count > 0 &&
                (AID)spell.Action.ID is AID.LongLickForward or AID.LongLickBackward or AID.LongLickLeftward or AID.LongLickRightward or AID.LongLickSecond
                                     or AID.HindWhipForward or AID.HindWhipBackward or AID.HindWhipLeftward or AID.HindWhipRightward or AID.HindWhipSecond)
            {
                _pendingCleaves.RemoveAt(0);
            }
        }

        private void InitCleaves(BossModule module, Angle reference, bool inverted)
        {
            _pendingCleaves.Clear();
            // TODO: current implementation looks correct, but buffs are sometimes picked in wrong order...
            foreach (var s in module.PrimaryActor.Statuses)
            {
                switch ((SID)s.ID)
                {
                    case SID.ForwardBearing:
                        _secondBearing = (SID)s.ID;
                        AddPendingCleave(reference, inverted);
                        break;
                    case SID.BackwardBearing:
                        _secondBearing = (SID)s.ID;
                        AddPendingCleave(reference + 180.Degrees(), inverted);
                        break;
                    case SID.LeftwardBearing:
                        _secondBearing = (SID)s.ID;
                        AddPendingCleave(reference + 90.Degrees(), inverted);
                        break;
                    case SID.RightwardBearing:
                        _secondBearing = (SID)s.ID;
                        AddPendingCleave(reference - 90.Degrees(), inverted);
                        break;
                }
            }
        }

        private void AddPendingCleave(Angle dir, bool inverted)
        {
            if (inverted)
                dir += 180.Degrees();
            _pendingCleaves.Add(dir);
            _pendingCleaves.Add(dir + 180.Degrees());
        }

        private void FixupOrder(SID expectedFirst)
        {
            if (expectedFirst == _secondBearing && _pendingCleaves.Count == 4)
            {
                var t1 = _pendingCleaves[0];
                var t2 = _pendingCleaves[1];
                _pendingCleaves[0] = _pendingCleaves[2];
                _pendingCleaves[1] = _pendingCleaves[3];
                _pendingCleaves[2] = t1;
                _pendingCleaves[3] = t2;
            }
        }
    }

    class SphatikaStates : StateMachineBuilder
    {
        public SphatikaStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Gnaw>()
                .ActivateOnEnter<Caterwaul>()
                .ActivateOnEnter<Stance>();
        }
    }

    public class Sphatika : SimpleBossModule
    {
        public Sphatika(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
