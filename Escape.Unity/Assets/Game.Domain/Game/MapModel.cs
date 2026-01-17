namespace Game.Domain.Game {
    using System.Collections.Generic;
    using Multicast;
    using UniMob;
    using UnityEngine;
    public class MapModel : Model {
        public MapModel(Lifetime lifetime) : base(lifetime) { }

        public List<UnitOnMapData> Enemies          { get; set; } = new(6);
        public List<UnitOnMapData> PartyMembers     { get; set; } = new(4);
        public List<Vector3>       InterestPoints   { get; set; } = new(2);
        public List<Vector3>       Waypoints        { get; set; } = new(4);
        public List<Vector3>       ExitPoints       { get; set; } = new(1);
        public List<Vector3>       SpawnedItemBoxes { get; set; } = new(2);
        public List<Vector3>       DroppedItemBoxes { get; set; } = new(4);
        public List<Vector3>       Grenades         { get; set; } = new(4);

        public float VisiblyRadius { get; set; }
    }

    public struct UnitOnMapData {
        public Vector3 WorldPosition  { get; set; }
        public float   alpha;
    }
}