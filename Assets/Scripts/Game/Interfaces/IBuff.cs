using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    public enum BuffType
    {
        DurationEffect = 0,
        EveryTurn = 1,
    }

    public interface IBuff : IVisualizable
    {
        BuffType BuffType { get; }
        bool isNegative { get; }

        UnitStat AffectStat { get; }

        int duration { get; }

        public void OnApply(IUnit unit);

        public void OnRemove(IUnit unit);

        public void EveryTurnUpdate(IUnit unit);
    }

}
