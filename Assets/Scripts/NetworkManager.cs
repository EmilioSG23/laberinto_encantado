using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIOClient;
using SocketIOClient.Transport;
using TMPro;
using System.Runtime.InteropServices;
using System;


public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public static SocketIOUnity socket;
    public Transform jugadores;
    public Transform laberinto;
    public GameObject playerUI;
    public TMP_Text teamPlayerID;
    public TMP_Text username;
    public Image lifebar;
    public Sprite[] lifebarSprites = new Sprite[6];
    public FixedJoystick joystick;
    public WeaponButtonController weaponButton;
    public CeldaController celdaPrefab;
    public GameObject jugadorPrefab;
    public GameObject BalaPrefab;
    public TMP_Text timer;

    private PlayerDTO localPlayer;
    //private string uri = "http://localhost:8000/game";
    private string uri = "https://laberinto-encantado-backend-k2qp.onrender.com/game";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void joinGame(string playerDTO);
    [DllImport("__Internal")]
    private static extern void disconnectPlayer(string playerDTO);
    [DllImport("__Internal")]
    private static extern void moves(string playerDTO);
    [DllImport("__Internal")]
    private static extern void configSocket(string uri);
#endif
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {


#if UNITY_WEBGL && !UNITY_EDITOR
        configSocket(uri);

#else

        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            //Query = new Dictionary<string, string>{{"token", "UNITY" }},
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log($"Conectado a {uri}");
            socket.Emit("joinGame", JsonUtility.ToJson(new PlayerDTO("", 0)));
            //socket.Emit("createMap", JsonUtility.ToJson(new MapDTO (15, 15, 1, 1)));
        };

        socket.OnDisconnected += (sender, e) =>
        {
            if (localPlayer != null)
                #if UNITY_WEBGL && !UNITY_EDITOR
                disconnectPlayer(JsonUtility.ToJson(localPlayer));
                #else
                socket.Emit("disconnectPlayer", JsonUtility.ToJson(localPlayer));
                #endif
        };

        socket.Connect();
        socket.On("init", (response) => { OnInit(); });
        socket.On("resetGame", (response) => { OnResetGame(response); });
        socket.On("message", (response) => { OnMessage(response); });
        socket.On("numberParticipants", (response) => { OnNumberParticipants(response); });
        socket.On("isStarted", (response) => { OnIsStarted(response); });
        socket.On("admin", (response) => { OnAdmin(); });
        socket.On("leftTime", (response) => { OnTimeLeft(response); });
        socket.On("endGame", (response) => { OnEndGame(response); });
        socket.On("getAllPlayers", (response) => { OnGetAllPlayers(response); });
        socket.On("createMap", (response) => { OnCreateMap(response); });
        socket.On("joinGame", (response) => { OnJoinGame(response); });
        socket.On("disconnectPlayer", (response) => { OnDisconnectPlayer(response); });

        socket.On("addPlayer", (response) => { OnAddPlayer(response); });
        socket.On("moves", (response) => { OnMovePlayer(response); });
        socket.On("rotates", (response) => { OnRotatePlayer(response); });
        socket.On("shoot", (response) => { OnShoot(response); });
        socket.On("hit", (response) => { OnHit(response); });
        socket.On("throwGrenade", (response) => { OnThrowGrenade(response); });
#endif
    }



    public void OnApplicationQuit()
    {
        if (localPlayer != null)
        #if UNITY_WEBGL && !UNITY_EDITOR
            disconnectPlayer(JsonUtility.ToJson(localPlayer));
        #else
            socket.Emit("disconnectPlayer", JsonUtility.ToJson(localPlayer));
        #endif
    }

    void OnDestroy ()
    {
        if (localPlayer != null)
        #if UNITY_WEBGL && !UNITY_EDITOR
            disconnectPlayer(JsonUtility.ToJson(localPlayer));
        #else
            socket.Emit("disconnectPlayer", JsonUtility.ToJson(localPlayer));
        #endif
    }

    //ON Events
    #if UNITY_WEBGL && !UNITY_EDITOR
    public void OnConnected(string data)
    {
        string json = JsonUtility.ToJson(new PlayerDTO("", "", 0));
        joinGame(json);
    }

    public void OnDisconnected(string data)
    {
        if (localPlayer != null)
        {
            string json = JsonUtility.ToJson(localPlayer);
            disconnectPlayer(json);
        }
    }
    #endif

    void OnMessage(object response)
    {
        Debug.Log(response);
    }
    void OnInit()
    {
        runAction(() =>
        {
            ControlJuego.instance.initGame(false);
        });
    }
    void OnResetGame(object response)
    {
        GameDTO gameInstance = CreateFromJSON<GameDTO>(response);
        runAction(() =>
        {
            //Reset players
            foreach (TeamDTO team in gameInstance.teams)
            {
                foreach (PlayerDTO player in team.players)
                {
                    Transform o = jugadores.Find(player.id) as Transform;
                    GameObject playerGO;
                    //resetear
                    if (o != null)
                    {
                        playerGO = o.gameObject;
                        playerGO.GetComponent<PlayerController>().resetPlayerGameObject();
                    }
                    //Nuevo objeto
                    else
                    {
                        playerGO = Instantiate(jugadorPrefab, jugadores);
                        playerGO.GetComponent<PlayerController>().isLocalPlayer = (localPlayer.id == player.id);
                        playerGO.GetComponent<PlayerController>().initPlayerGameObject(player, this.joystick, this.weaponButton);
                    }
                    Transform c = laberinto.Find(player.spawnpoint) as Transform;
                    if (c != null)
                    {
                        playerGO.transform.localPosition = new Vector2(c.position.x, c.position.y);
                        player.updateCoords(c.position.x, c.position.y);
                    #if UNITY_WEBGL && !UNITY_EDITOR
                        moves(JsonUtility.ToJson(player));
                    #else
                        socket.Emit("moves", JsonUtility.ToJson(player));
                    #endif

                    }
                }
            }
            ControlJuego.instance.resetGamePanels();
            lifebar.sprite = lifebarSprites[0];
        });
    }
    void OnIsStarted(object response)
    {
        bool isStarted = false;
        if (response is string jsonString)
        {
            isStarted = bool.Parse(jsonString);
        }
        else if (response is SocketIOResponse socketIOResponse)
        {
            isStarted = socketIOResponse.GetValue<bool>();
        }

        ControlJuego.instance.isStarted = isStarted;
    }
    void OnNumberParticipants(object response)
    {
        int numberParticipants = 0;
        if (response is string jsonString)
        {
            numberParticipants = int.Parse(jsonString);
        }
        else if (response is SocketIOResponse socketIOResponse)
        {
            numberParticipants = socketIOResponse.GetValue<int>();
        }
        runAction(() =>
        {
            ControlJuego.instance.receiveNumberParticipants(numberParticipants);
        });
    }
    void OnAdmin()
    {
        runAction(() => { ControlJuego.instance.initAdminPanel(); });
    }
    void OnGetAllPlayers(object response)
    {
        GameDTO gameInstance = CreateFromJSON<GameDTO>(response);
        foreach (TeamDTO team in gameInstance.teams)
        {
            foreach (PlayerDTO player in team.players)
            {
                CreatePlayerGameObject(player, false, false);
            }
        }
    }
    void OnCreateMap(object response)
    {
        MapDTO mapInstance = null;
        if (response is string jsonString)
        {
            mapInstance = MapDTO.CreateFromJSON(jsonString);
        }
        else if (response is SocketIOResponse socketIOResponse)
        {
            mapInstance = MapDTO.CreateFromJSON(socketIOResponse);
        }

        runAction(() =>
        {
            laberinto.gameObject.GetComponent<GeneradorLaberinto>().generateMaze(mapInstance);
        });
    }
    void OnTimeLeft(object response)
    {
        int timeLeft = 0;
        if (response is string jsonString)
        {
            timeLeft = int.Parse(jsonString);
        }
        else if (response is SocketIOResponse socketIOResponse)
        {
            timeLeft = socketIOResponse.GetValue<int>();
        }

        runAction(() =>
        {
            timer.text = (timeLeft / 1000).ToString();
        });
    }
    void OnEndGame(object response)
    {
        runAction(() =>
        {
            int winnerTeam = 0;
            if (response is string jsonString)
            {
                winnerTeam = int.Parse(jsonString);
            }
            else if (response is SocketIOResponse socketIOResponse)
            {
                winnerTeam = socketIOResponse.GetValue<int>();
            }

            ControlJuego.instance.endGame(winnerTeam);
            foreach (Transform jugador in jugadores)
            {
                jugador.GetComponent<PlayerController>().Stop();
            }
        });
    }
    void OnJoinGame(object response)
    {
        PlayerDTO playerInstance = CreateFromJSON<PlayerDTO>(response);
        CreatePlayerGameObject(playerInstance, true, true);
        localPlayer = playerInstance;
        initPlayerIndicator(playerInstance.name, playerInstance.numberInTeam, playerInstance.colorTeam);
    }

    void OnAddPlayer(object response)
    {
        PlayerDTO playerInstance = CreateFromJSON<PlayerDTO>(response);
        CreatePlayerGameObject(playerInstance, false, true);
    }

    void OnDisconnectPlayer(object response)
    {
        PlayerDTO playerInstance = CreateFromJSON<PlayerDTO>(response);
        GameObject playerGO = findPlayer(playerInstance);
        Destroy(playerGO);
    }

    void OnMovePlayer(object response)
    {
        PlayerDTO playerInstance = CreateFromJSON<PlayerDTO>(response);
        runAction(() =>
        {
            GameObject playerGO = findPlayer(playerInstance);
            if (playerGO == null || playerGO.GetComponent<PlayerController>().isLocalPlayer) return;
            playerGO.transform.localPosition = new Vector2(playerInstance.coordinateX, playerInstance.coordinateY);
            playerGO.GetComponent<PlayerController>().playerDTO.updateCoords(playerInstance.coordinateX, playerInstance.coordinateY);
        });
    }
    void OnRotatePlayer(object response)
    {
        PlayerDTO playerInstance = CreateFromJSON<PlayerDTO>(response);
        runAction(() =>
        {
            GameObject playerGO = findPlayer(playerInstance);
            if (playerGO.GetComponent<PlayerController>().isLocalPlayer) return;
            playerGO.GetComponent<PlayerController>().playerDTO.updateLookingAt(playerInstance.lookingAt);
            if (playerInstance.lookingAt == 0)
                playerGO.GetComponent<PlayerController>().actualizarDireccion(Vector2.up);
            if (playerInstance.lookingAt == 1)
                playerGO.GetComponent<PlayerController>().actualizarDireccion(Vector2.down);
            if (playerInstance.lookingAt == 2)
                playerGO.GetComponent<PlayerController>().actualizarDireccion(Vector2.right);
            if (playerInstance.lookingAt == 3)
                playerGO.GetComponent<PlayerController>().actualizarDireccion(Vector2.left);
        });
    }

    void OnShoot(object response)
    {
        PlayerDTO playerInstance = CreateFromJSON<PlayerDTO>(response);
        runAction(() =>
        {
            GameObject playerGO = findPlayer(playerInstance);
            if (playerGO.GetComponent<PlayerController>().isLocalPlayer) return;
            playerGO.GetComponent<PlayerController>().Disparar();
        });
    }
    void OnHit(object response)
    {
        PlayerDTO playerInstance = CreateFromJSON<PlayerDTO>(response);
        runAction(() =>
        {
            GameObject playerGO = findPlayer(playerInstance);
            //if (playerGO.GetComponent<PlayerController>().isLocalPlayer)    return;
            playerGO.GetComponent<PlayerController>().playerDTO.substractHealth();
            if (playerInstance.id == localPlayer.id)
                lifebar.sprite = lifebarSprites[5 - playerInstance.health];
            if (playerGO.GetComponent<PlayerController>().playerDTO.health <= 0)
            {
                playerGO.GetComponent<PlayerController>().Death();
                if (playerInstance.id == localPlayer.id)
                    ControlJuego.instance.showDeathPanel(playerInstance.colorTeam);
            }
        });
    }
    void OnThrowGrenade(object response)
    {
        PlayerDTO playerInstance = CreateFromJSON<PlayerDTO>(response);
        runAction(() =>
        {
            GameObject playerGO = findPlayer(playerInstance);
            if (playerGO.GetComponent<PlayerController>().isLocalPlayer) return;
            playerGO.GetComponent<PlayerController>().ThrowGrenade();
            playerGO.GetComponent<PlayerController>().playerDTO.substractGranade();
        });
    }

    private void CreatePlayerGameObject(PlayerDTO playerInstance, bool localPlayer, bool inSpawnpoint)
    {
        if (playerInstance.health <= 0)
            return;
        runAction(() =>
        {
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o != null) return;
            GameObject playerGO = Instantiate(jugadorPrefab, jugadores);
            playerGO.GetComponent<PlayerController>().isLocalPlayer = localPlayer;
            playerGO.GetComponent<PlayerController>().initPlayerGameObject(playerInstance, this.joystick, this.weaponButton);

            Transform c = laberinto.Find(playerInstance.spawnpoint) as Transform;
            if (c != null && inSpawnpoint)
            {
                playerGO.transform.localPosition = new Vector2(c.position.x, c.position.y);
                playerInstance.updateCoords(c.position.x, c.position.y);
                #if UNITY_WEBGL && !UNITY_EDITOR
                moves(JsonUtility.ToJson(playerInstance));
                #else
                socket.Emit("moves", JsonUtility.ToJson(playerInstance));
                #endif
            }
        });
    }

    private void initPlayerIndicator(string name, int numberInTeam, int colorTeam)
    {
        runAction(() =>
        {
            teamPlayerID.text = numberInTeam.ToString();
            username.text = name;
            //RED
            if (colorTeam == 0)
            {
                teamPlayerID.color = Color.white;
                playerUI.GetComponent<Image>().color = Color.red;
            }
            //BLUE
            if (colorTeam == 1)
            {
                teamPlayerID.color = Color.white;
                playerUI.GetComponent<Image>().color = Color.blue;
            }
            //GREEN
            if (colorTeam == 2)
            {
                teamPlayerID.color = Color.white;
                playerUI.GetComponent<Image>().color = Color.green;
            }
            //YELLOW
            if (colorTeam == 3)
            {
                teamPlayerID.color = Color.black;
                playerUI.GetComponent<Image>().color = Color.yellow;
            }
            if (!ControlJuego.instance.isStarted)
                ControlJuego.instance.initGamePanel();
        });
    }


    private GameObject findPlayer(PlayerDTO playerInstance)
    {
        Transform o = jugadores.Find(playerInstance.id) as Transform;
        if (o == null) return null;
        return o.gameObject;
    }

    private void runAction(Action action)
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        action();
    #else
        UnityThread.executeInUpdate(() =>
        {
            action();
        });
    #endif
    }

    #region JSON_DTO
    public static T CreateFromJSON<T>(object data)
    {
        T instance = default(T);

        if (data is string jsonData)
        {
            instance = JsonUtility.FromJson<T>(jsonData);
        }
        else if (data is SocketIOResponse socketIOResponse)
        {
            instance = JsonUtility.FromJson<T>(socketIOResponse.ToString().Substring(1, socketIOResponse.ToString().Length - 2));
        }
        else
        {
            throw new System.ArgumentException("El tipo de dato no es compatible");
        }

        return instance;
    }

    [System.Serializable]
    public class GameDTO
    {
        public List<TeamDTO> teams;
        public bool isEnded;

        public GameDTO(List<TeamDTO> _teams, bool _isEnded)
        {
            teams = _teams;
            isEnded = _isEnded;
        }

        public override string ToString()
        {
            string message = $"Laberinto Encantado with teams: \n";
            foreach (TeamDTO team in teams)
                message += (team + "\n");
            return message;
        }
    }
    [System.Serializable]
    public class TeamDTO
    {
        public int color;
        public List<PlayerDTO> players;

        public TeamDTO(int _color, List<PlayerDTO> _players)
        {
            color = _color;
            players = _players;
        }

        public override string ToString()
        {
            string message = $"Team {color} with members: ";
            foreach (PlayerDTO player in players)
                message += $"{player.name}, ";
            return message;
        }
    }
    [System.Serializable]
    public class PlayerDTO
    {
        public string id;
        public string name;
        public int numberInTeam;
        public float coordinateX;
        public float coordinateY;
        public int lookingAt;
        public int colorTeam;
        public int granade;
        public int health;
        public string spawnpoint;

        public PlayerDTO(string _id, string _name, int _numberInTeam, float _coordinateX, float _coordinateY, int _lookingAt, int _colorTeam, int _granade, int _health, string _spawnpoint)
        {
            id = _id;
            name = _name;
            numberInTeam = _numberInTeam;
            coordinateX = _coordinateX;
            coordinateY = _coordinateY;
            lookingAt = _lookingAt;
            colorTeam = _colorTeam;
            spawnpoint = _spawnpoint;
        }
        public PlayerDTO(string _id, string _name, Vector2 _position, int _lookingAt, int _colorTeam) : this(_id, _name, 0, _position.x, _position.y, _lookingAt, _colorTeam, 3, 5, "") { }
        public PlayerDTO(string _name, float _coordinateX, float _coordinateY, int _lookingAt, int _colorTeam) : this("", _name, 0, _coordinateX, _coordinateY, _lookingAt, _colorTeam, 3, 5, "") { }
        public PlayerDTO(string _id, string _name, int _colorTeam) : this(_id, _name, 0, 0.0f, 0.0f, 2, _colorTeam, 3, 5, "") { }
        public PlayerDTO(string _name, int _colorTeam) : this("", _name, _colorTeam) { }

        public void updateCoords(Vector2 position)
        {
            coordinateX = position.x;
            coordinateY = position.y;
        }
        public void updateCoords(float x, float y)
        {
            coordinateX = x;
            coordinateY = y;
        }
        public void updateLookingAt(int direction)
        {
            lookingAt = direction;
        }
        public void substractGranade()
        {
            granade--;
        }
        public void substractHealth()
        {
            health--;
        }
        public void resetPlayerDTO()
        {
            health = 5;
            granade = 3;
            updateLookingAt(2);
        }
        public override string ToString()
        {
            return $"Player {name} with ID: {id} that belongs to team {colorTeam} is in {coordinateX};{coordinateY}";
        }
    }
    [System.Serializable]
    public class MapDTO
    {
        public int sizeX { get; set; }
        public int sizeY { get; set; }
        public CellDTO[][] cells { get; set; }

        public MapDTO(int _sizeX, int _sizeY, CellDTO[][] _cells)
        {
            sizeX = _sizeX;
            sizeY = _sizeY;
            cells = _cells;
        }

        public static MapDTO CreateFromJSON(string data)
        {//data contains [] at the begin and end
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MapDTO>(data);
        }
        public static MapDTO CreateFromJSON(SocketIOResponse data)
        {//data contains [] at the begin and end
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MapDTO>(data.ToString().Substring(1, data.ToString().Length - 2));
        }
    }
    [System.Serializable]
    public class CellDTO
    {
        public bool right;
        public bool left;
        public bool up;
        public bool down;

        public CellDTO(bool _right, bool _left, bool _up, bool _down)
        {
            right = _right;
            left = _left;
            up = _up;
            down = _down;
        }
    }
    #endregion
}
