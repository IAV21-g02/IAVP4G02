using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g02
{
    public enum Unidad
    {
        MILITAR,DEFENSA
    }
    public class UnitType : MonoBehaviour
    {
        [Tooltip("Dueño de esta unidad")]
        public Type unitOwner;
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


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Casilla"))
            {
                CasillaBehaviour casilla = other.GetComponent<CasillaBehaviour>();
                MapManager.getInstance().actualizaPrioridadAlEntrar(casilla, this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Casilla"))
            {
                CasillaBehaviour casilla = other.GetComponent<CasillaBehaviour>();
                MapManager.getInstance().actualizaPrioridadAlSalir(casilla, this);
            }
        }
    }
};
