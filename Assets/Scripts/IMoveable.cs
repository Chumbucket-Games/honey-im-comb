using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    void MoveToPosition(Vector3 position, RaycastHit info, bool IsHiveMode, bool emptyStartCell = true);
}
