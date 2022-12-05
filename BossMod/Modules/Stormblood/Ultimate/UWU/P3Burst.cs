﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P3Burst : Components.GenericAOEs
    {
        private List<Actor> _bombs = new();
        private Dictionary<ulong, DateTime?> _bombActivation = new();

        private static AOEShape _shape = new AOEShapeCircle(6.3f);

        public P3Burst() : base(ActionID.MakeSpell(AID.Burst)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var b in _bombs)
            {
                var activation = _bombActivation.GetValueOrDefault(b.InstanceID);
                if (activation != null)
                    yield return (_shape, b.Position, b.Rotation, b.CastInfo?.FinishAt ?? activation.Value);
            }
        }

        public override void Init(BossModule module)
        {
            _bombs = module.Enemies(OID.BombBoulder);
        }

        public override void Update(BossModule module)
        {
            foreach (var b in _bombs.Where(b => !_bombActivation.ContainsKey(b.InstanceID)))
                _bombActivation[b.InstanceID] = module.WorldState.CurrentTime.AddSeconds(6.5f);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _bombActivation[caster.InstanceID] = null;
        }
    }
}
