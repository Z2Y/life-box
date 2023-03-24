using System;
using Controller;
using Cysharp.Threading.Tasks;
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
                    var map = await WorldMapController.LoadMapAsync(targetMapID);
                    var oldMap = LifeEngine.Instance.Map;
                    await map.InitMapWithPosition(enterPosition);
                    LifeEngine.Instance.MainCharacter.transform.position = enterPosition;
                    WorldCameraController.Instance.JumpToFollowPos();
                    WorldMapController.UnloadMap(oldMap.mapID);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }

                LifeEngine.Instance.lifeData.current.Location.MapID = targetMapID;
            }).Forget();
        }

        public bool Interactive()
        {
            return enabled;
        }
    }
}