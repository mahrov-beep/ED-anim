namespace Game.ECS.Systems.Camera {
    using Components.Audio;
    using Multicast;
    using Scellecs.Morpeh;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class AudioListenerSystem : SystemBase {
        [Inject] private Stash<AmbientAudioComponent> stashAmbient;

        private Filter ambientFilter;
        
        private SingletonFilter<AudioListenerComponent> audioListenerFilter;

        public override void OnAwake() {
            this.ambientFilter = this.World.Filter.With<AmbientAudioComponent>().Build();
            
            this.audioListenerFilter = this.World
                .FilterSingleton<AudioListenerComponent>();
        }

        public override void OnUpdate(float deltaTime) {
            if (!this.audioListenerFilter.IsValid) {
                return;
            }

            var audioListenerTransform = this.audioListenerFilter.Instance.audioListener.transform;

            foreach (var entity in this.ambientFilter) {
                ref var ambient = ref this.stashAmbient.Get(entity);

                var maxDistance    = ambient.audioSource.maxDistance;
                
                var sqrMaxDistance = maxDistance * maxDistance;

                var state = (audioListenerTransform.position - ambient.transform.position).sqrMagnitude <= sqrMaxDistance;

                if (state != ambient.audioSource.enabled) {
                    ambient.audioSource.enabled = state;
                }
            }

        }
    }
}