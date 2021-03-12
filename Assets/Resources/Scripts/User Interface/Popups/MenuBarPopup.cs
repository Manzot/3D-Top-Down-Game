using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBarPopup : Popup
{
    InventoryPopup inventoryPopup;
    QuestPopupUI questPopupUI;
    PauseMenuPopup pauseMenuPopup;
    SubMenuPopup subMenuPopup;
    MapPopup mapPopup;
    ShopPopup shopPopup;
    List<Popup> lstPopup;

    public ButtonElement questButton;
    public ButtonElement inventoryButton;
    public ButtonElement mapButton;
    public ButtonElement systemMenuButton;

    ButtonElement selectedButtonElement;
    private void Start()
    {
        open();
        lstPopup = new List<Popup>();
        inventoryPopup = PopupUIManager.Instance.inventoryPopup;
        lstPopup.Add(inventoryPopup);
        questPopupUI = PopupUIManager.Instance.questPopupUI;
        lstPopup.Add(questPopupUI);
        pauseMenuPopup = PopupUIManager.Instance.pauseMenuPopup;
        lstPopup.Add(pauseMenuPopup);
        subMenuPopup = PopupUIManager.Instance.subMenuPopup;
        lstPopup.Add(subMenuPopup);
        mapPopup = PopupUIManager.Instance.mapPopup;
        lstPopup.Add(mapPopup);
        shopPopup = PopupUIManager.Instance.shopPopup;
        lstPopup.Add(shopPopup);
        close();

    }

    public override void open()
    {
        base.open();
        GameController.bGamePaused = true;
        GameController.inPlayMode = false;
        Time.timeScale = 0f;
      //  selectedButtonElement = null;
    }
    public override void close()
    {
        GameController.bGamePaused = false;
        GameController.inPlayMode = true;
        Time.timeScale = 1f;
       // selectedButtonElement = null;
        base.close();
    }
    public void OpenInventoryUI()
    {
        CloseAllPopups();

        SetSelectedButton(inventoryButton);
        if (GameController.inPlayMode)
        {
            GameController.inPlayMode = false;
            inventoryPopup.UpdateInventoryUI(PlayerController.Instance.GetInventory());
            inventoryPopup.open();
        }
    }
    public void OpenPauseMenuUI()
    {
        CloseAllPopups();

        SetSelectedButton(systemMenuButton);
        if (GameController.inPlayMode)
        {
            GameController.inPlayMode = false;
            pauseMenuPopup.open();
        }
    }
    public void OpenMapUI()
    {
        CloseAllPopups();

        SetSelectedButton(mapButton);
        if (GameController.inPlayMode)
        {
            GameController.inPlayMode = false;
            mapPopup.open();
        }
    }
    public void OpenQuestUI()
    {
        CloseAllPopups();

        SetSelectedButton(questButton);
        if (GameController.inPlayMode)
        {
            GameController.inPlayMode = false;
            questPopupUI.open();
        }
    }
    public void CloseAllPopups()
    {
        for (int i = 0; i < lstPopup.Count; i++)
        {
            lstPopup[i].close();
        }
      //  if(selectedButtonElement != null)
           // selectedButtonElement.SetSelectedElement(false);
    }
    public void SetSelectedButton(ButtonElement _selectedBtn)
    {
        if(selectedButtonElement != null)
        {
            selectedButtonElement.SetSelectedElement(false);
        }

        selectedButtonElement = _selectedBtn;
        selectedButtonElement.SetSelectedElement(true);
    }
}
