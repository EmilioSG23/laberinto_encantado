using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject BalaPrefab;
    public GameObject joystick;
    public int vida = 3;
    public float Velocidad = 5;
    public float cooldownTiro = 0.25f;
    private Rigidbody2D Rigidbody2D;
    private float Horizontal;
    private float Vertical;
    private Vector3 direccion;
    private Vector3 ultimaDireccion;
    private Vector3 direccionColision;
    private float ultimoDisparo;
    private int kills = 0;
    public bool parado = false;

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        ultimaDireccion = Vector2.right;
    }

    void Update()
    {
        if(parado)
            return;
       MovimientoPC();
       //MovimientoMovil();

        if (Mathf.Abs (Horizontal) > Mathf.Abs (Vertical))
            direccion = new Vector2 (Horizontal, 0).normalized;
        else if (Mathf.Abs (Horizontal) < Mathf.Abs (Vertical))
            direccion = new Vector2 (0,Vertical).normalized;
        else
            direccion = Vector2.zero;

        if (direccion != Vector3.zero){
            ultimaDireccion = direccion;
            actualizarDireccion(ultimaDireccion);
        }
        if ((Input.GetKey(KeyCode.Space)||(Input.GetMouseButton(0)&&
        (joystick.GetComponent<Joystick>().Horizontal == 0 && joystick.GetComponent<Joystick>().Vertical == 0)))
        && Time.time > ultimoDisparo + cooldownTiro){
            Disparar();
            ultimoDisparo = Time.time;
        } 
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

    public void Disparar(){
        GameObject bala = Instantiate (BalaPrefab, transform.position + ultimaDireccion * 0.15f, Quaternion.identity);
        bala.GetComponent<BalaController>().setDireccion(ultimaDireccion);
        bala.GetComponent<BalaController>().setJugador(gameObject);
    }

    public void Golpe(){
        vida--;
        if(vida<=0)
            Destroy(gameObject);
    }
    public void Kill(){
        kills++;
        Debug.Log($"{gameObject} tiene {kills} kills");
    }

    private void actualizarDireccion(Vector2 direccion){
         if (direccion == Vector2.up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
        }
        else if (direccion == Vector2.down)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            transform.localScale = new Vector3 (-Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
        }
        else if (direccion == Vector2.left)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3 (-Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
        }
        else if (direccion == Vector2.right)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
        }
    }
    
    private void FixedUpdate(){
        Rigidbody2D.velocity = new Vector2(Horizontal*Velocidad, Vertical*Velocidad);
    }

    private void OnTriggerEnter2D(Collider2D colision){
        Debug.Log(colision);
    }
}