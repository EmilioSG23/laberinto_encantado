using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    private bool isExitDoor = false;
    private int team; //0 red, 1 blue, 2 green, 3 yellow

    public void initExitDoor (int _team){
        isExitDoor = true;
        team = _team;
    }

    private void OnCollisionEnter2D (Collision2D o){
        try{
        Debug.Log("colision!");
        if(isExitDoor){
            PlayerController player = o.gameObject.GetComponent<PlayerController>();
            Debug.Log($"{player.playerDTO.colorTeam}, {team}");
            if (player.playerDTO.colorTeam == team && player.isLocalPlayer){
                //Emit WIN
                Debug.Log($"Team {team} won the game!");
            }
        }
        }catch(System.Exception ex){
            Debug.LogError(ex);
        }
    }
}
