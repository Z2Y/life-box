using System;
using UnityEngine;

public class LifeEngine : MonoBehaviour {

    public static LifeEngine Instance { get; private set;}

    public LifeTime lifeTime {get; private set;}

    public LifeData lifeData {get; private set;}

    public Action AfterLifeChange;
    public Action OnLifeEnd;
    public Action OnLifeStart;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        lifeTime = this.gameObject.AddComponent<LifeTime>();
        lifeTime.OnNextMonth += OnNextMonth;
    }

    public void CreateNewGame() {
        lifeData = LifeData.CreateNew();
        lifeData.DoForcast(lifeTime);
        lifeData.current.ProcessEvent().Coroutine();
        LifeCardManager.Instance.UpdateCardActions();
        OnLifeStart?.Invoke();
        AfterLifeChange?.Invoke();
    }

    private async void OnNextMonth() {
        if (lifeData.current.IsDeath) {
            OnLifeEnd?.Invoke();
            GameRoot.Instance.EndGame();
            return;
        }
        await lifeData.DoNext();
        lifeData.DoForcast(lifeTime);
        LifeCardManager.Instance.UpdateCardActions();
        AfterLifeChange?.Invoke();
    }
    
}