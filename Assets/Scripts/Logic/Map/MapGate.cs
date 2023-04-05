using System;
using Cathei.LinqGen;
using Controller;
using Logic.Message;
using UniTaskPubSub;
using UnityEngine;
using Utils;

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
                var enterMapMsg = SimplePoolManager.Get<EnterMap>();
                var leaveMapMsg = SimplePoolManager.Get<LeaveMap>();
                try
                {
                    
                    var oldMap = LifeEngine.Instance.Map;
                    leaveMapMsg.mapID = oldMap.mapID;
                    await AsyncMessageBus.Default.PublishAsync<MapMessage>(leaveMapMsg);
                    
                    var map = await WorldMapController.LoadMapAsync(mapID);
                    
                    await map.InitMapWithPosition(Vector3.zero);
                    saveLifeNode(map, map.transform.TransformPoint(position));
                    WorldCameraController.Instance.JumpToFollowPos();
                    WorldMapController.UnloadMap(oldMap.mapID);

                    
                    enterMapMsg.mapID = mapID;
                    await AsyncMessageBus.Default.PublishAsync<MapMessage>(enterMapMsg);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
                finally
                {
                    enterMapMsg.Dispose();
                    leaveMapMsg.Dispose();
                }
                
            }).Coroutine();        
        }

        private static void saveLifeNode(WorldMapController map, Vector3 enterPosition)
        {
            var mainCharacter = LifeEngine.Instance.MainCharacter;
            var currentNode = LifeEngine.Instance.lifeData.current;
            var nextNode = LifeEngine.Instance.lifeData.NextNode();
            var placeContain = new PlaceContains() { position = enterPosition };
            currentNode.Location.Position = mainCharacter.transform.position;
            nextNode.Location.PlaceID = map.ActivePlaces.Gen().Where(placeContain).First().placeID;
            nextNode.Location.MapID = map.mapID;
            nextNode.Location.Position = enterPosition;
            mainCharacter.transform.position = enterPosition;
            LifeEngine.Instance.lifeData.current = nextNode;
        }

        public bool Interactive()
        {
            return enabled;
        }
    }
}