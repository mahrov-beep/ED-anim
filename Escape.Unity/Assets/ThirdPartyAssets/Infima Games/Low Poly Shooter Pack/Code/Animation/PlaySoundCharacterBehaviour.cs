//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    using Sirenix.OdinInspector;

    /// <summary>
    /// Helper StateMachineBehaviour that allows us to more easily play a specific weapon sound.
    /// </summary>
    public class PlaySoundCharacterBehaviour : StateMachineBehaviour
    {
        /// <summary>
        /// Type of weapon sound.
        /// </summary>
        private enum SoundType
        {
            //Character Actions.
            GrenadeThrow, Melee,
            //Holsters.
            Holster, Unholster,
            //Normal Reloads.
            Reload, ReloadEmpty,
            //Cycled Reloads.
            ReloadOpen, ReloadInsert, ReloadClose,
            //Firing.
            Fire, FireEmpty,
            //Bolt.
            BoltAction
        }

        #region FIELDS SERIALIZED

        [Title("Setup")]
        
        [Tooltip("Delay at which the audio is played.")]
        [SerializeField]
        private float delay;
        
        [Tooltip("Type of weapon sound to play.")]
        [SerializeField]
        private SoundType soundType;
        
        [Title("Audio Settings")]

        [Tooltip("Audio Settings.")]
        [SerializeField]
        [HideLabel]
        private AudioSettings audioSettings = new AudioSettings(1.0f);

        #endregion

        #region FIELDS

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;

        /// <summary>
        /// Player Inventory.
        /// </summary>
        private InventoryBehaviour playerInventory;

        #endregion
        
        #region UNITY

        /// <summary>
        /// On State Enter.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //We need to get the character component.
            playerCharacter ??= animator.GetComponent<CharacterBehaviour>();

            //Get Inventory.
            playerInventory ??= playerCharacter.GetInventory();

            //Try to get the equipped weapon's Weapon component.
            if (!(playerInventory.GetEquipped() is { } weaponBehaviour))
                return;

            //Switch.
            (CharacterAudioLayers layer, AudioClip clip) audio = soundType switch
            {
                //Grenade Throw.
                SoundType.GrenadeThrow => (CharacterAudioLayers.Fire, playerCharacter.GetConfig().audioClipsGrenadeThrow.GetClip()),
                //Melee.
                SoundType.Melee => (CharacterAudioLayers.Action, playerCharacter.GetConfig().audioClipsMelee.GetClip()),
                
                //Holster.
                SoundType.Holster => (CharacterAudioLayers.Action, weaponBehaviour.GetAudioClipHolster()),
                //Unholster.
                SoundType.Unholster => (CharacterAudioLayers.Action, weaponBehaviour.GetAudioClipUnholster()),
                
                SoundType.Reload => default,      // Moved to CharacterAudioPlayer
                SoundType.ReloadEmpty => default, // Moved to CharacterAudioPlayer
                
                //Reload Open.
                SoundType.ReloadOpen => (CharacterAudioLayers.Reload, weaponBehaviour.GetAudioClipReloadOpen()),
                //Reload Insert.
                SoundType.ReloadInsert => (CharacterAudioLayers.Reload, weaponBehaviour.GetAudioClipReloadInsert()),
                //Reload Close.
                SoundType.ReloadClose => (CharacterAudioLayers.Reload, weaponBehaviour.GetAudioClipReloadClose()),
                
                SoundType.Fire => default,      // moved to CharacterAudioPlayer
                SoundType.FireEmpty => default, // moved to CharacterAudioPlayer
                
                //Bolt Action.
                SoundType.BoltAction => (CharacterAudioLayers.Action, weaponBehaviour.GetAudioClipBoltAction()),
                
                //Default.
                _ => default
            };

            this.playerCharacter.GetAudioPlayer().PlayOneShot(audio.layer, audio.clip, this.audioSettings.Volume, this.delay);
        }
        
        #endregion
    }
}