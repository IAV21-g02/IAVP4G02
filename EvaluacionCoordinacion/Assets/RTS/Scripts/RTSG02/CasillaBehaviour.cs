using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace es.ucm.fdi.iav.rts.g02
{
    public class CasillaBehaviour : MonoBehaviour
    {
        public Type team_;
        public int prioridadMilitar;
        public int prioridadDefensa;
        public int unidadesCasilla;
        private int fil;
        private int col;
        // Start is called before the first frame update
        void Start()
        {
            prioridadMilitar = 0;
            prioridadDefensa = 0;
            unidadesCasilla = 0;
            team_ = Type.VACIA;
            cambiaCasillaColor();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void unidadEntraCasilla(UnitType unit_)
        {
            unidadesCasilla += 1;
            if (unit_.Equals(Type.VERDE))
            {
                team_ = Type.VERDE;
                prioridadMilitar += unit_.influencia;
            }
            else if (unit_.unitOwner == team_ || team_ == Type.NEUTRAL || team_ == Type.VACIA) 
            {
                if (unit_.unit == Unit.DEFENSA)
                    prioridadDefensa += unit_.influencia;
                else
                    prioridadMilitar += unit_.influencia;

                team_ = unit_.unitOwner;
            }
            else
            {
                prioridadMilitar -= unit_.influencia;
                if (prioridadMilitar < 0)
                {
                    team_ = unit_.unitOwner;
                    prioridadMilitar = Mathf.Abs(prioridadMilitar);
                }
                else if(prioridadMilitar == 0)
                {
                    team_ = Type.NEUTRAL;
                }
            }

            cambiaCasillaColor();
        }


        public void unidadSaleCasilla(UnitType unit_)
        {
            unidadesCasilla -= 1;
            if (unidadesCasilla <= 0)
            {
                team_ = Type.VACIA;
                prioridadMilitar = 0;
                prioridadDefensa = 0;
                unidadesCasilla = 0;
            }
            else if (team_.Equals(unit_.unitOwner) || team_.Equals(Type.NEUTRAL))
            {
                if (unit_.unit == Unit.DEFENSA)
                    prioridadDefensa -= unit_.influencia;
                else
                    prioridadMilitar -= unit_.influencia;

                if (prioridadMilitar < 0)
                {
                    team_ = unit_.unitOwner;
                    prioridadMilitar = Mathf.Abs(prioridadMilitar);
                }
                else if (prioridadMilitar == 0)
                {
                    team_ = Type.NEUTRAL;
                }
            }
            else
            {
                if (unit_.unit == Unit.DEFENSA)
                    prioridadDefensa -= unit_.influencia;
                else
                    prioridadMilitar += unit_.influencia;

            }
            cambiaCasillaColor();
        }

        public void setMatrixPos(int x, int y)
        {
            fil = x;
            col = y;
        }

        public int getFila()
        {
            return fil;
        }

        public int getCol()
        {
            return col;
        }


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
            cl.a = 0.2f;
            gameObject.GetComponent<MeshRenderer>().material.color = cl;
        }

    }
};
