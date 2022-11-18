using System;
using UnityEngine;


namespace MyGame
{
    public enum UnlockType
    {
        None = 0,
        Level = 1,
        InPack = 2,
    }

    [Serializable]
    public class UnlockCondition
    {
        [SerializeField]
        public UnlockType unlockType = UnlockType.None;

        [SerializeField]
        public int unlockValue = 0;
    }
}
