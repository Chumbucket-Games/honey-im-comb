using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;

public class MouseDrag : MonoBehaviour
{
    [SerializeField] private PlayerControls playerControls;
    [SerializeField] private Image mouseDragSelection;
    [SerializeField] private UnityEvent<Vector2, Vector3> onMouseDragEnd;

    private bool isMouseDown = false;
    private Vector3 startPosition = Vector3.zero;


    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.UnitManagement.Select.performed += ctx => OnSelect(ctx);
            playerControls.UnitManagement.Select.canceled += ctx => OnSelectCancelled(ctx);
        }

        playerControls.UnitManagement.Select.Enable();
    }

    private void OnDisable()
    {
        playerControls.UnitManagement.Select.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMouseDown)
        {
            var currentEndPosition = Mouse.current.position.ReadValue();
            var dimensions = GetSelectBoxWorldDimensions(startPosition, currentEndPosition);
            Vector3 position = GetSelectBoxWorldPosition(startPosition, currentEndPosition);

            mouseDragSelection.rectTransform.position = position;
            mouseDragSelection.rectTransform.up = Camera.main.transform.up;
            mouseDragSelection.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, dimensions.x);
            mouseDragSelection.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dimensions.y);
        }
    }

    void OnSelect(InputAction.CallbackContext context)
    {
        isMouseDown = true;
        Vector3 startInput = Mouse.current.position.ReadValue();
        startInput.z = Camera.main.transform.forward == Vector3.forward ? Mathf.Abs(Camera.main.transform.position.z) - 3.2f : Camera.main.transform.position.y - 3;
 
        startPosition = Camera.main.ScreenToWorldPoint(startInput);
        mouseDragSelection.gameObject.SetActive(true);
    }
    
    void OnSelectCancelled(InputAction.CallbackContext context)
    {
        isMouseDown = false;

        var endPosition = Mouse.current.position.ReadValue();
        var dimensions = GetSelectBoxWorldDimensions(startPosition, endPosition);
        var position = GetSelectBoxWorldPosition(startPosition, endPosition);
        
        onMouseDragEnd?.Invoke(dimensions, position);
        mouseDragSelection.gameObject.SetActive(false);
    }

    private Vector2 GetSelectBoxWorldDimensions(Vector3 startPosition, Vector2 currentEndPosition)
    {
        float zPosition = Camera.main.transform.forward == Vector3.forward ? Mathf.Abs(Camera.main.transform.position.z) - 3.2f : Camera.main.transform.position.y - 3;
        Vector3 currentEndWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentEndPosition.x, currentEndPosition.y, zPosition));

        Vector3 dimensions = currentEndWorldPosition - startPosition;
        dimensions = new Vector2(Mathf.Abs(dimensions.x), Camera.main.transform.forward == Vector3.forward ? Mathf.Abs(dimensions.y) : Mathf.Abs(dimensions.z));

        return dimensions;
    }

    private Vector3 GetSelectBoxWorldPosition(Vector3 startPosition, Vector2 currentEndPosition)
    {
        Vector3 position = new Vector3();
        float zPosition = Camera.main.transform.forward == Vector3.forward ? Mathf.Abs(Camera.main.transform.position.z) - 3.2f : Camera.main.transform.position.y - 3;
        Vector3 currentEndWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentEndPosition.x, currentEndPosition.y, zPosition));
        if (Camera.main.transform.forward == Vector3.forward)
        {
            if (startPosition.x < currentEndWorldPosition.x)
            {
                position.x = currentEndWorldPosition.x - ((currentEndWorldPosition.x - startPosition.x) / 2f);
            }
            else
            {
                position.x = startPosition.x - ((startPosition.x - currentEndWorldPosition.x) / 2f);
            }

            if (startPosition.y > currentEndWorldPosition.y)
            {
                position.y = currentEndWorldPosition.y - ((currentEndWorldPosition.y - startPosition.y) / 2f);
            }
            else
            {
                position.y = startPosition.y - ((startPosition.y - currentEndWorldPosition.y) / 2f);
            }
            position.z = -3;
        }
        else
        {
            if (startPosition.x < currentEndWorldPosition.x)
            {
                position.x = currentEndWorldPosition.x - ((currentEndWorldPosition.x - startPosition.x) / 2f);
            }
            else
            {
                position.x = startPosition.x - ((startPosition.x - currentEndWorldPosition.x) / 2f);
            }

            if (startPosition.z > currentEndWorldPosition.z)
            {
                position.z = currentEndWorldPosition.z - ((currentEndWorldPosition.z - startPosition.z) / 2f);
            }
            else
            {
                position.z = startPosition.z - ((startPosition.z - currentEndWorldPosition.z) / 2f);
            }
            position.y = 3;
        }
        

        return position;
    }
}
