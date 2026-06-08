using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiskerHaven.Core
{
    // ── Game Events ─────────────────────────────────────────────────────────
    public struct OnResourceChanged   { public ResourceType Type; public double NewAmount; public double Delta; }
    public struct OnCatRescued        { public string CatId; }
    public struct OnCatHappinessChanged { public string CatId; public float Happiness; }
    public struct OnHabitatUpgraded   { public string HabitatId; public int NewLevel; }
    public struct OnMissionCompleted  { public string MissionId; }
    public struct OnMissionProgress   { public string MissionId; public float Progress; }
    public struct OnAchievementUnlocked { public string AchievementId; }
    public struct OnPurrPowerChanged  { public float Multiplier; }
    public struct OnOfflineProgressReady { public double SnugglesEarned; public double TimeAwaySeconds; }
    public struct OnTutorialStep      { public int Step; }
    public struct OnSceneAreaChanged  { public string AreaId; public int Level; }
    public struct OnVolunteerAssigned { public string VolunteerId; public string HabitatId; }
    public struct OnDailyReset        { }
    public struct OnGameSaved         { }
    public struct OnGameLoaded        { }

    public enum ResourceType { Snuggles, GoldenPaw, Blueprints }

    // ── EventBus ────────────────────────────────────────────────────────────
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<object>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<object>();
            _handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public static void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list)) return;

            // iterate on copy to allow handlers to unsub during dispatch
            var copy = new List<object>(list);
            foreach (var h in copy)
            {
                try { ((Action<T>)h)(evt); }
                catch (Exception e) { Debug.LogError($"[EventBus] Handler error for {type.Name}: {e}"); }
            }
        }

        public static void Clear() => _handlers.Clear();
    }
}
