namespace Quantum
{
    using System.Collections.Generic;
    using Commands;
    using Photon.Deterministic;

    public static partial class DeterministicCommandSetup
    {
        static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig)
        {
            // Add or remove commands to the collection.
            // factories.Add(new NavMeshAgentTestSystem.RunTest());
            
            // loadout
            factories.Add(new MoveItemFromSlotToSlotLoadoutCommand());
            factories.Add(new MoveItemFromSlotToTetrisLoadoutCommand());
            factories.Add(new MoveItemFromTetrisToSlotLoadoutCommand());
            factories.Add(new ThrowAwayItemFromSlotLoadoutCommand());
            factories.Add(new ThrowAwayItemFromTetrisLoadoutCommand());
            factories.Add(new ThrowAwayAllItemsFromTrashLoadoutCommand());
            factories.Add(new SwapTetrisCommand());
            // loadout/weapon-attachments
            factories.Add(new MoveWeaponAttachmentFromSlotToSlotLoadoutCommand());
            factories.Add(new MoveWeaponAttachmentFromSlotToTetrisLoadoutCommand());
            factories.Add(new MoveWeaponAttachmentFromTetrisToSlotLoadoutCommand());
            factories.Add(new ThrowAwayWeaponAttachmentFromSlotLoadoutCommand());

            factories.Add(new OpenItemBoxCommand());
            factories.Add(new CloseItemBoxCommand());
            factories.Add(new PickUpBestFromNearbyItemBoxLoadoutCommand());
            factories.Add(new MoveItemFromTetrisToTetrisLoadoutCommand());
            factories.Add(new ReloadItemBoxCommand());
            
            factories.Add(new ReloadWeaponCommand());
            factories.Add(new SelectWeaponCommand());
            factories.Add(new UseItemCommand());
            factories.Add(new RotateItemCommand());
            factories.Add(new JumpCommand());
            factories.Add(new CrouchCommand());
            factories.Add(new ReviveCommand());
            factories.Add(new KnifeAttackCommand());
            factories.Add(new SetSystemEnabledCommand<BotBehaviourTreeUpdateSystem>());
            
        }
    }
}