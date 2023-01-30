using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeMenuPanel : UIBase {

    private Button nextMonthBtn;

    void Awake() {
        nextMonthBtn = transform.Find("Button").GetComponent<Button>();

        nextMonthBtn.onClick.AddListener(() => {
            LifeEngine.Instance.lifeTime.NextMonth();
        });
    }

}