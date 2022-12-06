using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.UI
{

    public class ItemSpell : PSYController
    {
        [SerializeField] Image spellImage;

        private ISpell spell;

        protected override void OnSetup(PSYParams param)
        {
            spell = param.Get<ISpell>();

            if(spell == null)
            {
                view.Disable();
            }
            else
            {
                view.Enable();
                spellImage.sprite = ARGUETools.LoadSprite(spell.SpriteName);
            }
        }


        protected override void OnChildClick(PSYViewEventArgs e)
        {
            PSYGUI.Event(PSYEvent.SpellClicked, PSYParams.New(spell));
        }
    }
}
