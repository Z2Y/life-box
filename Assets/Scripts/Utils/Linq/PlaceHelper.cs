using Controller;
using Model;
using StructLinq;
using UnityEngine;

namespace Utils
{
    public struct PlaceContains : IFunction<PlaceController, bool>
    {
        public Vector3 position;
        
        public bool Eval(PlaceController element)
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