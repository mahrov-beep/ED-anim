using UnityEngine;
using UnityEditor;
using Quantum;
using TerrainCollider = UnityEngine.TerrainCollider;

public class AddQuantumBoxCollider_AccurateMeshBounds : EditorWindow {
    [MenuItem("Tools/Convert Colliders To Accurate QuantumStaticBoxCollider3D")]
    static void ConvertAllToQuantumBox() {
        int totalObjects = 0;
        int totalUnityColliders = 0;
        int totalBoxesAdded = 0;
        int skippedTerrain = 0;
        int removedEmpty = 0;

        foreach (var obj in GameObject.FindObjectsOfType<GameObject>()) {
            if (!obj.activeInHierarchy) continue;
            if (obj.GetComponent<TerrainCollider>()) {
                skippedTerrain++;
                continue;
            }

            var unityColliders = obj.GetComponents<Collider>();
            if (unityColliders.Length == 0) continue;

            totalObjects++;

            foreach (var q in obj.GetComponents<QuantumMonoBehaviour>()) {
                if (!(q is QuantumStaticTerrainCollider3D)) {
                    DestroyImmediate(q, true);
                }
            }

            foreach (var col in unityColliders) {
                totalUnityColliders++;

                Bounds bounds;
                Quaternion rotation;
                Vector3 center;

                if (obj.TryGetComponent(out MeshFilter mf) && mf.sharedMesh != null) {
                    // Точный bounds по Mesh
                    bounds = mf.sharedMesh.bounds;
                    Matrix4x4 localToWorld = obj.transform.localToWorldMatrix;

                    Vector3 worldCenter = localToWorld.MultiplyPoint(bounds.center);
                    Vector3 worldSize = Vector3.Scale(bounds.size, obj.transform.lossyScale);

                    center = obj.transform.InverseTransformPoint(worldCenter);
                    rotation = Quaternion.identity; // считаем мировой ориентированный бокс
                } else {
                    bounds = col.bounds;
                    center = obj.transform.InverseTransformPoint(bounds.center);
                    rotation = Quaternion.Inverse(obj.transform.rotation);
                }

                var q = obj.AddComponent<QuantumStaticBoxCollider3D>();
                q.Size = bounds.size.ToFPVector3();
                q.PositionOffset = center.ToFPVector3();
                q.RotationOffset = rotation.eulerAngles.ToFPVector3();
                q.Settings.Trigger = col.isTrigger;
                totalBoxesAdded++;
            }
        }

        foreach (var q in GameObject.FindObjectsOfType<QuantumStaticBoxCollider3D>()) {
            if (q.Size.X.RawValue == 0 || q.Size.Y.RawValue == 0 || q.Size.Z.RawValue == 0) {
                DestroyImmediate(q, true);
                removedEmpty++;
            }
        }

        Debug.Log($"✅ Объектов с Unity Collider (не Terrain): {totalObjects}");
        Debug.Log($"🔢 Всего Unity-коллайдеров обработано: {totalUnityColliders}");
        Debug.Log($"📦 QuantumStaticBoxCollider3D добавлено: {totalBoxesAdded}");
        Debug.Log($"⛔ Пропущено объектов с TerrainCollider: {skippedTerrain}");
        Debug.Log($"🧹 Удалено пустых компонентов: {removedEmpty}");
    }
}
