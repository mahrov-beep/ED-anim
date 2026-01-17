using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts {
    public class FloorGridGenerator : MonoBehaviour {
        public Vector2    floorSize;
        public Vector2Int planeCount;
        public GameObject planePrefab;
        public Transform  root;

        [Button]
        public void GenerateFloor() {
            RemoveFloor();

            var planeSize = new Vector2(floorSize.x / planeCount.x, floorSize.y / planeCount.y);

            for (var x = 0; x < planeCount.x; x++) {
                for (var y = 0; y < planeCount.y; y++) {
                    var pos = new Vector3(
                                    -floorSize.x / 2 + planeSize.x * (x + 0.5f),
                                    0,
                                    -floorSize.y / 2 + planeSize.y * (y + 0.5f));

                    var plane = Instantiate(planePrefab, root);
                    plane.gameObject.SetActive(true);
                    plane.transform.localPosition = pos;
                    plane.transform.localScale    = new Vector3(planeSize.x / 10f, 1, planeSize.y / 10f);
                }
            }
        }

        [Button]
        private void RemoveFloor() {
            for (var i = root.childCount - 1; i >= 0; i--) {
                DestroyImmediate(root.GetChild(i).gameObject);
            }
        }
    }
}