namespace InfimaGames.LowPolyShooterPack {
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class CharacterSkinnedMeshVisibility : MonoBehaviour {
        [SerializeField, Required] private CharacterBehaviour characterBehaviour;

        [SerializeField, Required] private List<Transform> searchRoots;

        [SerializeField, Required] private List<SkinnedMeshRenderer> localVisibleMeshes;

        private HashSet<SkinnedMeshRenderer> localVisibleMeshesSet;
        private CharacterTypes               currentCharacterType;

        private void Awake() {
            this.localVisibleMeshesSet = new HashSet<SkinnedMeshRenderer>(this.localVisibleMeshes);
        }

        private void Update() {
            var characterType = this.characterBehaviour.GetCharacterType();

            if (characterType == this.currentCharacterType) {
                return;
            }

            this.currentCharacterType = characterType;

            using (ListPool<SkinnedMeshRenderer>.Get(out var meshes)) {
                foreach (var searchRoot in this.searchRoots) {
                    searchRoot.GetComponentsInChildren(includeInactive: true, meshes);

                    if (characterType == CharacterTypes.RemotePlayer) {
                        foreach (var mesh in meshes) {
                            mesh.enabled = true;
                        }
                    }
                    else {
                        foreach (var mesh in meshes) {
                            mesh.enabled = localVisibleMeshesSet.Contains(mesh);
                        }
                    }

                    meshes.Clear();
                }
            }
        }
    }
}