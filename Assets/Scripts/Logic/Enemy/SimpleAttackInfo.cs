using System;
using System.Threading.Tasks;
using TMPro;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Logic.Enemy
{

    [PrefabResource("Prefabs/AttackInfo")]
    public class SimpleAttackInfo : UIBase {

        public Slider HpSlider;

        public Transform uiTransform;

        private Vector3 localOffset;

        private void Awake() {
            localOffset = transform.localPosition;
            uiTransform = transform.Find("UI");
        }

        public async void ShowAttackInfo(string content, Vector3 jumpDirection, float duration = 1.25f)
        {
            var hudUI = await UIManager.Instance.FindOrCreateAsync<SimpleHUDText>() as SimpleHUDText;
            if (!ReferenceEquals(hudUI, null))
            {
                var hud = hudUI.hud;
                hud.transform.SetParent(uiTransform, false);
                hud.transform.localScale = new Vector3(Math.Sign(hud.transform.lossyScale.x), 1, 1);
                hud.text = content;
                hud.gameObject.SetActive(true);
                hud.DOFade(1f, 0.5f).From(0f).OnComplete(() => {
                    hud.DOFade(0f, Math.Max(0f, duration - 0.5f)).OnComplete(() => onHideInfo(hudUI));
                });
                hud.transform.DOJump(hud.transform.position + jumpDirection, 0.5f, 1, 1f);
            }
        }

        private void onHideInfo(SimpleHUDText hudUI)
        {
            hudUI.hud.transform.localPosition = localOffset;
            hudUI.Hide();
        }

        public void UpdateHp(PropertyValue Hp)
        {
            HpSlider.DOValue((float)Hp.value / Hp.max, 0.5f);
        }
        
        public static async Task<SimpleAttackInfo> Show()
        {
            var panel = await UIManager.Instance.FindOrCreateAsync<SimpleAttackInfo>() as SimpleAttackInfo;

            return panel;
        }
    }

}