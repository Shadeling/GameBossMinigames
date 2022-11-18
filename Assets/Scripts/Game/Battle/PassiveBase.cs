using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [CreateAssetMenu(fileName = "PassiveBase", menuName = "ScriptableObjects/Passives/PassiveBase", order = 1)]
    public class PassiveBase : ScriptableObject, IPassive
    {

        [SerializeField]
        private PassiveType passiveType;

        [SerializeField]
        private List<StatValue> stats;

        [SerializeField]
        private List<ResistanceValue> resistances;

        [SerializeField]
        private List<BuffBase> buffs;



        public PassiveType PassiveType => passiveType;

        public void ApplyEffect(IUnit ally)
        {
            throw new System.NotImplementedException();
        }
    }
}
