using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public TMP_Text textTeamID;
    public GameObject BalaPrefab;
    public GameObject GreanadePrefab;
    public GameObject joystick;
    [HideInInspector]
    public NetworkManager.PlayerDTO playerDTO;

    public float Velocidad = 5;

    public float cooldownTiro = 0.25f;
    public float cooldownGranade = 1.00f;
    private float cooldown;
    private float Horizontal;
    private float Vertical;

    private Vector2 position;
    private int weapon = 0;    //0 for Gun, 1 for Grenade

    private Vector2 direccion;
    [HideInInspector]
    public Vector2 ultimaDireccion;
    private float ultimoDisparo;
    private float lastTimeChangeWeapon;

    private int kills = 0;
    public bool parado = false;
    public bool isLocalPlayer = false;

    void Start()
    {
        //ultimaDireccion = Vector2.right;
        position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        cooldown = cooldownTiro;
    }

    void Update()
    {
        if(parado || !isLocalPlayer)
            return;
       MovimientoPC();
       //MovimientoMovil();

        if (Mathf.Abs (Horizontal) > Mathf.Abs (Vertical))
            direccion = new Vector2 (Horizontal, 0).normalized;
        else if (Mathf.Abs (Horizontal) < Mathf.Abs (Vertical))
            direccion = new Vector2 (0,Vertical).normalized;
        else
            direccion = Vector2.zero;

        if (direccion != Vector2.zero && ultimaDireccion != direccion){
            actualizarDireccion(direccion);
            NetworkManager.socket.Emit("rotates", JsonUtility.ToJson(playerDTO));
        }

        if ((Input.GetKey(KeyCode.Space)||(Input.GetMouseButton(0)&&
        (joystick.GetComponent<Joystick>().Horizontal == 0 && joystick.GetComponent<Joystick>().Vertical == 0)))
        && Time.time > ultimoDisparo + cooldown){
            if(weapon == 0){
                NetworkManager.socket.Emit("shoot", JsonUtility.ToJson(playerDTO));
                Disparar();
            }
            else if (weapon == 1){
                if (playerDTO != null){
                    if(playerDTO.granade > 0){
                        NetworkManager.socket.Emit("throwGrenade", JsonUtility.ToJson(playerDTO));
                        ThrowGrenade();
                        playerDTO.substractGranade();
                    }
                }else
                    ThrowGrenade();
            }
            ultimoDisparo = Time.time;
        } 

        if (Input.GetMouseButton(1) && Time.time > lastTimeChangeWeapon + 0.5f){
            ChangeWeapon();
            lastTimeChangeWeapon = Time.time;
        }
    }

    public void initPlayerGameObject(NetworkManager.PlayerDTO playerInstance){
        gameObject.transform.localPosition = new Vector2(playerInstance.coordinateX, playerInstance.coordinateY);
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.name = playerInstance.id;
        initTeamIndicator(playerInstance.numberInTeam, playerInstance.colorTeam);
        parado = true;
        playerDTO = playerInstance;
        ultimaDireccion = lookingAtVector(playerInstance.lookingAt);
    }

    private void initTeamIndicator(int numberInTeam, int colorTeam){
        textTeamID.text = numberInTeam.ToString();
        //RED
        if (colorTeam == 0){
            textTeamID.color = Color.white;
            transform.GetComponent<SpriteRenderer>().color = Color.red;
        }
        //BLUE
        if (colorTeam == 1){
            textTeamID.color = Color.white;
            transform.GetComponent<SpriteRenderer>().color = Color.blue;
        }
        //GREEN
        if (colorTeam == 2){
            textTeamID.color = Color.black;
            transform.GetComponent<SpriteRenderer>().color = Color.green;
        }
        //YELLOW
        if (colorTeam == 3){
            textTeamID.color = Color.black;
            transform.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }

    private Vector2 lookingAtVector (int direction){
        if (direction == 0)
            return Vector2.up;
        else if (direction == 1)
            return Vector2.down;
        else if (direction == 2)
            return Vector2.right;
        else
            return Vector2.left;
    }

    private void MovimientoPC(){
        joystick.SetActive(false);
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical = Input.GetAxisRaw("Vertical");
    }

    private void MovimientoMovil(){
        joystick.SetActive(true);
        Horizontal = joystick.GetComponent<Joystick>().Horizontal;
        Vertical = joystick.GetComponent<Joystick>().Vertical;

        //if (Horizontal > 0.0f) Horizontal = 0.5f;
        //if (Vertical > 0.0f) Vertical = 0.5f;
    }

    private void ChangeWeapon(){
        if (weapon == 0){
            weapon = 1;
            cooldown = cooldownGranade;
        }
        else if (weapon == 1){
            weapon = 0;
            cooldown = cooldownTiro;
        }
    }

    public void Disparar(){
        GameObject bala = Instantiate (BalaPrefab, transform.position + (Vector3) ultimaDireccion * 0.15f, Quaternion.identity);
        bala.GetComponent<BalaController>().setDireccion(ultimaDireccion);
        bala.GetComponent<BalaController>().setJugador(gameObject);
    }

    public void ThrowGrenade(){
        GameObject grenade = Instantiate (GreanadePrefab, transform.position + (Vector3) ultimaDireccion * 0.15f, Quaternion.identity);
        grenade.GetComponent<GrenadeController>().setDireccion(ultimaDireccion);
        grenade.GetComponent<GrenadeController>().setJugador(gameObject);
    }

    public void Golpe(){
        playerDTO.substractHealth();
        if(playerDTO.health <= 0)
            Death();
    }

    public void Death(){
        //TODO Emit
        Destroy (gameObject);
    }

    public void Kill(){
        kills++;
        //Debug.Log($"{gameObject} tiene {kills} kills");
    }

    public void actualizarDireccion(Vector2 direccion){
        ultimaDireccion = direccion;
         if (direccion == Vector2.up){
            transform.rotation = Quaternion.Euler(0, 0, 90);
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            textTeamID.transform.localScale = new Vector3 (Mathf.Abs(textTeamID.transform.localScale.x), Mathf.Abs(textTeamID.transform.localScale.y), Mathf.Abs(textTeamID.transform.localScale.z));
            playerDTO.updateLookingAt(0);
        }
        else if (direccion == Vector2.down){
            transform.rotation = Quaternion.Euler(0, 0, 90);
            transform.localScale = new Vector3 (-Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            textTeamID.transform.localScale = new Vector3 (-Mathf.Abs(textTeamID.transform.localScale.x), Mathf.Abs(textTeamID.transform.localScale.y), Mathf.Abs(textTeamID.transform.localScale.z));
            playerDTO.updateLookingAt(1);
        }
        else if (direccion == Vector2.right){
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            textTeamID.transform.localScale = new Vector3 (Mathf.Abs(textTeamID.transform.localScale.x), Mathf.Abs(textTeamID.transform.localScale.y), Mathf.Abs(textTeamID.transform.localScale.z));
            playerDTO.updateLookingAt(2);
        }
        else if (direccion == Vector2.left){
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3 (-Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            textTeamID.transform.localScale = new Vector3 (-Mathf.Abs(textTeamID.transform.localScale.x), Mathf.Abs(textTeamID.transform.localScale.y), Mathf.Abs(textTeamID.transform.localScale.z));
            playerDTO.updateLookingAt(3);
        }
        textTeamID.transform.rotation = Quaternion.Euler (0, 0, 0);
        //Debug.Log($"Player {playerDTO.id} is looking at: {playerDTO.lookingAt}");
    }
    
    private void FixedUpdate(){
        if(parado || !isLocalPlayer)
            return;
        GetComponent<Rigidbody2D>().velocity = new Vector2(Horizontal*Velocidad, Vertical*Velocidad);
        Vector2 newPosition = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        if (newPosition.x != position.x || newPosition.y != position.y){
            position = newPosition;
            playerDTO.updateCoords(position);
            //NetworkManager.EmitMovePlayer(playerDTO);
            //Debug.Log($"X: {playerDTO.coordinateX} Y: {playerDTO.coordinateY}");
            NetworkManager.socket.Emit("moves", JsonUtility.ToJson(playerDTO));
        }
    }

    private void OnCollisionEnter2D(Collision2D colision){
        if(parado || !isLocalPlayer)
            return;
        //Debug.Log(colision);
    }
}