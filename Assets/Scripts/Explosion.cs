using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    [SerializeField] ParticleSystem vfx;
    // Use this for initialization
    void OnEnable()
    {
        vfx.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!vfx.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
