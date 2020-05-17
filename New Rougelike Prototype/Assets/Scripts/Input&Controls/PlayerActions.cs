// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input&Controls/PlayerActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerActions"",
    ""maps"": [
        {
            ""name"": ""MainActions"",
            ""id"": ""bff7e0b4-31d5-4fd0-9d6c-a6977e46908c"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Button"",
                    ""id"": ""8726fac4-bd49-4aca-9821-27baf30c2ed1"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""f6e4bd03-708f-4025-bcc7-82a48a3f2eae"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Attack"",
                    ""type"": ""Button"",
                    ""id"": ""4d8882ca-88b6-4d50-9314-d55a9d409df8"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Aim"",
                    ""type"": ""Button"",
                    ""id"": ""376a6604-e079-4480-b175-73813198e769"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Swap Weapon"",
                    ""type"": ""Button"",
                    ""id"": ""a9c0e9a0-0f3a-4985-95b8-f04976334ab3"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""82ca7992-d135-455f-8397-c1c9ff35857d"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8dc949d6-9532-4fb1-b4a1-040cab8a254c"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""a0c7e0f9-745e-4303-b79c-5737ec07672c"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""088a8827-bfce-4b0b-9d49-816f048be8c9"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""333e3285-fcda-49a7-9dfc-e581ecfd6d36"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""71de85a1-4cd9-4360-87f8-9d18743d086c"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""10922e1c-3696-4fd3-9ec6-7845b64bcce2"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""befc3a0d-392e-48bc-b64a-c37c3d1e5007"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Aim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dcf971e0-1ecb-4fbf-813e-a06ba81cc564"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Swap Weapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MainActions
        m_MainActions = asset.FindActionMap("MainActions", throwIfNotFound: true);
        m_MainActions_Move = m_MainActions.FindAction("Move", throwIfNotFound: true);
        m_MainActions_Interact = m_MainActions.FindAction("Interact", throwIfNotFound: true);
        m_MainActions_Attack = m_MainActions.FindAction("Attack", throwIfNotFound: true);
        m_MainActions_Aim = m_MainActions.FindAction("Aim", throwIfNotFound: true);
        m_MainActions_SwapWeapon = m_MainActions.FindAction("Swap Weapon", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // MainActions
    private readonly InputActionMap m_MainActions;
    private IMainActionsActions m_MainActionsActionsCallbackInterface;
    private readonly InputAction m_MainActions_Move;
    private readonly InputAction m_MainActions_Interact;
    private readonly InputAction m_MainActions_Attack;
    private readonly InputAction m_MainActions_Aim;
    private readonly InputAction m_MainActions_SwapWeapon;
    public struct MainActionsActions
    {
        private @PlayerActions m_Wrapper;
        public MainActionsActions(@PlayerActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_MainActions_Move;
        public InputAction @Interact => m_Wrapper.m_MainActions_Interact;
        public InputAction @Attack => m_Wrapper.m_MainActions_Attack;
        public InputAction @Aim => m_Wrapper.m_MainActions_Aim;
        public InputAction @SwapWeapon => m_Wrapper.m_MainActions_SwapWeapon;
        public InputActionMap Get() { return m_Wrapper.m_MainActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MainActionsActions set) { return set.Get(); }
        public void SetCallbacks(IMainActionsActions instance)
        {
            if (m_Wrapper.m_MainActionsActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnMove;
                @Interact.started -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnInteract;
                @Interact.performed -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnInteract;
                @Interact.canceled -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnInteract;
                @Attack.started -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnAttack;
                @Attack.performed -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnAttack;
                @Attack.canceled -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnAttack;
                @Aim.started -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnAim;
                @Aim.performed -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnAim;
                @Aim.canceled -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnAim;
                @SwapWeapon.started -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnSwapWeapon;
                @SwapWeapon.performed -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnSwapWeapon;
                @SwapWeapon.canceled -= m_Wrapper.m_MainActionsActionsCallbackInterface.OnSwapWeapon;
            }
            m_Wrapper.m_MainActionsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Interact.started += instance.OnInteract;
                @Interact.performed += instance.OnInteract;
                @Interact.canceled += instance.OnInteract;
                @Attack.started += instance.OnAttack;
                @Attack.performed += instance.OnAttack;
                @Attack.canceled += instance.OnAttack;
                @Aim.started += instance.OnAim;
                @Aim.performed += instance.OnAim;
                @Aim.canceled += instance.OnAim;
                @SwapWeapon.started += instance.OnSwapWeapon;
                @SwapWeapon.performed += instance.OnSwapWeapon;
                @SwapWeapon.canceled += instance.OnSwapWeapon;
            }
        }
    }
    public MainActionsActions @MainActions => new MainActionsActions(this);
    public interface IMainActionsActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnAim(InputAction.CallbackContext context);
        void OnSwapWeapon(InputAction.CallbackContext context);
    }
}
