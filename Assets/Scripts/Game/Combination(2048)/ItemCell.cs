using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCell : MonoBehaviour
{

    [SerializeField]
    private int MaxValue = 10;

    [SerializeField]
    private Image image;

    [SerializeField]
    private Text number;

    [SerializeField]
    private RectTransform rt;


    public Image Image { get { return image; } }
    public RectTransform RT { get { return rt; } }

    public int X { get; private set; }
    public int Y { get; private set; }

    public int CurrentPower { get; private set; }

    public bool IsEmpty => CurrentPower == 0;
    public bool HasMerged { get; private set; }

    private CellAnimation currentAnimation;

    public void SetValue(int x, int y, int value, bool updateUI = true )
    {
        X = x;
        Y = y;
        CurrentPower = value;

        if(updateUI)
            UpdateCell();
    }

    public void UpdateCell()
    {
        number.text = IsEmpty ? String.Empty : CurrentPower.ToString();

        if(ARGUETools.TryGetSprite("Cell_Number_" + CurrentPower, out var sprite))
        {
            image.color.SetAlpha(1);
            image.sprite = sprite;
        }
        else
        {
            image.color.SetAlpha(0);
        }
    }

    public void IncreaseValue()
    {
        CurrentPower++;
        HasMerged = true;

    }

    public void ResetFlags()
    {
        HasMerged = false;
    }

    public void MergeWithCell(ItemCell otherCell)
    {
        CellAnimationController.Instance.SmoothTransition(this, otherCell, true);

        otherCell.IncreaseValue();
        SetValue(X, Y, 0);
    }

    public void MoveToCell(ItemCell target)
    {
        CellAnimationController.Instance.SmoothTransition(this, target, false);

        target.SetValue(target.X, target.Y, CurrentPower);
        SetValue(X, Y, 0);
    }

    public void SetAnimation(CellAnimation animation)
    {
        currentAnimation = animation;
    }

    public void CancelAnimation()
    {
        if(currentAnimation!= null)
        {
            currentAnimation.Destroy();
        }
    }
}
