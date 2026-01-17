using _Project.Scripts.GameView;
using InfimaGames.LowPolyShooterPack;
using Quantum;
using Sirenix.OdinInspector;
using UnityEngine;

public class EscapeCharacterAudio : QuantumEntityViewComponent<CustomViewContext> {
    [SerializeField, Required] private EscapeCharacterBehaviour characterBehaviour;
    
    private void Start() {
        QuantumEvent.Subscribe<EventAttackHitSynced>(this, this.OnAttackHit, onlyIfActiveAndEnabled: true, onlyIfEntityViewBound: true);
    }

    private void OnAttackHit(EventAttackHitSynced callback) {
        if (this.characterBehaviour.GetCharacterType() != CharacterTypes.LocalView) {
            return;
        }

        var entityRef = this.EntityRef;
        
        if (entityRef == EntityRef.None) {
            return;
        }

        var attack      = callback.attack;
        var isTargetHit = callback.targetRef == entityRef;
        var isSourceHit = attack.SourceUnitRef == entityRef;

        if (!isTargetHit && !isSourceHit) {
            return;
        }

        var audioPlayer = this.characterBehaviour.GetAudioPlayer();
        
        if (isTargetHit) {
            audioPlayer.PlayOneShot(CharacterAudioLayers.Hits, this.characterBehaviour.GetConfig().audioClipsHit, useBoosts: false);
            return;
        }

        var clipSettings = attack.IsHeadshot ? this.characterBehaviour.GetConfig().audioHeadshotEnemy : this.characterBehaviour.GetConfig().audioHitEnemy;

        if (clipSettings != null) {
            audioPlayer.PlayOneShot(CharacterAudioLayers.Hits, clipSettings, useBoosts: false);
        }
    }
}