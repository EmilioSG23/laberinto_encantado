using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public TMP_Text textTeamID;
    public GameObject BalaPrefab;
    public GameObject GreanadePrefab;
    public Joystick joystick;
    public WeaponButtonController weaponButton;

    [HideInInspector]
    public NetworkManager.PlayerDTO playerDTO;

    public float Velocidad = 75;

    public float cooldownTiro = 0.25f;
    public float cooldownGranade = 1.00f;
    [HideInInspector]
    public float cooldown;
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
    public GameObject localIndicator;
    public Image localIndicatorImage;
    public SpriteRenderer gunSprite;
    public SpriteRenderer grenadeSprite;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void rotates(string playerDTO);

    [DllImport("__Internal")]
    private static extern void shoot(string playerDTO);

    [DllImport("__Internal")]
    private static extern void throwGrenade(string playerDTO);

    [DllImport("__Internal")]
    private static extern void moves(string playerDTO);
#endif


    public int getWeapon()
    {
        return this.weapon;
    }
    public float getLastShoot()
    {
        return this.ultimoDisparo;
    }
    public float getLastTimeChangeWeapon()
    {
        return this.lastTimeChangeWeapon;
    }

    void Start()
    {
        position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        cooldown = cooldownTiro;
    }

    void Update()
    {
        if (parado || !isLocalPlayer || joystick == null)
            return;

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > Mathf.Abs(joystick.GetComponent<Joystick>().Horizontal))
            Horizontal = Input.GetAxisRaw("Horizontal");
        else
            Horizontal = joystick.GetComponent<Joystick>().Horizontal;

        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > Mathf.Abs(joystick.GetComponent<Joystick>().Vertical))
            Vertical = Input.GetAxisRaw("Vertical");
        else
            Vertical = joystick.GetComponent<Joystick>().Vertical;

        if (Mathf.Abs(Horizontal) > 0.1f || Mathf.Abs(Vertical) > 0.1f)
        {
            float angle = Mathf.Atan2(Vertical, Horizontal) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;
            float snappedAngleRad = snappedAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(snappedAngleRad);
            float y = Mathf.Sin(snappedAngleRad);
            Horizontal = x; Vertical = y;
        }
        else
        {
            Horizontal = 0f; Vertical = 0f;
        }

        if (Mathf.Abs(Horizontal) > Mathf.Abs(Vertical))
            direccion = new Vector2(Horizontal, 0).normalized;
        else if (Mathf.Abs(Horizontal) < Mathf.Abs(Vertical))
            direccion = new Vector2(0, Vertical).normalized;
        else
            direccion = Vector2.zero;

        if (direccion != Vector2.zero && ultimaDireccion != direccion)
        {
            actualizarDireccion(direccion);
#if UNITY_WEBGL && !UNITY_EDITOR
            rotates(JsonUtility.ToJson(playerDTO));
#else
            NetworkManager.socket.Emit("rotates", JsonUtility.ToJson(playerDTO));
#endif
        }

        if ((Input.GetKey(KeyCode.Space) || (Input.GetMouseButton(0) &&
        (joystick.GetComponent<Joystick>().Vertical == 0)))
         && !weaponButton.IsPointerOverUI()){
            UseWeapon();
        }

        if ((Input.touchCount < 1) && (Input.GetMouseButton(1) || Input.GetKey(KeyCode.C))){
            ChangeWeapon();
        }
    }

    public void UseWeapon()
    {
        if (Time.time > ultimoDisparo + cooldown)
        {
            if (weapon == 0)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                shoot(JsonUtility.ToJson(playerDTO));
#else
                NetworkManager.socket.Emit("shoot", JsonUtility.ToJson(playerDTO));
#endif
                Disparar();
            }
            else if (weapon == 1 && playerDTO.granade > 0)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                throwGrenade(JsonUtility.ToJson(playerDTO));
#else
                NetworkManager.socket.Emit("throwGrenade", JsonUtility.ToJson(playerDTO));
#endif
                ThrowGrenade();
                playerDTO.substractGranade();
                weaponButton.amountText.text = playerDTO.granade.ToString();
            }
            ultimoDisparo = Time.time;
        }
    }

    public void ChangeWeapon()
    {
        if (Time.time > lastTimeChangeWeapon + 0.5f)
        {
            if (weapon == 0)
            {
                weapon = 1;
                cooldown = cooldownGranade;
                //gunSprite.gameObject.SetActive(false);
                //grenadeSprite.gameObject.SetActive(true);
            }
            else if (weapon == 1)
            {
                weapon = 0;
                cooldown = cooldownTiro;
                //gunSprite.gameObject.SetActive(true);
                //grenadeSprite.gameObject.SetActive(false);
            }
            weaponButton.ChangeWeaponSprite();
            lastTimeChangeWeapon = Time.time;
        }
    }

    public void initPlayerGameObject(NetworkManager.PlayerDTO playerInstance, Joystick joystick, WeaponButtonController weaponButton)
    {
        gameObject.transform.localPosition = new Vector2(playerInstance.coordinateX, playerInstance.coordinateY);
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.name = playerInstance.id;
        initTeamIndicator(playerInstance.numberInTeam, playerInstance.colorTeam);
        parado = !ControlJuego.instance.isStarted;
        playerDTO = playerInstance;
        ultimaDireccion = lookingAtVector(playerInstance.lookingAt);
        if (isLocalPlayer)
        {
            this.joystick = joystick;
            this.weaponButton = weaponButton;
            this.localIndicator.SetActive(true);
            weaponButton.SetPlayer(this.gameObject);
        }
    }

    public void resetPlayerGameObject()
    {
        parado = true;
        playerDTO.resetPlayerDTO();
        if (weaponButton != null)
            weaponButton.ChangeWeaponSprite();
    }

    private void initTeamIndicator(int numberInTeam, int colorTeam)
    {
        textTeamID.text = numberInTeam.ToString();
        //RED
        if (colorTeam == 0)
        {
            textTeamID.color = Color.white;
            transform.GetComponent<SpriteRenderer>().color = Color.red;
            localIndicatorImage.GetComponent<Image>().color = Color.red;
        }
        //BLUE
        if (colorTeam == 1)
        {
            textTeamID.color = Color.white;
            transform.GetComponent<SpriteRenderer>().color = Color.blue;
            localIndicatorImage.GetComponent<Image>().color = Color.blue;
        }
        //GREEN
        if (colorTeam == 2)
        {
            textTeamID.color = Color.white;
            transform.GetComponent<SpriteRenderer>().color = Color.green;
            localIndicatorImage.GetComponent<Image>().color = Color.green;
        }
        //YELLOW
        if (colorTeam == 3)
        {
            textTeamID.color = Color.black;
            transform.GetComponent<SpriteRenderer>().color = Color.yellow;
            localIndicatorImage.GetComponent<Image>().color = Color.yellow;
        }
    }

    private Vector2 lookingAtVector(int direction)
    {
        if (direction == 0)
            return Vector2.up;
        else if (direction == 1)
            return Vector2.down;
        else if (direction == 2)
            return Vector2.right;
        else
            return Vector2.left;
    }

    public void Disparar()
    {
        GameObject bala = Instantiate(BalaPrefab, transform.position + (Vector3)ultimaDireccion * 0.15f, Quaternion.identity);
        bala.GetComponent<BalaController>().setDireccion(ultimaDireccion);
        bala.GetComponent<BalaController>().setJugador(gameObject);
    }

    public void ThrowGrenade()
    {
        GameObject grenade = Instantiate(GreanadePrefab, transform.position + (Vector3)ultimaDireccion * 0.15f, Quaternion.identity);
        grenade.GetComponent<GrenadeController>().setDireccion(ultimaDireccion);
        grenade.GetComponent<GrenadeController>().setJugador(gameObject);
    }

    public void Golpe()
    {
        playerDTO.substractHealth();
        if (playerDTO.health <= 0)
            Death();
    }

    public void Death()
    {
        //TODO Emit
        Destroy(gameObject);
    }

    public void Kill()
    {
        kills++;
        //Debug.Log($"{gameObject} tiene {kills} kills");
    }

    public void Stop()
    {
        parado = true;
        Horizontal = 0f;
        Vertical = 0f;
    }

    public void actualizarDireccion(Vector2 direccion)
    {
        ultimaDireccion = direccion;
        if (direccion == Vector2.up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            textTeamID.transform.localScale = new Vector3(Mathf.Abs(textTeamID.transform.localScale.x), Mathf.Abs(textTeamID.transform.localScale.y), Mathf.Abs(textTeamID.transform.localScale.z));
            playerDTO.updateLookingAt(0);
        }
        else if (direccion == Vector2.down)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            textTeamID.transform.localScale = new Vector3(-Mathf.Abs(textTeamID.transform.localScale.x), Mathf.Abs(textTeamID.transform.localScale.y), Mathf.Abs(textTeamID.transform.localScale.z));
            playerDTO.updateLookingAt(1);
        }
        else if (direccion == Vector2.right)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            textTeamID.transform.localScale = new Vector3(Mathf.Abs(textTeamID.transform.localScale.x), Mathf.Abs(textTeamID.transform.localScale.y), Mathf.Abs(textTeamID.transform.localScale.z));
            playerDTO.updateLookingAt(2);
        }
        else if (direccion == Vector2.left)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            textTeamID.transform.localScale = new Vector3(-Mathf.Abs(textTeamID.transform.localScale.x), Mathf.Abs(textTeamID.transform.localScale.y), Mathf.Abs(textTeamID.transform.localScale.z));
            playerDTO.updateLookingAt(3);
        }
        textTeamID.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void FixedUpdate()
    {
        if (parado || !isLocalPlayer)
            return;
        GetComponent<Rigidbody2D>().velocity = new Vector2(Horizontal * Velocidad, Vertical * Velocidad);
        Vector2 newPosition = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        if (newPosition.x != position.x || newPosition.y != position.y)
        {
            position = newPosition;
            playerDTO.updateCoords(position);
#if UNITY_WEBGL && !UNITY_EDITOR
            moves(JsonUtility.ToJson(playerDTO));
#else
            NetworkManager.socket.Emit("moves", JsonUtility.ToJson(playerDTO));
#endif
        }
    }
}