                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MyGame
{

    public class CellAnimationController : MonoBehaviour
    {

        public static CellAnimationController Instance { get; private set; }

        [SerializeField]
        private CellAnimation animPrefab;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            DOTween.Init();
        }

        public void SmoothTransition(ItemCell from, ItemCell to, bool isMerging)
        {
            to.HideUI();
            from.HideUI();
            Instantiate(animPrefab, transform, false).Move(from, to, isMerging);
        }

        public void SmoothAppear(ItemCell cell)
        {
            cell.HideUI();
            Instantiate(animPrefab, transform, false).Appear(cell);
        }
    }

}
