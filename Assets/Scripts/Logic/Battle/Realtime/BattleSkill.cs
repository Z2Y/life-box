using Logic.Battle.Realtime.SkillAction;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Logic.Battle.Realtime
{
    public class BattleSkill : MonoBehaviour
    {

        [SerializeField] public KeyCode keycode;

        [SerializeField] public JoyStickButton button;

        public ISkillAction action;

        private bool usingButton;

        private void Start()
        {
            action.Init();
            if (JoyStickController.isReady)
            {
                button = JoyStickController.Instance.GetButtonFor(keycode);
                if (button != null)
                {
                    button.onDown.AddListener(DoPrepareSkill);
                    button.onUp.AddListener(DoEndPrepareSkill);
                    usingButton = true;
                }
            }
        }

        private void Update()
        {
            if (usingButton) return;
            if (Input.GetKeyDown(keycode))
            {
                DoPrepareSkill();
            }



            if (Input.GetKeyUp(keycode))
            {
                DoEndPrepareSkill();
            }
        }

        private void LateUpdate()
        {
            action.Update();
        }

        private void DoPrepareSkill()
        {
            if (!action.isIdle())
            {
                return;
            }

            if (!action.isReady())
            {
                action.prepare();
            }
            else
            {
                action.DoSkill();
            }
        }

        private void DoEndPrepareSkill()
        {
            if (!action.isPreparing())
            {
                return;
            }
            action.endPrepare();
            
            if (!action.isReady())
            {
                return;
            }
            
            action.DoSkill();
        }

    }
}