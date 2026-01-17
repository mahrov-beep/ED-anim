namespace Quantum {

  public unsafe class RandomShuffleSpawnPoints : SystemSignalsOnly {

    public override void OnInit(Frame f) {
      var initializeData = f.Unsafe.GetOrAddSingletonPointer<InitializeData>();

      var spawnList = f.ResolveList(initializeData->NotUsedPlayerSpawnPoints);

      var allSpawns = f.FilterStruct(out Aspect<Transform3D, SpawnPoint> filter);
      while (allSpawns.Next(&filter)) {
        spawnList.Add(filter.entity);
      }

      f.LogTrace(EntityRef.None, $"shuffle spawn points count = {spawnList.Count}");

      initializeData->NotUsedPlayerSpawnPoints.RandomShuffle(f, f.RNG);

      Disable(f);
    }
  }
}