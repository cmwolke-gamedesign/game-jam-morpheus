using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class KeepScore : MonoBehaviour {

    public Text Score;
	// Use this for initialization
	void Start () {
        Score = GetComponent<Text>();
        Score.text = "Score: 3000";
	}
}
