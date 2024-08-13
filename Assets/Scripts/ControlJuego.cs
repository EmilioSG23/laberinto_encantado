using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlJuego : MonoBehaviour
{
    public static ControlJuego instance;
    public Transform jugadores;
    public GameObject jugadorPrefab;

    public GameObject panels;
    public GameObject fondo;
    public TMP_Text texto;

    public GameObject countdownGO;

    public GameObject waitRoomGO;
    public TMP_Text wailMessage;
    public TMP_Text initialMessage;
    public GameObject button;
    public TMP_Text buttonMessage;

    public GameObject panelEndgame;
    public TMP_Text winnerText;
    public GameObject[] team;
    
    private int countdown = 3;

    void Awake(){
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
    }

    private void Start()
    {
        
    }

    public void initGamePanel(){
        panels.SetActive (true);
        waitRoomGO.SetActive (true);
        fondo.SetActive (true);
        initialMessage.gameObject.SetActive (true);
    }

    public void initAdminPanel(){
        wailMessage.gameObject.SetActive (false);
        button.SetActive (true);
        buttonMessage.text = "Iniciar Juego";
        buttonMessage.color = Color.black;
    }

    public void receiveNumberParticipants(int participants){
        initialMessage.text = $"Conectados: {participants}";
    }

    public void initGame (bool isAdmin){
        waitRoomGO.SetActive (false);
        StartCoroutine (iniciarCountdown(isAdmin));
    }

    public void endGame (int colorWinner){
        panels.SetActive (true);
        panelEndgame.SetActive (true);
        foreach (Transform jugador in jugadores)
            jugador.gameObject.GetComponent<PlayerController>().parado = true;
            
        //No winner
        if (colorWinner == -1){
            winnerText.text = "Empate";
        }

        //RED
        if (colorWinner == 0){
            winnerText.text = "¡Equipo ROJO!";
            winnerText.color = Color.red;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.red;
        }
        //BLUE
        if (colorWinner == 1){
            winnerText.text = "¡Equipo AZUL!";
            winnerText.color = Color.blue;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.blue;
        }
        //GREEN
        if (colorWinner == 2){
            winnerText.text = "¡Equipo VERDE!";
            winnerText.color = Color.green;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.green;
        }
        //YELLOW
        if (colorWinner == 3){
            winnerText.text = "¡Equipo AMARILLO!";
            winnerText.color = Color.yellow;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.yellow;
        }
    }

    private IEnumerator iniciarCountdown(bool isAdmin){
        if (isAdmin){
            NetworkManager.socket.Emit ("init");
        }
        countdownGO.SetActive (true);
        while (countdown > 0){
            texto.text = countdown.ToString();
            yield return new WaitForSeconds (1);
            countdown--;
        }
        texto.text = "¡YA!";
        yield return new WaitForSeconds (1);
        countdownGO.SetActive (false);
        panels.SetActive(false);

        foreach (Transform jugador in jugadores.transform)
            jugador.transform.GetComponent<PlayerController>().parado = false;
    }
}
