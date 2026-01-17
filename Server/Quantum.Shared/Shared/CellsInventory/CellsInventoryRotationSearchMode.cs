namespace Quantum.CellsInventory {
    public enum CellsInventoryRotationSearchMode {
        /// <summary>
        /// Если возможно, добавляет в обычном состоянии, иначе<br/>
        /// Если возможно, добавляет в повернутом состоянии.
        /// </summary>
        Find,

        /// <summary>
        /// Добавляет предмет строго в обычном положении.
        /// </summary>
        Default,

        /// <summary>
        /// Добавляет предмет строго в повернутом положении.
        /// </summary>
        Rotated,
    }
}