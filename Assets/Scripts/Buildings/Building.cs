using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour, ISelectable
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material selectedMaterial;

    private MeshRenderer meshRenderer;

    float health;
    public bool IsDead { get; private set; } = false;
    public BuildingType type;
    public bool IsMovable()
    {
        return false;
    }

    public void MoveToPosition(Vector3 position, RaycastHit info, bool IsHiveMode)
    {
        throw new System.InvalidOperationException();
    }

    public void OnSelect()
    {
        meshRenderer.material = selectedMaterial;

        Debug.Log($"{type.label} selected.");
    }

    public void OnDeselect()
    {
        meshRenderer.material = defaultMaterial;
    }

    // Use this for initialization
    void Start()
    {
        health = type.maxHealth;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnDestroyed()
    {
        type.OnDestroyed(this);
        IsDead = true;
        Destroy(gameObject);
    }

    public void TakeDamage(int dmg)
    {
        health = Mathf.Max(0, health - dmg);

        if (health <= 0)
        {
            OnDestroyed();
        }
    }
}
