using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    public SpawnerMenu spawnerMenu;

    public Image sectorFillUpBarGerm;
    public Image sectorFillUpBarBubble;

    internal void DisableAllSelectionMenus()
    {
        spawnerMenu.gameObject.SetActive(false);
    }
}