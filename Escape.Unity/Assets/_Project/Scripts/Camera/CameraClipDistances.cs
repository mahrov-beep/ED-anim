namespace _Project.Scripts.Camera {
    using System;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public class CameraClipDistances : MonoBehaviour {
        [SerializeField, Required]
        public Camera targetCamera;

        [HideInInspector]
        public float[] distances = new float[32];

        [NonSerialized, ShowInInspector]
        [TableList(AlwaysExpanded = true, ShowPaging = false)]
        public LayerClipInfo[] Info;

        public void OnEnable() {
            this.targetCamera.layerCullSpherical = true;
            this.targetCamera.layerCullDistances = this.distances;
        }

        [OnInspectorInit]
        private void InInspectorInit() {
            this.Info = Enumerable.Range(0, 32)
                .Select(ind => new LayerClipInfo {
                    Self  = this,
                    Index = ind,
                    Layer = LayerMask.LayerToName(ind),
                })
                .Where(it => !string.IsNullOrEmpty(it.Layer))
                .ToArray();
        }

        public struct LayerClipInfo {
            [HideInInspector]
            public CameraClipDistances Self;

            [ReadOnly, TableColumnWidth(50, false)]
            public int Index;

            [ReadOnly]
            public string Layer;

            [ShowInInspector]
            public float Distance {
                get => this.Self.distances[this.Index];
                set => this.Self.distances[this.Index] = value;
            }
        }
    }
}