namespace Game.ECS.Components.Unit {
    using System;
    using Scellecs.Morpeh;
    using UnityEngine;
    using UnityEngine.Serialization;

    [Serializable, /*RequireFieldsInit*/] public struct AimAssistTarget : IComponent {
        /// сколько времени игрок удерживает прицел внутри outerRect у этой цели
        public float durationAimInOuterZone;        
        
        /// сколько времени игрок избегает прицелом outerRect у этой цели. Противоположность durationAimInOuterZone.
        public float durationWithoutCrossOuterZoneRect;

        /// <summary>
        /// Время с последнего попадания рейкастом по этой цели
        /// </summary>
        public float durationWithoutAimRaycastHit;
        
        /// <summary>
        /// https://habr.com/ru/companies/lightmap/articles/576812/
        /// это h из статьи
        /// h увеличивается со скоростью p, если внутри зоны прицел двигается к противнику;
        /// h уменьшается со скоростью m, если движение идет в сторону от противника;
        /// h уменьшается со скоростью m, если движение идет в сторону от противника;
        /// </summary>
        public float aimAssistPower;

        public Rect    innerRect, outerRect;
        
        /// <summary>
        /// эта позиция центра тела, т.е. примерно посередине туловища
        /// </summary>
        public Vector2 screenPos;
    }
}