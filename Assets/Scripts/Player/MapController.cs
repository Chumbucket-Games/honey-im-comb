using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour, PlayerControls.IUnitManagementActions
{
    [SerializeField] CinemachineVirtualCamera hiveCamera;
    [SerializeField] CinemachineVirtualCamera overworldCamera;

    private PlayerControls playerControls;
    private Vector2 cursorPosition = Vector2.zero;
    private List<ISelectable> selectedObjects = new List<ISelectable>();
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
        if (context.performed && selectedObjects.Count > 0)
        {
            foreach (var selectedObject in selectedObjects)
            {
                // Move the unit to the target location.
                if (selectedObject.IsMovable())
                {
                    Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        if (IsHiveMode)
                        {
                            selectedObject.MoveToPosition(hit.transform.position, hit, true);
                        }
                        else
                        {
                            selectedObject.MoveToPosition(hit.point, hit, false);
                        }
                    }
                }
            }
        }
    }

    public void OnCursor(InputAction.CallbackContext context)
    {
        cursorPosition = context.ReadValue<Vector2>();
    }

    public void OnBoxSelect(Vector2 dimensions, Vector2 position)
    {
        selectedObjects.Clear();

        Vector3 halfExtents = dimensions / 2f;
        var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, Camera.main.nearClipPlane));

        // Select the units within the bounding box
        RaycastHit[] hits = Physics.BoxCastAll(worldPos, halfExtents, Camera.main.transform.forward);
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Unit"))
            {
                selectedObjects.Add(hit.transform.gameObject.GetComponent<Unit>());
                
                Debug.Log($"Unit {hit.transform.gameObject.name} selected");
            }
        }
    }


    public void OnSelect(InputAction.CallbackContext context)
    {

    }

    //public void OnSelect(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //    {
    //        Ray ray = Camera.main.ScreenPointToRay(cursorPosition);

    //        if (Physics.Raycast(ray, out var hit))
    //        {
    //            if (selectedObject != null)
    //            {
    //                // Deselect the currently selected object.
    //                selectedObject.OnDeselect();
    //            }
    //            if (hit.collider.CompareTag("Unit"))
    //            {
    //                selectedObject = hit.transform.gameObject.GetComponent<Unit>();
    //                selectedObject.OnSelect();
    //            }
    //            else if (hit.collider.CompareTag("Building") || hit.collider.CompareTag("Hive"))
    //            {
    //                selectedObject = hit.transform.gameObject.GetComponent<Building>();
    //                selectedObject.OnSelect();
    //            }
    //            else if (hit.collider.CompareTag("ResourceNode"))
    //            {
    //                selectedObject = hit.transform.gameObject.GetComponent<ResourceNode>();
    //                selectedObject.OnSelect();
    //            }
    //        }
    //        else if (selectedObject != null)
    //        {
    //            // Deselect the currently selected object.
    //            selectedObject.OnDeselect();
    //            selectedObject = null;
    //        }
    //    }
    //}
}
