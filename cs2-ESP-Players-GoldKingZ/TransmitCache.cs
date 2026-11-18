using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using ESP_Players_GoldKingZ.Config;

namespace ESP_Players_GoldKingZ;

public partial class MainPlugin
{
    private readonly record struct TargetState(int Slot, byte Team, bool Valid, int Relay, int Glow);

    private sealed class TransmitPlan
    {
        public long Version = -1;
        public byte Team;
        public bool HideAll;
        public readonly List<int> Remove = new();
    }

    private List<TargetState> _targets = new();
    private List<TargetState> _nextTargets = new();
    private readonly Dictionary<int, TransmitPlan> _transmitPlans = new();
    private readonly HashSet<uint> _trackedEntities = new();
    private long _planVersion;
    private bool _enemyOnly;

    internal static bool ShowOnlyEnemyTeam(Config.Config config)
        => config.ShowOnlyEnemyTeam && config.Show_ESP_For is 0 or 1;

    internal void ForgetViewer(int slot) => _transmitPlans.Remove(slot);

    internal void ClearTransmitCache()
    {
        _trackedEntities.Clear();
        _transmitPlans.Clear();
        _targets.Clear();
        _nextTargets.Clear();
    }

    internal void TrackEspEntity(CDynamicProp prop) => _trackedEntities.Add(prop.EntityHandle.Raw);

    internal bool IsEspEntityAlive(CDynamicProp? prop)
        => prop != null && prop.IsValid && _trackedEntities.Contains(prop.EntityHandle.Raw);

    internal void RemoveEspEntity(CDynamicProp? prop)
    {
        if (prop != null && _trackedEntities.Remove(prop.EntityHandle.Raw) && prop.IsValid)
            prop.Remove();
    }

    internal void OnEspEntityDeleted(CEntityInstance entity)
    {
        if (_trackedEntities.Remove(entity.EntityHandle.Raw))
            ScheduleEspReconcile();
    }

    private int LiveIndex(CDynamicProp? prop) => IsEspEntityAlive(prop) ? (int)prop!.Index : -1;

    private void CaptureTargets(bool enemyOnly)
    {
        _nextTargets.Clear();
        foreach (var (slot, data) in g_Main.Player_Data)
        {
            int relay = LiveIndex(data.ModelRelay);
            int glow = LiveIndex(data.ModelGlow);
            if (relay < 0 && glow < 0) continue;

            var player = data.Player;
            bool valid = player.IsValid;
            _nextTargets.Add(new TargetState(slot, valid && enemyOnly ? player.TeamNum : (byte)0,
                valid, relay, glow));
        }

        if (_enemyOnly == enemyOnly && _targets.Count == _nextTargets.Count)
        {
            int index = 0;
            while (index < _targets.Count && _targets[index] == _nextTargets[index]) index++;
            if (index == _targets.Count) return;
        }

        (_targets, _nextTargets) = (_nextTargets, _targets);
        _enemyOnly = enemyOnly;
        _planVersion++;
    }

    private bool ShouldHideAllGlow(CCSPlayerController viewer, int slot, byte team, Config.Config config)
    {
        if (_prefs == null) return true;
        if (viewer.IsHLTV)
        {
            if (config.DisableGlowOnGOTV) return true;
        }
        else
        {
            bool toggled = _prefs.TryGetValue(slot, out var prefs) && prefs.Toggle_ESP;
            bool granted = g_Main.Player_Data.TryGetValue(slot, out var data) && data.Gived_ESP;
            if (!toggled && !granted) return true;
        }

        return (config.Show_ESP_For == 1 && viewer.IsAlive(true))
            || (config.Show_ESP_For == 2 && team > (byte)CsTeam.Spectator);
    }

    private void ApplyTransmitPlans(CCheckTransmitInfoList infos)
    {
        int count = infos.Count;
        if (_trackedEntities.Count == 0 || count == 0) return;

        var config = Configs.Instance;
        bool enemyOnly = ShowOnlyEnemyTeam(config);
        CaptureTargets(enemyOnly);
        if (_targets.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var (info, viewer) = infos[i];
            if (viewer == null || !viewer.IsValid) continue;

            int slot = viewer.Slot;
            byte team = enemyOnly || config.Show_ESP_For == 2 ? viewer.TeamNum : (byte)0;
            bool hideAll = ShouldHideAllGlow(viewer, slot, team, config);
            byte filterTeam = enemyOnly && !hideAll ? team : (byte)0;
            if (!_transmitPlans.TryGetValue(slot, out var plan))
                _transmitPlans.Add(slot, plan = new TransmitPlan());

            if (plan.Version != _planVersion || plan.Team != filterTeam || plan.HideAll != hideAll)
            {
                plan.Remove.Clear();
                foreach (var target in _targets)
                {
                    if (!hideAll && target.Slot != slot && target.Valid
                        && (!enemyOnly || target.Team != filterTeam)) continue;

                    if (target.Relay >= 0) plan.Remove.Add(target.Relay);
                    if (target.Glow >= 0) plan.Remove.Add(target.Glow);
                }

                plan.Version = _planVersion;
                plan.Team = filterTeam;
                plan.HideAll = hideAll;
            }

            foreach (int index in plan.Remove) info.TransmitEntities.Remove(index);
        }
    }
}
