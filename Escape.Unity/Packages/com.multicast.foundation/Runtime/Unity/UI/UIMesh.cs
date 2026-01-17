using System;
using UnityEngine;
using UnityEngine.UI;

namespace Multicast.Unity {
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIMesh : MaskableGraphic {
        [SerializeField] private Mesh    sourceMesh = default;
        [SerializeField] private Texture texture    = default;

        [SerializeField] private Vector3 rotationAngles = Vector3.zero;

        public override Texture mainTexture => this.texture;

        public void SetMesh(Mesh mesh, Texture tex) {
            if (this.sourceMesh == mesh && this.texture == tex) {
                return;
            }

            this.sourceMesh = mesh;
            this.texture    = tex;

            this.SetAllDirty();
        }

        public void SetMesh(GameObject prefab) {
            if (prefab == null) {
                this.SetMesh(null, null);
                return;
            }

            var meshFilter   = prefab.GetComponentInChildren<MeshFilter>();
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            var tex = meshRenderer != null && meshRenderer.sharedMaterial is var mat && mat != null
                ? mat.mainTexture
                : null;

            this.SetMesh(mesh, tex);
        }

        protected override void OnTransformParentChanged() {
            base.OnTransformParentChanged();

            this.SetVerticesDirty();
        }

        protected override void UpdateGeometry() {
            if (this.sourceMesh == null) {
                workerMesh.Clear();
                this.canvasRenderer.SetMesh(workerMesh);
                return;
            }

            var mesh = workerMesh;

            var meshBounds = this.sourceMesh.bounds;
            var rectBounds = this.rectTransform.rect;

            var scale = Mathf.Min(rectBounds.size.x / meshBounds.size.x, rectBounds.size.y / meshBounds.size.y);
            var rot   = Quaternion.Euler(this.rotationAngles);

            var size    = meshBounds.size;
            var maxSize = Mathf.Max(size.x, Mathf.Max(size.y, size.z));

            Vector3 ProcessVertex(Vector3 v) {
                v   -= meshBounds.center;
                v   =  rot * v;
                v.z += maxSize;
                v   *= scale;
                return v;
            }

            mesh.Clear();
            mesh.SetVertices(Array.ConvertAll(this.sourceMesh.vertices, ProcessVertex));
            mesh.SetTriangles(this.sourceMesh.triangles, 0);
            mesh.SetNormals(this.sourceMesh.normals);
            mesh.SetTangents(this.sourceMesh.tangents);
            mesh.SetColors(this.sourceMesh.colors);
            mesh.SetUVs(0, this.sourceMesh.uv);
            mesh.SetUVs(1, this.sourceMesh.uv2);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            this.canvasRenderer.SetMesh(mesh);
        }
    }
}