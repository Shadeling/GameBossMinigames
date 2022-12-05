using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.UI
{

    public class ItemSpell : PSYController
    {
        private ISpell spell;

        protected override void OnSetup(PSYParams param)
        {
            spell = param.Get<ISpell>();

            
        }


        protected override void OnChildClick(PSYViewEventArgs e)
        {
            PSYGUI.Event(PSYEvent.SpellClicked, PSYParams.New(spell));
        }
    }
}
