//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    using JetBrains.Annotations;
    using Sirenix.OdinInspector;

    /// <summary>
    /// Weapon Attachment Manager. Handles equipping and storing a Weapon's Attachments.
    /// </summary>
    public class WeaponAttachmentManager : WeaponAttachmentManagerBehaviour {
        [SerializeField, Required]
        [Tooltip("Прицел который отображается если на оружии не выбран кастомный прицел")]
        private ScopeBehaviour scopeDefaultBehaviour;

        [SerializeField, Required]
        [Tooltip("Muzzle по умолчанию. Нужен чтобы воспроизводить VFX выстрела")]
        private MuzzleBehaviour muzzleDefaultBehaviour;

        [CanBeNull] private MagazineCollection collectionMagazine;
        [CanBeNull] private GripCollection     collectionGrip;
        [CanBeNull] private LaserCollection    collectionLaser;
        [CanBeNull] private MuzzleCollection   collectionMuzzle;
        [CanBeNull] private ScopeCollection    collectionScope;

        private Attachment<ScopeBehaviour>    scope;
        private Attachment<MuzzleBehaviour>   muzzle;
        private Attachment<LaserBehaviour>    laser;
        private Attachment<GripBehaviour>     grip;
        private Attachment<MagazineBehaviour> magazine;

        protected override void Awake() {
            base.Awake();

            this.collectionMagazine = this.GetComponentInChildren<MagazineCollection>();
            this.collectionGrip     = this.GetComponentInChildren<GripCollection>();
            this.collectionLaser    = this.GetComponentInChildren<LaserCollection>();
            this.collectionMuzzle   = this.GetComponentInChildren<MuzzleCollection>();
            this.collectionScope    = this.GetComponentInChildren<ScopeCollection>();

            this.scope    = new Attachment<ScopeBehaviour>(this.collectionScope, this.scopeDefaultBehaviour);
            this.muzzle   = new Attachment<MuzzleBehaviour>(this.collectionMuzzle, this.muzzleDefaultBehaviour);
            this.laser    = new Attachment<LaserBehaviour>(this.collectionLaser);
            this.grip     = new Attachment<GripBehaviour>(this.collectionGrip);
            this.magazine = new Attachment<MagazineBehaviour>(this.collectionMagazine);
        }

        protected override void Start() {
            base.Start();
            
            this.scope.Init();
            this.muzzle.Init();
            this.laser.Init();
            this.grip.Init();
            this.magazine.Init();
        }

        public override Transform GetMuzzleSocket() => this.collectionMuzzle ? this.collectionMuzzle.transform : this.transform;

        public override ScopeBehaviour GetEquippedScope() => this.scope.Behaviour;

        public override MagazineBehaviour GetEquippedMagazine() => this.magazine.Behaviour;
        public override MuzzleBehaviour   GetEquippedMuzzle()   => this.muzzle.Behaviour;

        public override LaserBehaviour GetEquippedLaser() => this.laser.Behaviour;
        public override GripBehaviour  GetEquippedGrip()  => this.grip.Behaviour;

        public override void SyncWeaponSetup(WeaponSetup weaponSetup) {
            this.scope.Sync(weaponSetup.ScopePrefab);
            this.muzzle.Sync(weaponSetup.MuzzlePrefab);
            this.laser.Sync(weaponSetup.LaserPrefab);
            this.grip.Sync(weaponSetup.GripPrefab);
            // this.stock.Sync(weaponSetup.StockPrefab);
            this.magazine.Sync(weaponSetup.MagazinePrefab);
        }

        private class Attachment<T> where T : MonoBehaviour {
            [CanBeNull] private readonly AttachmentCollection<T> collection;

            [CanBeNull] private readonly T defaultBehaviour;

            [CanBeNull] private GameObject currentPrefab;

            public Attachment([CanBeNull] AttachmentCollection<T> collection, [CanBeNull] T defaultBehaviour = null) {
                this.collection       = collection;
                this.defaultBehaviour = defaultBehaviour;

                this.Behaviour = this.defaultBehaviour;
            }

            public void Init() {
                if (this.Behaviour) {
                    this.Behaviour.gameObject.SetActive(true);
                }                
            }

            public T Behaviour { get; private set; }

            public bool IsValid(GameObject newPrefab) {
                return newPrefab != null && newPrefab.TryGetComponent<T>(out _);
            }

            public void Sync(GameObject newPrefab) {
                if (this.currentPrefab == newPrefab || this.collection == null) {
                    return;
                }

                if (this.Behaviour) {
                    this.Behaviour.gameObject.SetActive(false);
                }

                this.currentPrefab = newPrefab;

                if (newPrefab && this.collection.GetByPrefab(newPrefab) is var newBehaviour && newBehaviour) {
                    this.Behaviour = newBehaviour;
                }
                else {
                    if (newPrefab) {
                        var owner = this.collection.GetComponentInParent<Weapon>();
                        Debug.LogError($"Failed to equip attachment '{newPrefab.name}' on '{(owner ? owner.name : "")}'");
                    }

                    this.Behaviour = this.defaultBehaviour;
                }

                if (this.Behaviour) {
                    this.Behaviour.gameObject.SetActive(true);
                }
            }
        }
    }
}