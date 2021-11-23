using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ResourceType", menuName = "Resource Type")]
public class ResourceType : ScriptableObject
{
    public string displayName;
    [Tooltip("The amount of the resource a bee can gather per second of interaction with the node.")]
    public float baseQuantityPerSecond;
    public Sprite image;
}
