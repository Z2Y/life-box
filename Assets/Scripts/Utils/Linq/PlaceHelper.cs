using Cathei.LinqGen;
using Controller;
using Model;
using UnityEngine;

namespace Utils
{
    public struct PlaceContains : IStructFunction<PlaceController, bool>
    {
        public Vector3 position;
        
        public bool Invoke(PlaceController element)
        {
            return element.bounds.Contains(position);
        }
    }

    public static class PlaceHelper
    {
        public static string NameOf(Place place)
        {
            return place.Name;
        }
    }
}