namespace Game.ECS.Components.WorldView {
using System;
using Scellecs.Morpeh;
using UnityEngine;

[Serializable]
public struct WorldViewRootComponent : ISingletonComponent{
    public Transform transform;
}
}