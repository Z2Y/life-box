namespace Logic.Battle.Realtime.SkillAction
{
    public interface ISkillAction
    {
        public bool isIdle();

        public bool isReady();

        public bool isPreparing();

        public void DoSkill();

        public void endPrepare();

        public void prepare();

        public void Init();

        public void Update();
    }
}