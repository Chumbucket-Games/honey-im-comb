using UnityEngine;
using System.Collections;

public interface ISelectable
{
    void OnSelect();
    void OnDeselect();

    System.Type GetObjectType();
}
