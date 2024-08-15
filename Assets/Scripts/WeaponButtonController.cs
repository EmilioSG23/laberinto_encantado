using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class WeaponButtonController: MonoBehaviour
{
    public Button mainWeaponButton;
    public Button secondaryWeaponButton;
    public Sprite gunWeaponButton;
    public Sprite grenadeWeaponButton;
    public TMP_Text amountText;
    private PlayerController player;
    private bool isMainWeaponUsing;

    void Start()
    {
        // Agregar los eventos al botón usando EventTrigger
        EventTrigger trigger = mainWeaponButton.gameObject.AddComponent<EventTrigger>();

        // Configurar el evento para OnPointerDown
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { isMainWeaponUsing = true; });
        trigger.triggers.Add(pointerDownEntry);

        // Configurar el evento para OnPointerUp
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { isMainWeaponUsing = false; });
        trigger.triggers.Add(pointerUpEntry);
    }

    void Update(){
        if (isMainWeaponUsing)
            UseWeapon();
    }

    public bool IsPointerOverUI(){
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public void SetPlayer(GameObject g){
        this.player = g.GetComponent<PlayerController>();
    }
    public void UseWeapon(){
        player.UseWeapon();
    }
    public void ChangeWeapon(){
        if (Time.time > player.getLastTimeChangeWeapon() + 0.5f){
            player.ChangeWeapon();
            ChangeWeaponSprite();
        }
    }
    public void ChangeWeaponSprite(){
        if (player.getWeapon() == 0){
            mainWeaponButton.GetComponent<Image>().sprite = gunWeaponButton;
            secondaryWeaponButton.GetComponent<Image>().sprite = grenadeWeaponButton;
            amountText.text = "∞";
        }
        else if (player.getWeapon() == 1){
            mainWeaponButton.GetComponent<Image>().sprite = grenadeWeaponButton;
            secondaryWeaponButton.GetComponent<Image>().sprite = gunWeaponButton;
            amountText.text = player.playerDTO.granade.ToString();
        }
    }
}
