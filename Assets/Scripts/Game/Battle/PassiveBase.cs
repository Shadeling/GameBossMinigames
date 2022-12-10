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
        public EffectType effectType;

        [SerializeField]
        private List<StatValue> stats;

        [SerializeField]
        private List<ResistanceValue> resistances;

        [SerializeField]
        private List<BuffWithDuration> buffs;



        public PassiveType PassiveType => passiveType;

        public EffectType EffectType => effectType;

        public void ApplyEffect(IUnit ally)
        {
            foreach (StatValue stat in stats)
            {
                ally.ChangeStats(stat.stat, stat.value);
            }

            foreach(ResistanceValue resistance in resistances)
            {
                ally.ChangeResistance(resistance.resistanceType, resistance.change);
            }

            foreach (var buff in buffs)
            {
                ally.AddBuff(buff.buff, buff.duration);
            }
        }

        public void CancelEffect(IUnit ally)
        {
            foreach (StatValue stat in stats)
            {
                ally.ChangeStats(stat.stat, -1 * stat.value);
            }

            foreach (ResistanceValue resistance in resistances)
            {
                ally.ChangeResistance(resistance.resistanceType, -1 * resistance.change);
            }
        }
    }
}
