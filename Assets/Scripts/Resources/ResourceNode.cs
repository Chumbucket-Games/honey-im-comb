using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour, ISelectable
{
    public ResourceType resource;
    [Tooltip("The total amount of the resource in this node.")]
    public int totalAmount;
    int remainingAmount; // This will be reduced based on the length of time a bee is harvesting the node.

    public bool IsMovable()
    {
        return false;
    }

    public void MoveToPosition(Vector3 position, RaycastHit info)
    {
        throw new System.NotImplementedException();
    }

    public void OnSelect()
    {
        Debug.Log($"{remainingAmount} {resource.displayName} remaining.");
    }

    // Start is called before the first frame update
    void Start()
    {
        remainingAmount = totalAmount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
