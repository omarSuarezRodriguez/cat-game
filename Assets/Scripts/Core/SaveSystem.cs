using System;
using System.IO;
using UnityEngine;
using WhiskerHaven.Utils;

namespace WhiskerHaven.Core
{
    public class SaveSystem : Singleton<SaveSystem>
    {
        private string _savePath;
        private SaveData _current;
        private float _autoSaveTimer;
        private float _autoSaveInterval = 60f;

        public SaveData Current => _current;
        public bool HasSave { get; private set; }

        protected override void OnAwake()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "whisker_save_v1.json");
            Debug.Log($"[SaveSystem] Save path: {_savePath}");
        }

        private void Start()
        {
            // Auto-save interval pulled from config after GameManager initialises
        }

        public void SetAutoSaveInterval(float seconds)
        {
            _autoSaveInterval = seconds;
        }

        private void Update()
        {
            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= _autoSaveInterval)
            {
                _autoSaveTimer = 0f;
                Save();
            }
        }

        // ── Load ────────────────────────────────────────────────────────────
        public SaveData Load()
        {
            if (File.Exists(_savePath))
            {
                try
                {
                    string json = File.ReadAllText(_savePath);
                    _current = JsonUtility.FromJson<SaveData>(json);
                    HasSave = true;
                    Debug.Log("[SaveSystem] Save loaded successfully.");
                    EventBus.Publish(new OnGameLoaded());
                    return _current;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Failed to load save: {e.Message}. Creating new save.");
                }
            }

            _current = CreateNewSave();
            HasSave = false;
            return _current;
        }

        // ── Save ────────────────────────────────────────────────────────────
        public void Save()
        {
            if (_current == null) return;
            try
            {
                _current.lastSaveTimeUtc = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(_current, true);

                // Write to temp first, then replace (atomic-ish)
                string tempPath = _savePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(_savePath)) File.Delete(_savePath);
                File.Move(tempPath, _savePath);

                EventBus.Publish(new OnGameSaved());
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save: {e.Message}");
            }
        }

        // ── Delete ──────────────────────────────────────────────────────────
        public void DeleteSave()
        {
            if (File.Exists(_savePath)) File.Delete(_savePath);
            _current = CreateNewSave();
            HasSave = false;
            Debug.Log("[SaveSystem] Save deleted.");
        }

        // ── Backup ──────────────────────────────────────────────────────────
        public void CreateBackup()
        {
            if (!File.Exists(_savePath)) return;
            string backupPath = _savePath + $".bak_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            File.Copy(_savePath, backupPath, true);
        }

        private SaveData CreateNewSave()
        {
            return new SaveData
            {
                lastSaveTimeUtc = DateTime.UtcNow.ToString("o"),
                lastLoginDateUtc = DateTime.UtcNow.ToString("o"),
                dailyMissionResetTimeUtc = DateTime.UtcNow.ToString("o"),
                snuggles = 50,
                blueprints = 10,
                hallLevel = 1,
                loginDays = 1
            };
        }

        // ── Offline Time ─────────────────────────────────────────────────────
        public double GetSecondsOffline()
        {
            if (_current == null || string.IsNullOrEmpty(_current.lastSaveTimeUtc))
                return 0;
            if (DateTime.TryParse(_current.lastSaveTimeUtc, out DateTime lastSave))
            {
                double seconds = (DateTime.UtcNow - lastSave).TotalSeconds;
                return Math.Max(0, seconds);
            }
            return 0;
        }

        protected override void OnApplicationQuit()
        {
            Save();
            base.OnApplicationQuit();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) Save();
        }
    }
}
