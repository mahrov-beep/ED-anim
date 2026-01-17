namespace Multicast.DropSystem {
    using JetBrains.Annotations;

    /// <summary>
    /// Генерирует Drop по его описанию DropDef
    /// и возвращает сгенерированный Drop.
    /// </summary>
    public readonly struct BuildDropCommand : ICommand<Drop> {
        public DropDef Def { get; }

        [PublicAPI]
        public BuildDropCommand(DropDef def) {
            this.Def = def;
        }
    }

    /// <summary>
    /// Удаляет Drop из списка наград и выдает награды из Drop игроку. 
    /// </summary>
    public readonly struct OpenDropCommand : ICommand {
        public string DropGuid { get; }

        [PublicAPI]
        public OpenDropCommand(string dropGuid) {
            this.DropGuid = dropGuid;
        }
    }
}