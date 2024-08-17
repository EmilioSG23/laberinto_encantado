using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    private bool isExitDoor = false;
    private int team; //0 red, 1 blue, 2 green, 3 yellow
    private bool reached = false;

    public void initExitDoor (int _team){
        isExitDoor = true;
        team = _team;
    }

    private void OnCollisionEnter2D (Collision2D o){
        if(isExitDoor && !reached){
            PlayerController player = o.gameObject.GetComponent<PlayerController>();
            if (player.playerDTO.colorTeam == team && player.isLocalPlayer && !player.parado){
                reached = true;
                NetworkManager.socket.Emit ("endGame", JsonUtility.ToJson(player.playerDTO));
            }
        }
    }
}
