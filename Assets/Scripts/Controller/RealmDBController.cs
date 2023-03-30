using System;
using Realms;
using UnityEngine;

namespace Controller
{
    public class RealmDBController : MonoBehaviour
    {
        
        private readonly RealmConfiguration config = new (Application.streamingAssetsPath + "/db.realm");
        
        public static Realm Realm { get; private set; }

        private void Awake()
        {
            Realm = Realm.GetInstance(config);
        }
    }
}