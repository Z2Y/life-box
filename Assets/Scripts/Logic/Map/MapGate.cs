using System;
using System.Linq;
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
            JumpTo(targetMapID, enterPosition);
        }

        public static void JumpTo(long mapID, Vector3 position)
        {
            GameLoader.Instance.LoadWithAnimation(async () =>
            {
                try
                {
                    var map = await WorldMapController.LoadMapAsync(mapID);
                    var oldMap = LifeEngine.Instance.Map;
                    await map.InitMapWithPosition(position);
                    saveLifeNode(map, position);
                    WorldCameraController.Instance.JumpToFollowPos();
                    WorldMapController.UnloadMap(oldMap.mapID);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
                
            }).Coroutine();        
        }

        private static void saveLifeNode(WorldMapController map, Vector3 enterPosition)
        {
            var mainCharacter = LifeEngine.Instance.MainCharacter;
            var currentNode = LifeEngine.Instance.lifeData.current;
            var nextNode = LifeEngine.Instance.lifeData.NextNode();
            currentNode.Location.Position = mainCharacter.transform.position;
            nextNode.Location.PlaceID = map.ActivePlaces.First((place) => place.bounds.Contains(enterPosition)).placeID;
            nextNode.Location.MapID = map.mapID;
            nextNode.Location.Position = enterPosition;
            mainCharacter.transform.position = enterPosition;
        }

        public bool Interactive()
        {
            return enabled;
        }
    }
}