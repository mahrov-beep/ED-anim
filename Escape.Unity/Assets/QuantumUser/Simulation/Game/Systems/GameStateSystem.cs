namespace Quantum {
using System;
using UnityEngine;
using UnityEngine.Scripting;
using static EGameStates;

[Preserve]
public unsafe class GameStateSystem : SystemMainThread {
  public override void OnInit(Frame f) {
    if (f.IsPredicted){
      return;
    }
    
    SetPresentationState(f);
  }

  public override void Update(Frame f) {
    if (f.IsPredicted){
      return;
    }
    
    if (f.Global->GameStateTimer.ProcessTimer(f)) {
      NextState(f);
    }
  }

  void NextState(Frame f) {
    EGameStates currentState = f.Global->GameState;
    switch (currentState) {
      case Presentation: SetGameState(f); break;

      case Game: SetBeforeExitState(f); break;

      case BeforeExit: ExitGame(f); break;
      
      case Exited: break;

      case None: Debug.LogError("Broken GameState"); break;

      default: throw new ArgumentOutOfRangeException();
    }
  }

  void SetPresentationState(Frame f) {
    // Debug.LogError($"Setting state to Presentation. Duration: {f.SimulationConfig.GameStateSettings.PresentationDurationSec} seconds.");

    f.Global->GameState      = Presentation;
    f.Global->GameStateTimer = f.GameMode.GameStateSettings.PresentationDurationSec;

    f.Signals.OnPresentationStart();
    f.Events.PresentationStart();
  }

  void SetGameState(Frame f) {
    // Debug.LogError($"Setting state to Game. Duration: {f.SimulationConfig.GameStateSettings.GameDurationSec} seconds.");

    f.Global->GameState      = Game;
    f.Global->GameStateTimer = f.GameMode.GameStateSettings.GameDurationSec;

    f.Signals.OnGameStart();
    f.Events.GameStart();
  }

  void SetBeforeExitState(Frame f) {
    // Debug.LogError($"Setting state to BeforeExit. Duration: {f.SimulationConfig.GameStateSettings.BeforeExitDurationSec} seconds.");

    f.Global->GameState      = BeforeExit;
    f.Global->GameStateTimer = f.GameMode.GameStateSettings.BeforeExitDurationSec;

    f.Signals.OnGameEnd();
    f.Events.GameEnd();
  }

  void ExitGame(Frame f) {
    f.Global->GameState = Exited;
    
    // Debug.LogError("Exiting game.");

    f.Events.GameExit(f, GameSnapshotHelper.Make(f));
  }
}

}