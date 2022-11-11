using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using DG.Tweening;
using Zenject;

namespace XD
{
    public class ARController : PSYController, ISelectHandler
    {
        public delegate void JoySelectHandler(ARController item);
        public event JoySelectHandler OnJoySelect;

        public delegate void EnabledHandler(ARController item);
        public event EnabledHandler OnEnabled;

        public delegate void DisabledHandler(ARController item);
        public event DisabledHandler OnDisabled;

        [SerializeField]
        protected bool  bt = false;

        [SerializeField]
        protected bool  soundAlways = false;

        [SerializeField]
        private bool    cameraLock = false;

        [SerializeField]
        private bool    soundOnClick = true;

        private List<Tweener> delayedFunctions = new List<Tweener>() { };

        /*[Inject] protected IInput m_input;
        [Inject] protected IProfile m_profile;
        [Inject] protected IOptions m_options;
        [Inject] protected ILocalization m_localization;
        [Inject] protected IMainData m_maindata;
        [Inject] protected Redux.IStore<XDevs.GlobalState> m_store;*/


        [SoundMixerSelectorAttribute(SoundMixer.UI)]
        public int      sound = -1;

        public string Tag { get; set; }

        [SoundMixerSelectorAttribute(SoundMixer.UI)]
        public int      soundDeselect = -1;
        public int      tutorialID = -1;

        
        public bool     monopolist = false;

        public Selectable selectable = null;

        public Image joyFrame = null;

        protected CanvasGroup canvasGroup = null;

        protected override void OnAwake()
        {

            AddSelectable();                
        }

        public void AddSelectable()
        {
            /*if (ARGUEManager.IsGamepad && GamepadSelectable)
            {
                selectable = gameObject.GetComponent<Selectable>();
                if (selectable == null)
                {
                    selectable = gameObject.AddComponent<Selectable>();
                }
                selectable.targetGraphic = view.GetWidget("JoyFrame").Image;
                ColorBlock block = new ColorBlock();
                block.highlightedColor = Color.white;
                block.colorMultiplier = 1.0f;
                selectable.colors = block;
                view.GetWidget("JoyFrame").Transform.gameObject.SetActive(true);
            }*/
        }

        public PSYController AddChild(PSYItem type, Transform parent = null, string name = "", bool monopolist = false)
        {
            PSYController child = base.AddChild(type, parent, name);

            return child;
        }

        public override PSYController AddChild(PSYItem type, Transform parent = null, string name = "")
        {
            PSYController child = base.AddChild(type, parent, name);

            return child;
        }

        public override PSYController AddChild(PSYController controller, Transform parent = null, string name = "")
        {
            PSYController child = base.AddChild(controller, parent, name);

            return child;
        }

        private Selectable FindSelectable(Transform t)
        {
            foreach (Transform tt in t)
                if (tt.gameObject.GetComponent<PSYView>() == null)
                {
                    if (tt.gameObject.GetComponent<Selectable>() != null)
                        return tt.gameObject.GetComponent<Selectable>();

                    Selectable inChild = FindSelectable(tt);
                    if (inChild != null)
                    {
                        return inChild;
                    }
                }
            return null;
        }

        private void OnDisable()
        {
            if (OnDisabled != null)
            {
                OnDisabled(this);
            }
        }

        private void OnEnable() 
        {
            if (OnEnabled != null)
            {
                OnEnabled(this);
            }
        }

        public virtual void OnSelect(BaseEventData eventData)
        {            
            if (OnJoySelect != null)
            {
                OnJoySelect(this);
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {            
            
        }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup == null)
                {
                    canvasGroup = view.GetComponent<CanvasGroup>();
                }

                return canvasGroup;
            }
        }        

        public void JoyFocus()
        {
            selectable.Select();
        }

        public override void PSYEventHandler(PSYEvent e, PSYParams param)
        {
            base.PSYEventHandler(e, param);

            switch (e)
            {
                case PSYEvent.CeaseUI:
                    view.PlayAnimation(param.Bool ? PSYAnimationEvent.Cease : PSYAnimationEvent.Actuate);
                    break;
            }
        }

        protected override void OnViewClick(PSYViewEventArgs e)
        {
            if (e.param.Bool)
            {
                return;
            }

            if (view.isSelectable && !soundAlways)
            {
                return;
            }

            if (sound != -1 && soundOnClick)
            {
                PSYGUI.Event(PSYEvent.SoundRequestUI, sound.ToParam());
            }
        }

        protected override void OnViewMouseDown(PSYViewEventArgs e)
        {
            if (e.param.Bool)
            {
                return;
            }

            if (view.isSelectable && !soundAlways)
            {
                return;
            }

            if (sound != -1 && !soundOnClick)
            {
                PSYGUI.Event(PSYEvent.SoundRequestUI, sound.ToParam());
            }
        }

        protected override void OnViewSelect(PSYViewEventArgs e)
        {
            if (e.param.Bool || soundAlways)
            {
                return;
            }

            if (sound != -1)
            {
                PSYGUI.Event(PSYEvent.SoundRequestUI, sound.ToParam());
            }
        }

        protected override void OnViewDeselect()
        {
            if (soundDeselect != -1)
                PSYGUI.Event(PSYEvent.SoundRequestUI, soundDeselect.ToParam());
        }

        public void SetAlpha(float val)
        {
            CanvasGroup.alpha = Mathf.Clamp01(val); 
        }

        private int tweenInt = 0;
        protected void DelayedFunction(float delay, Action func)
        {
            delayedFunctions.Add(DOTween.To(() => tweenInt, x => tweenInt = x, 0, delay).OnComplete(() => func()));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            for (int i = 0; i < delayedFunctions.Count; i++)
            {
                delayedFunctions[i].Kill();
            }

            if (cameraLock)
            {
                //m_ui.IsCameraLocked = false;
            }
        }
    }    
}