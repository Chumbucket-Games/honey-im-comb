using UnityEngine;
using System.Collections;

public class Notification : MonoBehaviour
{
    [SerializeField] int duration = 5;
    public bool soundAlarm = false;
    // Use this for initialization
    void Start()
    {
        StartCoroutine(WaitToDestroy(duration));
    }

    IEnumerator WaitToDestroy(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
