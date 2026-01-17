namespace InfimaGames.LowPolyShooterPack {
    using UnityEngine;

    // Behaviours are used from animator by GetComponent<T>
    // so it is required to attach all components to a single gameObject.
    // Ensure it by this component.
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterBehaviour))]
    [RequireComponent(typeof(MovementBehaviour))]
    [RequireComponent(typeof(CharacterAnimationEventHandler))]
    public class CharacterComponents : MonoBehaviour {
        
    }
}