using System;
using System.Collections.Generic;
using Model;
using UnityEngine;

namespace Logic.Detector.Config
{
    public static class InteractMenuConfig
    {
        public static readonly Dictionary<Type, long> DetectorMenus = new()
        {
            { typeof(TalkableNPCDetector), (long)InteractMenuType.NPC },
            { typeof(ShopableNPCDetector), (long)InteractMenuType.NPC }
        };


        public static List<InteractMenuItem> buildMenuItems(List<KeyValuePair<IDetector, Collision>> data)
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
        PICK = 2
    }


    public class InteractMenuItem
    {
        public IDetector detector;
        public Collision collision;
        public long menuID;
    }
}