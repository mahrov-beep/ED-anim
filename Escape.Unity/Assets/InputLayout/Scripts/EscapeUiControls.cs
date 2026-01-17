using UnityEngine;
using UnityEngine.InputSystem;

namespace InputLayout.Scripts {
    using UnityEngine.InputSystem.Controls;
    using UnityEngine.InputSystem.Layouts;

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class EscapeUiControls : InputDevice {
        static EscapeUiControls() {
            InputSystem.RegisterLayout<EscapeUiControls>("Escape UI Controls");
            InputSystem.AddDevice<EscapeUiControls>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer() {
        }

        // Далее список виртуальных кнопок чтобы InputSystem отображала их в выпадающем списке

        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        [InputControl] public ButtonControl OpenNearbyItemBox  { get; private set; }
        [InputControl] public ButtonControl OpenNearbyBackpack { get; private set; }

        [InputControl] public ButtonControl EquipBestFromNearbyItemBox  { get; private set; }
        [InputControl] public ButtonControl EquipBestFromNearbyBackpack { get; private set; }

        [InputControl] public ButtonControl OpenSettings  { get; private set; }
        [InputControl] public ButtonControl OpenInventory { get; private set; }

        [InputControl] public ButtonControl CloseInventory { get; private set; }
        
        [InputControl] public ButtonControl UseAbility  { get; private set; }
        [InputControl] public ButtonControl KnifeAttack { get; private set; }
        [InputControl] public ButtonControl Revive      { get; private set; }

        [InputControl] public ButtonControl AimButton { get; private set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore UnusedMember.Global
    }
}