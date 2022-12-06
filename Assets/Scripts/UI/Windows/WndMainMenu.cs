using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyGame.UI {

    public class WndMainMenu : ARController
    {


        protected override void OnChildClick(PSYViewEventArgs e)
        {
            switch (e.index)
            {
                case 0:
                    PSYGUI.Event(PSYEvent.LayoutRequest, PSYParams.New(ARPage.BattlePage));
                    break;
                case 1:
                    PSYGUI.Event(PSYEvent.LayoutRequest, PSYParams.New(ARPage.Combination));
                    break;
                case 2:

                    break;
            }
        }
    }

}
