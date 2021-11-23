using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class HUDManager : MonoBehaviour
{
    [SerializeField] Button[] actionButtons;
    [SerializeField] Image selectedObjectImage;
    [SerializeField] Text selectedObjectName;
    [SerializeField] Text selectedObjectHealth;
    [SerializeField] Text selectedObjectPebbles;
    [SerializeField] Text selectedObjectNectar;
    [SerializeField] Text totalPebbles;
    [SerializeField] Text totalNectar;
    [SerializeField] Text unitCap;
    [SerializeField] Text gameTime;
    static HUDManager instance;
    float elapsedTime = 0;
    private ISelectable selectedObject;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        TimeSpan t = TimeSpan.FromSeconds(elapsedTime);
        gameTime.text = $"{t.Hours}:{t.Minutes}:{t.Seconds}";
    }

    public static HUDManager GetInstance()
    {
        if (instance == null)
        {
            instance = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUDManager>();
        }
        return instance;
    }

    public void SetActionImages(Sprite[] images)
    {
        int totalActions = 0;
        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                actionButtons[i].transform.GetChild(1).GetComponent<Image>().sprite = images[i];
                actionButtons[totalActions++].gameObject.SetActive(true);
            }
        }

        while (totalActions < 9)
        {
            // Disable all remaining actions.
            actionButtons[totalActions].transform.GetChild(1).GetComponent<Image>().sprite = null;
            actionButtons[totalActions].gameObject.SetActive(false);
            totalActions++;
        }
    }

    /*public void SetSelectedObjectImage(Sprite image)
    {
        selectedObjectImage.sprite = image;
    }*/

    public void SetSelectedObjectDetails(string name, int health, int pebbles, int nectar)
    {
        selectedObjectName.text = name;
        selectedObjectHealth.text = health.ToString();
        selectedObjectPebbles.text = pebbles.ToString();
        selectedObjectNectar.text = nectar.ToString();
    }

    public void SetSelectedObjectHealth(int health)
    {
        selectedObjectHealth.text = health.ToString();
    }

    public void SetSelectedObjectResources(int pebbles, int nectar)
    {
        selectedObjectPebbles.text = pebbles.ToString();
        selectedObjectNectar.text = nectar.ToString();
    }

    public void SetTotalPebbles(int pebbles)
    {
        totalPebbles.text = pebbles.ToString();
    }

    public void SetTotalHoney(int nectar)
    {
        totalNectar.text = nectar.ToString();
    }

    public void SetUnitCap(int unitCap)
    {
        this.unitCap.text = unitCap.ToString();
    }

    public void SetSelectedObject(ISelectable selected)
    {
        selectedObject = selected;
    }

    public void RunAction(int actionID)
    {
        if (selectedObject.GetObjectType() == typeof(Unit))
        {
            ((Unit)selectedObject).type.PerformAction(actionID, (Unit)selectedObject);
        }
        else if (selectedObject.GetObjectType() == typeof(Building))
        {
            if (actionID == 1 && ((Building)selectedObject).type.label == "Throne")
            {
                ((Building)selectedObject).DismantleBuilding();
            }
            else
            {
                ((Building)selectedObject).DismantleBuilding();
            }
        }
    }
}
