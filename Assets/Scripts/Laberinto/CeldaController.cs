using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeldaController : MonoBehaviour
{
    public GameObject ParedDerecha;
    public GameObject ParedIzquierda;
    public GameObject ParedArriba;
    public GameObject ParedAbajo;

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
}
