namespace Scellecs.Morpeh {
    using UnityEngine;

    public abstract class MonoInstallerBase : MonoBehaviour {
        [SerializeField] private int order = 0;

        private SystemsGroup systemsGroup;

        private void OnEnable() {
            if (World.Default == null) {
                Debug.LogError($"Failed to install '{this.name}': World.Default is null");
            }

            this.systemsGroup = World.Default.CreateSystemsGroup();

            this.Install(this.systemsGroup);

            World.Default.AddSystemsGroup(this.order, this.systemsGroup);
        }

        private void OnDisable() {
            if (World.Default == null) {
                return;
            }

            World.Default.RemoveSystemsGroup(this.systemsGroup);
            this.systemsGroup = null;
        }

        public abstract void Install(SystemsGroup systems);
    }
}