using System;
using System.IO;
using Cysharp.Threading.Tasks;
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
            copyDbFile();
            config = new(Application.persistentDataPath + "/db.realm");
            Db = Realm.GetInstance(config);
        }

        private static void copyDbFile()
        {
            Debug.Log($"Checking Db File is in {Application.persistentDataPath}");
            if (!File.Exists($"{Application.persistentDataPath}/db.realm"))
            {
                Debug.Log($"Db File is Not Exist in {Application.persistentDataPath}, Copying...");
                File.Copy($"{Application.streamingAssetsPath}/db.realm", $"{Application.persistentDataPath}/db.realm");
            }
        }
    }
}