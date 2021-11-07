using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDebugShape : MonoBehaviour
{
    [SerializeField] private Color shapeColor = Color.red;
    [SerializeField] private Shape shape = Shape.Cube;
    [SerializeField] private float size = 1f;

    public enum Shape
    {
        Cube,
        Sphere
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = shapeColor;

        switch (shape)
        {
            case Shape.Cube:
                Gizmos.DrawCube(gameObject.transform.position, new Vector3(size, size, size));
                break;
            case Shape.Sphere:
                Gizmos.DrawSphere(gameObject.transform.position, size);
                break;
        }
    }
}
