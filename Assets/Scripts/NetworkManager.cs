using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIOClient.Transport;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public static SocketIOUnity socket;
    public Transform jugadores;
    public GameObject jugadorPrefab;
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
            socket.Emit("joinGame", JsonUtility.ToJson(new PlayerDTO("EmilioSG23", -37.5f, -37.5f, 1)));
        };
        socket.Connect();
        socket.On("init", (response => {OnInit(response);}));
        socket.On("message", (response) => {OnMessage(response);});
        socket.On("gameState", (response) => {OnGameState(response);});
        socket.On("getAllPlayers", (response) => {OnGetAllPlayers(response);});

        socket.On("joinGame", (response) => {OnJoinGame(response);});
        socket.On("addPlayer", (response) => {OnAddPlayer(response);});
        socket.On("moves", (response) => {OnMovePlayer(response);});
        socket.On("shot", (response) => {OnShoot(response);});
        socket.On("hit", (response) => {OnHit(response);});
        socket.On("throwGreanade", (response) => {OnThrowGreanade(response);});
    }

    //ON Events
    void OnMessage (SocketIOResponse response){
        Debug.Log(response.GetValue<string>());
    }
    void OnInit (SocketIOResponse response){

    }
    void OnJoinGame (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        CreatePlayerGameObject(playerInstance, true);
    }
    void OnAddPlayer (SocketIOResponse response){
        //Debug.Log(response.GetValue<string>());
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        CreatePlayerGameObject(playerInstance, false);
    }
    private void CreatePlayerGameObject (PlayerDTO playerInstance, bool localPlayer){
        UnityThread.executeInUpdate(() => {
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o != null)  return;
            GameObject playerGO = Instantiate(jugadorPrefab, jugadores);
            playerGO.transform.localPosition = new Vector2(playerInstance.coordinateX, playerInstance.coordinateY);
            playerGO.transform.rotation = Quaternion.identity;
            playerGO.name = playerInstance.id;
            playerGO.GetComponent<PlayerController>().parado = true;
            playerGO.GetComponent<PlayerController>().playerDTO = playerInstance;
            playerGO.GetComponent<PlayerController>().isLocalPlayer = localPlayer;
        });
    }
    void OnGameState (SocketIOResponse response){
        //Debug.Log(response.ToString());
        GameDTO gameInstance = GameDTO.CreateFromJSON(response);
        //Debug.Log(gameInstance);
    }
    void OnMovePlayer (SocketIOResponse response){
        PlayerDTO playerInstance = PlayerDTO.CreateFromJSON(response);
        UnityThread.executeInUpdate(() => {
            Transform o = jugadores.Find(playerInstance.id) as Transform;
            if (o == null)  return;
            GameObject playerGO = o.gameObject;
            if (playerGO.GetComponent<PlayerController>().isLocalPlayer)    return;
            playerGO.transform.localPosition = new Vector2(playerInstance.coordinateX, playerInstance.coordinateY);
            playerGO.transform.rotation = Quaternion.identity;
            playerGO.name = playerInstance.id;
            playerGO.GetComponent<PlayerController>().parado = true;
            playerGO.GetComponent<PlayerController>().playerDTO = playerInstance;
        });
    }
    void OnGetAllPlayers (SocketIOResponse response){
        GameDTO gameInstance = GameDTO.CreateFromJSON(response);
        foreach (TeamDTO team in gameInstance.teams){
            foreach (PlayerDTO player in team.players){
                CreatePlayerGameObject(player, false);
            }
        }
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

    }
    void OnThrowGreanade (SocketIOResponse response){

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
        public float coordinateX;
        public float coordinateY;
        public int colorTeam;

        public PlayerDTO (string _id, string _name, float _coordinateX, float _coordinateY, int _colorTeam){
            id = _id;
            name = _name;
            coordinateX = _coordinateX;
            coordinateY = _coordinateY;
            colorTeam = _colorTeam;
        }
        public PlayerDTO (string _id, string _name, Vector2 _position, int _colorTeam):this(_id, _name, _position.x, _position.y,_colorTeam){}
        public PlayerDTO (string _name, float _coordinateX, float _coordinateY, int _colorTeam):this("", _name, _coordinateX, _coordinateY, _colorTeam){}
        public PlayerDTO (string _id, string _name, int _colorTeam):this(_id,_name,0.0f,0.0f,_colorTeam){}
        public PlayerDTO (string _name, int _colorTeam):this("", _name, _colorTeam){}

        public void updateCoords(Vector2 position){
            coordinateX = position.x;
            coordinateY = position.y;
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
    public class BulletDTO {

        
    }
    [System.Serializable]
    public class GranadeDTO{

    }
    [System.Serializable]
    public class ParticipantsDTO{
        public List<PlayerDTO> players;

        public ParticipantsDTO(List<PlayerDTO> _players){
            players = _players;
        }
        public static ParticipantsDTO CreateFromJSON (string data){//data contains [] at the begin and end
            return JsonUtility.FromJson<ParticipantsDTO>(data.Substring(1 , data.Length - 2));
        }
        public static ParticipantsDTO CreateFromJSON (SocketIOResponse data){//data contains [] at the begin and end
            return JsonUtility.FromJson<ParticipantsDTO>(data.ToString().Substring(1 , data.ToString().Length - 2));
        }
    }
    #endregion
}
