﻿using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using BF2D.Combat;

namespace BF2D.Game
{
    public class GameInfo : MonoBehaviour
    {
        //Singleton Reference
        public static GameInfo Instance { get { return GameInfo.instance; } }
        private static GameInfo instance;

        public float ClockSpeed { get { return this.clockSpeed; } }

        [Header("Global Game Settings")]
        [SerializeField] private float clockSpeed = 0.03125f;

        [Header("Data File Paths")]
        [SerializeField] private string savesPath = "Saves";
        [SerializeField] private string playersPath = "Players";
        [SerializeField] private string enemiesPath = "Enemies";
        [SerializeField] private string itemsPath = "Items";
        [SerializeField] private string equipmentsPath = "Equipments";
        [SerializeField] private string statusEffectsPath = "StatusEffects";

        public List<CharacterStats> Players { get { return this.currentSave.Players; } }
        private SaveData currentSave = null;

        private readonly JsonStringCache<Item> items = new();
        private readonly JsonObjectCache<Equipment> equipments = new();
        private readonly JsonStringCache<StatusEffect> statusEffects = new();

        //AssetCollections
        [Header("Asset Collections")]
        [SerializeField] private SpriteCollection iconCollection = null;
        [SerializeField] private AudioClipCollection soundEffectCollection = null;

        public CombatManager.InitializeInfo UnstageCombatInfo()
        { 
            if (this.queuedCombats.Count < 1)
            {
                return null;
            }
            return this.queuedCombats.Dequeue(); 
        }
        private readonly Queue<CombatManager.InitializeInfo> queuedCombats = new();

        private void Awake()
        {
            //Set this object not to destroy on loading new scenes
            DontDestroyOnLoad(this.gameObject);

            //Setup of Monobehaviour Singleton
            if (GameInfo.instance != this && GameInfo.instance != null)
            {
                Destroy(GameInfo.instance.gameObject);
            }

            GameInfo.instance = this;

            TEST_INITIALIZE();
        }

        private void TEST_INITIALIZE()
        {
            Debug.Log($"Streaming Assets Path: {Application.streamingAssetsPath}");
            Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");

            this.currentSave = LoadSaveData("save1");
            List<CharacterStats> enemies = new()
            {
                LoadEnemy("lessergoblin")
            };

            this.queuedCombats.Enqueue(new CombatManager.InitializeInfo
            {
                players = this.Players,
                enemies = enemies
            });
        }

        public Sprite GetIcon(string key)
        {
            if (key == string.Empty)
            {
                Debug.LogWarning("[GameInfo:GetIcon] String was empty");
                return null;
            }
            return this.iconCollection[key];
        }

        public AudioClip GetSoundEffect(string key)
        {
            if (key == string.Empty)
            {
                Debug.LogWarning("[GameInfo:GetSoundEffect] String was empty");
                return null;
            }
            return this.soundEffectCollection[key];
        }

        public Item InstantiateItem(string key)
        {
            if (key == string.Empty)
            {
                Debug.LogWarning("[GameInfo:InstantiateItem] String was empty");
                return null;
            }
            this.items.Datapath = Path.Combine(Application.streamingAssetsPath, this.itemsPath);
            Item item = this.items.Get(key);
            return item;
        }

        public Equipment GetEquipment(string key)
        {
            if (key == string.Empty)
            {
                Debug.LogWarning("[GameInfo:GetEquipment] String was empty");
                return null;
            }
            this.equipments.Datapath = Path.Combine(Application.streamingAssetsPath, this.equipmentsPath);
            Equipment equipment = this.equipments.Get(key);
            return equipment;
        }

        public StatusEffect InstantiateStatusEffect(string key)
        {
            if (key == string.Empty)
            {
                Debug.LogWarning("[GameInfo:InstantiateStatusEffect] String was empty");
                return null;
            }
            this.statusEffects.Datapath = Path.Combine(Application.streamingAssetsPath, this.statusEffectsPath);
            StatusEffect statusEffect = this.statusEffects.Get(key);
            return statusEffect;
        }

        public CharacterStats LoadEnemy(string key)
        {
            if (key == string.Empty)
            {
                Debug.LogWarning("[GameInfo:LoadEnemy] String was empty");
                return null;
            }
            string content = BF2D.Utilities.TextFile.LoadFile(Path.Combine(Application.streamingAssetsPath, this.enemiesPath, key + ".json"));
            return BF2D.Utilities.TextFile.DeserializeString<CharacterStats>(content).Setup();
        }

        public void AddPlayer(string playerKey, string newName)
        {
            CharacterStats newPlayer = LoadPlayer(playerKey);
            if (newPlayer == null)
            {
                Debug.LogWarning("[GameInfo:AddPlayer] LoadPlayer failed");
                return;
            }
            newPlayer.SetName(newName);
            this.currentSave.AddPlayer(newPlayer);
        }

        private CharacterStats LoadPlayer(string key)
        {
            if (key == string.Empty)
            {
                Debug.LogWarning("[GameInfo:LoadPlayer] String was empty");
                return null;
            }
            string content = BF2D.Utilities.TextFile.LoadFile(Path.Combine(Application.streamingAssetsPath, this.playersPath, key + ".json"));
            return BF2D.Utilities.TextFile.DeserializeString<CharacterStats>(content).Setup();
        }

        private SaveData LoadSaveData(string saveKey)
        {
            if (saveKey == string.Empty)
            {
                Debug.LogWarning("[GameInfo:LoadEnemy] String was empty");
                return null;
            }
            string content = BF2D.Utilities.TextFile.LoadFile(Path.Combine(Application.persistentDataPath, this.savesPath, saveKey + ".json"));
            SaveData saveData = BF2D.Utilities.TextFile.DeserializeString<SaveData>(content);

            foreach (CharacterStats character in saveData.Players)
            {
                character.Setup();
            }

            return saveData;
        }
    }

}