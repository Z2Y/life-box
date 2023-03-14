using System;
using Model;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemTypeFilter: UIBase, IPointerClickHandler
{
    public ItemType ItemType;
    public bool None;
    public KnapsackPanel knapsack;
    

    public void OnPointerClick(PointerEventData eventData)
    {
        if (None) {
            knapsack?.UpdateItemGridView();
        } else {
            knapsack?.FilterItemByType(ItemType);
        }
    }
}

