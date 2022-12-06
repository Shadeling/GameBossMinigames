using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyGame.UI
{

    public enum ARPage
    {
        None = -1,
        BattlePage = 0,
        MainMenu = 1,
        Combination = 2,
    }

    public class MainUIManager : PSYManager
    {
        private static MainUIManager instance = null;

        private ARPage page = ARPage.MainMenu;

        protected override void Init()
        {
            instance = this;
            Layout(ARPage.MainMenu);
        }

        public override void PSYEventHandler(PSYEvent e, PSYParams param)
        {
            switch (e)
            {
            }
        }

        protected override void PSYEventPostHandler(PSYEvent e, PSYParams param)
        {
            switch (e)
            {
                case PSYEvent.LayoutRequest:
                    LayoutRequest(param);
                    break;
            }
        }

        private void SetPage(PSYParams param)
        {
            ARPage newPage = param.Get<ARPage>();

            switch (newPage)
            {
                case ARPage.BattlePage:
                    HideAllBut();
                    Show(PSYWindow.WndBattleField, param);
                    Show(PSYWindow.WndSpellSelecting, param);
                    break;

                case ARPage.MainMenu:
                    HideAllBut();
                    Show(PSYWindow.WndMainMenu, param);
                    break;

                case ARPage.Combination:
                    HideAllBut();
                    Show(PSYWindow.WndCombinationField, param);
                    break;


            }
        }

        private void LayoutRequest(PSYParams param)
        {
            Layout(param);
        }

        public static void Layout(ARPage newPage, PSYParams param = null)
        {
            if (param == null)
            {
                param = PSYParams.Empty;
            }

            instance.Layout(param.Add(newPage));
        }

        private void Layout(PSYParams param)
        {
            if (param == null)
            {
                return;
            }

            if (param.Contains<PSYWindow>())
            {
                SetWindow(param);
            }
            else
            {
                SetPage(param);
            }
        }

        private void SetWindow(PSYParams param)
        {
            switch (param.Get<PSYWindow>())
            {
                default:
                    Show(param.Get<PSYWindow>(), param);
                    break;
            }
        }
    }

}
