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
        public int prioridadDefensa;
        //  N�mero de unidades presentes en la casilla
        public int unidadesCasilla;
        //  Fila que representa a esta casilla en vector de mapManager
        private int fil;
        //  Columna que representa a esta casilla en vector de mapManager
        private int col;
        // Array con las unidades que est�n en esta casilla
        private List<UnitType> unidadesAmarillas = new List<UnitType>();
        private List<UnitType> unidadesVerdes = new List<UnitType>();
        private List<UnitType> unidadesAzules = new List<UnitType>();

        private int prioridadAmarilla = 0;
        private int prioridadAzul = 0;
        private int prioridadVerde = 0;

        void Start()
        {
            prioridadMilitar = 0;
            prioridadDefensa = 0;
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

            if (teamType_.Equals(team_) || team_.Equals(Type.NEUTRAL) || team_.Equals(Type.VACIA))
            {
                if (unit_.Equals(Unit.MILITAR))
                    prioridadMilitar += infl_;
                else
                    prioridadDefensa += infl_;

                team_ = teamType_;
            }
            else if (!teamType_.Equals(team_))
            {
                if (unit_.Equals(Unit.MILITAR))
                {
                    prioridadMilitar -= infl_;
                    team_ = getMayorInfluenciaEnCasilla();
                    if (team_.Equals(Type.VACIA) || team_.Equals(Type.NEUTRAL))
                    {
                        prioridadMilitar = 0;
                    }
                    else if (prioridadMilitar < 0)
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
                    prioridadDefensa += infl_;
            }
            cambiaCasillaColor();
        }

        public void modificaInfluenciaAlSalir(Type teamType_, Unit unit_, int infl_)
        {
            if (prioridadMilitar < 0)
            {
                prioridadMilitar = 0;
            }

            if (teamType_.Equals(team_) || team_.Equals(Type.NEUTRAL) || team_.Equals(Type.VERDE))
            {
                if (unit_.Equals(Unit.MILITAR))
                {
                    prioridadMilitar -= infl_;
                    team_ = getMayorInfluenciaEnCasilla();

                    if (team_.Equals(Type.NEUTRAL) || teamType_.Equals(Type.VACIA))
                    {
                        prioridadMilitar = 0;
                    }
                    else if (prioridadMilitar < 0)
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
                    prioridadDefensa -= infl_;


            }
            else if (!teamType_.Equals(team_))
            {
                if (unit_.Equals(Unit.MILITAR))
                    prioridadMilitar += infl_;
                else
                    prioridadDefensa -= infl_;

            }
            cambiaCasillaColor();
        }

        //  Gestiona la entrada de una unidad a esta casilla
        public void unidadEntraCasilla(UnitType unit_)
        {
            switch (unit_.unitOwner)
            {
                case Type.AMARILLO:
                    unidadesAmarillas.Add(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadAmarilla += unit_.influencia;
                    }
                    break;
                case Type.VERDE:
                    unidadesVerdes.Add(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadVerde += unit_.influencia;
                    }
                    break;
                case Type.AZUL:
                    unidadesAzules.Add(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadAzul += unit_.influencia;
                    }
                    break;
                default:
                    break;
            }
            modificaInfluenciaAlEntrar(unit_.unitOwner, unit_.unit, unit_.influencia);
            cambiaCasillaColor();
            //return;

            //unidadesCasilla += 1;
            //if (unit_.Equals(Type.VERDE))
            //{
            //    team_ = Type.VERDE;
            //    prioridadMilitar += unit_.influencia;
            //}
            //else if (unit_.unitOwner == team_ || team_ == Type.NEUTRAL || team_ == Type.VACIA) 
            //{
            //    if (unit_.unit == Unit.DEFENSA)
            //        prioridadDefensa += unit_.influencia;
            //    else
            //        prioridadMilitar += unit_.influencia;

            //    team_ = unit_.unitOwner;
            //}
            //else
            //{
            //    prioridadMilitar -= unit_.influencia;
            //    if (prioridadMilitar < 0)
            //    {
            //        team_ = unit_.unitOwner;
            //        prioridadMilitar = Mathf.Abs(prioridadMilitar);
            //    }
            //    else if(prioridadMilitar == 0)
            //    {
            //        team_ = Type.NEUTRAL;
            //    }
            //}

            //cambiaCasillaColor();
        }
        private int getInfluenciaAmarilla()
        {
            int inf = 0;
            foreach (UnitType unit_ in unidadesAmarillas)
            {
                if (unit_.unit.Equals(Unit.MILITAR))
                {
                    inf += unit_.influencia;
                }
            }
            return inf;
        }

        private int getInfluenciaAzul()
        {
            int inf = 0;
            foreach (UnitType unit_ in unidadesAzules)
            {
                if (unit_.unit.Equals(Unit.MILITAR))
                {
                    inf += unit_.influencia;
                }
            }
            return inf;
        }

        private int getInfluenciaVerde()
        {
            int inf = 0;
            foreach (UnitType unit_ in unidadesVerdes)
            {
                if (unit_.unit.Equals(Unit.MILITAR))
                {
                    inf += unit_.influencia;
                }
            }
            return inf;
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
        public void unidadSaleCasilla(UnitType unit_)
        {
            switch (unit_.unitOwner)
            {
                case Type.AMARILLO:
                    unidadesAmarillas.Remove(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadAmarilla -= unit_.influencia;
                    }
                    break;
                case Type.VERDE:
                    unidadesVerdes.Remove(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadVerde -= unit_.influencia;
                    }
                    break;
                case Type.AZUL:
                    unidadesAzules.Remove(unit_);
                    if (unit_.unit.Equals(Unit.MILITAR))
                    {
                        prioridadAzul -= unit_.influencia;
                    }
                    break;
                default:
                    break;
            }

            modificaInfluenciaAlSalir(unit_.unitOwner, unit_.unit, unit_.influencia);



            //unidadesCasilla -= 1;
            //if (unidadesCasilla <= 0)
            //{
            //    team_ = Type.VACIA;
            //    prioridadMilitar = 0;
            //    prioridadDefensa = 0;
            //    unidadesCasilla = 0;
            //}
            //else if (team_.Equals(unit_.unitOwner) || team_.Equals(Type.NEUTRAL))
            //{
            //    if (unit_.unit == Unit.DEFENSA)
            //        prioridadDefensa -= unit_.influencia;
            //    else
            //        prioridadMilitar -= unit_.influencia;

            //    if (prioridadMilitar < 0)
            //    {
            //        team_ = unit_.unitOwner;
            //        prioridadMilitar = Mathf.Abs(prioridadMilitar);
            //    }
            //    else if (prioridadMilitar == 0)
            //    {
            //        team_ = Type.NEUTRAL;
            //    }
            //}
            //else
            //{
            //    if (unit_.unit == Unit.DEFENSA)
            //        prioridadDefensa -= unit_.influencia;
            //    else
            //        prioridadMilitar += unit_.influencia;

            //}
            //cambiaCasillaColor();
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

        //  Configura el color que le corresponde seg�n que equipo domina esta casilla
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
