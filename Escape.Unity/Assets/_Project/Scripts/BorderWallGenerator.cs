using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts {
    public class BorderWallGenerator : MonoBehaviour {
        public GameObject _wallPrefab;
        public float      _wallHeight     = 3f;
        public int        _objectsPerSide = 10;
        public float      _levelSize      = 512f;

        [Button]
        public void GenerateWalls() {
            ClearWalls();

            GenerateWall(Vector3.forward, Vector3.right, true);
            GenerateWall(Vector3.back, Vector3.right, true);
            GenerateWall(Vector3.right, Vector3.forward, false);
            GenerateWall(Vector3.left, Vector3.forward, false);
        }

        void GenerateWall(Vector3 sideDirection, Vector3 stepDirection, bool isHorizontal) {
            var spacing  = _levelSize / _objectsPerSide;
            var basePos  = sideDirection * (_levelSize / 2f);
            var startPos = basePos - stepDirection * (_levelSize / 2f) + stepDirection * (spacing / 2f);

            for (var i = 0; i < _objectsPerSide; i++) {
                var position = startPos + stepDirection * spacing * i;
                position.y = _wallHeight / 2f;

                var scale = _wallPrefab.transform.localScale;
                scale.y = _wallHeight;

                if (isHorizontal) {
                    scale.x = spacing;
                }
                else {
                    scale.z = spacing;
                }

                InstantiateWall(position, Quaternion.identity, scale);
            }
        }

        void InstantiateWall(Vector3 position, Quaternion rotation, Vector3 scale) {
            if (_wallPrefab != null) {
                var wall = Instantiate(_wallPrefab, transform);
                wall.transform.localPosition = position;
                wall.transform.localRotation = rotation;
                wall.transform.localScale    = scale;
            }
        }

        void ClearWalls() {
            for (var i = transform.childCount - 1; i >= 0; i--) {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}