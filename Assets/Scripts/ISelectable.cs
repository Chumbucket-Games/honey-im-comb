using UnityEngine;
using System.Collections;

public interface ISelectable
{
    bool IsMovable();
    void MoveToPosition(Vector3 position);
    void OnSelect();
}
