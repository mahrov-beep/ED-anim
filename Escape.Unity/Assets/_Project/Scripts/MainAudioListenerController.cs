using Game.ECS.Providers.Audio;
using Sirenix.OdinInspector;
using UnityEngine;

public class MainAudioListenerController : MonoBehaviour {
    [SerializeField, Required] private AudioListenerProvider audioListenerProvider;
    
    private Camera activeMainCamera;

    private void Update() {
        var mainCamera = Camera.main;

        if (this.activeMainCamera == mainCamera) {
            return;
        }

        if (this.activeMainCamera) {
            Destroy(this.activeMainCamera.gameObject.GetComponent<AudioListener>());
        }

        if (mainCamera) {
            var audioListener = mainCamera.gameObject.AddComponent<AudioListener>();
            
            this.audioListenerProvider.GetData().audioListener = audioListener;
        }

        this.activeMainCamera = mainCamera;
    }
}