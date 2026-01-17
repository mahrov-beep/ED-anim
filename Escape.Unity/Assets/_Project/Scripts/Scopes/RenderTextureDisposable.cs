namespace _Project.Scripts.Scopes {
    using System;
    using UnityEngine;

    public sealed class RenderTextureScope : IDisposable {
        public RenderTexture Texture => texture;

        private readonly RenderTexture texture;
        private readonly RenderTexture previous;
        private readonly bool          created;

        private RenderTextureScope(RenderTexture rt, bool isCreated) {
            texture              = rt;
            created              = isCreated;
            previous             = RenderTexture.active;
            RenderTexture.active = rt;
        }

        public static RenderTextureScope Acquire(int width, int height, int depth = 24, RenderTextureFormat format = RenderTextureFormat.ARGB32) {
            var rt = new RenderTexture(width, height, depth, format);
            return new RenderTextureScope(rt, true);
        }

        public static RenderTextureScope Activate(RenderTexture existing) {
            return new RenderTextureScope(existing, false);
        }

        public void Dispose() {
            RenderTexture.active = previous;

            if (!created) return;

            texture.Release();

            if (Application.isPlaying) {
                UnityEngine.Object.Destroy(texture);
            }
            else {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}