using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlJuego : MonoBehaviour
{
    public Transform jugadores;
    public GameObject jugadorPrefab;

    public TMP_Text texto;
    public GameObject fondo;
    private int countdown = 3;
    private void Start()
    {
        StartCoroutine (iniciarCountdown());
    }

    private IEnumerator iniciarCountdown(){
        while (countdown > 0){
            texto.text = countdown.ToString();
            yield return new WaitForSeconds (1);
            countdown--;
        }
        texto.text = "¡YA!";
        yield return new WaitForSeconds (1);
        texto.gameObject.SetActive(false);
        fondo.SetActive(false);

        foreach (Transform jugador in jugadores.transform)
            jugador.transform.GetComponent<PlayerController>().parado = false;
    }
}
