using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIOClient.Transport;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public static SocketIOUnity socket;
    public Transform jugadores;
    public GameObject jugadorPrefab;
    public GameObject BalaPrefab;
    public TMP_Text timer;
    private string uri = "http://localhost:8000/game";

    void Awake(){
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    void Start(){
        socket = new SocketIOUnity(uri, new SocketIOOptions{
            Query = new Dictionary<string, string>{{"token", "UNITY" }},
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.OnConnected += (sender, e) => {
            Debug.Log ($"Conectado a {uri}");
            socket.Emit("joinGame", JsonUtility.ToJson(new PlayerDTO("EmilioSG23", 125.0f, 120.0f, 2, 1)));
            socket.Emit("init");
            //socket.Emit("createMap", JsonUtility.ToJson(new MapDTO (15, 15, 1, 1)));
        };
        socket.Connect();
        socket.On("init", (response => {OnInit(response);}));
        socket.On("message", (response) => {OnMessage(response);});
        socket.On("gameState", (response) => {OnGameState(response);});
        socket.On("leftTime", (response) => {OnTimeLeft(response);});
        socket.On("endGame", (response) => {OnEndGame(response);});
        socket.On("getAllPlayers", (response) => {OnGetAllPlayers(response);});
        socket.On("map", (response) => {OnCreateMap(response);});

        socket.On("joinGame", (response) => {OnJoinGame(response);});
        socket.On("addPlayer", (response) => {OnAddPlayer(response);});
        socket.On("moves", (response) => {OnMovePlayer(response);});
        socket.On("rotates", (response) => {OnRotatePlayer(response);});
        socket.On("shoot", (response) => {OnShoot(response);});
        socket.On("hit", (response) => {OnHit(response);});
        socket.On("throwGrenade", (response) => {OnThrowGrenade(response);});
    }

    //ON Events
    void OnMessage (SocketIOResponse response){
        Debug.Log(response.GetValue<string>());
    }
    void OnInit (SocketIOResponse response){
        //TODO se inicia la partida desde aquí
    }
    void OnGameState (SocketIOResponse response){
        //Debug.Log(response.ToString());
        GameDTO gameInstance = GameDTO.CreateFromJSON(response);
        //Debug.Log(gameInstance);
    }
    void OnGetAllPlayers (SocketIOResponse response){
        GameDTO gameInstance = GameDTO.CreateFromJSON(response);
        foreach (TeamDTO team in gameInstance.teams){
            foreach (PlayerDTO player in team.players){
                CreatePlayerGameObject(player, false);
            }
        }
    }

    void OnCreateMap (SocketIOResponse response){
        Debug.Log(response.ToString());
    }
    void OnTimeLeft(SocketIOResponse response){
        int timeLeft = response.GetValue<int>();
        timer.text = timeLeft.ToString();
    }
    void OnEndGame(SocketIOResponse response){
        
    }
    void OnJoinGame (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        CreatePlayerGameObject(playerInstance, true);
    }
    void OnAddPlayer (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        CreatePlayerGameObject(playerInstance, false);
    }
    private void CreatePlayerGameObject (PlayerDTO playerInstance, bool localPlayer){
        if (playerInstance.health <= 0)
            return;
        UnityThread.executeInUpdate(() => {
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o != null)  return;
            GameObject playerGO = Instantiate(jugadorPrefab, jugadores);
            playerGO.GetComponent<PlayerController>().initPlayerGameObject (playerInstance);
            playerGO.GetComponent<PlayerController>().isLocalPlayer = localPlayer;
        });
    }
    
    void OnMovePlayer (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        UnityThread.executeInUpdate(() => {
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o == null)  return;
            GameObject playerGO = o.gameObject;
            if (playerGO.GetComponent<PlayerController>().isLocalPlayer)    return;
            playerGO.transform.localPosition = new Vector2(playerInstance.coordinateX, playerInstance.coordinateY);
            playerGO.GetComponent<PlayerController>().playerDTO.updateCoords(playerInstance.coordinateX, playerInstance.coordinateY);
        });
    }
    void OnRotatePlayer (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        UnityThread.executeInUpdate(() => {
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o == null)  return;
            GameObject playerGO = o.gameObject;
            if (playerGO.GetComponent<PlayerController>().isLocalPlayer)    return;
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
    
    void OnShoot (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        UnityThread.executeInUpdate(()=>{
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o == null)  return;
            GameObject playerGO = o.gameObject;
            if (playerGO.GetComponent<PlayerController>().isLocalPlayer)    return;
            playerGO.GetComponent<PlayerController>().Disparar();
        });
    }
    void OnHit (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        UnityThread.executeInUpdate(()=>{
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o == null)  return;
            GameObject playerGO = o.gameObject;
            //if (playerGO.GetComponent<PlayerController>().isLocalPlayer)    return;
            playerGO.GetComponent<PlayerController>().playerDTO.substractHealth();
            if (playerGO.GetComponent<PlayerController>().playerDTO.health <= 0)
                playerGO.GetComponent<PlayerController>().Death();
        });
    }
    void OnThrowGrenade (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        UnityThread.executeInUpdate(()=>{
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o == null)  return;
            GameObject playerGO = o.gameObject;
            if (playerGO.GetComponent<PlayerController>().isLocalPlayer)    return;
            playerGO.GetComponent<PlayerController>().ThrowGrenade();
            playerGO.GetComponent<PlayerController>().playerDTO.substractGranade();
        });
    }

    #region JSON_DTO
    [System.Serializable]
    public class GameDTO{
        public List<TeamDTO> teams;
        public bool isEnded;

        public GameDTO (List<TeamDTO> _teams, bool _isEnded){
            teams = _teams;
            isEnded = _isEnded;
        }

        public static GameDTO CreateFromJSON (string data){//data contains [] at the begin and end
            return JsonUtility.FromJson<GameDTO>(data.Substring(1 , data.Length - 2));
        }
        public static GameDTO CreateFromJSON (SocketIOResponse data){//data contains [] at the begin and end
            return JsonUtility.FromJson<GameDTO>(data.ToString().Substring(1 , data.ToString().Length - 2));
        }
        public override string ToString (){
            string message = $"Laberinto Encantado with teams: \n";
            foreach (TeamDTO team in teams)
                message += (team + "\n");
            return message;
        }
    }
    [System.Serializable]
    public class TeamDTO{
        public int color;
        public List<PlayerDTO> players;

        public TeamDTO (int _color, List<PlayerDTO> _players){
            color = _color;
            players = _players;
        }

        public override string ToString(){
            string message = $"Team {color} with members: ";
            foreach (PlayerDTO player in players)
                message += $"{player.name}, ";
            return message;
        }
    }
    [System.Serializable]
    public class PlayerDTO{
        public string id;
        public string name;
        public int numberInTeam;
        public float coordinateX;
        public float coordinateY;
        public int lookingAt;
        public int colorTeam;
        public int granade;
        public int health;

        public PlayerDTO (string _id, string _name, int _numberInTeam, float _coordinateX, float _coordinateY, int _lookingAt, int _colorTeam, int _granade, int _health){
            id = _id;
            name = _name;
            numberInTeam = _numberInTeam;
            coordinateX = _coordinateX;
            coordinateY = _coordinateY;
            lookingAt = _lookingAt;
            colorTeam = _colorTeam;
        }
        public PlayerDTO (string _id, string _name, Vector2 _position, int _lookingAt, int _colorTeam):this(_id, _name, 0, _position.x, _position.y, _lookingAt, _colorTeam, 3, 5){}
        public PlayerDTO (string _name, float _coordinateX, float _coordinateY, int _lookingAt, int _colorTeam):this("", _name, 0, _coordinateX, _coordinateY, _lookingAt, _colorTeam, 3, 5){}
        public PlayerDTO (string _id, string _name, int _colorTeam):this(_id,_name, 0,0.0f,0.0f, 2,_colorTeam, 3, 5){}
        public PlayerDTO (string _name, int _colorTeam):this("", _name, _colorTeam){}

        public void updateCoords(Vector2 position){
            coordinateX = position.x;
            coordinateY = position.y;
        }
        public void updateCoords(float x, float y){
            coordinateX = x;
            coordinateY = y;
        }
        public void updateLookingAt(int direction){
            lookingAt = direction;
        }
        public void substractGranade(){
            granade--;
        }
        public void substractHealth(){
            health--;
        }

        public static PlayerDTO CreateFromJSON (string data){//data contains [] at the begin and end
            return JsonUtility.FromJson<PlayerDTO>(data.Substring(1 , data.Length - 2));
        }
        public static PlayerDTO CreateFromJSON (SocketIOResponse data){//data contains [] at the begin and end
            return JsonUtility.FromJson<PlayerDTO>(data.ToString().Substring(1 , data.ToString().Length - 2));
        }

        public override string ToString(){
            return $"Player {name} with ID: {id} that belongs to team {colorTeam} is in {coordinateX};{coordinateY}";
        }
    }
    [System.Serializable]
    public class MapDTO{
        List<int> size;
        List<int> scale;

        public MapDTO (int sizeX, int sizeY, int scaleX, int scaleY){
            size = new List<int> {sizeX, sizeY};
            scale = new List<int> {scaleX, scaleY};
        }
    }
    #endregion
}
