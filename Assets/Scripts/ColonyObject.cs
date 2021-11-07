using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public abstract class ColonyObject : ScriptableObject
{
    public string label;
    public Image image;
    public float maxHealth;
}