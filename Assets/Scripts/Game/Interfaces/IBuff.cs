using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    public enum BuffType
    {
        DurationEffect = 0,
        EveryTurn = 1,
        OnEndEffect = 2,
    }

    public interface IBuff : IVisualizable
    {
        BuffType BuffType { get; }
        DamageType DamageType { get; }

        bool IsNegative { get; }

        public abstract void OnApply(IUnit unit);

        public abstract void OnRemove(IUnit unit);

        public abstract void EveryTurnUpdate(IUnit unit);
    }

}
