using System;

namespace MyGame
{

    public enum PassiveType
    {
        OngoingToTeam = 0,
        OnBattleStart = 1,
        AfterTurn = 2,
        OnAttack = 3,
        OnHitTaken = 4,

    }

    public interface IPassive
    {
        public PassiveType PassiveType { get; }

        public void ApplyEffect(IUnit ally);
    }

}
