using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }
    

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }

        if (Application.isMobilePlatform)
        {
            Application.targetFrameRate = 120;
        }
    }

    public void NewGame() {
        GameLoader.Instance.LoadGame();
    }

    public void EndGame() {
        SceneManager.LoadScene("MainScene");
    }
}
