//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.1.1
//     from Assets/Scripts/Input System/PlayerControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Common Controls"",
            ""id"": ""019747f0-e0c6-47af-9cbe-0de1ff25020d"",
            ""actions"": [
                {
                    ""name"": ""Pan"",
                    ""type"": ""Value"",
                    ""id"": ""31387cf6-5189-4783-988c-45ecaf521bbe"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""Value"",
                    ""id"": ""37e04113-c019-4080-8403-841a95ecfc6d"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": ""Normalize(min=-1,max=1)"",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""89d60ab7-ccae-4b1c-95fa-af8858f68381"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""57e89b22-037d-41da-9d94-2a1e22c9f858"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a3d6d29b-3658-40d5-922e-c6d03ddd6c71"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""be4b9a29-a17c-4361-a0a0-e6501179fa32"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e47c2f92-d430-4369-ab45-282b26466fff"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""fd1c3b23-ad5e-4217-83e7-44c286e1546c"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""881bd941-8be4-4d14-9501-397f9ffce797"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": ""Normalize(min=-1,max=1)"",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Common Controls
        m_CommonControls = asset.FindActionMap("Common Controls", throwIfNotFound: true);
        m_CommonControls_Pan = m_CommonControls.FindAction("Pan", throwIfNotFound: true);
        m_CommonControls_Zoom = m_CommonControls.FindAction("Zoom", throwIfNotFound: true);
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
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Common Controls
    private readonly InputActionMap m_CommonControls;
    private ICommonControlsActions m_CommonControlsActionsCallbackInterface;
    private readonly InputAction m_CommonControls_Pan;
    private readonly InputAction m_CommonControls_Zoom;
    public struct CommonControlsActions
    {
        private @PlayerControls m_Wrapper;
        public CommonControlsActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Pan => m_Wrapper.m_CommonControls_Pan;
        public InputAction @Zoom => m_Wrapper.m_CommonControls_Zoom;
        public InputActionMap Get() { return m_Wrapper.m_CommonControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CommonControlsActions set) { return set.Get(); }
        public void SetCallbacks(ICommonControlsActions instance)
        {
            if (m_Wrapper.m_CommonControlsActionsCallbackInterface != null)
            {
                @Pan.started -= m_Wrapper.m_CommonControlsActionsCallbackInterface.OnPan;
                @Pan.performed -= m_Wrapper.m_CommonControlsActionsCallbackInterface.OnPan;
                @Pan.canceled -= m_Wrapper.m_CommonControlsActionsCallbackInterface.OnPan;
                @Zoom.started -= m_Wrapper.m_CommonControlsActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_CommonControlsActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_CommonControlsActionsCallbackInterface.OnZoom;
            }
            m_Wrapper.m_CommonControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Pan.started += instance.OnPan;
                @Pan.performed += instance.OnPan;
                @Pan.canceled += instance.OnPan;
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
            }
        }
    }
    public CommonControlsActions @CommonControls => new CommonControlsActions(this);
    public interface ICommonControlsActions
    {
        void OnPan(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
    }
}
