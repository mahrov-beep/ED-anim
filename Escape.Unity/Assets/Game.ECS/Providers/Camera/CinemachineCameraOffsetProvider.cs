namespace Game.ECS.Providers.Camera {
    using Components.Camera;
    using Scellecs.Morpeh.Providers;
    using Unity.Cinemachine;

    public class CinemachineCameraOffsetProvider : MonoProvider<CinemachineCameraOffsetComponent> {
        private void Reset() {
            this.GetData().cameraOffset = this.GetComponent<CinemachineCameraOffset>();
        }
    }
}