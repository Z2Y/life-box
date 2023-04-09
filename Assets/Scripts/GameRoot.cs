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

        if (Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer)
        {
            Application.targetFrameRate = 120;
        }
        // Application.targetFrameRate = 512;
    }

    public void NewGame() {
        GameLoader.Instance.LoadGame();
    }

    public void EndGame() {
        SceneManager.LoadScene("MainScene");
    }
}
