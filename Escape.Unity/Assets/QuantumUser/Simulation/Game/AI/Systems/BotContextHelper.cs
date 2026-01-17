namespace Quantum {
  /// <summary>
  /// Отдельный класс, ибо это может использоваться в BT\HFSM\UT\GOAP в мульти-агентных кейсах
  /// Выделить в отдельную систему не получится
  /// </summary>
  public static unsafe class BotContextHelper {
    public static void FillBotUserContext(Frame f, EntityRef botRef, ref AIContextUser data) {
      GetBotComponentPointers(f, botRef, ref data);
      f.TryGetPointer(data.Bot->WayRef, out data.CurrentWay);
      FillWeaponData(f, ref data);
    }

    /// <summary>
    /// компоненты которые должны быть на боте
    /// </summary>
    static void GetBotComponentPointers(Frame f, EntityRef botRef, ref AIContextUser data) {
      data.Bot              = f.GetPointer<Bot>(botRef);
      data.PerceptionMemory = f.GetPointer<PerceptionMemory>(botRef);

      data.Unit            = f.GetPointer<Unit>(botRef);
      data.Transform       = f.GetPointer<Transform3D>(botRef);
      data.InputContainer  = f.GetPointer<InputContainer>(botRef);
      data.Pathfinder      = f.GetPointer<NavMeshPathfinder>(botRef);
      data.SpectatorCamera = f.GetPointer<CharacterSpectatorCamera>(botRef);
      data.KCC             = f.GetPointer<KCC>(botRef);
    }

    static void FillWeaponData(Frame f, ref AIContextUser data) {
      var unit = data.Unit;
      bool success = unit->TryGetActiveWeapon(f, out data.ActiveWeapon);
    }
  }
}