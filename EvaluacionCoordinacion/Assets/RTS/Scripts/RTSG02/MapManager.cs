using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g02
{
    public class MapManager : MonoBehaviour
    {
        public int filas;
        public int columnas;
        private Grid grid;
        private Terrain terrain;
        public GameObject ejemplo;
        // private casilla[,] matriz;
        private CasillaBehaviour[,] matriz;
        private static MapManager instance_;

        // Start is called before the first frame update

        private void Awake()
        {
            if (instance_ == null)
            {
                instance_ = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public static MapManager getInstance()
        {
            return instance_;
        }


        void Start()
        {
            grid = GetComponentInParent<Grid>();
            terrain = GetComponentInParent<Terrain>();

            filas = (int)(terrain.terrainData.size.x / grid.cellSize.x);
            columnas = (int)(terrain.terrainData.size.z / grid.cellSize.z);

            //matriz = new casilla[filas, columnas];
            matriz = new CasillaBehaviour[filas, columnas];
            float gap = 0.0f;
            Vector3 minPos;
            minPos.x = (grid.cellSize.x / 2);
            minPos.z = (grid.cellSize.z / 2);
            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    //casilla currCasilla = new casilla();
                    //currCasilla.celda = Instantiate(ejemplo, transform);
                    //currCasilla.prioridad = 0;
                    //currCasilla.celda.transform.localScale = grid.cellSize;
                    //Vector3 pos = new Vector3(minPos.x + (i * grid.cellSize.x), 0, minPos.z + (j * grid.cellSize.z));
                    //currCasilla.celda.transform.localPosition = pos;
                    //matriz[i, j] = currCasilla;

                    //casilla currCasilla = new GameObject();
                    GameObject currCasilla = Instantiate(ejemplo, transform);
                    //currCasilla.prioridad = 0;
                    currCasilla.transform.localScale = grid.cellSize;
                    Vector3 pos = new Vector3(minPos.x + (i * grid.cellSize.x), 0, minPos.z + (j * grid.cellSize.z));
                    currCasilla.transform.localPosition = pos;
                    matriz[i, j] = currCasilla.GetComponent<CasillaBehaviour>();
                    matriz[i, j].setMatrixPos(i,j);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void actualizaPrioridadAlEntrar(CasillaBehaviour casilla,UnitType unit_)
        {
            casilla.unidadEntraCasilla(unit_);
            if (unit_.unit == Unit.DEFENSA) return;

            UnitType aux = unit_;
            aux.influencia--;
            
            //int i = fil-radius;
            //int j = col-radius;
            //recorremos la submatriz correspondiente
            for (int i = casilla.getFila() - 1; i <= casilla.getFila() + 1; i++) 
            {
                for (int j = casilla.getCol() - 1; j <= casilla.getCol() + 1; j++)
                {
                    //comprobamos que no nos salimos de la matriz
                    if (i >= 0 && i < filas && j >= 0 && j < columnas && casilla != matriz[i,j]) 
                    {
                        //comprobamos que prioridad le corresponde
                        matriz[i, j].unidadEntraCasilla(aux);
                           
                    }
                }
            }

        }

        public void actualizaPrioridadAlSalir(CasillaBehaviour casilla, UnitType unit_)
        {
            casilla.unidadSaleCasilla(unit_);
            if (unit_.unit == Unit.DEFENSA) return;

            UnitType aux = unit_;
            aux.influencia--;

            //int i = fil-radius;
            //int j = col-radius;
            //recorremos la submatriz correspondiente
            for (int i = casilla.getFila() - 1; i <= casilla.getFila() + 1; i++)
            {
                for (int j = casilla.getCol() - 1; j <= casilla.getCol() + 1; j++)
                {
                    //comprobamos que no nos salimos de la matriz
                    if (i >= 0 && i < filas && j >= 0 && j < columnas && casilla != matriz[i, j])
                    {
                        //comprobamos que prioridad le corresponde
                        matriz[i, j].unidadSaleCasilla(aux);

                    }
                }
            }

        }

    }
};
