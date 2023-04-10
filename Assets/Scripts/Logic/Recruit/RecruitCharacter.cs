using System;
using Model;

namespace Logic.Recruit
{
    [Serializable]
    public class RecruitCharacter
    {
        public Character Character;
        public LifeProperty Property;
        public int Cost;
        public int Level;
    }
}