using System;
using Controller;
using Cysharp.Threading.Tasks;
using Logic.Loot;
using UnityEngine;

public class LifeEngine : MonoBehaviour {

    public static LifeEngine Instance { get; private set;}
    public LifeTime lifeTime {get; private set;}
    public LifeData lifeData {get; private set;}

    public bool isReady;

    public WorldMapController Map => WorldMapController.GetWorldMapController(lifeData.current.Location.MapID);

    public PlaceController Place => PlaceController.GetPlaceController(lifeData.current.Location.PlaceID);

    public NPCController MainCharacter => NPCController.GetCharacterController(0); 

    public Action AfterLifeChange;
    public Action OnLifeStart;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        lifeTime = gameObject.AddComponent<LifeTime>();
        lifeTime.OnNextMonth += OnNextMonth;
    }

    public async UniTask CreateNewGame() {
        lifeData = LifeData.CreateNew();
        var map = await WorldMapController.LoadMapAsync(lifeData.current.Location.MapID);
        var mainCharacter = await NPCController.LoadCharacterAsync(0);
        var enemy = await NPCController.LoadCharacterAsync(1);

        await map.InitMapWithPosition(lifeData.current.Location.Position);
        
        // setup main character for player
        mainCharacter.SetLocation(lifeData.current.Location);
        mainCharacter.SetAsPlayer(true);
        WorldCameraController.Instance.FollowTo(mainCharacter.gameObject).Forget();
        isReady = true;
        
        // LifeCardManager.Instance.UpdateCardActions();
        OnLifeStart?.Invoke();
        AfterLifeChange?.Invoke();
    }

    public void GameEnd()
    {
        WorldCameraController.Instance.FollowTo(null).Coroutine();
        SwordSlashController.Pool.RecycleUsed();
        WorldLootObject.Pool.RecycleUsed();
        GameLoader.Instance.LoadSceneWithAnimation("MainScene").Coroutine();
    }

    private async void OnNextMonth() {
        if (lifeData.current.IsDeath) {
            GameRoot.Instance.EndGame();
            return;
        }
        await lifeData.DoNext();
        lifeData.DoForecast(lifeTime);
        LifeCardManager.Instance.UpdateCardActions();
        AfterLifeChange?.Invoke();
    }
    
}