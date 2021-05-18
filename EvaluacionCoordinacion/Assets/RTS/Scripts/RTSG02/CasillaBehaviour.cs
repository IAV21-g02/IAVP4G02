using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace es.ucm.fdi.iav.rts.g02
{

  
    public class CasillaBehaviour : MonoBehaviour
    {
        private Type team_;
        private int prioridadMilitar;
        private int prioridadDefensa;
        private int fil;
        private int col;
        // Start is called before the first frame update
        void Start()
        {
            prioridadMilitar = 0;
            prioridadDefensa = 0;
            team_ = Type.VACIA;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            Team t = other.gameObject.GetComponentInParent<Team>();
            if (t)
            {
                switch (t.myTeam())
                {
                    case Type.AZUL:
                        if(team_ == Type.VACIA) team_ = Type.AZUL;
                        if (other.gameObject.GetComponent<ProcessingFacility>())
                        {


                        }
                        else if (other.gameObject.GetComponent<ExplorationUnit>())
                        {

                        }
                        else if (other.gameObject.GetComponent<DestructionUnit>())
                        {

                        }
                        else if (other.gameObject.GetComponent<ExtractionUnit>())
                        {

                        }


                        break;
                    case Type.AMARILLO:
                        team_ = Type.AMARILLO;


                        break;
                    case Type.VERDE:
                        team_ = Type.VERDE;

                      

                        break;
                }
            }

        }

        private void OnTriggerExit(Collider other)
        {
            
        }

        private void OnTriggerStay(Collider other)
        {
            
        }

        public void setMatrixPos(int x, int y)
        {
            fil = x;
            col = y;
        }

    }
};
