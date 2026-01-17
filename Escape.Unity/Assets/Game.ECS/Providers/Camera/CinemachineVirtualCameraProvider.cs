namespace Game.ECS.Providers.Camera {
    using System;
    using System.Linq;
    using Unity.Cinemachine;
    using Components.Camera;
    using Scellecs.Morpeh.Providers;

    public class CinemachineVirtualCameraProvider : MonoProvider<CinemachineVirtualCameraComponent> {
        private void Reset() {
            ref var data = ref GetData();
            data.camera = GetComponent<CinemachineCamera>();
        }

        protected override void Initialize() {
            ref var data = ref GetData();
            data.extensionsMap = GetComponents<CinemachineExtension>().ToDictionary(ext => ext.GetType());
        }
    }
}