using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorLaberinto : MonoBehaviour
{
    public CeldaController celdaPrefab;
    public GameObject goalPrefab;
    public Vector2Int tamano;

    public void generateMaze (NetworkManager.MapDTO mapInstance){
        foreach (Transform celda in transform){
            Destroy (celda.gameObject);
        }
        tamano.x = mapInstance.sizeX;
        tamano.y = mapInstance.sizeY;
        for (int x = 0; x < tamano.x; x++){
            for (int y = 0; y < tamano.y; y++){
                Vector2 posicionCelda = new Vector2 ((x -(tamano.x / 2f)) * celdaPrefab.transform.localScale.x, (y - (tamano.y / 2f)) * celdaPrefab.transform.localScale.y);
                CeldaController celda = Instantiate(celdaPrefab, posicionCelda, Quaternion.identity, transform);
                celda.name = $"[{x+1};{y+1}]";
                if (!mapInstance.cells[x][y].right)
                    celda.GetComponent<CeldaController>().eliminarMuro(0);
                if (!mapInstance.cells[x][y].left)
                    celda.GetComponent<CeldaController>().eliminarMuro(1);
                if (!mapInstance.cells[x][y].up)
                    celda.GetComponent<CeldaController>().eliminarMuro(2);
                if (!mapInstance.cells[x][y].down)
                    celda.GetComponent<CeldaController>().eliminarMuro(3);
            }
        }
        prepareSpawnPoints();
        setExitDoors();
    }

    private void prepareSpawnPoints(){
        List<CeldaController> redCells = new List<CeldaController>();
        List<CeldaController> greenCells = new List<CeldaController>();
        List<CeldaController> blueCells = new List<CeldaController>();
        List<CeldaController> yellowCells = new List<CeldaController>();
        redCells.Clear();
        greenCells.Clear();
        blueCells.Clear();
        yellowCells.Clear();
        foreach (Transform o in gameObject.transform) {
            CeldaController cell = o.gameObject.GetComponent<CeldaController>();
            string[] parts = o.gameObject.name.Trim('[',']').Split(";");
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);

            //Spawnpoint YELLOW
            if ((x == 1 || x == 2) && (y == 1 || y == 2)){
                cell.prepareSpawnPoint(3);
                yellowCells.Add(cell);
            }
            //Spawnpoint RED
            if ((x == 1 || x == 2) && (y == tamano.y || y == tamano.y - 1)){
                cell.prepareSpawnPoint(0);
                redCells.Add(cell);
            }
            //Spawnpoint BLUE
            if ((x == tamano.x || x == tamano.x - 1) && (y == tamano.y || y == tamano.y - 1)){
                cell.prepareSpawnPoint(1);
                blueCells.Add(cell);
            }
            //Spawnpoint GREEN
            if ((x == tamano.x || x == tamano.x - 1) && (y == 1 || y == 2)){
                cell.prepareSpawnPoint(2);
                greenCells.Add(cell);
            }
        }
        for (int i = 0; i < redCells.Count; i++){
            redCells[i].destroySpawnPointWall(i%4);
            greenCells[i].destroySpawnPointWall(i%4);
            blueCells[i].destroySpawnPointWall(i%4);
            yellowCells[i].destroySpawnPointWall(i%4);
        }
    }

    private void setExitDoors(){
        foreach (Transform o in gameObject.transform) {
            CeldaController cell = o.gameObject.GetComponent<CeldaController>();
            string[] parts = o.gameObject.name.Trim('[',']').Split(";");
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);

            //Exit Door BLUE
            if (x == 1 && (y == 1 || y == 2)){
                cell.setExitDoor(1, false);
                if (y == 1 && !ControlJuego.instance.existsGoalIndicator(1))
                    ControlJuego.instance.setGoalIndicator(1, Instantiate (goalPrefab, 
                    new Vector2 (cell.transform.position.x + celdaPrefab.transform.localScale.x*-1.5f, cell.transform.position.y + (celdaPrefab.transform.localScale.y/2)), Quaternion.identity));
            }
            //Exit Door GREEN
            if (x == 1 && (y == tamano.y || y == tamano.y - 1)){
                cell.setExitDoor(2, false);
                if (y == tamano.y && !ControlJuego.instance.existsGoalIndicator(2))
                    ControlJuego.instance.setGoalIndicator(2, Instantiate (goalPrefab, 
                    new Vector2 (cell.transform.position.x + celdaPrefab.transform.localScale.x*-1.5f, cell.transform.position.y - (celdaPrefab.transform.localScale.y/2)), Quaternion.identity));
            }
            //Exit Door YELLOW
            if (x == tamano.x && (y == tamano.y || y == tamano.y - 1)){
                cell.setExitDoor(3, true);
                if (y == tamano.y && !ControlJuego.instance.existsGoalIndicator(3))
                    ControlJuego.instance.setGoalIndicator(3, Instantiate (goalPrefab, 
                    new Vector2 (cell.transform.position.x + celdaPrefab.transform.localScale.x*1.5f, cell.transform.position.y - (celdaPrefab.transform.localScale.y/2)), Quaternion.identity));
            }
            //Exit Door RED
            if (x == tamano.x && (y == 1 || y == 2)){
                cell.setExitDoor(0, true);
                if (y == 1 && !ControlJuego.instance.existsGoalIndicator(0))
                    ControlJuego.instance.setGoalIndicator(0, Instantiate (goalPrefab, 
                    new Vector2 (cell.transform.position.x + celdaPrefab.transform.localScale.x*1.5f, cell.transform.position.y + (celdaPrefab.transform.localScale.y/2)), Quaternion.identity));
            }
        }
    }
}
