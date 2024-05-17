namespace BossMod;

public class DemoModule : BossModule
{
    private class DemoComponent(BossModule module) : BossComponent(module)
    {
        public override void AddHints(int slot, Actor actor, TextHints hints)
        {
            hints.Add("提示", false);
            hints.Add("警告");
        }

        public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
        {
            movementHints.Add(actor.Position, actor.Position + new WDir(10, 10), ArenaColor.Danger);
        }

        public override void AddGlobalHints(GlobalHints hints)
        {
            hints.Add("全局提示");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc)
        {
            Arena.ZoneCircle(Module.Center, 10, ArenaColor.AOE);
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc)
        {
            Arena.Actor(Module.Center, 0.Degrees(), ArenaColor.PC);
        }
    }

    public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
    {
        ActivateComponent<DemoComponent>();
    }
}
