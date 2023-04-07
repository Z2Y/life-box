using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Realms;
using UnityEngine;
using UnityEngine.Networking;

namespace Controller
{
    public class RealmDBController : MonoBehaviour
    {
        
        private static RealmConfiguration config;
        
        public static Realm Db { get; private set; }

        public static async UniTask copyDbFile()
        {
            var sourcePath = $"{Application.streamingAssetsPath}/db.realm";
            var targetPath = $"{Application.persistentDataPath}/db.realm";
            Debug.Log($"Checking Db File is in {Application.persistentDataPath}");
            if (File.Exists(targetPath))
            {
                Debug.Log($"Db File is Not Exist in {Application.persistentDataPath}, Copying...");
                if (Application.platform != RuntimePlatform.Android)
                {
                    File.Copy(sourcePath, targetPath);
                }
                else
                {
                    Debug.Log($"Read Streaming Assets...");
                    var req = UnityWebRequest.Get(sourcePath);
                    await req.SendWebRequest();
                    Debug.Log($"Writing PersistentDataPath Assets... {req.downloadHandler.data.Length}");
                    File.WriteAllBytes(targetPath, req.downloadHandler.data);
                }
                Debug.Log($"Copy Completed");
            }
            else
            {
                // todo check db version and update db assets
            }
            config = new(targetPath);
            Db = Realm.GetInstance(config);
            Debug.Log($"Db Connected.");
        }
    }
}