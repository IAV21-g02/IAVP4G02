using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace es.ucm.fdi.iav.rts.g02
{
    public class CasillaPrioMilitar : IComparable<CasillaPrioMilitar> 
    {
        private CasillaBehaviour casilla;
        public CasillaPrioMilitar(CasillaBehaviour other)
        {
            casilla = other;
        }
        public int CompareTo(CasillaPrioMilitar other)
        {
            int result = casilla.prioridadMilitar - other.casilla.prioridadMilitar;
            if (this.Equals(other) && result == 0)
                return 0;
            else return result;
        }

        public bool Equals(CasillaPrioMilitar other)
        {
            return (this.Equals(other) && this.casilla.Equals(other));
        }

        public override bool Equals(object obj)
        {
            CasillaBehaviour other = (CasillaBehaviour)obj;
            return Equals(other);
        }

        public CasillaBehaviour getCasilla()
        {
            return casilla;
        }

        //public override int GetHashCode()
        //{           
        //    return this
        //}
    }

    public class CasillaPrioDefensa : IComparable<CasillaPrioDefensa>
    {
        private CasillaBehaviour casilla;
        public CasillaPrioDefensa(CasillaBehaviour other)
        {
            casilla = other;
        }
        public int CompareTo(CasillaPrioDefensa other)
        {
            int result = casilla.prioridadMilitar - other.casilla.prioridadMilitar;
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

        public CasillaBehaviour getCasilla()
        {
            return casilla;
        }

        //public override int GetHashCode()
        //{           
        //    return this
        //}
    }
}

