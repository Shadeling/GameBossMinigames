using System;

namespace MyGame
{

    public enum PassiveType
    {
        WhileAlive       = 0,
        OnBattleStart    = 1,
        AfterTurn        = 2,
        OnAttack         = 3,
        OnHitTaken       = 4,
        OnDeath          = 5,

    }

    public enum EffectType
    {
        OnSelf = 0,
        OnMyTeam = 1,
        OnEnemyTeam = 2,
        All = 3,
    }

    public interface IPassive
    {
        public PassiveType PassiveType { get; }

        public EffectType EffectType { get; }

        public void ApplyEffect(IUnit ally);

        public void CancelEffect(IUnit ally);
    }

}
