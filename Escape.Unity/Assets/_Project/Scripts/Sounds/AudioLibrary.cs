using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Escape.Audio {
    using UnityEngine.Serialization;

    [CreateAssetMenu (menuName = "ESCAPE/AudioLibrary")]
    public class AudioLibrary : ScriptableObject
    {
        [SerializeField] private SoundTypeToClip clipsLibrary;

        public SoundData GetSoundData(SoundType soundType) {
            return this.clipsLibrary[soundType];
        }
        
    #if UNITY_EDITOR
        private void OnEnable() {
            this.UpdateKeys();
        }
    #endif

        [Button]
        private void UpdateKeys() 
        {
            foreach (SoundType soundType in Enum.GetValues(typeof(SoundType))) 
            {
                this.clipsLibrary.TryAdd(soundType, new SoundData());
            }
        }
    }

    public enum SoundType {
        // music = 0,
        
        //Character
        GetDamage = 1,
        Dead = 2,
        Step = 3,
        ReloadStart = 4,
        ReloadEnd = 5,
        HideWeapon = 6,
        ShowWeapon = 7,
        OpenInventory = 8,
        OpenLootBox = 9,
        CloseLootBox = 10,
        BotHasTarget = 11,
        BotLostTarget = 12,
        BotReloading = 13,
        BotStrafing = 14,
        ItemBoxTimerOpening = 15,
        AimIn = 16,
        AimOut = 17,
        Roll = 18,
        
        //UI
        // BtnClick = 50,
        // Buy = 51,
        // Sell = 52,
        // SelectItem = 53,
        // MoveItem = 54,
        // EquipItem_Helmet = 55,
        // EquipItem_Armor = 56,
        // EquipItem_Weapon = 57,
        // EquipItem_Backpack = 58,
        // EquipItem_Skill = 59,
        // EquipItem_Perk = 60,
        
        //Events
        // Win = 100,
        // Lose = 101,
    }

    [Serializable]
    public struct SoundData {
        public bool isMultiSound;
        
        [HideIf("isMultiSound")]
        public AudioClip clip;
        
        public float playChance;
        
        [ShowIf("isMultiSound")]
        public AudioClip[] clips;

        public float minDistance;
        public float maxDistance;
        public float volume;
        public int   priority;
    } 
    
    [Serializable]
    public class SoundTypeToClip : UnitySerializedDictionary<SoundType, SoundData> { } 

    [Serializable]
    public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>,
        ISerializationCallbackReceiver {
        [SerializeField, HideInInspector]
        private List<TKey> keyData = new List<TKey>();

        [SerializeField, HideInInspector]
        private List<TValue> valueData = new List<TValue>();

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            this.Clear();
            for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++) {
                this[this.keyData[i]] = this.valueData[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            this.keyData.Clear();
            this.valueData.Clear();

            foreach (var item in this) {
                this.keyData.Add(item.Key);
                this.valueData.Add(item.Value);
            }
        }
    }
}
