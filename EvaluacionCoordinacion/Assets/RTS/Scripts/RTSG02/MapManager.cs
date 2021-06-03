using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g02
{
    public class MapManager : MonoBehaviour
    {
        //  Número de filas de este escenario
        private int filas;
        //  Número de columnas de este escenario
        private int columnas;
        //  Grid del escenario
        private Grid grid;
        //  Matriz de casillas
        private CasillaBehaviour[,] matriz;
        //  instancia estática de mapManager
        private static MapManager instance_;
        //  Para ocultar el mapa
        private bool visible = false;
        //  Terrain del escenario
        private Terrain terrain;

        private CasillaBehaviour MaxprioAzul;
        private CasillaBehaviour MaxprioAmarillo;

        //  GameObject usado para instanciar el mapa
        [Tooltip("Prefab de las casillas")]
        public GameObject ejemplo;

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

        public static MapManager GetInstance()
        {
            return instance_;
        }

        //  Construye las casillas a lo largo del mapa
        void Start()
        {
            grid = GetComponentInParent<Grid>();
            terrain = GetComponentInParent<Terrain>();

            filas = (int)(terrain.terrainData.size.x / grid.cellSize.x);
            columnas = (int)(terrain.terrainData.size.z / grid.cellSize.z);

            matriz = new CasillaBehaviour[filas, columnas];
            Vector3 minPos;
            minPos.x = (grid.cellSize.x / 2);
            minPos.z = (grid.cellSize.z / 2);
            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    GameObject currCasilla = Instantiate(ejemplo, transform);
                    currCasilla.transform.localScale = grid.cellSize;
                    Vector3 pos = new Vector3(minPos.x + (i * grid.cellSize.x), 0, minPos.z + (j * grid.cellSize.z));
                    currCasilla.transform.localPosition = pos;
                    matriz[i, j] = currCasilla.GetComponent<CasillaBehaviour>();
                    matriz[i, j].setMatrixPos(i, j);
                    if (i == 0 && j == 0)
                    {
                        Debug.Log(matriz[i, j].transform.position);
                    }
                }
            }
        }

        //  Actualiza una casilla al entrar una unidad en ella
        public void ActualizaPrioridadAlEntrar(CasillaBehaviour casilla, UnitType unit_)
        {
            casilla.UnidadEntraCasilla(unit_, unit_.influencia);

            if (unit_.unit == Unidad.DEFENSA) return;

            //Casillas adyacentes
            for (int i = casilla.GetFila() - unit_.rango; i <= casilla.GetFila() + unit_.rango; i++)
            {
                for (int j = casilla.GetCol() - unit_.rango; j <= casilla.GetCol() + unit_.rango; j++)
                {
                    //comprobamos que no nos salimos de la matriz
                    if (i >= 0 && i < filas && j >= 0 && j < columnas && casilla != matriz[i, j])
                    {
                        //comprobamos que prioridad le corresponde
                        matriz[i, j].UnidadEntraCasilla(unit_, unit_.influencia - 1);
                    }
                }
            }



            if (MaxprioAzul == null || MaxprioAzul.prioridadAzul < casilla.prioridadAzul)
            {
                MaxprioAzul = casilla;
            }
            if (MaxprioAmarillo == null || MaxprioAmarillo.prioridadAmarilla < casilla.prioridadAmarilla)
            {
                MaxprioAmarillo = casilla;
            }

        }

        //  Actualiza una casilla al salir
        public void ActualizaPrioridadAlSalir(CasillaBehaviour casilla, UnitType unit_)
        {
            casilla.UnidadSaleCasilla(unit_, unit_.influencia);
            if (unit_.unit == Unidad.DEFENSA) return;

            int inf = unit_.influencia - 1;
            //recorremos la submatriz correspondiente
            for (int i = casilla.GetFila() - unit_.rango; i <= casilla.GetFila() + unit_.rango; i++)
            {
                for (int j = casilla.GetCol() - unit_.rango; j <= casilla.GetCol() + unit_.rango; j++)
                {
                    //comprobamos que no nos salimos de la matriz
                    if (i >= 0 && i < filas && j >= 0 && j < columnas && casilla != matriz[i, j])
                    {
                        //comprobamos que prioridad le corresponde
                        matriz[i, j].UnidadSaleCasilla(unit_, unit_.influencia - 1);
                    }
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (visible)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                else
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.SetActive(true);
                    }
                }

                visible = !visible;
            }
        }
        //  Devuelve la casilla en función de un transform
        //public CasillaBehaviour GetCasillaCercana(Transform pos)
        //{
        //    int indX = Mathf.Abs((int)(pos.position.x / grid.cellSize.x));
        //    int indZ = Mathf.Abs((int)(pos.position.z / grid.cellSize.z));
        //    return matriz[indX, indZ];
        //}
        public CasillaBehaviour getEnemyMaxPrio(ColorTeam team)
        {

            if (team == ColorTeam.AMARILLO)
            {
                return MaxprioAzul;
            }
            else
            {
                return MaxprioAmarillo;
            }
        }
    }
};
