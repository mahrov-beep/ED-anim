namespace Game.ECS.Systems.Unit {
    using DamageNumbersPro;
    using UnityEngine;
    using UnityEngine.Serialization;
    [CreateAssetMenu(menuName = "Create FloatingTextConfig", fileName = "FloatingTextConfig", order = 0)]
    public class FloatingTextConfig : ScriptableObject {
        [field: SerializeField] public Vector3      OffsetDamageFloating    { get; private set; } = Vector3.up * 2;
        [field: SerializeField] public Vector3      OffsetHealFloating      { get; private set; } = Vector3.up * 2;
        [field: SerializeField] public DamageNumber MeleeDamage             { get; private set; }
        [field: SerializeField] public DamageNumber BulletDamage            { get; private set; }
        [field: SerializeField] public DamageNumber BulletDamageCritical    { get; private set; }
        [field: SerializeField] public DamageNumber ExplosionDamage         { get; private set; }
        [field: SerializeField] public DamageNumber ExplosionDamageCritical { get; private set; }
        [field: SerializeField] public DamageNumber Heal                    { get; private set; }
        [field: SerializeField] public DamageNumber FireDamage              { get; private set; }
    }
}