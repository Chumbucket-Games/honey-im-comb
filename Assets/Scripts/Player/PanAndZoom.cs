using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PanAndZoom : MonoBehaviour, PlayerControls.ICommonControlsActions
{
    [Tooltip("The threshold for the mouse before the screen begins to pan on the X axis (percentage)")]
    [Range(0f, 100f)]
    [SerializeField()] private float panEdgeXThreshold = 5f;
    
    [Tooltip("The Y threshold for the mouse before the screen begins to pan on the Y axis (percentage)")]
    [Range(0f, 100f)]
    [SerializeField] private float panEdgeYThreshold = 5f;

    [Space][Space]
    [Tooltip("The speed multiplier for panning")]
    [SerializeField] private float panSpeed = 2f;

    private PlayerControls playerControls;
    private CinemachineVirtualCamera virtualCamera;
    private Transform cameraTransform;

    private Vector2 panDirection = Vector2.zero;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.CommonControls.SetCallbacks(this);
        }

        playerControls.Enable();
        playerControls.CommonControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.CommonControls.Disable();
    }

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        cameraTransform = virtualCamera.VirtualCameraGameObject.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (panDirection.x != 0 || panDirection.y != 0)
        {
            Pan(panDirection.x, panDirection.y);
        }
    }

    public Vector2 CalculatePanDirection(float x, float y)
    {
        Vector2 direction = Vector2.zero;

        if (y >= Screen.height * (1 - (panEdgeYThreshold / 100f)))
        {
            direction.y += 1;
        }
        else if (y <= Screen.height * (panEdgeYThreshold / 100f))
        {
            direction.y -= 1;
        }

        if (x >= Screen.width * (1 - (panEdgeXThreshold / 100f)))
        {
            direction.x += 1;
        }
        else if (x <= Screen.width * (panEdgeXThreshold / 100f))
        {
            direction.x -= 1;
        }

        return direction;
    }

    private void Pan(float x, float y)
    {
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, 
            cameraTransform.position + (Vector3)panDirection * panSpeed, Time.deltaTime);
    }

    public void OnPanMouse(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var input = context.ReadValue<Vector2>();
            panDirection = CalculatePanDirection(input.x, input.y);
        }

        if (context.canceled)
        {
            panDirection = Vector2.zero;
        }
    }

    public void OnPanKeyboard(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            panDirection = context.ReadValue<Vector2>();
        }

        if (context.canceled)
        {
            panDirection = Vector2.zero;
        }
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        
    }
}
