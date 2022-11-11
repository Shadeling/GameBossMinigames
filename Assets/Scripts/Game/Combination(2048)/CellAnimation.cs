using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellAnimation : MonoBehaviour
{
    [SerializeField]
    private Image image;

    [SerializeField]
    private Text number;

    [SerializeField]
    private RectTransform rt;

    private float moveTime = 0.1f;
    private float appearTime = 0.1f;

    private Sequence sequence;

    public void Move(ItemCell from, ItemCell to, bool isMerging)
    {
        from.CancelAnimation();
        to.SetAnimation(this);

        CopyCellUI(from);


        transform.position = from.transform.position;
        rt.sizeDelta = from.RT.sizeDelta;

        sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(to.transform.position, moveTime).SetEase(Ease.InOutQuad));

        if (isMerging)
        {
            sequence.AppendCallback(() =>
            {
                CopyCellUI(to);
            });

            sequence.Append(transform.DOScale(1.2f, appearTime));
            sequence.Append(transform.DOScale(1f, appearTime));
        }

        sequence.AppendCallback(() =>
        {
            to.UpdateCell();
            Destroy();
        });
    }

    public void Appear(ItemCell cell)
    {
        cell.CancelAnimation();
        cell.SetAnimation(this);

        CopyCellUI(cell);

        transform.position = cell.transform.position;
        rt.sizeDelta = cell.RT.sizeDelta;
        transform.localScale = Vector3.zero;

        sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(1.2f, appearTime * 2 ));
        sequence.Append(transform.DOScale(1f, appearTime * 2));

        sequence.AppendCallback(() =>
        {
            cell.UpdateCell();
            Destroy();
        });
    }

    private void CopyCellUI(ItemCell cell)
    {
        image.sprite = cell.Image.sprite;
        image.color.SetAlpha(cell.Image.color.a);
        number.text = cell.CurrentPower.ToString();
    }

    public void Destroy()
    {
        sequence.Kill();
        Destroy(gameObject);
    }
    
}
