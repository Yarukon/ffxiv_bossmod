﻿namespace BossMod.Stormblood.Ultimate.UWU
{
    class UWUStates : StateMachineBuilder
    {
        private UWU _module;

        public UWUStates(UWU module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1Garuda, "P1: Garuda")
                .ActivateOnEnter<P1Plumes>()
                .ActivateOnEnter<P1Gigastorm>()
                .ActivateOnEnter<P1GreatWhirlwind>() // TODO: not sure about this...
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HP.Cur <= 1 && !Module.PrimaryActor.IsTargetable;
            SimplePhase(1, Phase2Ifrit, "P2: Ifrit")
                .ActivateOnEnter<P2Nails>()
                .ActivateOnEnter<P2InfernalFetters>()
                .ActivateOnEnter<P2SearingWind>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (_module.Ifrit()?.HP.Cur <= 1 && !(_module.Ifrit()?.IsTargetable ?? true));
            SimplePhase(2, Phase3Titan, "P3: Titan")
                .ActivateOnEnter<P3Geocrush2>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (_module.Titan()?.HP.Cur <= 1 && !(_module.Titan()?.IsTargetable ?? true));
            SimplePhase(3, Phase4LahabreaUltima, "P4: Lahabrea + Ultima")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed; // TODO: condition
        }

        private void Phase1Garuda(uint id)
        {
            P1SlipstreamMistralSong(id, 5.2f);
            P1Adds1(id + 0x10000, 8.5f);
            P1Frictions(id + 0x20000, 7.4f);
            P1GarudaFeatherRainRaidwide(id + 0x30000, 12.1f, AID.AerialBlast);
            P1SistersFeatherRain(id + 0x40000, 13.7f);
            P1MistralSongEyeOfTheStormWickedWheelFeatherRain(id + 0x50000, 4.4f);
            P1Adds2(id + 0x60000, 5.7f);
            // awakening happens here...
            P1SistersFeatherRain(id + 0x70000, 2.0f);
            P1Slipstream(id + 0x80000, 10.0f);
            P1AwakenedWickedWheelDownburst(id + 0x90000, 5.5f);
            P1Slipstream(id + 0xA0000, 5.5f);
            P1Enrage(id + 0xB0000, 9.4f);
        }

        private State P1Slipstream(uint id, float delay)
        {
            return ActorCast(id, _module.Garuda, AID.Slipstream, delay, 2.5f, true, "Slipstream")
                .ActivateOnEnter<P1Slipstream>()
                .DeactivateOnExit<P1Slipstream>();
        }

        private void P1SlipstreamMistralSong(uint id, float delay)
        {
            ActorCastStart(id, _module.Garuda, AID.Slipstream, delay, true)
                .ActivateOnEnter<P1MistralSongBoss>(); // icon appears slightly before cast start
            ActorCastEnd(id + 1, _module.Garuda, 2.5f, true, "Slipstream")
                .ActivateOnEnter<P1Slipstream>()
                .DeactivateOnExit<P1Slipstream>();
            ComponentCondition<P1MistralSongBoss>(id + 0x10, 2.7f, comp => comp.NumCasts > 0, "Mistral song")
                .DeactivateOnExit<P1MistralSongBoss>();
            // note: mistral song leaves a puddle which explodes 3 times, with 3.1s delay between casts and 3s cast time; it overlaps with logically-next mechanic
            //ComponentCondition<P1GreatWhirlwind>(id + 0x20, 3.1f, comp => comp.Casters.Count > 0)
            //    .ActivateOnEnter<P1GreatWhirlwind>();
            //ComponentCondition<P1GreatWhirlwind>(id + 0x21, 3, comp => comp.Casters.Count == 0, "Puddle")
            //    .DeactivateOnExit<P1GreatWhirlwind>();
        }

        private void P1Adds1(uint id, float delay)
        {
            ComponentCondition<P1Plumes>(id, delay, comp => comp.Active, "Adds");
            P1Slipstream(id + 0x10, 1.8f);
            ComponentCondition<P1Downburst>(id + 0x20, 3.5f, comp => comp.NumCasts > 0, "Cleave + Cyclone 1") // note: cyclone happens ~0.1s after cleave
                .ActivateOnEnter<P1Downburst>()
                .DeactivateOnExit<P1Downburst>();
            // great whirlwind disappears somewhere here...

            P1GarudaFeatherRainRaidwide(id + 0x1000, 7.3f, AID.MistralShriek, "Cyclone 2 + Feathers"); // note: cyclone happens ~0.6s before feathers
        }

        private void P1Frictions(uint id, float delay)
        {
            ActorCast(id, _module.Garuda, AID.Friction, delay, 2, true, "Friction 1");
            ActorCast(id + 0x10, _module.Garuda, AID.Friction, 4.2f, 2, true, "Friction 2");
        }

        private void P1GarudaFeatherRainRaidwide(uint id, float delay, AID raidwide, string name = "Feathers")
        {
            ActorTargetable(id, _module.Garuda, false, delay, "Disappear");
            ComponentCondition<P1FeatherRain>(id + 1, 1.6f, comp => comp.CastsActive)
                .ActivateOnEnter<P1FeatherRain>();
            ComponentCondition<P1FeatherRain>(id + 2, 1, comp => !comp.CastsActive, name)
                .DeactivateOnExit<P1FeatherRain>();
            ActorTargetable(id + 3, _module.Garuda, true, 1.7f, "Reappear");

            ActorCast(id + 0x10, _module.Garuda, raidwide, 0.1f, 3, true, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P1SistersFeatherRain(uint id, float delay)
        {
            ComponentCondition<P1FeatherRain>(id, delay, comp => comp.CastsPredicted, "Feathers bait")
                .ActivateOnEnter<P1FeatherRain>();
            ComponentCondition<P1FeatherRain>(id + 1, 1.5f, comp => comp.CastsActive);
            ComponentCondition<P1FeatherRain>(id + 2, 1, comp => !comp.CastsActive, "Feathers")
                .DeactivateOnExit<P1FeatherRain>();
        }

        private void P1MistralSongEyeOfTheStormWickedWheelFeatherRain(uint id, float delay)
        {
            ActorCastStart(id, _module.Garuda, AID.WickedWheel, delay, true)
                .ActivateOnEnter<P1MistralSongAdds>() // icons on targets ~2.7s before cast start
                .ActivateOnEnter<P1EyeOfTheStorm>(); // cast starts ~0.2s before
            ComponentCondition<P1MistralSongAdds>(id + 1, 2.5f, comp => comp.NumCasts > 0, "Mistral song")
                .ActivateOnEnter<P1WickedWheel>()
                .DeactivateOnExit<P1MistralSongAdds>();
            ActorCastEnd(id + 2, _module.Garuda, 0.5f, true)
                .DeactivateOnExit<P1EyeOfTheStorm>() // finishes ~0.2s before
                .DeactivateOnExit<P1WickedWheel>();

            P1SistersFeatherRain(id + 0x10, 1.6f);
            ComponentCondition<P1GreatWhirlwind>(id + 0x20, 1.5f, comp => comp.Casters.Count == 0, "Whirlwind");
        }

        private void P1Adds2(uint id, float delay)
        {
            ComponentCondition<P1Plumes>(id, delay, comp => comp.Active, "Adds");
            P1Slipstream(id + 0x10, 7.8f)
                .ActivateOnEnter<P1EyeOfTheStorm>() // starts ~0.9s after cast start
                .ActivateOnEnter<P1Mesohigh>(); // tethers appear ~0.9s after cast start
            ComponentCondition<P1Downburst>(id + 0x20, 3.5f, comp => comp.NumCasts > 0, "Cleave + Mesohigh")
                .ActivateOnEnter<P1Downburst>()
                .DeactivateOnExit<P1Downburst>()
                .DeactivateOnExit<P1EyeOfTheStorm>() // ends ~2.2s before cleave
                .DeactivateOnExit<P1Mesohigh>(); // resolves at the same time as cleave
        }

        private void P1AwakenedWickedWheelDownburst(uint id, float delay)
        {
            ActorCast(id, _module.Garuda, AID.WickedWheel, delay, 3, true, "Out")
                .ActivateOnEnter<P1WickedWheel>();
            ComponentCondition<P1WickedWheel>(id + 2, 2.1f, comp => comp.NumCasts >= 2 || comp.Sources.Count == 0 && _module.WorldState.CurrentTime >= comp.AwakenedResolve, "In") // complicated condition handles fucked up awakening
                .DeactivateOnExit<P1WickedWheel>();
            ComponentCondition<P1Downburst>(id + 0x10, 2.8f, comp => comp.NumCasts > 0, "Cleave")
                .ActivateOnEnter<P1Downburst>()
                .DeactivateOnExit<P1Downburst>();
        }

        private void P1Enrage(uint id, float delay)
        {
            // note: similar to P1GarudaFeatherRainRaidwide, except that garuda doesn't reappear
            ActorTargetable(id, _module.Garuda, false, delay, "Disappear");
            ComponentCondition<P1FeatherRain>(id + 1, 1.6f, comp => comp.CastsActive)
                .ActivateOnEnter<P1FeatherRain>();
            ComponentCondition<P1FeatherRain>(id + 2, 1, comp => !comp.CastsActive)
                .DeactivateOnExit<P1FeatherRain>();
            ActorCast(id + 0x10, _module.Garuda, AID.AerialBlast, 1.8f, 3, true, "Enrage");
        }

        private void Phase2Ifrit(uint id)
        {
            P2CrimsonCycloneRadiantPlumeHellfire(id, 4.2f);
            P2VulcanBurst(id + 0x10000, 8.2f);
            P2Incinerate(id + 0x20000, 2.8f);
            P2Nails(id + 0x30000, 7.2f);
            P2InfernoHowlEruptionCrimsonCyclone(id + 0x40000, 6.3f);
            P2Incinerate(id + 0x50000, 4.2f);

            // TODO: eruptions > flaming crush > enrage
            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void P2CrimsonCycloneRadiantPlumeHellfire(uint id, float delay)
        {
            ComponentCondition<P2CrimsonCyclone>(id, delay, comp => comp.CastsPredicted)
                .ActivateOnEnter<P2CrimsonCyclone>()
                .ActivateOnEnter<P2RadiantPlume>(); // casts starts at the same time as charge (2.1s)
            ComponentCondition<P2CrimsonCyclone>(id + 0x10, 5.1f, comp => comp.NumCasts > 0, "Charge")
                .DeactivateOnExit<P2CrimsonCyclone>();
            ComponentCondition<P2RadiantPlume>(id + 0x20, 1, comp => comp.NumCasts > 0, "Plumes")
                .DeactivateOnExit<P2RadiantPlume>();

            Condition(id + 0x100, 3.2f, () => _module.Ifrit() != null, "Ifrit appears");
            ActorCast(id + 0x200, _module.Ifrit, AID.Hellfire, 0.1f, 3, true, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P2VulcanBurst(uint id, float delay)
        {
            ComponentCondition<P2VulcanBurst>(id, delay, comp => comp.NumCasts > 0, "Knockback")
                .ActivateOnEnter<P2VulcanBurst>()
                .DeactivateOnExit<P2VulcanBurst>();
        }

        private void P2Incinerate(uint id, float delay)
        {
            ComponentCondition<P2Incinerate>(id, delay, comp => comp.NumCasts > 0, "Incinerate 1")
                .ActivateOnEnter<P2Incinerate>();
            ComponentCondition<P2Incinerate>(id + 1, 3.1f, comp => comp.NumCasts > 1, "Incinerate 2");
            ComponentCondition<P2Incinerate>(id + 2, 4.1f, comp => comp.NumCasts > 2, "Incinerate 3")
                .DeactivateOnExit<P2Incinerate>();
        }

        private void P2Nails(uint id, float delay)
        {
            ComponentCondition<P2Nails>(id, delay, comp => comp.Active, "Nails spawn");
            // +5.0s: fetters
            ActorCast(id + 0x100, _module.Ifrit, AID.InfernoHowl, 5.2f, 2, true, "Searing wind start");
            ActorCastStart(id + 0x110, _module.Ifrit, AID.Eruption, 3.2f, true, "Eruption baits")
                .ActivateOnEnter<P2Eruption>(); // activate early to show bait hints
            ActorCastEnd(id + 0x111, _module.Ifrit, 2.5f, true);
            ComponentCondition<P2Eruption>(id + 0x120, 6.5f, comp => comp.NumCasts >= 8)
                .DeactivateOnExit<P2Eruption>();
            ComponentCondition<P2SearingWind>(id + 0x130, 5.8f, comp => !comp.Active, "Searing wind end");

            ActorTargetable(id + 0x200, _module.Ifrit, false, 5.1f, "Disappear");
            ActorTargetable(id + 0x201, _module.Ifrit, true, 4.3f, "Reappear");
            ActorCast(id + 0x210, _module.Ifrit, AID.Hellfire, 0.1f, 3, true, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P2InfernoHowlEruptionCrimsonCyclone(uint id, float delay)
        {
            ActorCast(id, _module.Ifrit, AID.InfernoHowl, delay, 2, true, "Searing wind 1 start");
            ActorCastStart(id + 0x10, _module.Ifrit, AID.Eruption, 3.2f, true, "Eruption baits")
                .ActivateOnEnter<P2Eruption>(); // activate early to show bait hints
            ActorCastEnd(id + 0x21, _module.Ifrit, 2.5f, true);
            // +0.3s: searing wind 1
            ComponentCondition<P2CrimsonCyclone>(id + 0x30, 2.5f, comp => comp.CastsPredicted)
                .ActivateOnEnter<P2CrimsonCyclone>();
            // +3.8s: searing wind 2
            ComponentCondition<P2Eruption>(id + 0x40, 4, comp => comp.NumCasts >= 8)
                .DeactivateOnExit<P2Eruption>();
            ComponentCondition<P2CrimsonCyclone>(id + 0x50, 1.0f, comp => comp.NumCasts > 0, "Charges")
                .DeactivateOnExit<P2CrimsonCyclone>();

            ActorCast(id + 0x1000, _module.Ifrit, AID.InfernoHowl, 2.9f, 2, true, "Searing wind 2 start");
            // +0.0s: searing wind 3 (on first target only)
            ComponentCondition<P2FlamingCrush>(id + 0x1010, 5.1f, comp => comp.Active)
                .ActivateOnEnter<P2FlamingCrush>();
            // +0.8s: searing wind 4 (1st target) / 1 (2nd target)
            ComponentCondition<P2FlamingCrush>(id + 0x1020, 5.1f, comp => !comp.Active, "Stack")
                .DeactivateOnExit<P2FlamingCrush>();
            // +1.7s: searing wind 5 (1st target) / 2 (2nd target)

            ActorTargetable(id + 0x2000, _module.Ifrit, false, 4.1f, "Disappear");
            ComponentCondition<P2CrimsonCyclone>(id + 0x2001, 2.3f, comp => comp.CastsPredicted)
                .ActivateOnEnter<P2CrimsonCyclone>();
            // +2.2s: PATE 1E43 on 4 ifrits
            // +3.6s: searing wind 3 (2nd target)
            // +4.4s: first charge start (others are staggered by 1.4s); 3s cast duration, ~2.2s after awakened charge we get 2 'cross' charges
            // +9.6s: searing wind 4 (2nd target)
            // +15.6s: searing wind 5 (2nd target)
            ActorTargetable(id + 0x2100, _module.Ifrit, true, 13.5f, "Awakened charges + Reappear")
                .DeactivateOnExit<P2CrimsonCyclone>();
        }

        private void Phase3Titan(uint id)
        {
            P3GeocrushEarthenFury(id, 2.2f);
            P3RockBusterMountainBuster(id + 0x10000, 8.2f, false);
            P3WeightOfTheLandGeocrush(id + 0x20000, 2.1f);
            P3UpheavalGaolsLandslideTumult(id + 0x30000, 2.2f);
            P3WeightOfTheLandLandslideAwakened(id + 0x40000, 5.1f);
            P3Geocrush3(id + 0x50000, 4.4f);
            P3LandslideAwakened(id + 0x60000, 12.3f);
            P3Tumult(id + 0x70000, 3.2f, 6);
            P3RockBusterMountainBuster(id + 0x80000, 2.2f, true);
            P3TripleWeightOfTheLandLandslideAwakenedBombs(id + 0x90000, 4.2f);
            P3RockBusterMountainBuster(id + 0xA0000, 4.2f, true);

            // TODO: weights > tumults > enrage
            SimpleState(id + 0xFF0000, 10000, "???")
                .ActivateOnEnter<P3WeightOfTheLand>()
                .ActivateOnEnter<P3Landslide>();
        }

        private void P3RockBusterMountainBuster(uint id, float delay, bool longDelay)
        {
            ComponentCondition<P3RockBuster>(id, delay, comp => comp.NumCasts > 0, "Cleave 1")
                .ActivateOnEnter<P3RockBuster>()
                .DeactivateOnExit<P3RockBuster>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P3MountainBuster>(id + 1, longDelay ? 4.1f : 3.1f, comp => comp.NumCasts > 0, "Cleave 2")
                .ActivateOnEnter<P3MountainBuster>()
                .DeactivateOnExit<P3MountainBuster>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        // note: keeps component active for second and maybe third sets
        private State P3WeightOfTheLandFirst(uint id, float delay, string name = "Puddles")
        {
            ActorCast(id, _module.Titan, AID.WeightOfTheLand, delay, 2.5f, true)
                .ActivateOnEnter<P3WeightOfTheLand>();
            return ComponentCondition<P3WeightOfTheLand>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, name);
        }

        private State P3LandslideNormal(uint id, float delay, string name = "Landslide")
        {
            return ActorCast(id, _module.Titan, AID.LandslideBoss, delay, 2.2f, true, name)
                .ActivateOnEnter<P3Landslide>()
                .DeactivateOnExit<P3Landslide>();
        }

        private State P3LandslideAwakened(uint id, float delay)
        {
            ActorCastMulti(id, _module.Titan, new AID[] { AID.LandslideBoss, AID.LandslideBossAwakened }, delay, 2.2f, true, "Landslide (awakened)")
                .ActivateOnEnter<P3Landslide>();
            return ComponentCondition<P3Landslide>(id + 0x10, 2, comp => comp.Awakened ? comp.NumCasts >= 10 : (comp.NumCasts >= 5 && Module.WorldState.CurrentTime >= comp.PredictedActivation), "Landslide second hit")
                .DeactivateOnExit<P3Landslide>();
        }

        private State P3GeocrushSide(uint id, float delay)
        {
            ActorTargetable(id, _module.Titan, false, delay, "Disappear");
            ActorCast(id + 1, _module.Titan, AID.Geocrush2, 2.2f, 3, true, "Proximity");
            return ActorTargetable(id + 3, _module.Titan, true, 2.4f, "Reappear");
        }

        private State P3Tumult(uint id, float delay, uint numHits)
        {
            ComponentCondition<P3Tumult>(id, delay, comp => comp.NumCasts > 0, "Raidwide 1")
                .ActivateOnEnter<P3Tumult>()
                .SetHint(StateMachine.StateHint.Raidwide);
            return ComponentCondition<P3Tumult>(id + numHits - 1, 0.1f + 1.1f * (numHits - 1), comp => comp.NumCasts >= numHits, $"Raidwide {numHits}")
                .DeactivateOnExit<P3Tumult>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P3GeocrushEarthenFury(uint id, float delay)
        {
            ActorCast(id, _module.Titan, AID.Geocrush1, delay, 3, true, "Proximity")
                .ActivateOnEnter<P3Geocrush1>()
                .DeactivateOnExit<P3Geocrush1>();
            ActorTargetable(id + 0x10, _module.Titan, true, 2.4f, "Titan appears");
            ActorCast(id + 0x20, _module.Titan, AID.EarthenFury, 0.1f, 3, true, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P3WeightOfTheLandGeocrush(uint id, float delay)
        {
            P3WeightOfTheLandFirst(id, delay, "Puddles x2");
            P3GeocrushSide(id + 0x1000, 3)
                .DeactivateOnExit<P3WeightOfTheLand>(); // note: weight of the land typically ends ~0.3s after cast start, but sometimes slightly earlier
        }

        private void P3UpheavalGaolsLandslideTumult(uint id, float delay)
        {
            ActorCast(id, _module.Titan, AID.Upheaval, delay, 4, true, "Knockback")
                .ActivateOnEnter<P3Upheaval>()
                .ActivateOnEnter<P3Burst>() // bombs appear ~0.2s after cast start
                .DeactivateOnExit<P3Upheaval>();
            ComponentCondition<P3Gaols>(id + 0x10, 2.1f, comp => comp.CurState == P3Gaols.State.TargetSelection)
                .ActivateOnEnter<P3Gaols>();
            ComponentCondition<P3Burst>(id + 0x11, 0.4f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<P3Burst>();

            P3LandslideNormal(id + 0x20, 1.8f, "Landslide 1")
                .ActivateOnEnter<P3Burst>(); // extra bomb appears ~0.1s before landslide start
            // +0.6s: fetters for gaols
            P3LandslideNormal(id + 0x30, 2.3f, "Landslide 2")
                .DeactivateOnExit<P3Burst>(); // bomb explodes ~0.5s before landslide end

            P3Tumult(id + 0x1000, 2.1f, 8)
                .DeactivateOnExit<P3Gaols>(); // if everything is done correctly, last gaol explodes ~0.7s before raidwide
        }

        private void P3WeightOfTheLandLandslideAwakened(uint id, float delay)
        {
            // TODO: gaol voidzones disappear during cast
            P3WeightOfTheLandFirst(id, delay, "Puddles x2");

            // titan normally awakens here...
            P3LandslideAwakened(id + 0x1000, 2.8f)
                .DeactivateOnExit<P3WeightOfTheLand>(); // note: weight of the land typically ends ~0.3s after cast start
        }

        private void P3TripleWeightOfTheLandLandslideAwakenedBombs(uint id, float delay)
        {
            ComponentCondition<P3WeightOfTheLand>(id, delay, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P3WeightOfTheLand>();
            ComponentCondition<P3WeightOfTheLand>(id + 1, 3, comp => comp.NumCasts > 0, "Puddles x3")
                .ActivateOnEnter<P3Burst>(); // bombs appear immediately after first puddles cast start
            // +0.0s: second set of weights start
            // +3.0s: second set of weights end, third set start

            P3LandslideAwakened(id + 0x1000, 3.3f)
                .DeactivateOnExit<P3WeightOfTheLand>(); // third set ends ~1.5s before landslide

            ComponentCondition<P3Burst>(id + 0x2000, 2.1f, comp => comp.NumCasts >= 4, "Central bombs")
                .DeactivateOnExit<P3Burst>();
        }

        private void P3Geocrush3(uint id, float delay)
        {
            P3GeocrushSide(id, delay);
            // +0.1s: rock throw
            // +5.0s: fetters
            // +7.1s: gaol targetable
            // +14.1s: impact end
        }

        private void Phase4LahabreaUltima(uint id)
        {
            P4Lahabrea(id, 9.1f);
            P4BeforePredation(id + 0x10000, 39.7f);
            P4UltimatePredation(id + 0x20000, 3.2f);
            P4BeforeAnnihilation(id + 0x30000, 2.0f);

            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void P4Lahabrea(uint id, float delay)
        {
            ComponentCondition<P4Freefire>(id, delay, comp => comp.NumCasts > 0, "Proximity")
                .ActivateOnEnter<P4Freefire>()
                .DeactivateOnExit<P4Freefire>();
            ComponentCondition<P4MagitekBits>(id + 0x1000, 2.2f, comp => comp.Active, "Caster LB")
                .ActivateOnEnter<P4MagitekBits>();
            ComponentCondition<P4Blight>(id + 0x2000, 11.5f, comp => comp.NumCasts > 0, "Heal LB")
                .ActivateOnEnter<P4Blight>()
                .DeactivateOnExit<P4Blight>();
            ActorTargetable(id + 0x3000, _module.Lahabrea, true, 9.1f, "Melee LB")
                .DeactivateOnExit<P4MagitekBits>(); // long since gone
            ActorCast(id + 0x4000, _module.Ultima, AID.Ultima, 18, 5, true, "Tank LB");
        }

        private void P4BeforePredation(uint id, float delay)
        {
            ActorTargetable(id, _module.Ultima, true, 39.7f, "Ultima appears");
            ActorCast(id + 0x10, _module.Ultima, AID.TankPurge, 0.1f, 4, true, "Raidwide")
                .ActivateOnEnter<P4ViscousAetheroplasmApply>() // show MT hint early
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<P4ViscousAetheroplasmApply>(id + 0x20, 2.2f, comp => comp.NumCasts > 0, "Aetheroplasm apply")
                .ActivateOnEnter<P4ViscousAetheroplasmResolve>() // activate early to let component determine aetheroplasm target
                .DeactivateOnExit<P4ViscousAetheroplasmApply>();
            ActorCast(id + 0x30, _module.Ultima, AID.HomingLasers, 3.2f, 4, true, "Tankbuster")
                .ActivateOnEnter<P4HomingLasers>()
                .DeactivateOnExit<P4HomingLasers>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P4ViscousAetheroplasmResolve>(id + 0x40, 4.9f, comp => !comp.Active, "Aetheroplasm resolve")
                .DeactivateOnExit<P4ViscousAetheroplasmResolve>();
        }

        private void P4UltimatePredation(uint id, float delay)
        {
            ActorCast(id, _module.Ultima, AID.UltimatePredation, delay, 3, true);
            ActorTargetable(id + 0x10, _module.Ultima, false, 4.4f, "Disappear (predation)");

            ComponentCondition<P4WickedWheel>(id + 0x20, 10.3f, comp => comp.NumCasts > 0, "Predation 1") // wicked wheel + crimson cyclone + landslides
                .ActivateOnEnter<P4WickedWheel>()
                .ActivateOnEnter<P4CrimsonCyclone>()
                .ActivateOnEnter<P4Landslide>()
                .ActivateOnEnter<P4CeruleumVent>();
            // awakened landslided and ceruleum vent happen ~0.1s earlier, awakened cyclone and tornado happen at the same time
            ComponentCondition<P4WickedWheel>(id + 0x30, 2.1f, comp => comp.NumCasts > 1, "Predation 2")
                .DeactivateOnExit<P4WickedWheel>()
                .DeactivateOnExit<P4CrimsonCyclone>()
                .DeactivateOnExit<P4Landslide>()
                .DeactivateOnExit<P4CeruleumVent>();

            // PATE happens ~1.5s earlier
            ComponentCondition<P1FeatherRain>(id + 0x41, 4.3f, comp => comp.CastsActive)
                .ActivateOnEnter<P1FeatherRain>();
            ComponentCondition<P1FeatherRain>(id + 0x42, 1, comp => !comp.CastsActive, "Feathers")
                .DeactivateOnExit<P1FeatherRain>();
        }

        private void P4BeforeAnnihilation(uint id, float delay)
        {
            ActorTargetable(id, _module.Ultima, true, delay, "Reappear");
            ActorCast(id + 0x10, _module.Ultima, AID.PrepareIfrit, 3.2f, 2, true);

            ActorCastStart(id + 0x1000, _module.Ifrit, AID.Eruption, 4.3f, false, "Eruption baits")
                .ActivateOnEnter<P2Eruption>(); // activate early to show bait hints
            ActorCastEnd(id + 0x1001, _module.Ifrit, 2.5f, false);
            ActorCast(id + 0x1010, _module.Ultima, AID.PrepareTitan, 2.4f, 2, true);
            ComponentCondition<P2Eruption>(id + 0x1020, 2.1f, comp => comp.NumCasts >= 8)
                .ActivateOnEnter<P2InfernalFetters>() // ~1s after previous cast end
                .DeactivateOnExit<P2Eruption>();

            ActorCast(id + 0x2000, _module.Ultima, AID.RadiantPlumeUltima, 1.1f, 3.2f, true)
                .ActivateOnEnter<P2RadiantPlume>();
            ComponentCondition<P2RadiantPlume>(id + 0x2010, 0.8f, comp => comp.NumCasts > 0, "Outer plumes")
                .ActivateOnEnter<P3Burst>() // first bomb appears ~0.1s after cast end
                .DeactivateOnExit<P2RadiantPlume>();

            ComponentCondition<Landslide>(id + 0x3000, 1.4f, comp => comp.CastsActive)
                .ActivateOnEnter<Landslide>();
            ComponentCondition<Landslide>(id + 0x3001, 2.2f, comp => comp.NumCasts > 0, "Landslides first");
            ComponentCondition<Landslide>(id + 0x3002, 2, comp => !comp.CastsActive, "Landslides last")
                .DeactivateOnExit<Landslide>();

            // note: there are tumults during these casts; first cast happens ~1.3s after next cast start, 7 total
            ActorCast(id + 0x4000, _module.Ultima, AID.PrepareGaruda, 1.8f, 2, true)
                .ActivateOnEnter<P4ViscousAetheroplasmApply>(); // activate early to show hint for MT
            ComponentCondition<P4ViscousAetheroplasmApply>(id + 0x4010, 2.1f, comp => comp.NumCasts > 0, "Aetheroplasm apply")
                .ActivateOnEnter<P4ViscousAetheroplasmResolve>()
                .DeactivateOnExit<P4ViscousAetheroplasmApply>();

            ComponentCondition<WickedWheel>(id + 0x5000, 2.1f, comp => comp.Sources.Count > 0)
                .ActivateOnEnter<WickedWheel>();
            ComponentCondition<WickedWheel>(id + 0x5001, 3, comp => comp.NumCasts > 0, "Wheels")
                .DeactivateOnExit<WickedWheel>();
            // TODO: aetheroplasm resolve, shriek, deactivate bombs & fetters, feather rains, homing lasers
        }
    }
}
