using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace es.ucm.fdi.iav.rts.g02
{
    public class CasillaBehaviour : MonoBehaviour
    {
        //  Representa al equipo que domina esta casilla
        public Type team_;
        //  influencia del equipo que domina esta casilla
        public int prioridadMilitar;
        //  Influencia de defensa de esta casilla 
        //public int prioridadDefensa;
        //  Número de unidades presentes en la casilla
        public int unidadesCasilla;
        //  Fila que representa a esta casilla en vector de mapManager
        private int fil;
        //  Columna que representa a esta casilla en vector de mapManager
        private int col;
        // Array con las unidades que están en esta casilla
        private List<UnitType> unidadesAmarillas = new List<UnitType>();
        private List<UnitType> unidadesVerdes = new List<UnitType>();
        private List<UnitType> unidadesAzules = new List<UnitType>();

        private int prioridadAmarilla = 0;
        private int prioridadAzul = 0;
        private int prioridadVerde = 0;

        private int defensaAzul = 0;
        private int defensaAmarilla = 0;

        void Start()
        {
            prioridadMilitar = 0;
           // prioridadDefensa = 0;
            unidadesCasilla = 0;
            team_ = Type.VACIA;
            cambiaCasillaColor();
        }

        public void modificaInfluenciaAlEntrar(Type teamType_, Unit unit_, int infl_)
        {
            if (prioridadMilitar < 0)
            {
                prioridadMilitar = 0;
            }
            //si entre un de mi mismo equipo, en uno neutral o en uno vacio
            if (teamType_.Equals(team_) || team_.Equals(Type.NEUTRAL) || team_.Equals(Type.VACIA))
            {
                //si es una unidad militar
                if (unit_.Equals(Unit.MILITAR))
                {
                    //aumentamos la priorida militar de esa zona
                    prioridadMilitar += infl_;
                    //la casilla pasa a ser de tu equipo
                    team_ = teamType_;

                }
                else {// si es de defensa
                    switch (teamType_)
                    {
                        case Type.AMARILLO:
                            defensaAmarilla += infl_;
                            break;
                        case Type.AZUL:
                            defensaAzul += infl_;
                            break;
                        
                    }
                }
                   

            }
            //si no es de tu equipo
            else if (!teamType_.Equals(team_))
            {
                //es una unidad Militar
                if (unit_.Equals(Unit.MILITAR))
                {

                    //prioridadMilitar -= infl_;
                    //cogemos el team con mayor influencia en la casilla
                    team_ = getMayorInfluenciaEnCasilla();
                    //si la casilla esta vacia o es neutral 
                    if (team_.Equals(Type.VACIA) || team_.Equals(Type.NEUTRAL))
                    {
                        //prioridad militar =0
                        prioridadMilitar = 0;
                    }
                    else //si el team es de algun equipo
                    {
                        switch (team_)
                        {
                            case Type.AMARILLO:
                                prioridadMilitar = prioridadAmarilla;
                                break;
                            case Type.AZUL:
                                prioridadMilitar = prioridadAzul;
                                break;
                            case Type.VERDE:
                                prioridadMilitar = prioridadVerde;
                                break;
                        }
                    }
                }
                else
                    switch (teamType_)
                    {
                        case Type.AMARILLO:
                            defensaAmarilla += infl_;
                            break;
                        case Type.AZUL:
                            defensaAzul += infl_;
                            break;

                    }
            }
            //cambiaCasillaColor();
        }

        public void modificaInfluenciaAlSalir(Type teamType_, Unit unit_, int infl_)
        {
            if (prioridadMilitar < 0)
            {
                prioridadMilitar = 0;
            }
            // si salgo en una casilla de mi equipo, neutral o verde
            if (teamType_.Equals(team_) || team_.Equals(Type.NEUTRAL) /*|| team_.Equals(Type.VERDE)*/)
            {
                //si es militar
                if (unit_.Equals(Unit.MILITAR))
                {
                   // prioridadMilitar -= infl_;
                    team_ = getMayorInfluenciaEnCasilla();

                    if (team_.Equals(Type.NEUTRAL) || teamType_.Equals(Type.VACIA))
                    {
                        prioridadMilitar = 0;
                    }
                    else
                    {
                        switch (team_)
                        {
                            case Type.AMARILLO:
                                prioridadMilitar = prioridadAmarilla;
                                break;
                            case Type.AZUL:
                                prioridadMilitar = prioridadAzul;
                                break;
                            case Type.VERDE:
                                prioridadMilitar = prioridadVerde;
                                break;
                        }
                    }
                }
                else// si soy de defensa
                    switch (teamType_)
                    {
                        case Type.AMARILLO:
                            defensaAmarilla -= infl_;
                            break;
                        case Type.AZUL:
                            defensaAzul -= infl_;
                            break;

                    }


            }
            //si salgo de una casilla que no es de mi equipo
            else if (!teamType_.Equals(team_))
            {
                //si soy de defensa
                if (!unit_.Equals(Unit.MILITAR))
                {
                    switch (teamType_)
                    {
                        case Type.AMARILLO:
                            defensaAmarilla -= infl_;
                            break;
                        case Type.AZUL:
                            defensaAzul -= infl_;
                            break;

                    }
                }
                    
                
                   

            }
            
        }

        //  Gestiona la entrada de una unidad a esta casilla
        public void unidadEntraCasilla(UnitType unit_, int influ)
        {
            switch (unit_.unitOwner)
            {
                case Type.AMARILLO:
                    unidadesAmarillas.Add(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadAmarilla += influ;
                    }
                    break;
                case Type.VERDE:
                    unidadesVerdes.Add(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadVerde += influ;
                    }
                    break;
                case Type.AZUL:
                    unidadesAzules.Add(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadAzul += influ;
                    }
                    break;
                default:
                    break;
            }
            modificaInfluenciaAlEntrar(unit_.unitOwner, unit_.unit, influ);
            cambiaCasillaColor();
      
        }
    

        private Type getMayorInfluenciaEnCasilla()
        {

            if (prioridadVerde == 0 && prioridadAzul == 0 && prioridadAmarilla == 0)
            {
                return Type.VACIA;
            }
            else if (prioridadAmarilla > prioridadAzul && prioridadAmarilla > prioridadVerde)
            {
                return Type.AMARILLO;
            }
            else if (prioridadAzul > prioridadAmarilla && prioridadAzul > prioridadVerde)
            {
                return Type.AZUL;
            }
            else if (prioridadVerde == 0 && prioridadAzul == prioridadAmarilla)
            {
                return Type.NEUTRAL;
            }
            else
            {
                return Type.VERDE;
            }
        }
        //  Gestiona la salida de una unidad a esta casilla
        public void unidadSaleCasilla(UnitType unit_, int influ)
        {
            switch (unit_.unitOwner)
            {
                case Type.AMARILLO:
                    unidadesAmarillas.Remove(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadAmarilla -= influ;
                    }
                    break;
                case Type.VERDE:
                    unidadesVerdes.Remove(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadVerde -= influ;
                    }
                    break;
                case Type.AZUL:
                    unidadesAzules.Remove(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadAzul -= influ;
                    }
                    break;
                default:
                    break;
            }

            modificaInfluenciaAlSalir(unit_.unitOwner, unit_.unit, influ);
            cambiaCasillaColor();
        }

        //  Configura la fila y la columan de esta casilla
        public void setMatrixPos(int x, int y)
        {
            fil = x;
            col = y;
        }

        //  Devuelve la fila que represeta a esta casilla dentro del array de casilla de MapManager
        public int getFila()
        {
            return fil;
        }

        //  Devuelve la columna que represeta a esta casilla dentro del array de casilla de MapManager
        public int getCol()
        {
            return col;
        }

        //  Configura el color que le corresponde según que equipo domina esta casilla
        private void cambiaCasillaColor()
        {
            Color cl = Color.red;
            switch (team_)
            {
                case Type.AMARILLO:
                    cl = Color.yellow;
                    break;
                case Type.AZUL:
                    cl = Color.blue;
                    break;
                case Type.VERDE:
                    cl = Color.green;
                    break;
                case Type.NEUTRAL:
                    cl = Color.gray;
                    break;
                case Type.VACIA:
                    cl = Color.white;
                    break;
                default:
                    break;
            }

            //cl.r = cl.r * Mathf.Abs(prioridadMilitar / 10);
            //cl.g = cl.g * Mathf.Abs(prioridadMilitar / 10);
            //cl.b = cl.b * Mathf.Abs(prioridadMilitar / 10);
            cl.a = 0.2f;
            gameObject.GetComponent<MeshRenderer>().material.color = cl;
        }

        private void actualizaInfluenciaEnCasilla()
        {
            if (prioridadMilitar < 0)   // prov
            {
                prioridadMilitar = 0;
            }

            foreach (UnitType unit_ in unidadesAmarillas)
            {
                modificaInfluenciaAlEntrar(unit_.unitOwner, unit_.unit, unit_.influencia);
            }
        }
    }
};
