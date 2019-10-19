using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    int idiotCnt;
    public Text idiotTxt;
	// Use this for initialization
	void Start () {
        idiotCnt = 0;
        SceneManager.UnloadScene(1);
    }

    public void exitGameButton()
    {
        Application.Quit();
    }


    public void startNewGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void selectLevel()
    {
        if (!idiotTxt.IsActive())
        {
            idiotTxt.gameObject.SetActive(true);
        }
        idiotTxt.text = "Idiot Counter: "+ ++idiotCnt;
    }
}
