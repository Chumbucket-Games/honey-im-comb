using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour, PlayerControls.IUnitManagementActions
{
    [SerializeField] CinemachineVirtualCamera hiveCamera;
    [SerializeField] CinemachineVirtualCamera overworldCamera;

    PlayerControls playerControls;
    Vector2 cursorPosition = Vector2.zero;
    //Vector2 startPosition = Vector2.zero;
    //Vector2 endPosition = Vector2.zero;
    ISelectable selectedObject;
    public bool IsHiveMode = true;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.UnitManagement.SetCallbacks(this);

            playerControls.CommonControls.ToggleMapMode.performed += _ => OnMapToggle();
        }

        playerControls.UnitManagement.Enable();
        playerControls.CommonControls.ToggleMapMode.Enable();
    }

    private void OnDisable()
    {
        playerControls.UnitManagement.Disable();
    }

    public void OnMapToggle()
    {
        IsHiveMode = !IsHiveMode;

        if (IsHiveMode)
        {
            hiveCamera.gameObject.SetActive(true);
            overworldCamera.gameObject.SetActive(false);
        }
        else
        {
            hiveCamera.gameObject.SetActive(false);
            overworldCamera.gameObject.SetActive(true);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && selectedObject != null)
        {
            // Move the unit to the target location.
            if (selectedObject.IsMovable())
            {
                Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (IsHiveMode)
                    {
                        selectedObject.MoveToPosition(hit.transform.position, hit,true);
                    }
                    else
                    {
                        selectedObject.MoveToPosition(hit.point, hit,false);
                    }
                }
            }
        }
    }

    public void OnCursor(InputAction.CallbackContext context)
    {
        cursorPosition = context.ReadValue<Vector2>();
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        /*if (context.started)
        {
            startPosition = cursorPosition;
        }

        if (context.performed)
        {
            endPosition = cursorPosition;
            Vector3 minBoxPosition = Camera.main.ScreenToWorldPoint(startPosition);
            Vector3 maxBoxPosition = Camera.main.ScreenToWorldPoint(endPosition);

            Vector3 center = maxBoxPosition - minBoxPosition;
            Vector3 halfExtents = maxBoxPosition - center;

            // Select the units within the bounding box.
            RaycastHit[] hits = Physics.BoxCastAll(center, halfExtents, Vector3.forward);
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Unit"))
                {

                }
            }
        }

        if (context.canceled)
        {
            // Select the unit at the cursor position.
            Ray ray = Camera.main.ScreenPointToRay(cursorPosition);

            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.CompareTag("Unit"))
                {

                }
            }
        }*/
        if (context.performed)
        {
            Ray ray = Camera.main.ScreenPointToRay(cursorPosition);

            if (Physics.Raycast(ray, out var hit))
            {
                if (selectedObject != null)
                {
                    // Deselect the currently selected object.
                    selectedObject.OnDeselect();
                }
                if (hit.collider.CompareTag("Unit"))
                {
                    selectedObject = hit.transform.gameObject.GetComponent<Unit>();
                    selectedObject.OnSelect();
                }
                else if (hit.collider.CompareTag("Building") || hit.collider.CompareTag("Hive"))
                {
                    selectedObject = hit.transform.gameObject.GetComponent<Building>();
                    selectedObject.OnSelect();
                }
                else if (hit.collider.CompareTag("ResourceNode"))
                {
                    selectedObject = hit.transform.gameObject.GetComponent<ResourceNode>();
                    selectedObject.OnSelect();
                }
            }
            else if (selectedObject != null)
            {
                // Deselect the currently selected object.
                selectedObject.OnDeselect();
                selectedObject = null;
            }
        }
    }
}
