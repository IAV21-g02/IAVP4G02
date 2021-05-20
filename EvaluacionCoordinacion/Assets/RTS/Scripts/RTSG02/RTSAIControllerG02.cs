using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace es.ucm.fdi.iav.rts.g02
{

    struct Batallon
    {
        public int numeroExploradores;
        public int numeroDestructores;
        public int numeroExtractores;
        List<ExtractionUnit> extractores;
        List<DestructionUnit> destructores;
        List<ExplorationUnit> exploradores;
        EstOfensiva ofensiva;
        EstDefensivo defensa;
        TipoBatallon tipoBatallon;
        public bool completado;
        public bool construyendo;

        public void agregaUnidad(DestructionUnit unidad)
        {
            destructores.Add(unidad);
        }
        public void agregaUnidad(ExtractionUnit unidad)
        {
            extractores.Add(unidad);
        }
        public void agregaUnidad(ExplorationUnit unidad)
        {
            exploradores.Add(unidad);
        }
    }

    public enum TipoBatallon
    {
        //  Dos destructores
        BatallonTiwardo,
        //  Dos exploradores y un destructor
        BatallonDobleDesayuno,
        //  Dos exploradores
        BatallonAurgar,

    }

    //  Estado que se encarga de gestionar la ofensiva de la IA
    public enum EstOfensiva
    {
        //  Ataque directo al nexo
        AtaqueAlNexo,
        //  Ataque a todo lo que este cercano a una melange(mina)
        AtaqueMelange,
        //  Ataque directo a la factoria
        AtaqueFactoria,
        //  Ataque a una concentraci�n de prioridad alta enemiga
        AtaqueMayorPrio,
        //  Ataque a las unidades neutrales (verdes)  
        AtaqueNeutral,
        //  Ataque a una concetraci�n de menor prioridad enemiga
        AtaqueMenorPrio,
        //  
        Festivo
    }

    //  Estado que se encarga de gestionar la defensiva de la IA
    public enum EstDefensivo
    {
        //  Defiende una mina y ataca a todo lo que se acerque
        DefiendeRecurso,
        //  Defiende a la base
        DefiendeBase,
        //  Defiende a la factoria
        DefiedeFactoria,
        //  Defiende a un extractor en concreto
        DefiendeExtractor,
        //  Defiende de una pos a otra
        Patrulla,
    }

    //  Estado que se encarga de gestionar las compras de la IA
    public enum EstCompras
    {
        Extractor,
        Exploradores,
        Destructores,
    }


    public class RTSAIControllerG02 : RTSAIController
    {
        private int MyIndex { get; set; }
        //private int FirstEnemyIndex { get; set; }
        //private BaseFacility MyFirstBaseFacility { get; set; }
        //private ProcessingFacility MyFirstProcessingFacility { get; set; }
        //private BaseFacility FirstEnemyFirstBaseFacility { get; set; }
        //private ProcessingFacility FirstEnemyFirstProcessingFacility { get; set; }

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

        // Las listas completas de accesos limitados y torretas 
        private List<LimitedAccess> Recursos;
        private List<Tower> Torretas;

        //Indice correspondiente a mi enemigo
        int FirstEnemyIndex;

        // N�mero de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;

        //  Empieza con la estrategia de farmeo intenso
        private EstOfensiva ofensiva;
        private EstDefensivo defensa;
        private EstCompras economia;
        private List<Batallon> batallones;

        private int dineroSuficiente = 60000;

        // �ltima unidad creada
        private Unidad LastUnit { get; set; }

        private void Awake()
        {
            Name = "IAV21G02";
            Author = "IAV21G02";
        }

        // El m�todo de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {
            // Actualizo el mapa de influencia 
            // ...

            // Para decidir sobre las �rdenes se comprueba que tengo dinero suficiente y que se dan las condiciones que hagan falta...
            // (Ojo: lo suyo siempre es comprobar que cada llamada tiene sentido y es posible hacerla)

            // Aqu� se intenta elegir bien la acci�n a realizar.  
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

            // Construyo por primera vez el mapa de influencia (con las 'capas' que necesite)
            // ...
            //�TO DO:?
        }

        //TO DO: pueeees, todo, pa que nos vamos a enga�ar :D
        private void AIGameLoop()
        {
            // Como no es demasiado costoso, vamos a tomar las listas completas en cada paso de pensamiento
            ActualizeGameElements();
            // TODO Analisis del juego para gestionar las posibles compras
            // TODO Mover unidades
            ShoppingManagement();

            #region CONTROLLER 3
            //// Variables auxiliares
            //int rand = 0;
            //int probability = 0;

            //// Escojo el enumerado de movimiento correspondiente a un �ndice que ir� variando
            //// (esto habr�a sido m�s elegante hacerlo con una lista, pero nos interesaba que fueran enumerados por si alguien los quiere usar desde fuera)
            //switch (Moves[nextMove])
            //{
            //    case PosibleMovement.MoveRandomExtraction:
            //        if (MisExtractores != null && MisExtractores.Count > 0)
            //        {
            //            // Mover unidades extractoras suele ser muy mala idea, por eso s�lo lo hago 1 de cada 10 veces
            //            probability = Random.Range(0, 10);
            //            if (probability == 0)
            //            {
            //                rand = Random.Range(0, MisExtractores.Count);
            //                RTSGameManager.Instance.MoveUnit(this, MisExtractores[rand], chooseObjective(MisExtractores[rand].transform));
            //                movedUnit = MisExtractores[rand]; // Por indicar lo que estoy moviendo
            //            }
            //        }
            //        break;
            //    case PosibleMovement.MoveAllExtraction:
            //        if (MisExtractores != null && MisExtractores.Count > 0)
            //        {
            //            // Mover unidades extractoras suele ser muy mala idea, por eso s�lo lo hago 1 de cada 10 veces
            //            probability = Random.Range(0, 10);
            //            if (probability == 0)
            //            {
            //                foreach (Unit x in MisExtractores)
            //                {
            //                    RTSGameManager.Instance.MoveUnit(this, x, chooseObjective(x.transform));
            //                    movedUnit = x; // Por indicar lo que estoy moviendo
            //                }
            //            }
            //        }
            //        break;
            //    case PosibleMovement.MoveLastExtraction:
            //        if (MisExtractores != null && MisExtractores.Count > 0)
            //        {
            //            // Mover unidades extractoras suele ser muy mala idea, por eso s�lo lo hago 1 de cada 10 veces
            //            probability = Random.Range(0, 10);
            //            if (probability == 0)
            //            {
            //                RTSGameManager.Instance.MoveUnit(this, MisExtractores[MisExtractores.Count - 1], chooseObjective(MisExtractores[MisExtractores.Count - 1].transform));
            //                movedUnit = MisExtractores[MisExtractores.Count - 1]; // Por indicar lo que estoy moviendo
            //            }
            //        }
            //        break;
            //    case PosibleMovement.MoveRandomExplorer:
            //        if (MisExploradores != null && MisExploradores.Count > 0)
            //        {
            //            rand = Random.Range(0, MisExploradores.Count);
            //            RTSGameManager.Instance.MoveUnit(this, MisExploradores[rand], chooseObjective(MisExploradores[rand].transform));
            //            movedUnit = MisExploradores[rand]; // Por indicar lo que estoy moviendo
            //        }
            //        break;
            //    case PosibleMovement.MoveAllExplorer:
            //        if (MisExploradores != null && MisExploradores.Count > 0)
            //        {
            //            foreach (Unit x in MisExploradores)
            //            {
            //                RTSGameManager.Instance.MoveUnit(this, x, chooseObjective(x.transform));
            //                movedUnit = x; // Por indicar lo que estoy moviendo
            //            }
            //        }
            //        break;
            //    case PosibleMovement.MoveLastExplorer:
            //        if (MisExploradores != null && MisExploradores.Count > 0)
            //        {
            //            RTSGameManager.Instance.MoveUnit(this, MisExploradores[MisExploradores.Count - 1], chooseObjective(MisExploradores[MisExploradores.Count - 1].transform));
            //            movedUnit = MisExploradores[MisExploradores.Count - 1]; // Por indicar lo que estoy moviendo
            //        }
            //        break;
            //    case PosibleMovement.MoveRandomDestroyer:
            //        if (MisDestructores != null && MisDestructores.Count > 0)
            //        {
            //            rand = Random.Range(0, MisDestructores.Count);
            //            RTSGameManager.Instance.MoveUnit(this, MisDestructores[rand], chooseObjective(MisDestructores[rand].transform));
            //            movedUnit = MisDestructores[rand]; // Por indicar lo que estoy moviendo
            //        }
            //        break;
            //    case PosibleMovement.MoveAllDestroyer:
            //        if (MisDestructores != null && MisDestructores.Count > 0)
            //        {
            //            foreach (Unit x in MisDestructores)
            //            {
            //                RTSGameManager.Instance.MoveUnit(this, x, chooseObjective(x.transform));
            //                movedUnit = x; // Por indicar lo que estoy moviendo
            //            }
            //        }
            //        break;
            //    case PosibleMovement.MoveLastDestroyer:
            //        if (MisDestructores != null && MisDestructores.Count > 0)
            //        {
            //            RTSGameManager.Instance.MoveUnit(this, MisDestructores[MisDestructores.Count - 1], chooseObjective(MisDestructores[MisDestructores.Count - 1].transform));
            //            movedUnit = MisDestructores[MisDestructores.Count - 1]; // Por indicar lo que estoy moviendo
            //        }
            //        break;

            //}

            //// Nuestra pol�tica es muy tonta: voy recorriendo todos los tipos de movimiento que conozco, haciendo uno cada vez
            //nextMove = (nextMove + 1) % Moves.Count;
            //// Con los objetivos, la pol�tica es igual de est�pida
            //nextObjective = (nextObjective + 1) % Objectives.Count;
            #endregion

            //El bucle de juego termina cuando una de las bases es destruida 
            if ((BaseEnemiga == null || BaseEnemiga.Count == 0 || MiBase == null || MiBase.Count == 0))
            {
                Stopthinking();
            }
        }

        private void ActualizeGameElements()
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

        private void ShoppingManagement()
        {
            // Si tengo alguna instalaci�n base y dinero para comprar la unidad que menos cuesta me puedo plantear comprar
            if (MiBase.Count > 0 && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExtractionUnitCost)
            {
                #region Emergencia
                //Si tenemos menos extractores del minimo deseado priorizamos el construirlos
                while (MisExtractores.Count < minDesiredExtractors && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExtractionUnitCost)
                {
                    RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION);
                }

                //Lo mismo con los destructores
                while (MisDestructores.Count < minDesiredDestructors && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.DestructionUnitCost)
                {
                    RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION);
                }

                //Lo mismo con los Exploradores
                while (MisExploradores.Count < minDesiredExplorers && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExplorationUnitCost)
                {
                    RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION);
                }
                #endregion

                //TO DO: darle un par de vueltas a esto
                //Una vez hemos priorizado el tener nuestro minimo de unidades, priorizamos tener mas unidades del enemigo
                //De estas, priorizaremos tener mas Exploradores, despues m�s destructores y por ultimo mas Extractores
                //if (MisExploradores.Count < ExploradoresEnemigos.Count + 2 && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExplorationUnitCost
                //    && MisExploradores.Count < RTSGameManager.Instance.ExplorationUnitsMax)
                //{
                //    RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION);
                //}
                //else if (MisDestructores.Count < DestructoresEnemigos.Count + 2 && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.DestructionUnitCost
                //    && MisDestructores.Count < RTSGameManager.Instance.DestructionUnitsMax)
                //{
                //    RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION);
                //}
                //else if ()
                //{
                //}

            }
        }
        private void gestionaCompra()
        {

        }

        private void gestionaDefensa()
        {
            if (batallones.Count == 0) return;

        }

        private void gestionaAtaque()
        {
            if (batallones.Count == 0) return;
        }
    }
}