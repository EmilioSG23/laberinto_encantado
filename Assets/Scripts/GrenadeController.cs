using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class GrenadeController : MonoBehaviour
{
    public GameObject explosionEffect;

    public float Velocidad = 175;
    public float radioExplosion = 20;
    private Rigidbody2D Rigidbody2D;
    private GameObject jugador;
    private NetworkManager.PlayerDTO playerDTO;
    private Vector2 Direccion;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void hit(string playerDTO);
#endif

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        Rigidbody2D.velocity = Direccion * Velocidad;
    }

    public void setJugador(GameObject jugador)
    {
        this.jugador = jugador;
        this.playerDTO = jugador.GetComponent<PlayerController>().playerDTO;
    }
    public void setDireccion(Vector2 direccion)
    {
        Direccion = direccion;
    }
    public void ExplodeGrenade()
    {
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Collider2D[] affecteds = Physics2D.OverlapCircleAll(transform.position, radioExplosion);
        foreach (Collider2D affected in affecteds)
        {
            if (affected.gameObject != gameObject)
            {
                PlayerController rival = affected.GetComponent<PlayerController>();
                if (rival != null && jugador.GetComponent<PlayerController>().isLocalPlayer)
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                    hit(JsonUtility.ToJson(rival.playerDTO));
#else
                    NetworkManager.socket.Emit("hit", JsonUtility.ToJson(rival.playerDTO));
                    //rival.Golpe();
#endif
                }
            }
        }
        Destroy(gameObject);
    }
}
