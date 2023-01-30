using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
    }

    public void NewGame() {
        GameLoader.Instance.LoadGame();
    }

    public void EndGame() {
        SceneManager.LoadScene("MainScene");
    }
}
