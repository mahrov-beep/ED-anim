namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  public static class SystemsOrder {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AfterAttribute : Attribute {
      public Type PrevSystem { get; }

      public AfterAttribute(Type prevSystem) {
        PrevSystem = prevSystem;
      }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BeforeAttribute : Attribute {
      public Type NextSystem { get; }

      public BeforeAttribute(Type nextSystem) {
        NextSystem = nextSystem;
      }
    }

    [Conditional("DEBUG")]
    public static void Validate(ICollection<SystemBase> systems) {
      var flatSystems = systems.SelectMany(it => Flat(it)).ToList();
      var typeList    = flatSystems.Select(it => it.GetType()).ToList();

      foreach (var systemBase in flatSystems) {
        var systemType  = systemBase.GetType();
        var systemIndex = typeList.IndexOf(systemType);

        foreach (var before in systemType.GetCustomAttributes(typeof(BeforeAttribute), false).Cast<BeforeAttribute>()) {
          var nextSystemIndex = typeList.IndexOf(before.NextSystem);

          if (nextSystemIndex != -1 && nextSystemIndex < systemIndex) {
            Log.Error($"Invalid systems order: " +
                      $"System '{systemType.Name}({systemIndex})' must be before '{before.NextSystem.Name}({nextSystemIndex})'");
          }
        }

        foreach (var after in systemType.GetCustomAttributes(typeof(AfterAttribute), false).Cast<AfterAttribute>()) {
          var prevSystemIndex = typeList.IndexOf(after.PrevSystem);

          if (prevSystemIndex == -1 || prevSystemIndex > systemIndex) {
            Log.Error($"Invalid systems order: " +
                      $"System '{systemType.Name}({systemIndex})' must be after '{after.PrevSystem.Name}({prevSystemIndex})'");
          }
        }
      }

      IEnumerable<SystemBase> Flat(SystemBase systemBase) {
        yield return systemBase;

        foreach (var childSystem in systemBase.ChildSystems) {
          foreach (var flatChild in Flat(childSystem)) {
            yield return flatChild;
          }
        }
      }
    }
  }
}