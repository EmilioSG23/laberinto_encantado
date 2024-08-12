using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotonDeslizante : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector2 startPoint;
    private Vector2 endPoint;
    private bool isDragging = false;

    public Image botonImage;  // Componente de la imagen del bot�n  

    public Sprite BotonGranada;     // Imagen para disparar
    public Sprite BotonArma;      // Imagen para lanzar granada

   // public Sprite BotonItem1;         // Imagen para lanzar item


    void Start()
    {
        if (botonImage == null)
        {
            botonImage = GetComponent<Image>();
        }
        botonImage.sprite = BotonArma;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPoint = eventData.position;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            endPoint = eventData.position;
            CambiarImagen(endPoint - startPoint);  // Cambiar la imagen seg�n la direcci�n
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        Vector2 direction = endPoint - startPoint;

        if (direction.magnitude < 50)
        {
            Disparar();
        }
        else
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                if (direction.x > 0)
                {
                    LanzarGranada();
                }
                else
                {
                    CambiarArma();


                }
            }

   //         else

  //          {
  //              if (direction.y > 0)
  //              {
  //                  Recargar();
 //               }
  //              else
  //              {
  //                  CambiarArma();
  //              }
  //          }
        }
    }

    public void CambiarImagen(Vector2 direction)
    {
        // Cambiar la imagen dependiendo de la direcci�n del deslizamiento
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                botonImage.sprite = BotonGranada;
            }
            else
            {
                botonImage.sprite = BotonArma;
            }
            
         
        }
      //  else
     //   {
     //       if (direction.y > 0)
     //       {
     //           botonImage.sprite = imagenRecargar;
     //       }
     //       else
     //       {
     //           botonImage.sprite = imagenCambiarArma;
     //       }
      //  }
    }

    private void Disparar()
    {
        Debug.Log("Disparar");
        botonImage.sprite = BotonArma;
    }

    private void LanzarGranada()
    {
        Debug.Log("Lanzar Granada");
    }

    private void LanzarItem()
    {
        Debug.Log("Lanzar Item");
    }

    private void Recargar()
    {
        Debug.Log("Recargar");
    }

    private void CambiarArma()
    {
        Debug.Log("Cambiar Arma");
    }
}
