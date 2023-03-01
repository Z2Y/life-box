using HeroEditor.Common.Enums;
using CharacterScripts = Assets.HeroEditor.Common.Scripts.CharacterScripts;
using UnityEngine;

namespace Controller
{
    public class NPCAttackController : MonoBehaviour
    {
        [SerializeField] private NPCAnimationController animator;
        [SerializeField] private bool isPlayer;
        [SerializeField] public KeyCode normalAtk = KeyCode.Mouse0;
        [SerializeField] private bool fromJoystick;

        // private Transform armL;
        // private Transform armR;

        private CharacterScripts.Character character;

        private void Start()
        {
            character = gameObject.GetComponent<CharacterScripts.Character>();
            animator = gameObject.GetComponent<NPCAnimationController>();
            // armL = character.BodyRenderers[0].transform;
            // armR = character.BodyRenderers[1].transform;
        }

        public void SetAsPlayer(bool player)
        {
            isPlayer = player;
            enabled = player;
        }


        public void Update()
        {
            switch (character.WeaponType)
            {
                case WeaponType.Melee1H:
                case WeaponType.Melee2H:
                case WeaponType.MeleePaired:
                    if (Input.GetKeyDown(normalAtk))
                    {
                       animator.AttackNormal();
                    }
                    break;
            }
            
            if (Input.GetKeyDown(normalAtk))
            {
                animator.GetReady();
            }
        }
    }
}