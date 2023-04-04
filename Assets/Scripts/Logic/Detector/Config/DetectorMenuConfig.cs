using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.Detector.Config
{
    public static class InteractMenuConfig
    {
        private static readonly Dictionary<Type, long> DetectorMenus = new()
        {
            { typeof(TalkableNPCDetector), (long)InteractMenuType.NPC },
            { typeof(ShopableNPCDetector), (long)InteractMenuType.NPC },
            { typeof(MapGateDetector), (long)InteractMenuType.Gate }
        };


        public static List<InteractMenuItem> buildMenuItems(IEnumerable<KeyValuePair<IDetector, Collider2D>> data)
        {
            var result = new List<InteractMenuItem>();
            foreach (var item in data)
            {
                if (DetectorMenus.ContainsKey(item.Key.GetType()))
                {
                    result.Add(new InteractMenuItem()
                    {
                        detector = item.Key,
                        collision = item.Value,
                        menuID = DetectorMenus[item.Key.GetType()]
                    });
                }
            }

            return result;
        }
    }

    public enum InteractMenuType
    {
        NPC = 1,
        PICK = 2,
        Gate = 3
    }


    public struct InteractMenuItem
    {
        public IDetector detector;
        public Collider2D collision;
        public long menuID;
    }
}