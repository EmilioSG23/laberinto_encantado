using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorLaberinto : MonoBehaviour
{
    public CeldaController celdaPrefab;
    public Transform jugadores;
    public GameObject jugadorPrefab;
    public Vector2Int tamano;

    private void Start () {
        generarLaberinto(tamano);
        //StartCoroutine(generarLaberintoAnimado(tamano));
    }

    private void generarLaberinto(Vector2Int tamano){
        List<CeldaController> celdas = new List<CeldaController>();
        List<CeldaController> celdasSpawn = new List<CeldaController>();

        for (int x = 0; x < tamano.x; x++){
            for (int y = 0; y < tamano.y; y++){
                Vector2 posicionCelda = new Vector2 ((x -(tamano.x / 2f))*celdaPrefab.transform.localScale.x, (y - (tamano.y / 2f))*celdaPrefab.transform.localScale.y);
                CeldaController celda = Instantiate(celdaPrefab, posicionCelda, Quaternion.identity, transform);
                celdas.Add(celda);
            }
        }

        List<CeldaController> caminoActual = new List<CeldaController>();
        List<CeldaController> completados = new List<CeldaController>();

        caminoActual.Add(celdas[Random.Range(0, celdas.Count)]);
        
        while (completados.Count < celdas.Count){
            List<int> posiblesSiguientesCeldas = new List<int>();
            List<int> posiblesDirecciones = new List <int> ();

            int celdaActualIndex = celdas.IndexOf(caminoActual[caminoActual.Count-1]);
            int celdaActualX = celdaActualIndex / tamano.y;
            int celdaActualY = celdaActualIndex % tamano.y;

            //Salidas y celdas de aparición
            if (celdaActualX == 0 && celdaActualY == 0 || celdaActualX == 0 && celdaActualY == tamano.y-1){
                CeldaController celdaActual = celdas[celdaActualIndex];
                celdaActual.eliminarMuro(1);
                if(!celdasSpawn.Contains(celdaActual))
                    celdasSpawn.Add(celdaActual);
            }
            if (celdaActualX == tamano.x-1 && celdaActualY == 0 || celdaActualX == tamano.x-1 && celdaActualY == tamano.y-1){
                CeldaController celdaActual = celdas[celdaActualIndex];
                celdaActual.eliminarMuro(0);
                if(!celdasSpawn.Contains(celdaActual))
                    celdasSpawn.Add(celdaActual);
            }

            if (celdaActualX < tamano.x -1){
                if (!completados.Contains(celdas[celdaActualIndex + tamano.y]) && !caminoActual.Contains(celdas[celdaActualIndex + tamano.y])){
                    posiblesDirecciones.Add(1);
                    posiblesSiguientesCeldas.Add(celdaActualIndex + tamano.y);
                }
            }
            if (celdaActualX > 0){
                if (!completados.Contains(celdas[celdaActualIndex - tamano.y]) && !caminoActual.Contains(celdas[celdaActualIndex - tamano.y])){
                    posiblesDirecciones.Add(2);
                    posiblesSiguientesCeldas.Add(celdaActualIndex - tamano.y);
                }
            }
            if (celdaActualY < tamano.y -1){
                if (!completados.Contains(celdas[celdaActualIndex + 1]) && !caminoActual.Contains(celdas[celdaActualIndex + 1])){
                    posiblesDirecciones.Add(3);
                    posiblesSiguientesCeldas.Add(celdaActualIndex + 1);
                }
            }
            if (celdaActualY > 0){
                if (!completados.Contains(celdas[celdaActualIndex - 1]) && !caminoActual.Contains(celdas[celdaActualIndex - 1])){
                    posiblesDirecciones.Add(4);
                    posiblesSiguientesCeldas.Add(celdaActualIndex - 1);
                }
            }

            if (posiblesDirecciones.Count > 0){
                int direccionEscogida = Random.Range(0, posiblesDirecciones.Count);
                CeldaController celdaEscogida = celdas[posiblesSiguientesCeldas[direccionEscogida]];

                switch (posiblesDirecciones[direccionEscogida]){
                    case 1:
                        celdaEscogida.eliminarMuro(1);
                        caminoActual[caminoActual.Count - 1].eliminarMuro(0);
                        break;
                    case 2:
                        celdaEscogida.eliminarMuro(0);
                        caminoActual[caminoActual.Count - 1].eliminarMuro(1);
                        break;
                    case 3:
                        celdaEscogida.eliminarMuro(3);
                        caminoActual[caminoActual.Count - 1].eliminarMuro(2);
                        break;
                    case 4:
                        celdaEscogida.eliminarMuro(2);
                        caminoActual[caminoActual.Count - 1].eliminarMuro(3);
                        break;
                }

                caminoActual.Add(celdaEscogida);
                //celdaEscogida.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else{
                completados.Add(caminoActual[caminoActual.Count - 1]);
                //caminoActual[caminoActual.Count - 1].GetComponent<Renderer>().material.color = Color.green;
                caminoActual.RemoveAt(caminoActual.Count - 1);
            }
        }

        spawnearJugadores(celdasSpawn);
    }

    private void spawnearJugadores(List<CeldaController> celdasSpawn){
        for (int i = 0; i < celdasSpawn.Count; i++){
            //Debug.Log($"Celda #{i} X: {celdasSpawn[i].transform.position.x} Y:{celdasSpawn[i].transform.position.y}");
            /*GameObject jugador = Instantiate(jugadorPrefab, jugadores);
            jugador.transform.localPosition = new Vector2(celdasSpawn[i].transform.position.x, celdasSpawn[i].transform.position.y);
            jugador.transform.rotation = Quaternion.identity;
            jugador.GetComponent<PlayerController>().parado = true;*/
        }
    }

    private IEnumerator generarLaberintoAnimado(Vector2Int tamano){
        List<CeldaController> celdas = new List<CeldaController>();

        for (int x = 0; x < tamano.x; x++){
            for (int y = 0; y < tamano.y; y++){
                Vector2 posicionCelda = new Vector2 ((x -(tamano.x / 2f))*celdaPrefab.transform.localScale.x, (y - (tamano.y / 2f))*celdaPrefab.transform.localScale.y);
                CeldaController celda = Instantiate(celdaPrefab, posicionCelda, Quaternion.identity, transform);
                celdas.Add(celda);
                yield return null;
            }
        }

        List<CeldaController> caminoActual = new List<CeldaController>();
        List<CeldaController> completados = new List<CeldaController>();

        caminoActual.Add(celdas[Random.Range(0, celdas.Count)]);
        
        while (completados.Count < celdas.Count){
            List<int> posiblesSiguientesCeldas = new List<int>();
            List<int> posiblesDirecciones = new List <int> ();

            int celdaActualIndex = celdas.IndexOf(caminoActual[caminoActual.Count-1]);
            int celdaActualX = celdaActualIndex / tamano.y;
            int celdaActualY = celdaActualIndex % tamano.y;

            //Salidas
            if (celdaActualX == 0 && celdaActualY == 0 || celdaActualX == 0 && celdaActualY == tamano.y-1){
                CeldaController celdaActual = celdas[celdaActualIndex];
                celdaActual.eliminarMuro(1);
            }
            if (celdaActualX == tamano.x-1 && celdaActualY == 0 || celdaActualX == tamano.x-1 && celdaActualY == tamano.y-1){
                CeldaController celdaActual = celdas[celdaActualIndex];
                celdaActual.eliminarMuro(0);
            }

            if (celdaActualX < tamano.x -1){
                if (!completados.Contains(celdas[celdaActualIndex + tamano.y]) && !caminoActual.Contains(celdas[celdaActualIndex + tamano.y])){
                    posiblesDirecciones.Add(1);
                    posiblesSiguientesCeldas.Add(celdaActualIndex + tamano.y);
                }
            }
            if (celdaActualX > 0){
                if (!completados.Contains(celdas[celdaActualIndex - tamano.y]) && !caminoActual.Contains(celdas[celdaActualIndex - tamano.y])){
                    posiblesDirecciones.Add(2);
                    posiblesSiguientesCeldas.Add(celdaActualIndex - tamano.y);
                }
            }
            if (celdaActualY < tamano.y -1){
                if (!completados.Contains(celdas[celdaActualIndex + 1]) && !caminoActual.Contains(celdas[celdaActualIndex + 1])){
                    posiblesDirecciones.Add(3);
                    posiblesSiguientesCeldas.Add(celdaActualIndex + 1);
                }
            }
            if (celdaActualY > 0){
                if (!completados.Contains(celdas[celdaActualIndex - 1]) && !caminoActual.Contains(celdas[celdaActualIndex - 1])){
                    posiblesDirecciones.Add(4);
                    posiblesSiguientesCeldas.Add(celdaActualIndex - 1);
                }
            }

            if (posiblesDirecciones.Count > 0){
                int direccionEscogida = Random.Range(0, posiblesDirecciones.Count);
                CeldaController celdaEscogida = celdas[posiblesSiguientesCeldas[direccionEscogida]];

                switch (posiblesDirecciones[direccionEscogida]){
                    case 1:
                        celdaEscogida.eliminarMuro(1);
                        caminoActual[caminoActual.Count - 1].eliminarMuro(0);
                        break;
                    case 2:
                        celdaEscogida.eliminarMuro(0);
                        caminoActual[caminoActual.Count - 1].eliminarMuro(1);
                        break;
                    case 3:
                        celdaEscogida.eliminarMuro(3);
                        caminoActual[caminoActual.Count - 1].eliminarMuro(2);
                        break;
                    case 4:
                        celdaEscogida.eliminarMuro(2);
                        caminoActual[caminoActual.Count - 1].eliminarMuro(3);
                        break;
                }

                caminoActual.Add(celdaEscogida);
                //celdaEscogida.Fondo.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else{
                completados.Add(caminoActual[caminoActual.Count - 1]);
                //caminoActual[caminoActual.Count - 1].Fondo.GetComponent<Renderer>().material.color = new Color (84/255.0f, 118/255.0f, 154/255.0f);
                caminoActual.RemoveAt(caminoActual.Count - 1);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
}
