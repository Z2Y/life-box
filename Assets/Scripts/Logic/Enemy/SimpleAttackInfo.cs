using System;
using System.Collections;
using System.Threading.Tasks;
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

        private void OnDisable()
        {
            StopCoroutine(nameof(autoHideHpBar));
            HpSlider.value = 1f;
        }

        public async void ShowAttackInfo(string content, Vector3 jumpDirection, float duration = 1.25f)
        {
            if (await UIManager.Instance.FindOrCreateAsync<SimpleHUDText>() is SimpleHUDText hudUI)
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
            hudUI.hud.transform.localScale = Vector3.one;
            hudUI.Hide();
        }

        public void UpdateHp(PropertyValue Hp)
        {
            
            if (gameObject.activeSelf)
            {
                HpSlider.DOValue((float)Hp.value / Hp.max, 0.5f);
            }
            else
            {
                Show();
                HpSlider.value = (float)Hp.value / Hp.max;
            }
            
            StopCoroutine(nameof(autoHideHpBar));
            StartCoroutine(nameof(autoHideHpBar));
        }

        private IEnumerator autoHideHpBar()
        {
            yield return new WaitForSeconds(5f);
            Hide();
        }

        public static async Task<SimpleAttackInfo> CreateAsync()
        {
            var panel = await UIManager.Instance.FindOrCreateAsync<SimpleAttackInfo>() as SimpleAttackInfo;

            return panel;
        }
    }

}