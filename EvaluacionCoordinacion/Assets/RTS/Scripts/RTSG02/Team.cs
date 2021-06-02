using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g02
{


public enum ColorTeam{ AZUL,AMARILLO,VERDE, NEUTRAL, VACIA }


public class Team : MonoBehaviour
{
    public ColorTeam team_;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public ColorTeam myTeam()
    {
        return team_;
    }
}
}
