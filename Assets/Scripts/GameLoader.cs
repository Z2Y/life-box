using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    private static GameLoader _instance;
    public Text loadingText;
    public CanvasGroup loadingCanvas;
    private bool isLoading;

    public static GameLoader Instance
    {
        get
        {
            if (_instance != null) return _instance;
            
            _instance = Instantiate(Resources.Load<GameObject>("Prefabs/LoadingPanel")).GetComponent<GameLoader>();
            return _instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadGame()
    {
        LoadGameAsync().Coroutine();
    }

    public async Task LoadSceneWithAnimation(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        await CrossFade(async () =>
        {
            await LoadSceneAsync(sceneName, mode);
        });
    }

    public async Task SwitchSceneWithAnimation(Scene origin, Scene current)
    {
        await CrossFade(async () =>
        {
            SceneManager.SetActiveScene(current);
            await YieldCoroutine.WaitForInstruction(SceneManager.UnloadSceneAsync(origin));
        });
    }

    public async Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        var loadOp = SceneManager.LoadSceneAsync(sceneName, mode);
        while (!loadOp.isDone)
        {
            loadingText.text = $"载入中。。。 {(int)(loadOp.progress * 100)}%";
            await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
        }
        loadingText.text = "载入中。。。 100%";
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    private async Task LoadGameAsync()
    {
        if (isLoading) return;
        try
        {
            isLoading = true;
            await LoadSceneAsync("LifeScene");
            await LoadModelData();
            await LifeEngine.Instance.CreateNewGame();
            await FadeOut(0f);
        }
        catch (Exception e)
        {
            Debug.Log($"Load Game Failed! ${e.StackTrace} ");
        }

        isLoading = false;
    }

    private async Task CrossFade(Func<Task> action)
    {
        await FadeIn(0.5f);
        await action();
        await FadeOut(0.5f);
    }

    private async Task FadeOut(float duration)
    {
        await YieldCoroutine.WaitForInstruction(loadingCanvas.DOFade(0f, duration).WaitForCompletion());
    }

    private async Task FadeIn(float duration)
    {
        await YieldCoroutine.WaitForInstruction(loadingCanvas.DOFade(1f, duration).WaitForCompletion());
    }

    private async Task LoadModelData()
    {
        while (!(ModelLoader.Instance?.loaded ?? false))
        {
            loadingText.text = "读取数据。。。";
            await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
        }
    }
}
