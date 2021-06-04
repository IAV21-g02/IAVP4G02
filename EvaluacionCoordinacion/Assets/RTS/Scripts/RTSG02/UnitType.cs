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

        //Referencia a la casilla en la que nos encontrabamos en la iteracion anterior
        private CasillaBehaviour curCasilla;
        //Referencia a la casilla en la que nos encontramos en la iteracion actual
        private CasillaBehaviour prevCasilla;


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

        //Gestión del movimiento de las unidades para actualizar el mapa de influencia
        private void Update()
        {
            curCasilla = MapManager.GetInstance().GetCasillaCercana(transform);

            //Ha habido cambio de casilla
            if(prevCasilla != null && curCasilla != prevCasilla)
            {
                MapManager.GetInstance().ActualizaPrioridadAlSalir(prevCasilla, this);
                MapManager.GetInstance().ActualizaPrioridadAlEntrar(curCasilla, this);
            }
            //Si la prevCasilla es null, significa que estamos en la primera iteración del bucle
            else if(prevCasilla == null) MapManager.GetInstance().ActualizaPrioridadAlEntrar(curCasilla, this);

            prevCasilla = curCasilla;

        }

        private void OnDestroy()
        {
            if (prevCasilla)
            {
                //Cuando se destruye esta entidad hay que quitar los valores de influencia de la misma en el mapa
                MapManager.GetInstance().ActualizaPrioridadAlSalir(prevCasilla, this);  
            }
        }

    }
};
