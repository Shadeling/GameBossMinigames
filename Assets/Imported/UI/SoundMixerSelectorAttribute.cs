using System.Collections.Generic;
using UnityEngine;

namespace XD
{
    public class SoundMixerSelectorAttribute : PropertyAttribute
    {
        public readonly List<int> mixer = null;

        public SoundMixerSelectorAttribute(params SoundMixer[] mixer)
        {
            this.mixer = new List<int>(mixer.Length);
            for (int i = 0; i < mixer.Length; i++)
            {
                this.mixer.Add((int)mixer[i]);
            }
        }
    }
}
