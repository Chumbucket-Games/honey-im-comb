using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour, ISelectable
{
    public bool IsMovable()
    {
        return false;
    }

    public void MoveToPosition(Vector3 position)
    {
        throw new System.InvalidOperationException();
    }

    public void OnSelect()
    {
        Debug.Log("Building selected.");
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
