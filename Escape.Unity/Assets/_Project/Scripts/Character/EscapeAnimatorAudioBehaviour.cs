using InfimaGames.LowPolyShooterPack;
using Sirenix.OdinInspector;
using UnityEngine;

public class EscapeAnimatorAudioBehaviour : MonoBehaviour {
    [SerializeField, Required] private EscapeCharacterBehaviour   characterBehaviour;
    [SerializeField, Required] private EscapeCharacterQuantumView quantumView;

    // вызывается из анимации ходьбы
    public void StepSound() {
        if (this.quantumView.CurrentSpeedCoefficient > 0.1f) {
            var clip = this.characterBehaviour.IsCrouching() ? this.characterBehaviour.GetConfig().audioClipsStepCrouching : 
                this.characterBehaviour.IsRunning() ? this.characterBehaviour.GetConfig().audioClipStepRunning : this.characterBehaviour.GetConfig().audioClipsStep;
            
            this.characterBehaviour.GetAudioPlayer().PlayOneShot(CharacterAudioLayers.Env, clip, speedNormalized: this.quantumView.CurrentSpeedCoefficient);
        }
    }
}
