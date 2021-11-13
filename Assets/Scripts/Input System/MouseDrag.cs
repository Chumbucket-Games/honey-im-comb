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
    [SerializeField] private UnityEvent<Vector2, Vector2> onMouseDragEnd;

    private bool isMouseDown = false;
    private Vector2 startPosition = Vector2.zero;


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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isMouseDown)
        {
            var currentEndPosition = Mouse.current.position.ReadValue();
            var dimensions = GetSelectBoxDimensions(startPosition, currentEndPosition);
            var position = GetSelectBoxPosition(startPosition, currentEndPosition);

            mouseDragSelection.rectTransform.position = position;
            mouseDragSelection.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, dimensions.x);
            mouseDragSelection.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dimensions.y);
        }
    }

    void OnSelect(InputAction.CallbackContext context)
    {
        isMouseDown = true;
        startPosition = Mouse.current.position.ReadValue();
        mouseDragSelection.gameObject.SetActive(true);
    }
    
    void OnSelectCancelled(InputAction.CallbackContext context)
    {
        isMouseDown = false;

        var endPosition = Mouse.current.position.ReadValue();
        var dimensions = GetSelectBoxDimensions(startPosition, endPosition);
        var position = GetSelectBoxPosition(startPosition, endPosition);
        
        onMouseDragEnd?.Invoke(dimensions, position);
        mouseDragSelection.gameObject.SetActive(false);
    }

    private Vector2 GetSelectBoxDimensions(Vector2 startPosition, Vector2 currentEndPosition)
    {
        Vector2 dimensions = currentEndPosition - startPosition;
        dimensions = new Vector2(Mathf.Abs(dimensions.x), Mathf.Abs(dimensions.y));

        return dimensions;
    }

    private Vector2 GetSelectBoxPosition(Vector2 startPosition, Vector2 currentEndPosition)
    {
        Vector2 position = new Vector2();
        if (startPosition.x < currentEndPosition.x)
        {
            position.x = currentEndPosition.x - ((currentEndPosition.x - startPosition.x) / 2f);
        }
        else
        {
            position.x = startPosition.x - ((startPosition.x - currentEndPosition.x) / 2f);
        }

        if (startPosition.y > currentEndPosition.y)
        {
            position.y = currentEndPosition.y - ((currentEndPosition.y - startPosition.y) / 2f);
        }
        else
        {
            position.y = startPosition.y - ((startPosition.y - currentEndPosition.y) / 2f);
        }

        return position;
    }
}
