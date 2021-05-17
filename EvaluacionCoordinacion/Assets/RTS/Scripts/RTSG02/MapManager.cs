using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g02
{
    public struct casilla
    {
        public int prioridad;
        public GameObject celda;
    }


    public class MapManager : MonoBehaviour
    {
        public int filas;
        public int columnas;
        private Grid grid;
        private Terrain terrain;
        public GameObject ejemplo;
        private casilla[,] matriz;

        // Start is called before the first frame update
        void Start()
        {
            grid = GetComponentInParent<Grid>();
            terrain = GetComponentInParent<Terrain>();

            filas = (int)(terrain.terrainData.size.x / grid.cellSize.x);
            columnas = (int)(terrain.terrainData.size.z / grid.cellSize.z);

            matriz = new casilla[filas, columnas];
            float gap = 0.0f;
            Vector3 minPos;
            minPos.x = (grid.cellSize.x / 2);
            minPos.z = (grid.cellSize.z / 2);
            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    casilla currCasilla = new casilla();
                    currCasilla.celda = Instantiate(ejemplo, transform);
                    currCasilla.prioridad = 0;
                    currCasilla.celda.transform.localScale = grid.cellSize;
                    Vector3 pos = new Vector3(minPos.x + (i * grid.cellSize.x), 0, minPos.z + (j * grid.cellSize.z));
                    currCasilla.celda.transform.localPosition = pos;
                    matriz[i, j] = currCasilla;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }


    }
};
