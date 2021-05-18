using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g02
{


public enum Type{ AZUL,AMARILLO,VERDE, NEUTRAL, VACIA }
public class Team : MonoBehaviour
{
    public Type team_;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Type myTeam()
    {
        return team_;
    }
}
}
