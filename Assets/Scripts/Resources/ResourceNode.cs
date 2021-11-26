using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour, ISelectable
{
    public ResourceType resource;
    [Tooltip("The total amount of the resource in this node.")]
    public int totalAmount;
    int remainingAmount; // This will be reduced based on the length of time a bee is harvesting the node.
    [SerializeField] GameObject selectionRing;
    [SerializeField] Camera selectionView;

    public bool IsMovable()
    {
        return false;
    }

    public void MoveToPosition(Vector3 position, RaycastHit info, bool IsHiveMode)
    {
        throw new System.InvalidOperationException();
    }

    public void OnSelect()
    {
        Debug.Log($"{remainingAmount} {resource.displayName} remaining.");
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(true);
        }
        HUDManager.GetInstance().SetSelectedObjectDetails(resource.displayName, remainingAmount, 0, 0);
        HUDManager.GetInstance().SetActionImages(null);
        HUDManager.GetInstance().ShowSelectedObjectDetails(selectionView);
    }

    public void OnDeselect()
    {
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(false);
        }
        HUDManager.GetInstance().HideSelectedObjectDetails(selectionView);
    }

    // Start is called before the first frame update
    void Start()
    {
        remainingAmount = totalAmount;
    }

    // Update is called once per frame
    void Update()
    {
        if (remainingAmount <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public int HarvestResources(float harvestDuration)
    {
        int value = Mathf.RoundToInt(resource.baseQuantityPerSecond * harvestDuration);
        remainingAmount = Mathf.Max(0, remainingAmount - value);
        return value;
    }

    public System.Type GetObjectType()
    {
        return this.GetType();
    }

    public GameObject GetGameObject()
    {
        if (gameObject != null)
            return gameObject;
        return null;
    }
}
