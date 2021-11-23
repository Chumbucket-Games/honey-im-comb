using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public abstract class ColonyObject : ScriptableObject
{
    public string label;
    public Sprite image;
    public float maxHealth;
    public Sprite[] actionSprites;
}