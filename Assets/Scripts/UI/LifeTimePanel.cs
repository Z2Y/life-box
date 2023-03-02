using UnityEngine.UI;

public class LifeTimePanel : UIBase {
    private Text timeText;

    void Awake() {
        timeText = transform.Find("Text").GetComponent<Text>();
    }

    void Start() {
        ListenLifeTime();
        Refresh();
    }

    void OnDisable() {
        if (LifeEngine.Instance != null) {
            LifeEngine.Instance.AfterLifeChange -= Refresh;
        }
    }

    void ListenLifeTime() {
        if (LifeEngine.Instance != null) {
            LifeEngine.Instance.AfterLifeChange += Refresh;
        }        
    }

    public void Refresh() {
        timeText.text = LifeEngine.Instance.lifeTime.timeStr;
    }
}