using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalaController : MonoBehaviour
{
    public float Velocidad = 20;
    private Rigidbody2D Rigidbody2D;
    private GameObject jugador;
    private Vector2 Direccion;

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

    }
    
    private void FixedUpdate(){
        Rigidbody2D.velocity = Direccion * Velocidad; 
        if (Direccion == Vector2.up || Direccion == Vector2.down)
            transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (Direccion == Vector2.right || Direccion == Vector2.left)
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void setJugador (GameObject jugador){
        this.jugador = jugador;
    }
    public void setDireccion(Vector2 direccion){
        Direccion = direccion;
    }
    public void DestruirBala(){
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D colision){
        //Debug.Log(colision.gameObject);
        //Debug.Log(jugador);
        if(colision.gameObject != jugador){
            PlayerController rival = colision.GetComponent<PlayerController>();
            if (rival != null){
                //Debug.Log ($"Bala de {jugador} impactó a {rival}");
                rival.Golpe();
                if(rival.vida <=0)
                    jugador.GetComponent<PlayerController>().Kill();
            }
            DestruirBala();
        }
    }
}
