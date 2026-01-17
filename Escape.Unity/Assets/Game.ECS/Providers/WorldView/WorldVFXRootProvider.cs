namespace Game.ECS.Components.WorldView {
    using Scellecs.Morpeh.Providers;
    using UnityEngine;

    public class WorldVFXRootProvider : MonoProvider<WorldVFXRoot> {
        private void Reset() {
            ref var data = ref GetData();
            data.Root = GetComponent<Transform>();
        }
    }
}