using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void OnNewGameButtonPressed()
    {
        SceneManager.LoadScene(Constants.Scenes.TheHive);
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }
}
