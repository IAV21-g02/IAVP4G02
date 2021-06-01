using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g02
{
    public enum Unidad
    {
        MILITAR, DEFENSA
    }

    public class UnitType : MonoBehaviour
    {
        [Tooltip("Dueño de esta unidad")]
        public ColorTeam unitOwner;
        [Tooltip("Tipo de unidad")]
        public Unidad unit;
        [Tooltip("Cantidad de puntos de influencia de esta unidad")]
        public int influencia;
        [Tooltip("Rango de influencia de esta unidad")]
        public int rango = 0;

        //  Constructor por copia
        public UnitType(UnitType unitCopy)
        {
            unitOwner = unitCopy.unitOwner;
            unit = unitCopy.unit;
            influencia = unitCopy.influencia;
            rango = unitCopy.rango;
        }

        public ColorTeam getUnitType()
        {
            return unitOwner;
        }
    }
};
