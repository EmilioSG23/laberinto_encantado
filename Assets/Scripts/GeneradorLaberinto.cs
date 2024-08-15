using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorLaberinto : MonoBehaviour
{
    public CeldaController celdaPrefab;
    public Vector2Int tamano;

    public void generateMaze (NetworkManager.MapDTO mapInstance){
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
        for (int i = 0; i < 4; i++){
            redCells[i].destroySpawnPointWall(i);
            greenCells[i].destroySpawnPointWall(i);
            blueCells[i].destroySpawnPointWall(i);
            yellowCells[i].destroySpawnPointWall(i);
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
            }
            //Exit Door GREEN
            if (x == 1 && (y == tamano.y || y == tamano.y - 1)){
                cell.setExitDoor(2, false);
            }
            //Exit Door YELLOW
            if (x == tamano.x && (y == tamano.y || y == tamano.y - 1)){
                cell.setExitDoor(3, true);
            }
            //Exit Door RED
            if (x == tamano.x && (y == 1 || y == 2)){
                cell.setExitDoor(0, true);
            }
        }
    }
}
