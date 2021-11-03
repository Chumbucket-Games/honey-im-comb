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

    private Vector2 panInput = Vector2.zero;

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
        if (panInput.x != 0 || panInput.y != 0)
        {
            Pan(panInput.x, panInput.y);
        }
    }

    public Vector2 PanDirection(float x, float y)
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
        Vector2 direction = PanDirection(x, y);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, 
            cameraTransform.position + (Vector3)direction * panSpeed, Time.deltaTime);
    }

    public void OnPan(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            panInput = context.ReadValue<Vector2>();
        }

        if (context.canceled)
        {
            panInput = Vector2.zero;
        }
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        
    }
}
