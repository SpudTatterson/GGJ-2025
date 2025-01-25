using System;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    public SpawnerMenu spawnerMenu;

    internal void DisableAllSelectionMenus()
    {
        spawnerMenu.gameObject.SetActive(false);
    }
}