using Dalamud.Game.ClientState.Objects.Types;

namespace BossMod;

sealed class IPCProvider : IDisposable
{
    private Action? _disposeActions;

    public IPCProvider(BossModuleManager _bossmod)
    {
        // TODO: this really needs to be reconsidered, this exposes implementation detail
        // for usecase description, see PR 330 - really AI itself should handle heal range
        Register("ActiveModuleComponentBaseList", () => _bossmod.ActiveModule?.Components.Select(c => c.GetType().BaseType?.Name).ToList() ?? default);
        Register("ActiveModuleComponentList", () => _bossmod.ActiveModule?.Components.Select(c => c.GetType().Name).ToList() ?? default);
        Register("ActiveModuleHasComponent", (string name) => _bossmod.ActiveModule?.Components.Any(c => c.GetType().Name == name || c.GetType().BaseType?.Name == name) ?? false);

        Register("HasModule", (GameObject obj) => ModuleRegistry.FindByOID(obj.DataId) != null);
        Register("IsMoving", () => ActionManagerEx.Instance!.InputOverride.IsMoving());
        Register("ForbiddenZonesCount", () => 0);
        Register("InitiateCombat", () => { });
        Register("SetAutorotationState", (bool state) => false);
    }

    public void Dispose() => _disposeActions?.Invoke();

    private void Register<TRet>(string name, Func<TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    private void Register<TRet, T1>(string name, Func<TRet, T1> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<TRet, T1>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    private void Register(string name, Action func)
    {
        var p = Service.PluginInterface.GetIpcProvider<object>("BossMod." + name);
        p.RegisterAction(func);
        _disposeActions += p.UnregisterAction;
    }

    //private void Register<T1>(string name, Action<T1> func)
    //{
    //    var p = Service.PluginInterface.GetIpcProvider<T1, object>("BossMod." + name);
    //    p.RegisterAction(func);
    //    _disposeActions += p.UnregisterAction;
    //}
}
