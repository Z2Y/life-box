using System;
using Realms;
using UnityEngine;

namespace Controller
{
    public class RealmDBController : MonoBehaviour
    {
        
        private RealmConfiguration config;
        
        public static Realm Db { get; private set; }

        private void Awake()
        {
            config = new(Application.persistentDataPath + "/db.realm");
            Db = Realm.GetInstance(config);
        }
    }
}