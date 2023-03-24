using System;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleCharacterView : UIBase {

    public Slider HpSlider;

    public TMP_Text HUDText;

    private void Awake() {
        HUDText = transform.Find("UI/HUDText").GetComponent<TMP_Text>();
    }

    public void ShowAttackInfo(string content, Vector3 jumpDirection, float duration = 2f)
    {
        TMP_Text hudText = Instantiate(HUDText, HUDText.transform.parent);
        hudText.text = content;
        hudText.gameObject.SetActive(true);
        hudText.DOFade(1f, 0.5f).From(0f).OnComplete(() => {
            hudText.DOFade(0f, Math.Max(0f, duration - 0.5f)).OnComplete(() => Destroy(hudText));
        });
        hudText.transform.DOJump(hudText.transform.position + jumpDirection, 0.5f, 1, 1f);
    }
    public static BattleCharacterView CreateNew()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/BattleCharacter");
        return Instantiate(prefab, GameObject.Find("Battle").transform).GetComponent<BattleCharacterView>();
    }
}

