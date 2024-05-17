namespace BossMod.AI;

[ConfigDisplay(Name = "AI设置 (非常实验性!!!)", Order = 6)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("启用 AI")]
    public bool Enabled = false;

    [PropertyDisplay("渲染 UI")]
    public bool DrawUI = true;

    [PropertyDisplay("跟随队长")]
    public bool FollowLeader = true;

    [PropertyDisplay("聚焦目标队长")]
    public bool FocusTargetLeader = true;

    [PropertyDisplay("广播按键到其他窗口")]
    public bool BroadcastToSlaves = false;
}
