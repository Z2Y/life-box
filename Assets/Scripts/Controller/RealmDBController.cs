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
            Debug.Log($"Checking Db File is in {Application.persistentDataPath}");
            if (!File.Exists($"{Application.persistentDataPath}/db.realm"))
            {
                Debug.Log($"Db File is Not Exist in {Application.persistentDataPath}, Copying...");
                if (Application.platform != RuntimePlatform.Android)
                {

                    await UniTask.RunOnThreadPool(() =>
                    {
                        File.Copy($"{Application.streamingAssetsPath}/db.realm",
                            $"{Application.persistentDataPath}/db.realm");
                    });
                }
                else
                {
                    var req = UnityWebRequest.Get($"{Application.streamingAssetsPath}/db.realm");
                    await req.SendWebRequest();
                    await File.WriteAllBytesAsync($"{Application.persistentDataPath}/db.realm",
                        req.downloadHandler.data);
                }
            }
            else
            {
                // todo check db version and update db assets
            }
            config = new(Application.persistentDataPath + "/db.realm");
            Db = await Realm.GetInstanceAsync(config);
        }
    }
}