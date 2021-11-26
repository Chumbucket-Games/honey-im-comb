using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] Button[] actionButtons;
    [SerializeField] Image selectedObjectView;
    [SerializeField] Text selectedObjectName;
    [SerializeField] Text selectedObjectHealth;
    [SerializeField] Text selectedObjectPebbles;
    [SerializeField] Text selectedObjectNectar;
    [SerializeField] Text totalPebbles;
    [SerializeField] Text totalNectar;
    [SerializeField] Text unitCap;
    [SerializeField] Text gameTime;
    [SerializeField] Text errorDisplay;
    [SerializeField] float errorDisplaySeconds = 5;
    [SerializeField] VerticalLayoutGroup notificationPane;
    [SerializeField] VerticalLayoutGroup objectDetails;
    [SerializeField] VerticalLayoutGroup pauseMenu;
    [SerializeField] Image winDialogue;
    [SerializeField] Image lossDialogue;
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
        if (t.Hours != 0)
        {
            gameTime.text = $"{t.Hours}:{t.Minutes}:{t.Seconds}";
        }
        else
        {
            gameTime.text = $"{t.Minutes}:{t.Seconds}";
        }
        
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

    public void SetUnitCap(int currentUnits, int unitCap)
    {
        this.unitCap.text = $"{currentUnits.ToString()}/{unitCap.ToString()}";
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
            if (actionID == 1 && ((Building)selectedObject).type.label == "Throne Room")
            {
                ((Building)selectedObject).GrowBee();
            }
            else
            {
                ((Building)selectedObject).DismantleBuilding();
            }
        }
    }

    public void CreateNotification(Notification n)
    {
        Instantiate(n, notificationPane.transform);
    }

    public void ShowSelectedObjectDetails(Camera selectedObjectCamera)
    {
        if (selectedObjectCamera != null)
        {
            selectedObjectCamera.gameObject.SetActive(true);
            selectedObjectView.gameObject.SetActive(true);
        }
        objectDetails.gameObject.SetActive(true);
    }

    public void HideSelectedObjectDetails(Camera selectedObjectCamera)
    {
        objectDetails.gameObject.SetActive(false);
        if (selectedObjectCamera != null)
        {
            selectedObjectCamera.gameObject.SetActive(false);
            selectedObjectView.gameObject.SetActive(false);
        }
    }

    public void PauseGame()
    {
        pauseMenu.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenu.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(Constants.Scenes.MainMenu);
    }

    public void ExitToDesktop()
    {
        Application.Quit();
    }

    public void LoseGame()
    {
        lossDialogue.gameObject.SetActive(true);
        StartCoroutine(DelayedSceneTransition(Constants.Scenes.MainMenu));
    }

    public void WinGame()
    {
        lossDialogue.gameObject.SetActive(true);
        StartCoroutine(DelayedSceneTransition(Constants.Scenes.Credits));
    }

    IEnumerator DelayedSceneTransition(int sceneId)
    {
        yield return new WaitForSeconds(10);
        SceneManager.LoadScene(sceneId);
    }

    public void DisplayErrorMessage(string error)
    {
        errorDisplay.text = error;
        errorDisplay.gameObject.SetActive(true);
        StartCoroutine(DismissDialogue());
    }

    IEnumerator DismissDialogue()
    {
        yield return new WaitForSeconds(errorDisplaySeconds);
        errorDisplay.gameObject.SetActive(false);
    }
}
