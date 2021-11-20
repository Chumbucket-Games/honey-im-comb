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
    static int Pebbles;
    static int Honey;

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
        Debug.Log("You lost. This world will fall to Decay.");
        SceneManager.LoadScene("Main Menu");
    }

    public static void WinGame()
    {
        Debug.Log("You won! The Decay is in retreat!");
        SceneManager.LoadScene("Main Menu");
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
                            selectedObject.MoveToPosition(hit.point + new Vector3(0, 3, 0), hit, false);
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

    public void OnBoxSelect(Vector3 dimensions, Vector3 position)
    {
        foreach (var selectedObject in selectedObjects)
        {
            selectedObject.OnDeselect();
        }
        selectedObjects.Clear();

        Vector3 halfExtents = dimensions / 2f;

        // Select the units within the bounding box. Cast up from the ground in overworld mode; otherwise cast forward.
        RaycastHit[] hits = Physics.BoxCastAll(position, halfExtents, Camera.main.transform.forward == Vector3.forward ? Camera.main.transform.forward : Vector3.up, Quaternion.identity, 20);

        if (hits.Length == 1)
        {
            if (hits[0].collider.CompareTag("Building"))
            {
                hits[0].transform.gameObject.GetComponent<Building>().OnSelect();
                selectedObjects.Add(hits[0].transform.gameObject.GetComponent<Building>());
            }
            else if (hits[0].collider.CompareTag("ResourceNode"))
            {
                hits[0].transform.gameObject.GetComponent<ResourceNode>().OnSelect();
                selectedObjects.Add(hits[0].transform.gameObject.GetComponent<ResourceNode>());
            }
        }
            
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Unit"))
            {
                hit.transform.gameObject.GetComponent<Unit>().SelectUnit();
                selectedObjects.Add(hit.transform.gameObject.GetComponent<Unit>());

                Debug.Log($"Unit {hit.transform.gameObject.name} selected");
            }
        }
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
