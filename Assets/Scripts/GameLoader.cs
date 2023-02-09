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
        var anim = crossFade();
        await LoadSceneAsync(sceneName, mode);
        await YieldCoroutine.WaitForInstruction(anim.WaitForCompletion());
    }

    public async Task SwitchSceneWithAnimation(Scene origin, Scene current)
    {
        var anim = crossFade();
        await YieldCoroutine.WaitForSeconds(0.5f);
        SceneManager.UnloadSceneAsync(origin);
        SceneManager.SetActiveScene(current);
        await YieldCoroutine.WaitForInstruction(anim.WaitForCompletion());
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
        await LoadSceneAsync("LifeScene");
        await loadModelData();
        LifeEngine.Instance.CreateNewGame();
    }

    private Sequence crossFade()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(loadingCanvas.DOFade(1f, 0.5f));
        sequence.Append(loadingCanvas.DOFade(0f, 0.5f));
        return sequence;
    }

    private async Task loadModelData()
    {
        while (!(ModelLoader.Instance?.loaded ?? false))
        {
            loadingText.text = "读取数据。。。";
            await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
        }
    }
}
