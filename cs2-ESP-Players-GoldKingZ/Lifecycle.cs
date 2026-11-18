using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using ESP_Players_GoldKingZ.Config;

namespace ESP_Players_GoldKingZ;

public partial class MainPlugin
{
    private bool _hooksRegistered;
    private bool _runtimeActive;
    private int _runtimeGeneration;
    private bool _reconcilePending;
    private Dictionary<int, uint>? _pendingSpawns;

    internal bool StartEspHooks()
    {
        if (_hooksRegistered) return false;
        _hooksRegistered = true;
        _runtimeActive = true;
        return true;
    }

    internal bool StopEspHooks()
    {
        if (!_hooksRegistered) return false;
        _hooksRegistered = false;
        StopEspRuntime();
        return true;
    }

    internal void StopEspRuntime()
    {
        _runtimeActive = false;
        _runtimeGeneration++;
        _reconcilePending = false;
        _pendingSpawns = null;
    }

    public void OnMapStart(string mapname)
    {
        StopEspRuntime();
        // The old map owns its timer and entities.
        g_Main.Timer = null!;
        g_Main.Player_Data.Clear();
        ClearTransmitCache();
        _runtimeActive = _hooksRegistered;
        Helper.StartTimer();
        ScheduleEspReconcile();
    }

    internal CounterStrikeSharp.API.Modules.Timers.Timer CreateEspTimer()
    {
        int generation = _runtimeGeneration;
        return AddTimer(3.0f, () =>
        {
            if (_runtimeActive && generation == _runtimeGeneration && g_EnablePlugin.Value)
                Helper.ESP_Timer();
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    internal HookResult OnEspSpawn(EventPlayerSpawn ev, GameEventInfo info)
    {
        var player = ev.Userid;
        if (!_runtimeActive || player == null || !player.IsValid) return HookResult.Continue;

        Helper.CheckPlayerInGlobals(player);
        if (!_reconcilePending || _pendingSpawns != null)
        {
            _pendingSpawns ??= new();
            _pendingSpawns[player.Slot] = player.EntityHandle.Raw;
            QueueReconcileFrame();
        }
        return HookResult.Continue;
    }

    internal HookResult OnEspTeam(EventPlayerTeam ev, GameEventInfo info)
    {
        ScheduleEspReconcile();
        return HookResult.Continue;
    }

    internal HookResult OnEspBotTakeover(EventBotTakeover ev, GameEventInfo info)
    {
        ScheduleEspReconcile();
        return HookResult.Continue;
    }

    internal void OnEspClientDisconnect(int slot)
    {
        if (g_Main.Player_Data.TryGetValue(slot, out var data))
        {
            Helper.RemoveGlowData(data);
            if (!Configs.Instance.Give_ESP.Give_ESP_SaveOnDisconnect)
                g_Main.Player_Data.Remove(slot);
        }
        ForgetViewer(slot);
        ScheduleEspReconcile();
    }

    internal void ScheduleEspReconcile()
    {
        _pendingSpawns = null;
        QueueReconcileFrame();
    }

    private void QueueReconcileFrame()
    {
        if (!_runtimeActive || _reconcilePending) return;
        _reconcilePending = true;
        int generation = _runtimeGeneration;
        Server.NextFrame(() =>
        {
            if (generation != _runtimeGeneration) return;
            _reconcilePending = false;
            var spawns = _pendingSpawns;
            _pendingSpawns = null;
            if (_runtimeActive && g_EnablePlugin.Value) Helper.ESP_Timer(spawns);
        });
    }

    internal void ScheduleConfigReload()
    {
        int generation = _runtimeGeneration;
        Server.NextFrame(() =>
        {
            if (generation != _runtimeGeneration || !_runtimeActive || !g_EnablePlugin.Value) return;
            Helper.RemoveRegisterCommandsAndHooks();
            Helper.ClearVariables();
            Configs.Load(ModuleDirectory, true);
            Helper.RegisterCommandsAndHooks();
            Helper.ReloadPlayersGlobals();
        });
    }
}
