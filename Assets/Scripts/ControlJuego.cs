using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class ControlJuego : MonoBehaviour
{
    public static ControlJuego instance;
    [HideInInspector]
    public bool isStarted = false;
    public Transform jugadores;
    public GameObject mapCamera;
    public GameObject playerCamera;
    public Button cameraSwitchButton;
    private int usingCamera = 0; //0 map, 1 player
    public Slider cameraSlider;

    public TMP_Text timer;
    public Image timerImage;

    public GameObject panels;
    public GameObject fondo;
    public TMP_Text texto;
    public GameObject panelNoConnection;
    public GameObject panelInfo;
    public GameObject panelInfoInfo;
    public GameObject panelPCControls;
    public GameObject panelMovilControls;
    public GameObject countdownGO;

    public GameObject waitRoomGO;
    public TMP_Text waitMessage;
    public TMP_Text initialMessage;
    public GameObject button;
    public TMP_Text buttonMessage;

    public GameObject panelEndgame;
    public TMP_Text winnerText;
    public GameObject[] team;

    public GameObject panelDeath;
    public GameObject playerDeathSprite;
    public GameObject resetButton;
    private GameObject[] goalIndicators = new GameObject[4];
    private int teamGoalIndicator = -1;

    private int countdown = 3;
    private bool isAdmin = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void init();

    [DllImport("__Internal")]
    private static extern void resetGameIO();
#endif

    public bool getIsAdmin()
    {
        return this.isAdmin;
    }
    
    public void setGoalIndicator(int colorTeam, GameObject goalIndicator){
        this.goalIndicators[colorTeam] = goalIndicator;
        this.goalIndicators[colorTeam].SetActive(false);
    }
    public void setTeamGoalIndicator (int colorTeam){
        if (colorTeam > 3 || colorTeam < 0)
            this.teamGoalIndicator = -1;
        this.teamGoalIndicator = colorTeam;
    }
    public bool existsGoalIndicator(int colorTeam){
        return this.goalIndicators[colorTeam] != null;
    }
    public void initGoalIndicator (){
        this.goalIndicators[this.teamGoalIndicator].SetActive(true);
    }
    public void disableGoalIndicator (){
        foreach (GameObject g in this.goalIndicators)
            g.SetActive(false);
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
    }
    void Start(){
        playerCamera.gameObject.SetActive(false);
        mapCamera.gameObject.SetActive(true);
    }
    public void disablePanelNoConnection(){
        panelNoConnection.SetActive(false);
    }

    public void initGamePanel(){
        resetCameras();
        updateTimeTimer(60);
        disablePanelNoConnection();
        panels.SetActive(true);
        closePanelInfo();
        panelEndgame.SetActive(false);
        panelDeath.SetActive(false);
        waitRoomGO.SetActive(true);
        fondo.SetActive(true);
        initialMessage.gameObject.SetActive(true);
    }

    public void initAdminPanel()
    {
        this.isAdmin = true;
        waitMessage.gameObject.SetActive(false);
        button.SetActive(true);
        buttonMessage.text = "Iniciar Juego";
        buttonMessage.color = Color.black;
    }

    public void receiveNumberParticipants(int participants)
    {
        initialMessage.text = $"Conectados: {participants}";
    }

    public void initGame(bool isAdmin)
    {
        if (isAdmin)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            init();
            return;
#else
            NetworkManager.socket.Emit("init");
            return;
#endif
        }
        waitRoomGO.SetActive(false);
        closePanelInfo();
        StartCoroutine(iniciarCountdown());
    }
    
    public void updateTimeTimer(int timeLeft){
        timer.text = timeLeft <= 60 ? timeLeft.ToString() : "60";
            if (timeLeft <= 15){
                timer.color = Color.red;
                timerImage.color = Color.red;
            }else if (timeLeft <= 30){
                timer.color = new Color(1.0f, 0.65f, 0.0f);
                timerImage.color = new Color(1.0f, 0.65f, 0.0f);
            }else if (timeLeft <= 45){
                timer.color = Color.yellow;
                timerImage.color = Color.yellow;
            }else{
                timer.color = Color.white;
                timerImage.color = Color.white;
            }
    }

    public void switchCamera(){
        if (usingCamera == 0)
            usingCamera = 1;
        else
            usingCamera = 0;

        playerCamera.gameObject.SetActive(!playerCamera.gameObject.activeSelf);
        mapCamera.gameObject.SetActive(!mapCamera.gameObject.activeSelf);

        cameraSlider.GetComponent<CameraZoomController>().setCamera(usingCamera);
    }
    public void resetCameras(){
        usingCamera = 0;
        cameraSlider.GetComponent<CameraZoomController>().setCamera(usingCamera);
        cameraSlider.GetComponent<CameraZoomController>().CanUse(false);
        playerCamera.gameObject.SetActive(false);
        mapCamera.gameObject.SetActive(true);
        cameraSwitchButton.interactable = false;
    }

    public void endGame(int colorWinner)
    {
        disableGoalIndicator();
        panels.SetActive(true);
        closePanelInfo();
        panelDeath.SetActive(false);
        panelEndgame.SetActive(true);
        foreach (Transform jugador in jugadores)
        {
            jugador.gameObject.GetComponent<PlayerController>().Stop();
        }
        //RED
        if (colorWinner == 0)
        {
            winnerText.text = "¡Equipo ROJO!";
            winnerText.color = Color.red;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.red;
        }
        //BLUE
        else if (colorWinner == 1)
        {
            winnerText.text = "¡Equipo AZUL!";
            winnerText.color = Color.blue;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.blue;
        }
        //GREEN
        else if (colorWinner == 2)
        {
            winnerText.text = "¡Equipo VERDE!";
            winnerText.color = Color.green;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.green;
        }
        //YELLOW
        else if (colorWinner == 3)
        {
            winnerText.text = "¡Equipo AMARILLO!";
            winnerText.color = Color.yellow;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            winnerText.text = "Empate";
            winnerText.color = Color.white;
            foreach (GameObject playerSprite in team)
                playerSprite.GetComponent<Image>().color = Color.white;
        }
        if (isAdmin)
            resetButton.SetActive(true);
    }

    public void resetGamePanels(){
        panels.SetActive(true);
        panelEndgame.SetActive(false);
        panelDeath.SetActive(false);
        waitRoomGO.SetActive(true);

        initGamePanel();
        if (isAdmin)
            initAdminPanel();
    }

    public void resetGame()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        resetGameIO();
#else
        NetworkManager.socket.Emit("resetGame");
#endif
    }

    public void showDeathPanel(int color)
    {
        panels.SetActive(true);
        panelDeath.SetActive(true);
        if (color == 0)
            playerDeathSprite.GetComponent<Image>().color = Color.red;
        if (color == 1)
            playerDeathSprite.GetComponent<Image>().color = Color.blue;
        if (color == 2)
            playerDeathSprite.GetComponent<Image>().color = Color.green;
        if (color == 3)
            playerDeathSprite.GetComponent<Image>().color = Color.yellow;
    }

    public void closeDeathPanel(){
        resetCameras();
        panelDeath.SetActive(false);
        panels.SetActive(false);
    }

    private void closePanelInfo(){
        panelInfo.SetActive(false);
        panelInfoInfo.SetActive(false);
        panelPCControls.SetActive(false);
        panelMovilControls.SetActive(false);
    }

    private IEnumerator iniciarCountdown()
    {
        countdown = 3;
        countdownGO.SetActive(true);
        while (countdown > 0)
        {
            texto.text = countdown.ToString();
            yield return new WaitForSeconds(1);
            countdown--;
        }
        texto.text = "¡YA!";
        yield return new WaitForSeconds(1);
        countdownGO.SetActive(false);
        panels.SetActive(false);

        foreach (Transform jugador in jugadores.transform)
            jugador.transform.GetComponent<PlayerController>().parado = false;
        
        initGoalIndicator();
        cameraSwitchButton.interactable = true;
        cameraSlider.GetComponent<CameraZoomController>().CanUse(true);
    }
}
