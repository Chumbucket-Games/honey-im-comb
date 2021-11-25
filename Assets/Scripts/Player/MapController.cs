using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour, PlayerControls.IUnitManagementActions
{
    [SerializeField] CinemachineVirtualCamera hiveCamera;
    [SerializeField] CinemachineVirtualCamera overworldCamera;

    private PlayerControls playerControls;
    private Vector2 cursorPosition = Vector2.zero;
    private List<ISelectable> selectedObjects = new List<ISelectable>();
    public bool IsHiveMode { get; private set; } = true;
    [SerializeField] EnemySpawner[] spawners;
    static int Pebbles = 200;
    static int Honey = 200;

    static int MaxPebbles;
    static int MaxHoney;

    bool boxSelectQueued = false;
    Vector3 dimensions;
    Vector3 position;


    public static int GetTotalPebbles()
    {
        return Pebbles;
    }

    public static int GetTotalHoney()
    {
        return Honey;
    }

    public void ChangePebbles(int pebbles, bool reduce = true)
    {
        if (reduce)
        {
            Pebbles = Mathf.Max(0, Pebbles - pebbles);
        }
        else
        {
            Pebbles += pebbles;
        }
    }

    public void ChangeHoney(int honey, bool reduce = true)
    {
        if (reduce)
        {
            Honey = Mathf.Max(0, Honey - honey);
        }
        else
        {
            Honey += honey;
        }
    }

    private void Update()
    {
        int destroyedSpawners = 0;
        foreach (var spawner in spawners)
        {
            if (spawner.IsDead)
            {
                spawner.gameObject.SetActive(false);
                destroyedSpawners++;
            }
        }

        if (destroyedSpawners == spawners.Length)
        {
            // Win the game.
            WinGame();
        }

        HUDManager.GetInstance().SetTotalPebbles(Pebbles);
        HUDManager.GetInstance().SetTotalHoney(Honey);
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.UnitManagement.SetCallbacks(this);

            playerControls.CommonControls.ToggleMapMode.performed += OnMapToggle;
        }

        playerControls.UnitManagement.Enable();
        playerControls.CommonControls.ToggleMapMode.Enable();
    }

    public static void LoseGame()
    {
        Debug.Log("The Hive has fallen. This world will fall to Decay.");
        SceneManager.LoadScene(Constants.Scenes.MainMenu);
    }

    public static void WinGame()
    {
        Debug.Log("You won! The Decay is in retreat!");
        SceneManager.LoadScene(Constants.Scenes.Credits);
    }

    private void OnDisable()
    {
        playerControls.UnitManagement.Disable();
    }

    public void OnMapToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
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
    }

    void LateUpdate()
    {
        if (boxSelectQueued)
        {
            PerformBoxSelect();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && selectedObjects.Count > 0)
        {
            foreach (var selectedObject in selectedObjects)
            {
                var moveableObject = selectedObject as IMoveable;

                if (moveableObject != null)
                {
                    // Move the unit to the target location.
                    Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        if (IsHiveMode)
                        {
                            moveableObject.MoveToPosition(hit.transform.position, hit, true);
                        }
                        else
                        {
                            moveableObject.MoveToPosition(hit.point + Vector3.up * 3, hit, false);
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
    
    // We want to defer the box select until the end of the update cycle (LateUpdate) to allow UI events (eg. buttons) to be captured first.
    public void OnBoxSelect(Vector3 dimensions, Vector3 position)
    {
        this.dimensions = dimensions;
        this.position = position;
        boxSelectQueued = true;
    }

    public void PerformBoxSelect()
    {
        foreach (var selectedObject in selectedObjects)
        {
            selectedObject.OnDeselect();
        }
        selectedObjects.Clear();

        HUDManager.GetInstance().SetSelectedObject(null);

        Vector3 halfExtents = dimensions / 2f;
        RaycastHit[] hits = Physics.BoxCastAll(position, halfExtents, Camera.main.transform.forward == Vector3.forward ? -Vector3.forward : Vector3.up, Quaternion.identity, 20);
        ISelectable firstSelectedObject = null;
        int totalSelectedObjects = 0;

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Unit"))
            {
                if (firstSelectedObject == null)
                {
                    firstSelectedObject = hit.transform.gameObject.GetComponent<Unit>();
                }
                hit.transform.gameObject.GetComponent<Unit>().OnSelect();
                selectedObjects.Add(hit.transform.gameObject.GetComponent<Unit>());
                totalSelectedObjects++;
            }
            else if (hit.collider.CompareTag("Building") || hit.collider.CompareTag("Hive"))
            {
                if (firstSelectedObject == null)
                {
                    firstSelectedObject = hit.transform.gameObject.GetComponent<Building>();
                }
                totalSelectedObjects++;
            }
            else if (hit.collider.CompareTag("ResourceNode"))
            {
                if (firstSelectedObject == null)
                {
                    firstSelectedObject = hit.transform.gameObject.GetComponent<ResourceNode>();
                }
                totalSelectedObjects++;
            }
        }

        if (totalSelectedObjects == 1 && selectedObjects.Count == 0)
        {
            firstSelectedObject.OnSelect();
            selectedObjects.Add(firstSelectedObject);
            HUDManager.GetInstance().SetSelectedObject(selectedObjects[0]);
        }
        if (selectedObjects.Count > 0)
        {
            HUDManager.GetInstance().SetSelectedObject(selectedObjects[0]);
        }
        
        boxSelectQueued = false;
        dimensions = Vector3.zero;
        position = Vector3.zero;
    }

    public void OnSelect(InputAction.CallbackContext context)
    {

    }

    public void DisableInput()
    {
        playerControls.UnitManagement.Disable();
        playerControls.CommonControls.ToggleMapMode.Disable();
    }

    public void EnableInput()
    {
        playerControls.UnitManagement.Enable();
        playerControls.CommonControls.ToggleMapMode.Enable();
    }
}
