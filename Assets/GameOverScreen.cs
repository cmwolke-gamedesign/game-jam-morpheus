using UnityEngine;
using System.Collections;
using TinyMessenger;
using Assets.Scripts.message.custom;
using Assets.Scripts.manager;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour {

    public GameObject GameOverPanel;
    public GameObject GameWonPanel;
    public MeterFillScript PowerMeter;
    public Text Score;
    private IMessageBus _bus;

	void Start () {
        _bus = Initialiser.Instance.GetService<IMessageBus>();
        _bus.Subscribe<GameOverMessage>(OnGameOver);
        _bus.Subscribe<GoalReachedMessage>(OnGoalReched);
	}

    public void OnGameOver(GameOverMessage msg)
    {
        GameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void OnGoalReched(GoalReachedMessage msg)
    {
        GameWonPanel.SetActive(true);
        PowerMeter.StartScoreDown = false;
        Score.text = "Your Score: " + PowerMeter.score.ToString();
    }

    public void backToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

}
