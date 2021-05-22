using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g02
{
    public class MapManager : MonoBehaviour
    {
        //  N�mero de filas de este escenario
        public int filas;
        //  N�mero de columnas de este escenario
        public int columnas;
        //  Grid del escenario
        private Grid grid;
        //  Terrain del escenario
        private Terrain terrain;
        [Tooltip("Prefab de las casillas")]
        public GameObject ejemplo;
        //  Matriz de casillas
        private CasillaBehaviour[,] matriz;
        //  instancia est�tica de mapManager
        private static MapManager instance_;
        //  Cola de prioridad de enemigos en el map
        private Priority_Queue<CasillaPrio> prioCasillas;
        //
        private List<CasillaBehaviour> casillasADefender;

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
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void actualizaPrioridadAlEntrar(CasillaBehaviour casilla, UnitType unit_)
        {
            casilla.unidadEntraCasilla(unit_, unit_.influencia);
            if (unit_.unit == Unidad.DEFENSA) return;



            for (int i = casilla.getFila() - unit_.rango; i <= casilla.getFila() + unit_.rango; i++)
            {
                for (int j = casilla.getCol() - unit_.rango; j <= casilla.getCol() + unit_.rango; j++)
                {
                    //comprobamos que no nos salimos de la matriz
                    if (i >= 0 && i < filas && j >= 0 && j < columnas && casilla != matriz[i, j])
                    {
                        //comprobamos que prioridad le corresponde
                        matriz[i, j].unidadEntraCasilla(unit_, unit_.influencia - 1);
                    }
                }
            }



        }

        public void actualizaPrioridadAlSalir(CasillaBehaviour casilla, UnitType unit_)
        {
            casilla.unidadSaleCasilla(unit_, unit_.influencia);
            if (unit_.unit == Unidad.DEFENSA) return;

            int inf = unit_.influencia - 1;
            //recorremos la submatriz correspondiente
            for (int i = casilla.getFila() - unit_.rango; i <= casilla.getFila() + unit_.rango; i++)
            {
                for (int j = casilla.getCol() - unit_.rango; j <= casilla.getCol() + unit_.rango; j++)
                {
                    //comprobamos que no nos salimos de la matriz
                    if (i >= 0 && i < filas && j >= 0 && j < columnas && casilla != matriz[i, j])
                    {
                        //comprobamos que prioridad le corresponde
                        matriz[i, j].unidadSaleCasilla(unit_, unit_.influencia - 1);
                    }
                }
            }


        }

        public Priority_Queue<CasillaPrio> getMayorCasillaPrio()
        {
            return prioCasillas;
        }

        // BETA
        public Transform getPosADefender(Team team)
        {
            CasillaBehaviour casillaADef = null;
            int prioAtaque = 0;
            int prioDef = 0;
            int actPrioDef = 0;
            int areaRevisar = 3;
            foreach (CasillaBehaviour casilla in casillasADefender)
            {
                switch (team.myTeam())
                {
                    case Type.AZUL:
                        actPrioDef = casilla.defensaAzul;
                        break;
                    case Type.AMARILLO:
                        actPrioDef = casilla.defensaAmarilla;
                        break;
                }
                if (getPrioAtaque(casilla, areaRevisar, team) > prioAtaque && prioDef > actPrioDef ) 
                {
                    casillaADef = casilla;
                }
            }
            return casillaADef.transform;
        }


        //BETA
        private int getPrioAtaque(CasillaBehaviour casilla, int area,Team team)
        {
            int prio = 0;
            for (int i = casilla.getFila() - area; i < casilla.getFila() + area; i++) 
            {
                for (int j = casilla.getCol() - area; j < casilla.getCol() + area; j++)
                {
                    if (casilla.team_ != team.myTeam())
                    {
                        prio += matriz[i, j].prioridadMilitar;
                    }
                }
            }
            return prio;
        }
    }
};
