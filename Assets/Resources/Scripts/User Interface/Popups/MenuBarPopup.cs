using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBarPopup : Popup
{
    InventoryPopup inventoryPopup;
    QuestPopupUI questPopupUI;
    PauseMenuPopup pauseMenuPopup;
  //  SubMenuPopup subMenuPopup;
    MapPopup mapPopup;
    //ShopPopup shopPopup;
    List<Popup> lstPopups;

    int iSelectedMenu = 0;

    public ButtonElement questButton;
    public ButtonElement inventoryButton;
    public ButtonElement mapButton;
    public ButtonElement systemMenuButton;

    ButtonElement selectedButtonElement;
    private void Start()
    {
        open();
        lstPopups = new List<Popup>();
        questPopupUI = PopupUIManager.Instance.questPopupUI;
        lstPopups.Add(questPopupUI);
        inventoryPopup = PopupUIManager.Instance.inventoryPopup;
        lstPopups.Add(inventoryPopup);
        mapPopup = PopupUIManager.Instance.mapPopup;
        lstPopups.Add(mapPopup);
        pauseMenuPopup = PopupUIManager.Instance.pauseMenuPopup;
        lstPopups.Add(pauseMenuPopup);
      //  subMenuPopup = PopupUIManager.Instance.subMenuPopup;
      //  lstPopups.Add(subMenuPopup);
       // shopPopup = PopupUIManager.Instance.shopPopup;
       // lstPopups.Add(shopPopup);
        close();

    }

    private void Update()
    {
        if (container.gameObject.activeSelf)
        {
            if (Input.GetButtonDown("PreviousMenu"))
            {
                if (iSelectedMenu > 0)
                    ChangeMenu(iSelectedMenu - 1);
            }
            if (Input.GetButtonDown("NextMenu"))
            {
                if (iSelectedMenu < lstPopups.Count)
                    ChangeMenu(iSelectedMenu + 1);
            }
        }
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
    public void OpenQuestUI()
    {
        CloseAllPopups();

        SetSelectedButton(questButton);
        if (GameController.inPlayMode)
        {
            GameController.inPlayMode = false;
            questPopupUI.open();
        }
        iSelectedMenu = 1;
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
        iSelectedMenu = 2;
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
        iSelectedMenu = 3;
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
        iSelectedMenu = 4;
    }
    public void CloseAllPopups()
    {
        for (int i = 0; i < lstPopups.Count; i++)
        {
            lstPopups[i].close();
        }
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
    void ChangeMenu(int _iMenuToOpen)
    {
        switch (_iMenuToOpen)
        {
            case 1:
                OpenQuestUI();
                break;
            case 2:
                OpenInventoryUI();
                break;
            case 3:
                OpenMapUI();
                break;
            case 4:
                OpenPauseMenuUI();
                break;
        }
    }
}
