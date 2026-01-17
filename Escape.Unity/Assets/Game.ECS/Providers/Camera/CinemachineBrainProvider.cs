namespace Game.ECS.Providers.Camera {
    using Unity.Cinemachine;
    using Components.Camera;
    using Scellecs.Morpeh.Providers;
    using UnityEngine;

    public class CinemachineBrainProvider : MonoProvider<CinemachineBrainComponent> {
        private void Reset() {
            this.GetData().brain = this.GetComponent<CinemachineBrain>();

            this.GetData().brain.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
            this.GetData().Transform = transform;
        }
    }
}