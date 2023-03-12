using Controller;

namespace Battle.Realtime.Ai
{
    public class AnimalNormalAttack : NPBehave.Task
    {
        private IAttackAnimator animator;
        
        public AnimalNormalAttack(IAttackAnimator animator) : base("AnimalNormalAttack")
        {
            this.animator = animator;
        }

        protected override void DoStart()
        {
            Clock.AddTimer(0.8f, 0.05f, -1, doAttack);
        }

        private void doAttack()
        {
            animator.AttackNormal();
        }

        protected override void DoStop()
        {
            Clock.RemoveTimer(doAttack);
            Stopped(true);
        }
    }
}