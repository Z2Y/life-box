using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FancyScrollView;


public class UIItemGridView : FancyGridView<ItemCellData, ItemCellContext>
{
    class CellGroup : DefaultCellGroup { }

    [SerializeField] UIItemViewCell cellPrefab = default;


    protected override void SetupCellTemplate() => Setup<CellGroup>(cellPrefab);

    public void OnPointerEnterCell(Action<int> callback)
    {
        Context.OnPointerEnterCell = callback;
    }

    public void OnPointerExitCell(Action<int> callback)
    {
        Context.OnPointerExitCell = callback;
    }

    public void OnPointerClickCell(Action<int> callback)
    {
        Context.OnPointerClickCell = callback;
    }

}

