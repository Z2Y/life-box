using System;
using Controller;
using Cysharp.Threading.Tasks;
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
        CrossFade(LoadGameAsync).Coroutine();
    }

    public async UniTask LoadSceneWithAnimation(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        await CrossFade(async () =>
        {
            await LoadSceneAsync(sceneName, mode);
        });
    }

    public async UniTask LoadWithAnimation(Func<UniTask> action)
    {
        await CrossFade(action);
    }
    
    public async UniTask SwitchSceneWithAnimation(Scene origin, Scene current)
    {
        await CrossFade(async () =>
        {
            SceneManager.SetActiveScene(current);
            await SceneManager.UnloadSceneAsync(origin);
        });
    }

    public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        var loadOp = SceneManager.LoadSceneAsync(sceneName, mode);
        while (!loadOp.isDone)
        {
            loadingText.text = $"载入中。。。 {(int)(loadOp.progress * 100)}%";
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);;
        }
        loadingText.text = "载入中。。。 100%";
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    private async UniTask LoadGameAsync()
    {
        if (isLoading) return;
        try
        {
            isLoading = true;
            await LoadSceneAsync("LifeScene");
            if (Application.isMobilePlatform)
            {
                await JoyStickController.LoadAsync();
            }
            await LifeEngine.Instance.CreateNewGame();
            await FadeOut(0f);
        }
        catch (Exception e)
        {
            Debug.Log($"Load Game Failed! ${e.Message} ${e.StackTrace} ");
        }

        isLoading = false;
    }

    private async UniTask CrossFade(Func<UniTask> action)
    {
        await FadeIn(0.5f);
        await action();
        await FadeOut(0.5f);
    }

    private async UniTask FadeOut(float duration)
    {
        await YieldCoroutine.WaitForInstruction(loadingCanvas.DOFade(0f, duration).WaitForCompletion());
    }

    private async UniTask FadeIn(float duration)
    {
        await YieldCoroutine.WaitForInstruction(loadingCanvas.DOFade(1f, duration).WaitForCompletion());
    }

    private async UniTask LoadModelData()
    {
        while (!(ModelLoader.Instance?.loaded ?? false))
        {
            loadingText.text = "读取数据。。。";
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }
    }
}
