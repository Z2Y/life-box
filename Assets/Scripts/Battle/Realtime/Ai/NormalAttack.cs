using NPBehave;
// ReSharper disable Unity.NoNullPropagation

namespace Battle.Realtime.Ai
{
    public class NormalAttack : NPBehave.Task
    {
        private readonly BattleSkillAction skillAction;
        
        public NormalAttack(BattleSkillAction skillAction) : base("NormalAttack")
        {
            this.skillAction = skillAction;
            skillAction.Init();
        }

        protected override void DoStart()
        {
            Clock.AddTimer(skillAction.skill.CoolDown, 0.05f, -1, doAttack);
        }

        private async void doAttack()
        {
            if (!skillAction.isIdle())
            {
                return;
            }
            skillAction.prepare();
            await YieldCoroutine.WaitForSeconds(skillAction.skillAnimation?.length ?? 0.005f);
            skillAction.endPrepare();
            
            if (!skillAction.isReady())
            {
                return;
            }

            skillAction.DoSkill();
        }

        protected override void Stopped(bool success)
        {
            Clock.RemoveTimer(doAttack);
        }
    }
}