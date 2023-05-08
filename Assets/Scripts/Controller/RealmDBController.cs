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

        // ReSharper disable method MethodHasAsyncOverload
        public static async UniTask copyDbFile()
        {
            var sourcePath = $"{Application.streamingAssetsPath}/db.realm";
            var targetPath = $"{Application.persistentDataPath}/db.realm";
            // Debug.Log($"Checking Db File is in {Application.persistentDataPath}");
            if (!File.Exists(targetPath))
            {
                Debug.Log($"Db File is Not Exist in {Application.persistentDataPath}, Copying...");
                if (Application.platform != RuntimePlatform.Android)
                {
                    File.Copy(sourcePath, targetPath);
                }
                else
                {
                    var req = UnityWebRequest.Get(sourcePath);
                    await req.SendWebRequest();
                    File.WriteAllBytes(targetPath, req.downloadHandler.data);
                }
            }
            else
            {
                // todo check db version and update db assets
            }
            config = new RealmConfiguration(targetPath) { FallbackPipePath = getAndroidFilesDir()};
            
            Db = Realm.GetInstance(config);
            Debug.Log($"Db Connected.");
        }

        private static string getAndroidFilesDir()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            return currentActivity.Call<AndroidJavaObject>("getFilesDir").Call<string>("getCanonicalPath");
#else
            return "";
#endif
        }
    }
}