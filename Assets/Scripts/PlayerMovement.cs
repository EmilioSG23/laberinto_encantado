using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5;
    private Rigidbody2D Rigidbody2D;
    private float Horizontal;
    private float Vertical;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical = Input.GetAxisRaw("Vertical");
    }
    
    private void FixedUpdate(){
        Rigidbody2D.velocity = new Vector2(Horizontal*speed, Vertical*speed);
    }
}
