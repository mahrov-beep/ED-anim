namespace Game.Domain {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Quantum;
    using UniMob;

    public class AtomEntityRefList<TModel> : IEnumerable<TModel>
        where TModel : class {
        private readonly Func<TModel>              factory;
        private readonly FrameDelegate             getFrame;
        private readonly EntityRefDelegate         getEntityRef;
        private readonly MutableAtom<List<TModel>> items;

        public List<TModel> AsList => this.items.Value;

        public delegate ref int FrameDelegate(TModel it);

        public delegate ref EntityRef EntityRefDelegate(TModel it);

        public AtomEntityRefList(Lifetime lifetime,
            Func<TModel> factory,
            FrameDelegate getFrame,
            EntityRefDelegate getEntityRef) {
            this.factory      = factory;
            this.getFrame     = getFrame;
            this.getEntityRef = getEntityRef;
            this.items        = Atom.Value(lifetime, new List<TModel>());
        }

        public bool TryGet<TFilter>([RequireStaticDelegate] Func<TModel, TFilter, bool> where, TFilter filter, out TModel result)
            where TFilter : struct {
            foreach (var other in this.items.Value) {
                if (where != null && !where(other, filter)) {
                    continue;
                }

                result = other;
                return true;
            }

            result = null;
            return false;
        }

        public TModel GetAndRefresh(int frameNum, EntityRef entityRef, int? insertIndex = null) {
            return this.GetAndRefresh(frameNum, entityRef, null, false, insertIndex);
        }

        public TModel GetAndRefresh<TFilter>(int frameNum, EntityRef entityRef,
            [RequireStaticDelegate] Func<TModel, TFilter, bool> where, TFilter filter,
            int? insertIndex = null)
            where TFilter : struct {
            foreach (var other in this.items.Value) {
                if (this.getEntityRef(other) != entityRef) {
                    continue;
                }

                if (where != null && !where(other, filter)) {
                    continue;
                }

                this.getFrame(other)     = frameNum;
                this.getEntityRef(other) = entityRef;
                return other;
            }

            // это реализация не поддерживает изменение порядка элементов в коллекции,
            // только вставку новых (по любому индексу) и удаление существующих
            var newTrashItem = this.factory();
            this.getFrame(newTrashItem)     = frameNum;
            this.getEntityRef(newTrashItem) = entityRef;

            this.items.Value.Insert(insertIndex ?? this.items.Value.Count, newTrashItem);
            this.items.Invalidate();

            return newTrashItem;
        }

        public void DeleteOutdatedItems(int frameNum) {
            for (var i = this.items.Value.Count - 1; i >= 0; i--) {
                if (this.getFrame(this.items.Value[i]) == frameNum) {
                    continue;
                }

                this.items.Value.RemoveAt(i);
                this.items.Invalidate();
            }
        }

        public List<TModel>.Enumerator GetEnumerator() {
            return this.items.Value.GetEnumerator();
        }

        IEnumerator<TModel> IEnumerable<TModel>.GetEnumerator() {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}