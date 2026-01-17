using System;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class NativeMemorySnapshot : MonoBehaviour {
    [SerializeField, Required] private Button   makeSnapshotButton;
    [SerializeField, Required] private TMP_Text snapshotText;

    private StringBuilder builder = new StringBuilder();

    private void Start() {
        this.makeSnapshotButton.onClick.AddListener(MakeSnapshot);
    }

    private void MakeSnapshot() {
        const int count = 500;

        try {
            var textures = Array.ConvertAll(Resources.FindObjectsOfTypeAll<Texture>(), it => new ObjectData {
                Obj        = it,
                MemorySize = Profiler.GetRuntimeMemorySizeLong(it),
            });
            var meshes = Array.ConvertAll(Resources.FindObjectsOfTypeAll<Mesh>(), it => new ObjectData {
                Obj        = it,
                MemorySize = Profiler.GetRuntimeMemorySizeLong(it),
            });
            var audioClips = Array.ConvertAll(Resources.FindObjectsOfTypeAll<AudioClip>(), it => new ObjectData {
                Obj        = it,
                MemorySize = Profiler.GetRuntimeMemorySizeLong(it),
            });

            var assets = Enumerable.Empty<ObjectData>()
                .Concat(textures)
                .Concat(meshes)
                .Concat(audioClips)
                .OrderByDescending(it => it.MemorySize)
                .Take(count);

            this.builder.Append("Memory snapshot: found ");
            this.builder.Append(textures.Length).Append(" textures (").Append(GetHumanSize(textures)).Append("), ");
            this.builder.Append(meshes.Length).Append(" meshes (").Append(GetHumanSize(meshes)).Append("), ");
            this.builder.Append(audioClips.Length).Append(" audio clips (").Append(GetHumanSize(audioClips)).Append("), ");
            this.builder.AppendLine();
            this.builder.Append("Details for top ").Append(count).AppendLine(" assets:");

            foreach (var asset in assets) {
                AppendObjectInfo(this.builder, asset);
            }

            this.snapshotText.SetText(this.builder);
        }
        finally {
            this.builder.Clear();
        }
    }

    private static void AppendObjectInfo(StringBuilder sb, ObjectData data) {
        var typeName = data.Obj.GetType().Name;
        var objName  = data.Obj.name;

        sb.Append('[').Append(typeName).Append("] ").Append(' ', Math.Max(0, 20 - typeName.Length));
        sb.Append(objName).Append(' ', Math.Max(0, 50 - objName.Length)).Append(": ").Append(GetHumanSize(data.MemorySize));
        sb.AppendLine();
    }

    private static string GetHumanSize(ObjectData[] data) {
        return GetHumanSize(data.Sum(it => it.MemorySize));
    }

    private static string GetHumanSize(long bytes) => bytes switch {
        >= 1024 * 1024 => $"{bytes / 1024 / 1024} MB",
        >= 1024 => $"{bytes / 1024} kb",
        _ => $"{bytes} b",
    };

    private struct ObjectData {
        public Object Obj;
        public long   MemorySize;
    }
}