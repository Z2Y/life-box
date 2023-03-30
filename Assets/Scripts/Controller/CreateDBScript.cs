using UnityEngine;
using SQLite;
using UnityEngine.UI;

public class CreateDBScript : MonoBehaviour {

    public Text DebugText;

    // Use this for initialization
    void Start () {
        StartSync();
    }

    private void StartSync()
    {
        var filepath = $"{Application.persistentDataPath}/TestRoom.db";
        var _connection = new SQLiteConnection(filepath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        Debug.Log(filepath);
        _connection.CreateTable<Model.Character>();
    }
}