using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PanAndZoom : MonoBehaviour, PlayerControls.ICommonControlsActions
{
    [Tooltip("The threshold for the mouse before the screen begins to pan on the X axis (percentage)")]
    [Range(0f, 100f)]
    [SerializeField()] private float panEdgeHorizontalThreshold = 10f;

    [Tooltip("The Y threshold for the mouse before the screen begins to pan on the Y axis (percentage)")]
    [Range(0f, 100f)]
    [SerializeField] private float panEdgeVerticalThreshold = 10f;

    [Space]
    [Space]
    [Tooltip("The speed multiplier for panning")]
    [SerializeField] private float panSpeed = 4f;

    [SerializeField] private float zoomMultiplier = 2f;
    [SerializeField] private PanAxis panAxis = PanAxis.XY;

    private PlayerControls playerControls;
    private CinemachineVirtualCamera virtualCamera;
    private Transform cameraTransform;

    private Vector2 panDirection = Vector2.zero;
    private float zoomInput = 0f;

    public enum PanAxis
    {
        XY = 0,
        XZ = 1
    }

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
    void LateUpdate()
    {
        if (panDirection.x != 0 || panDirection.y != 0)
        {
            Pan(panDirection.x, panDirection.y);
        }

        if (zoomInput != 0)
        {
            Zoom();
        }
    }

    public Vector2 CalculatePanDirection(float x, float y)
    {
        Vector2 direction = Vector2.zero;

        if (y >= Screen.height * (1 - (panEdgeVerticalThreshold / 100f)))
        {
            direction.y += 1;
        }
        else if (y <= Screen.height * (panEdgeVerticalThreshold / 100f))
        {
            direction.y -= 1;
        }

        if (x >= Screen.width * (1 - (panEdgeHorizontalThreshold / 100f)))
        {
            direction.x += 1;
        }
        else if (x <= Screen.width * (panEdgeHorizontalThreshold / 100f))
        {
            direction.x -= 1;
        }

        return direction;
    }

    private void Pan(float x, float y)
    {
        var panAmount = (Vector3)panDirection * panSpeed;

        if (panAxis == PanAxis.XY)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position,
                cameraTransform.position + panAmount, Time.deltaTime);
        }

        if (panAxis == PanAxis.XZ)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position,
                cameraTransform.position + new Vector3(panAmount.x, 0f, panAmount.y), Time.deltaTime);
        }
    }

    private void Zoom()
    {
        float zoomAmount = zoomInput * zoomMultiplier;
        Vector3 zoomedPosition = Vector3.zero;

        if (panAxis == PanAxis.XY)
        {
            zoomedPosition = Vector3.Lerp(cameraTransform.position,
                cameraTransform.position + new Vector3(0, 0, zoomAmount), Time.deltaTime);
        }

        if (panAxis == PanAxis.XZ)
        {
            zoomedPosition = Vector3.Lerp(cameraTransform.position,
                cameraTransform.position + new Vector3(0, zoomAmount, 0), Time.deltaTime);
        }

        cameraTransform.position = new Vector3(zoomedPosition.x, zoomedPosition.y, zoomedPosition.z);
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
        if (context.performed)
        {
            zoomInput = context.ReadValue<float>();
        }

        if (context.canceled)
        {
            zoomInput = 0f;
        }
    }

    public void OnToggleMapMode(InputAction.CallbackContext context)
    {

    }
}
