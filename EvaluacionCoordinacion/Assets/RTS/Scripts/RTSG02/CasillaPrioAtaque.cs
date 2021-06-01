using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace es.ucm.fdi.iav.rts.g02
{
    public class CasillaPrioAtaque
    {
        private ColorTeam team;
        private int influencia;
        private CasillaBehaviour casilla;

        public void ActualizaAtaque()
        {
            team = casilla.team_;
            influencia = casilla.currMiliPrio;
        }

        public CasillaPrioAtaque(CasillaBehaviour other)
        {
            casilla = other;
            team = casilla.team_;
            influencia = casilla.currMiliPrio;
        }

        public int CompareTo(CasillaPrioAtaque other)
        {
            int result = casilla.currMiliPrio - other.casilla.currMiliPrio;
            if (this.Equals(other) && result == 0)
                return 0;
            else return result;
        }

        public bool Equals(CasillaPrioAtaque other)
        {
            return (this.Equals(other) && this.casilla.Equals(other));
        }

        public override bool Equals(object obj)
        {
            CasillaBehaviour other = (CasillaBehaviour)obj;
            return Equals(other);
        }

        public CasillaBehaviour GetCasilla()
        {
            return casilla;
        }
    }

    //Clase para el comparador de CasillaPrioAtaque
    public class ComparerAtaque : IComparer<CasillaPrioAtaque> {
        public int Compare(CasillaPrioAtaque x, CasillaPrioAtaque y)
        {
            int result = y.GetCasilla().currMiliPrio - x.GetCasilla().currMiliPrio;
            if (this.Equals(y) && result == 0)
                return 0;
            else return result;
        }
    }

    public class CasillaPrioDefensa
    {
        private ColorTeam team;
        private int influencia;
        private CasillaBehaviour casilla;

        public void ActualizaDefensa() {
            team = casilla.team_;
            influencia = casilla.GetCurrDefPrio();
        }

        public CasillaPrioDefensa(CasillaBehaviour other)
        {
            casilla = other;
            team = casilla.team_;
            influencia = casilla.GetCurrDefPrio();
        }

        public int CompareTo(CasillaPrioDefensa other)
        {
            int result = casilla.currMiliPrio - other.casilla.currMiliPrio;
            if (this.Equals(other) && result == 0)
                return 0;
            else return result;
        }

        public bool Equals(CasillaPrioDefensa other)
        {
            return (this.Equals(other) && this.casilla.Equals(other));
        }

        public override bool Equals(object obj)
        {
            CasillaBehaviour other = (CasillaBehaviour)obj;
            return Equals(other);
        }

        public CasillaBehaviour GetCasilla()
        {
            return casilla;
        }
    }

    public class ComparerDef : IComparer<CasillaPrioDefensa> {
        public int Compare(CasillaPrioDefensa x, CasillaPrioDefensa y)
        {
            int result = y.GetCasilla().GetCurrDefPrio() - x.GetCasilla().GetCurrDefPrio();
            if (this.Equals(y) && result == 0)
                return 0;
            else return result;
        }
    }
}

