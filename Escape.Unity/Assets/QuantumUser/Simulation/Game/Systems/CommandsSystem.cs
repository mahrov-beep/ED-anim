
namespace Quantum {
  using Commands;
  using UnityEngine.Scripting;

  // This system is used to perform "Game Master" commands to facilitate testing
  [Preserve]
  public unsafe class CommandsSystem : SystemMainThreadFilter<CommandsSystem.Filter> {
    public struct Filter {
      public EntityRef   Entity;
      public Unit*       Unit;
    }

    public override void Update(Frame f, ref Filter filter) {
      var e = filter.Entity;

      var command = f.GetPlayerCommand(filter.Unit->PlayerRef);

      switch (command) {
        case CharacterCommandBase c:
          c.Execute(f, e);
          return;
      }
    }
  }
}