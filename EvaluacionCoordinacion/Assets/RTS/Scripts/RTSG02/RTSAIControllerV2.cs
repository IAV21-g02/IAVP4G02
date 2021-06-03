using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace es.ucm.fdi.iav.rts.g02
{
    //Nueva versión de la inteligencia artificial
    public class RTSAIControllerV2 : RTSAIController
    {
        private int MyIndex { get; set; }
        private ColorTeam myType;


        public int minDesiredExtractors = 2;
        public int minDesiredDestructors = 2; //TO DO: configurar
        public int minDesiredExplorers = 2; //TO DO: configurar

        // Mis listas completas de instalaciones y unidades
        private List<BaseFacility> MiBase;
        private List<ProcessingFacility> MiFactoria;
        private List<ExtractionUnit> MisExtractores;
        private List<ExplorationUnit> MisExploradores;
        private List<DestructionUnit> MisDestructores;

        // Las listas completas de instalaciones y unidades del enemigo
        private List<BaseFacility> BaseEnemiga;
        private List<ProcessingFacility> FactoriaEnemiga;
        private List<ExtractionUnit> ExtractoresEnemigos;
        private List<ExplorationUnit> ExploradoresEnemigos;
        private List<DestructionUnit> DestructoresEnemigos;

        private List<CasillaPrioDefensa> prioDefensa;
        private List<CasillaPrioAtaque> prioMilitar;

        // Las listas completas de accesos limitados y torretas 
        private List<LimitedAccess> Recursos;
        private List<Tower> Torretas;

        //Indice correspondiente a mi enemigo
        int FirstEnemyIndex;

        // Número de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;

        // Última unidad creada
        private Unidad LastUnit { get; set; }

        private void Awake()
        {
            Name = "IAV21G02V2";
            Author = "IAV21G02";
        }

        // El método de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {
            // Actualizo el mapa de influencia 
            
            // Para decidir sobre las órdenes se comprueba que tengo dinero suficiente y que se dan las condiciones que hagan falta...
            // (Ojo: lo suyo siempre es comprobar que cada llamada tiene sentido y es posible hacerla)

            // Aquí se intenta elegir bien la acción a realizar.  
            switch (ThinkStepNumber)
            {
                case 0: // Al inicio, en el primer paso de pensamiento
                    InitController();
                    break;

                case 1: // Durante toda la partida, en realidad
                    AIGameLoop();
                    break;
                case 2:
                    Stop = true;
                    break;

            }
        }

        public void Stopthinking()
        {
            ThinkStepNumber = 2;
        }

        private void InitController()
        {
            // Coger indice asignado por el gestor del juego
            MyIndex = RTSGameManager.Instance.GetIndex(this);
            myType = RTSGameManager.Instance.GetBaseFacilities(MyIndex)[0].GetComponent<UnitType>().getUnitType();
            //enemyType = RTSGameManager.Instance.GetBaseFacilities(FirstEnemyIndex)[0].GetComponent<UnitType>().getUnitType();
            // Obtengo referencias a las cosas de mi enemigo cogiendo la lista de indices
            //correspondientes a cada jugador
            var indexList = RTSGameManager.Instance.GetIndexes();
            //Quito mi indice de esa lista
            indexList.Remove(MyIndex);
            //Asumo que el primer indice es el de mi enemigo
            FirstEnemyIndex = indexList[0];

            // Obtengo lista de accesos limitados
            Recursos = RTSScenarioManager.Instance.LimitedAccesses;

            //Pasamos a AIGameLoop()
            ThinkStepNumber++;

            //batallones = new List<Batallon>();
            //unidadesSinBatallon = new List<Unit>();
            //misiones = new Priority_Queue<Mision>();
        }

        //TO DO: pueeees, todo, pa que nos vamos a engañar :D
        private void AIGameLoop()
        {
            // Como no es demasiado costoso, vamos a tomar las listas completas en cada paso de pensamiento
            UpdateGameElements();

            //TO DO:Mirar el mapa de influencia y ver que tareas necesitamos hacer con nuestros batallones

            //TO DO:Actualizar tareas que estan realizando nuestros batallones y reasignarselas si es necesario


            //TO DO: Si nos han sobrado tareas por realizar, tratamos de comprar mas batallones para encargarles dichas tareas
            //ShoppingManagement();

            //El bucle de juego termina cuando una de las bases es destruida 
            if ((BaseEnemiga == null || BaseEnemiga.Count == 0 || MiBase == null || MiBase.Count == 0))
            {
                Stopthinking();
            }
        }

        private void UpdateGameElements()
        {
            MiBase = RTSGameManager.Instance.GetBaseFacilities(MyIndex);
            MiFactoria = RTSGameManager.Instance.GetProcessingFacilities(MyIndex);
            MisExtractores = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
            MisExploradores = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
            MisDestructores = RTSGameManager.Instance.GetDestructionUnits(MyIndex);


            BaseEnemiga = RTSGameManager.Instance.GetBaseFacilities(FirstEnemyIndex);
            FactoriaEnemiga = RTSGameManager.Instance.GetProcessingFacilities(FirstEnemyIndex);
            ExtractoresEnemigos = RTSGameManager.Instance.GetExtractionUnits(FirstEnemyIndex);
            ExploradoresEnemigos = RTSGameManager.Instance.GetExplorationUnits(FirstEnemyIndex);
            DestructoresEnemigos = RTSGameManager.Instance.GetDestructionUnits(FirstEnemyIndex);

            Torretas = RTSScenarioManager.Instance.Towers;
        }

        public ColorTeam GetMyType()
        {
            return myType;
        }


        //  Actualiza las prioridades de defensa y ataque del mapa de influencias
    //    public void ActualizaPrioridades()
    //    {
    //        //-------------MILICIA------------//
    //        prioMilitar = new List<CasillaPrioAtaque>();
    //        foreach (DestructionUnit unit in DestructoresEnemigos)
    //        {
    //            CasillaBehaviour currCasilla = MapManager.GetInstance().GetCasillaCercana(unit.transform);
    //            CasillaPrioAtaque atq = currCasilla.GetCasillaPrioMilitar();
    //            atq.ActualizaAtaque();

    //            if (!prioMilitar.Contains(atq))
    //            {
    //                prioMilitar.Add(atq);
    //            }
    //        }

    //        foreach (ExplorationUnit unit in ExploradoresEnemigos)
    //        {
    //            CasillaBehaviour currCasilla = MapManager.GetInstance().GetCasillaCercana(unit.transform);
    //            CasillaPrioAtaque atq = currCasilla.GetCasillaPrioMilitar();
    //            atq.ActualizaAtaque();

    //            if (!prioMilitar.Contains(currCasilla.GetCasillaPrioMilitar()))
    //            {
    //                prioMilitar.Add(currCasilla.GetCasillaPrioMilitar());
    //            }
    //        }

    //        //------------DEFENSA---------------//
    //        prioDefensa = new List<CasillaPrioDefensa>();
    //        foreach (ExtractionUnit unit in MisExtractores)
    //        {
    //            CasillaBehaviour currCasilla = MapManager.GetInstance().GetCasillaCercana(unit.transform);
    //            CasillaPrioDefensa def = currCasilla.getCasillaPrioDefensa();
    //            def.ActualizaDefensa();

    //            if (!prioDefensa.Contains(def))
    //            {
    //                prioDefensa.Add(def);
    //            }
    //        }

    //        //Base
    //        CasillaPrioDefensa defAux = MapManager.GetInstance().GetCasillaCercana(MiBase[0].transform).getCasillaPrioDefensa();
    //        defAux.ActualizaDefensa();

    //        if (!prioDefensa.Contains(defAux))
    //        {
    //            prioDefensa.Add(defAux);
    //        }
            
    //        //Factoria
    //        defAux = MapManager.GetInstance().GetCasillaCercana(MiFactoria[0].transform).getCasillaPrioDefensa();
    //        defAux.ActualizaDefensa();

    //        if (!prioDefensa.Contains(defAux))
    //        {
    //            prioDefensa.Add(defAux);
    //        }

    //        //Ordenamiento de prioridades
    //        ComparerAtaque compATQ = new ComparerAtaque();
    //        prioMilitar.Sort(compATQ);
    //        ComparerDef compDEF = new ComparerDef();
    //        prioDefensa.Sort(compDEF);
    //    }
    }
}