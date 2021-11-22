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
    }

    public void OnDeselect()
    {
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(false);
        }
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
}
