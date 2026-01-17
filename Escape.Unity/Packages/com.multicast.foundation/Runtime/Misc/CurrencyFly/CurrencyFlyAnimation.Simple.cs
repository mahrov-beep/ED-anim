namespace Multicast.Misc.CurrencyFly {
    using System;
    using System.Collections.Generic;
    using BrunoMikoski.AnimationSequencer;
    using Cysharp.Threading.Tasks;
    using TMPro;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public partial class CurrencyFlyAnimation {
        public class Simple : ICurrencyFlyAnimationPlayer {
            private readonly float      spawnInterval;
            private readonly GameObject particlePrefab;

            public int                   ParticleCount { get; set; }
            public CurrencyFlySourceInfo Source        { get; set; }
            public RectTransform         Destination   { get; set; }
            public string                CurrencyKey   { get; set; }

            public List<IDisposable> DelayedVisualizations { get; set; }

            public Simple(GameObject particlePrefab = null, float spawnInterval = 0.07f) {
                this.spawnInterval  = spawnInterval;
                this.particlePrefab = particlePrefab ? particlePrefab : Resources.Load<GameObject>("CurrencyFly Particle");
            }

            void ICurrencyFlyAnimationPlayer.Setup() {
            }

            async UniTask ICurrencyFlyAnimationPlayer.PlayParticle(int index) {
                await UniTask.Delay(TimeSpan.FromSeconds(index * this.spawnInterval));

                var particleObj = Object.Instantiate(this.particlePrefab, this.Destination, false);

                particleObj.transform.SetPositionAndRotation(this.Source.position, Quaternion.identity);
                particleObj.transform.SetAsFirstSibling();

                if (particleObj.TryGetComponent(out RectTransform particleObjRect)) {
                    var destinationSize = this.Destination.rect.size;

                    particleObjRect.sizeDelta = destinationSize;
                    particleObjRect.localScale = new Vector3(
                        this.Source.size.x / destinationSize.x,
                        this.Source.size.y / destinationSize.y,
                        1
                    );
                }

                if (particleObj.TryGetComponent(out TMP_Text tmpText)) {
                    tmpText.text = $"<sprite name={this.CurrencyKey}>";
                }

                if (particleObj.TryGetComponent(out AnimationSequencerController particleSequencer)) {
                    particleSequencer.PlayForward(onCompleteCallback: () => Object.Destroy(particleObj));

                    await UniTask.WaitWhile(() => particleObj != null && particleObj.activeSelf && Application.isPlaying);
                }
                else {
                    Object.Destroy(particleObj);

                    Debug.LogError($"Prefab '{this.particlePrefab.name}' does not contains AnimationSequencerController component");
                }

                this.DelayedVisualizations[index].Dispose();
            }
        }
    }
}