using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace es.ucm.fdi.iav.rts.g02
{
    public class CasillaBehaviour : MonoBehaviour
    {
        //  Representa al equipo que domina esta casilla
        public ColorTeam team_;
        //  influencia del equipo que domina esta casilla
        public int currMiliPrio;

        //  Fila que representa a esta casilla en vector de mapManager
        private int fil;
        //  Columna que representa a esta casilla en vector de mapManager
        private int col;

        // Array con las unidades que están en esta casilla
        private List<UnitType> unidadesAmarillas = new List<UnitType>();
        private List<UnitType> unidadesVerdes = new List<UnitType>();
        private List<UnitType> unidadesAzules = new List<UnitType>();

        //Prioridad total de las unidades amarillas
        public int prioridadAmarilla = 0;
        //Prioridad total de las unidades azules
        public int prioridadAzul = 0;
        //Prioridad total de las unidades verdes
        private int prioridadVerde = 0;

        private int defensaAzul = 0;
        private int defensaAmarilla = 0;

        private CasillaPrioAtaque casillaPrioAtq;
        private CasillaPrioDefensa casillaPrioDef;

        private void Start()
        {
            currMiliPrio = 0;            
            team_ = ColorTeam.VACIA;
            
            CambiaCasillaColor();
 
            //Datos de prioridad y ataque IComparer
            casillaPrioAtq = new CasillaPrioAtaque(this);
            casillaPrioDef= new CasillaPrioDefensa(this);
        }

        //  Gestiona la entrada de una unidad a esta casilla
        public void UnidadEntraCasilla(UnitType unit_, int influ)
        {
            switch (unit_.unitOwner)
            {
                case ColorTeam.AMARILLO:
                    unidadesAmarillas.Add(unit_);
                    if (unit_.unit.Equals(Unidad.MILITAR))
                    {
                        prioridadAmarilla += influ;
                    }
                    break;
                case ColorTeam.VERDE:
                    unidadesVerdes.Add(unit_);
                    if (unit_.unit.Equals(Unidad.MILITAR))
                    {
                        prioridadVerde += influ;
                    }
                    break;
                case ColorTeam.AZUL:
                    unidadesAzules.Add(unit_);
                    if (unit_.unit.Equals(Unidad.MILITAR))
                    {
                        prioridadAzul += influ;
                    }
                    break;
                default:
                    currMiliPrio = 0;
                    break;
            }
            ModificaInfluenciaAlEntrar(unit_.unitOwner, unit_.unit, influ);
            CambiaCasillaColor();

        }

        //  Gestiona la salida de una unidad a esta casilla
        public void UnidadSaleCasilla(UnitType unit_, int influ)
        {
            switch (unit_.unitOwner)
            {
                case ColorTeam.AMARILLO:
                    unidadesAmarillas.Remove(unit_);
                    if (unit_.unit.Equals(Unidad.MILITAR))
                    {
                        prioridadAmarilla -= influ;
                    }
                    break;
                case ColorTeam.VERDE:
                    unidadesVerdes.Remove(unit_);
                    if (unit_.unit.Equals(Unidad.MILITAR))
                    {
                        prioridadVerde -= influ;
                    }
                    break;
                case ColorTeam.AZUL:
                    unidadesAzules.Remove(unit_);
                    if (unit_.unit.Equals(Unidad.MILITAR))
                    {
                        prioridadAzul -= influ;
                    }
                    break;
                default:
                    break;
            }

            ModificaInfluenciaAlSalir(unit_.unitOwner, unit_.unit, influ);
            CambiaCasillaColor();
        }

        //  Configura la fila y la columan de esta casilla
        public void setMatrixPos(int x, int y)
        {
            fil = x;
            col = y;
        }

        //  Devuelve la fila que represeta a esta casilla dentro del array de casilla de MapManager
        public int GetFila()
        {
            return fil;
        }

        //  Devuelve la columna que represeta a esta casilla dentro del array de casilla de MapManager
        public int GetCol()
        {
            return col;
        }

        public CasillaPrioAtaque GetCasillaPrioMilitar()
        {
            return casillaPrioAtq;
        }

        public CasillaPrioDefensa getCasillaPrioDefensa()
        {
            return casillaPrioDef;
        }

        //Devuelve la prioridad de defensa actual
        public int GetCurrDefPrio() {
            if (team_.Equals(ColorTeam.AZUL)) return defensaAzul;
            
            return defensaAmarilla;
        }

        //-------------------------------------------------------------------------------------//

        //Devuelve el la mejor prioridad de la casilla
        private ColorTeam GetMayorPrio()
        {

            if (prioridadVerde == 0 && prioridadAzul == 0 && prioridadAmarilla == 0)
            {
                return ColorTeam.VACIA;
            }
            else if (prioridadAmarilla > prioridadAzul && prioridadAmarilla > prioridadVerde)
            {
                return ColorTeam.AMARILLO;
            }
            else if (prioridadAzul > prioridadAmarilla && prioridadAzul > prioridadVerde)
            {
                return ColorTeam.AZUL;
            }
            else if (prioridadVerde == 0 && prioridadAzul == prioridadAmarilla)
            {
                return ColorTeam.NEUTRAL;
            }
            else
            {
                return ColorTeam.VERDE;
            }
        }

        //  Configura el color que le corresponde según que equipo domina esta casilla
        private void CambiaCasillaColor()
        {
            Color cl = Color.red;
            switch (team_)
            {
                case ColorTeam.AMARILLO:
                    cl = Color.yellow;
                    break;
                case ColorTeam.AZUL:
                    cl = Color.blue;
                    break;
                case ColorTeam.VERDE:
                    cl = Color.green;
                    break;
                case ColorTeam.NEUTRAL:
                    cl = Color.gray;
                    break;
                case ColorTeam.VACIA:
                    cl = Color.white;
                    break;
                default:
                    break;
            }

            cl.a = 0.2f;
            gameObject.GetComponent<MeshRenderer>().material.color = cl;
        }

        //Actualiza la prioridad de la casilla
        private void ActualizaPrioridadCasilla(ColorTeam dominanUnit)
        {
            switch (dominanUnit)
            {
                case ColorTeam.AMARILLO:
                    currMiliPrio = prioridadAmarilla;
                    break;
                case ColorTeam.AZUL:
                    currMiliPrio = prioridadAzul;
                    break;
                case ColorTeam.VERDE:
                    currMiliPrio = prioridadVerde;
                    break;
                case ColorTeam.VACIA:
                    currMiliPrio = 0;
                    break;
                case ColorTeam.NEUTRAL:
                    currMiliPrio = 0;
                    break;
                default:
                    currMiliPrio = 0;
                    break;
            }
        }

        //Actualiza la influencia de la casilla cuando ha entrado una nueva unidad
        private void ModificaInfluenciaAlEntrar(ColorTeam teamType_, Unidad unit_, int infl_)
        {
            if (currMiliPrio < 0)
            {
                currMiliPrio = 0;
            }

            if (prioridadAmarilla < 0) prioridadAmarilla = 0;
            if (prioridadAzul < 0) prioridadAzul = 0;
            if (prioridadVerde < 0) prioridadVerde = 0;


            //Si es del mismo tipo que la casilla, la casilla es neutral o está vacía
            if (teamType_.Equals(team_) || team_.Equals(ColorTeam.NEUTRAL) || team_.Equals(ColorTeam.VACIA))
            {
                //Si es una unidad militar
                if (unit_.Equals(Unidad.MILITAR))
                {
                    //La casilla es del equipo de la unidad entrante
                    team_ = teamType_;

                    //Actualizamos valor de la prioridadMilitar
                    ActualizaPrioridadCasilla(team_);
                }
                //Si es una unidad de defensa
                else
                {
                    switch (teamType_)
                    {
                        case ColorTeam.AMARILLO:
                            defensaAmarilla += infl_;
                            break;
                        case ColorTeam.AZUL:
                            defensaAzul += infl_;
                            break;

                    }
                }

            }
            //Si no es del mismo equipo
            else if (!teamType_.Equals(team_))
            {
                //es una unidad Militar
                if (unit_.Equals(Unidad.MILITAR))
                {
                    //cogemos el team con mayor influencia en la casilla
                    team_ = GetMayorPrio();

                    //si la casilla esta vacia o es neutral la prioridad militar es cero
                    if (team_.Equals(ColorTeam.VACIA) || team_.Equals(ColorTeam.NEUTRAL))
                    {
                        currMiliPrio = 0;
                    }
                    else ActualizaPrioridadCasilla(team_);
                }
                else
                    switch (teamType_)
                    {
                        case ColorTeam.AMARILLO:
                            defensaAmarilla += infl_;
                            break;
                        case ColorTeam.AZUL:
                            defensaAzul += infl_;
                            break;

                    }
            }
        }

        //Actualiza la influencia de la casilla cuando ha salida una unidad
        private void ModificaInfluenciaAlSalir(ColorTeam teamType_, Unidad unit_, int infl_)
        {
            if (currMiliPrio < 0)
            {
                currMiliPrio = 0;
            }

            if (prioridadAmarilla < 0) prioridadAmarilla = 0;
            if (prioridadAzul < 0) prioridadAzul = 0;
            if (prioridadVerde < 0) prioridadVerde = 0;

            // si salgo en una casilla de mi equipo o neutral
            if (teamType_.Equals(team_) || team_.Equals(ColorTeam.NEUTRAL))
            {
                //si es militar
                if (unit_.Equals(Unidad.MILITAR))
                {
                    team_ = GetMayorPrio();

                    if (team_.Equals(ColorTeam.NEUTRAL) || teamType_.Equals(ColorTeam.VACIA))
                    {
                        currMiliPrio = 0;
                    }
                    else
                    {
                        ActualizaPrioridadCasilla(team_);
                    }
                }
                else// si soy de defensa
                    switch (teamType_)
                    {
                        case ColorTeam.AMARILLO:
                            defensaAmarilla -= infl_;
                            break;
                        case ColorTeam.AZUL:
                            defensaAzul -= infl_;
                            break;

                    }
            }
            //si salgo de una casilla que no es de mi equipo
            else if (!teamType_.Equals(team_))
            {
                //si soy de defensa
                if (!unit_.Equals(Unidad.MILITAR))
                {
                    switch (teamType_)
                    {
                        case ColorTeam.AMARILLO:
                            defensaAmarilla -= infl_;
                            break;
                        case ColorTeam.AZUL:
                            defensaAzul -= infl_;
                            break;

                    }
                }

            }

        }

    }
};
