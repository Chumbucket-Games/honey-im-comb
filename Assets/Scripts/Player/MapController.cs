using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour, PlayerControls.IUnitManagementActions
{
    PlayerControls playerControls;
    Vector2 cursorPosition = Vector2.zero;
    //Vector2 startPosition = Vector2.zero;
    //Vector2 endPosition = Vector2.zero;
    ISelectable selectedObject;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.UnitManagement.SetCallbacks(this);
        }
        playerControls.UnitManagement.Enable();
    }

    private void OnDisable()
    {
        playerControls.UnitManagement.Disable();
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
                    selectedObject.MoveToPosition(hit.point);
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
                if (hit.collider.CompareTag("Unit"))
                {
                    selectedObject = hit.transform.gameObject.GetComponent<Unit>();
                    selectedObject.OnSelect();
                }
                else if (hit.collider.CompareTag("Building"))
                {
                    selectedObject = hit.transform.gameObject.GetComponent<Building>();
                    selectedObject.OnSelect();
                }
            }
        }
    }
}
