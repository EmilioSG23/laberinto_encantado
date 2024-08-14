using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeldaController : MonoBehaviour
{
    public GameObject ParedDerecha;
    public GameObject ParedIzquierda;
    public GameObject ParedArriba;
    public GameObject ParedAbajo;
    public GameObject Fondo;

    public void eliminarMuro (int muroInt){
        /*if(muroInt == 0) ParedDerecha.SetActive(false);
        if(muroInt == 1) ParedIzquierda.SetActive(false);
        if(muroInt == 2) ParedArriba.SetActive(false);
        if(muroInt == 3) ParedAbajo.SetActive(false);*/
        if(muroInt == 0) Destroy(ParedDerecha);
        if(muroInt == 1) Destroy(ParedIzquierda);
        if(muroInt == 2) Destroy(ParedArriba);
        if(muroInt == 3) Destroy(ParedAbajo);
    }

    public void setExitDoor (int colorTeam, bool rightSide){
        Color color;
        if (colorTeam == 0) color = Color.red;
        else if (colorTeam == 1) color = Color.blue;
        else if (colorTeam == 2) color = Color.green;
        else color = Color.yellow;

        if (rightSide){
            ParedDerecha.GetComponent<SpriteRenderer>().color = color;
            ParedDerecha.GetComponent<WallController>().initExitDoor(colorTeam);
            Vector3 scale = ParedDerecha.transform.localScale;
            scale.x = 0.3f;
            ParedDerecha.transform.localScale = scale;
        }else{
            ParedIzquierda.GetComponent<SpriteRenderer>().color = color;
            ParedIzquierda.GetComponent<WallController>().initExitDoor(colorTeam);
            Vector3 scale = ParedIzquierda.transform.localScale;
            scale.x = 0.3f;
            ParedIzquierda.transform.localScale = scale;
        }
    }
    public void prepareSpawnPoint (int color){
        //RED
        if (color == 0){
            Fondo.GetComponent<SpriteRenderer>().color = new Color (255f/255f, 0f/255f, 0f/255f, 48f/255);
        }
        //BLUE
        if (color == 1){
            Fondo.GetComponent<SpriteRenderer>().color = new Color (0f/255f, 0f/255f, 255f/255f, 48f/255);
        }
        //GREEN
        if (color == 2){
            Fondo.GetComponent<SpriteRenderer>().color = new Color (0f/255f, 255f/255f, 0f/255f, 48f/255);
        }
        //YELLOW
        if (color == 3){
            Fondo.GetComponent<SpriteRenderer>().color = new Color (255f/255f, 255f/255f, 0f/255f, 48f/255);
        }
    }
    public void destroySpawnPointWall(int cellIndex){
        if (cellIndex == 0){//1;1
            eliminarMuro(0);
            eliminarMuro(2);
        }
        if (cellIndex == 1){//1;2
            eliminarMuro(0);
            eliminarMuro(3);
        }
        if (cellIndex == 2){//2;1
            eliminarMuro(1);
            eliminarMuro(2);
        }
        if (cellIndex == 3){//2;2
            eliminarMuro(1);
            eliminarMuro(3);
        }
    }
}
