namespace _Project.Scripts.Scopes {
    using System;
    using UnityEngine;
    using Camera = UnityEngine.Camera;

    public sealed class CameraScope : IDisposable {
        public Camera Camera => camera;

        private readonly Camera        camera;
        private readonly bool          created;
        private readonly bool          active;
        private readonly bool          ortho;
        private readonly Vector3       pos;
        private readonly Quaternion    rot;
        private readonly RenderTexture rt;

        private CameraScope(Camera cam, bool isCreated) {
            camera  = cam;
            created = isCreated;
            active  = cam.gameObject.activeSelf;
            ortho   = cam.orthographic;

            var transform = cam.transform;
            pos = transform.position;
            rot = transform.rotation;
            rt  = cam.targetTexture;
        }

        public static CameraScope Acquire(Camera cam = null) {
            var useTempCamera = !cam;
            if (useTempCamera) {
                cam = new GameObject("TempCamera").AddComponent<Camera>();
            }
            else {
                cam.gameObject.SetActive(true);
            }

            return new CameraScope(cam, useTempCamera);
        }

        public void Dispose() {
            RenderTexture.active = null;

            if (created) {
                if (Application.isPlaying) {
                    UnityEngine.Object.Destroy(camera.gameObject);
                }
                else {
                    UnityEngine.Object.DestroyImmediate(camera.gameObject);
                }

                return;
            }

            camera.transform.SetPositionAndRotation(pos, rot);
            camera.orthographic  = ortho;
            camera.targetTexture = rt;
            camera.gameObject.SetActive(active);
        }
    }
}