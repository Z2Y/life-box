using System.Collections;
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
            if (_instance == null)
            {
                GameObject loadPrefab = Resources.Load<GameObject>("Prefabs/LoadingPanel");
                _instance = GameObject.Instantiate(loadPrefab).GetComponent<GameLoader>();
            }
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

    public async Task LoadSceneWithAnimation(string name, LoadSceneMode mode = LoadSceneMode.Single)
    {
        Sequence animation = crossFase();
        await LoadSceneAsync(name, mode);
        await YieldCoroutine.WaitForInstruction(animation.WaitForCompletion());
    }

    public async Task SwitchSceneWithAnimation(Scene origin, Scene current)
    {
        Sequence animation = crossFase();
        await YieldCoroutine.WaitForSeconds(0.5f);
        UnityEngine.Debug.Log(origin.name);
        UnityEngine.Debug.Log(current.name);
        SceneManager.UnloadSceneAsync(origin);
        SceneManager.SetActiveScene(current);
        await YieldCoroutine.WaitForInstruction(animation.WaitForCompletion());
    }

    public async Task LoadSceneAsync(string name, LoadSceneMode mode = LoadSceneMode.Single)
    {
        var loadOp = SceneManager.LoadSceneAsync(name, mode);
        while (!loadOp.isDone)
        {
            loadingText.text = string.Format("载入中。。。 {0}%", (int)(loadOp.progress * 100));
            await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
        }
        loadingText.text = "载入中。。。 100%";
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
    }

    public async Task LoadGameAsync()
    {
        await LoadSceneAsync("LifeScene");
        await loadModelData();
        LifeEngine.Instance.CreateNewGame();
    }

    private Sequence crossFase()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(loadingCanvas.DOFade(1f, 0.5f));
        sequence.Append(loadingCanvas.DOFade(0f, 0.5f));
        return sequence;
    }

    private async Task loadModelData()
    {
        while (!(ModelLoader.Instance?.loaded ?? false))
        {
            loadingText.text = string.Format("读取数据。。。");
            await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
        }
    }
}
