using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
public class ButtonElement : MonoBehaviour, IPointerClickHandler
{
    public Image imgSelected;
    bool bSelected;
    public UnityEvent clickAction;

    public void SetSelectedElement(bool _bSelected)
    {
        bSelected = _bSelected;
        imgSelected.gameObject.SetActive(bSelected);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        clickAction.Invoke();
        if (PopupUIManager.Instance)
            PopupUIManager.Instance.subMenuPopup.close();
    }
}

