using System;
using Controller;
using UnityEngine;

namespace Logic.Map
{
    public class MapGate : MonoBehaviour, IMapGate
    {
        public long targetMapID;

        public Vector3 enterPosition;

        public void OnEnter()
        {
            GameLoader.Instance.LoadWithAnimation(async () =>
            {
                try
                {
                    await WorldMapController.LoadMapAsync(targetMapID);
                    var oldMap = LifeEngine.Instance.Map;
                    LifeEngine.Instance.MainCharacter.transform.position = enterPosition;
                    WorldMapController.UnloadMap(oldMap.mapID);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }

                LifeEngine.Instance.lifeData.current.Location.MapID = targetMapID;
            }).Coroutine();
        }
        
    }
}