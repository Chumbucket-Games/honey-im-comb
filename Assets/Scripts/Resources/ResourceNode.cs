using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceType resource;
    [Tooltip("The total amount of the resource in this node.")]
    public int totalAmount;
    int remainingAmount; // This will be reduced based on the length of time a bee is harvesting the node.
    
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
