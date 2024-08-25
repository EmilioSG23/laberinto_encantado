using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [HideInInspector]
    public GameObject player;

    void Update(){
        if (player != null)
            transform.position = new Vector2 (player.transform.position.x, player.transform.position.y);
    }

    public void setPlayer(GameObject player){
        this.player = player;
    }
}
