/*    
   Copyright (C) 2021 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples) y Alejandro Ansón
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using UnityEngine;
using System.Collections.Generic;
namespace es.ucm.fdi.iav.rts
{
    /*
     * Ejemplo sobre cómo crear un controlador basado en IA para el minijuego RTS.
     * Manda unas órdenes variadas pero sin mucho sentido, para probar cosas... pero no usa puntos de ruta tácticos, ni realiza análisis táctico, ni coordina acciones de ningún tipo.
     */
    public class RTSAIControllerExample3 : RTSAIController
    {
        // El estilo para las etiquetas de la interfaz
        private GUIStyle _labelStyle;
        private GUIStyle _labelSmallStyle;

        // No necesita guardar mucha información porque puede consultar la que desee por sondeo,
        // incluida toda la información de instalaciones y unidades, tanto propias como ajenas...
        // ... sin embargo se establecen estos enumerados para poder manipular desde fuera esta IA (con algoritmos genéticos, por ejemplo)

        // Todos los posibles movimientos que puede decidir realizar esta IA
        public enum PosibleMovement
        {
            MoveRandomExtraction, MoveAllExtraction, MoveLastExtraction,
            MoveRandomExplorer, MoveAllExplorer, MoveLastExplorer,
            MoveRandomDestroyer, MoveAllDestroyer, MoveLastDestroyer
        }

        // Todos los posibles objetivos que puede poner a sus movimientos este IA
        public enum PosibleObjective
        {
            FurthestEnemyBase, FurthestEnemyProcesingFacility, ClosestEnemyBase,ClosestEnemyProcesingFacility,
            ClosestResource,FurthestResource,RandomResource,ClosestTower,FurthestTower,
            LastEnemyRandomUnit,LastEnemyDestroyer,LastEnemyExplorer,LastEnemyExtraction,
            ClosestEnemyRandomUnit,ClosestEnemyDestroyer,ClosestEnemyExplorer,ClosestEnemyExtraction,
            ClosestBase,ClosestProcesingFacility,FurthestBase,FurthestProcesingFacility,
            LastRandomUnit, LastDestroyer, LastExplorer, LastExtraction,
            ClosestRandomUnit, ClosestDestroyer, ClosestExplorer, ClosestExtraction
        }

        // Estos valores son los máximos personales que la IA considera que es razonable no superar en cuanto a número de unidades de cada tipo
        public int PersonalMaxExtractor;
        public int PersonalMaxExplorer;
        public int PersonalMaxDestroyer;

        [SerializeField]
        // Los movimientos que estarán disponibles de verdad
        public List<PosibleMovement> Moves;
        private int nextMove=0; // Índice para ir eligiendo enumerados de movimiento

        // La última unidad movida (por mi, de mi ejército)
        private Unit movedUnit;

        // Mi índice de controlador y unas cuantas instalaciones típicas para referenciar
        [SerializeField]
        // Los objetivos que estarán disponibles de verdad
        public List<PosibleObjective> Objectives;
        private int nextObjective = 0;
        private int MyIndex { get; set; }
        private int FirstEnemyIndex { get; set; }
        private BaseFacility MyFirstBaseFacility { get; set; }
        private ProcessingFacility MyFirstProcessingFacility { get; set; }
        private BaseFacility FirstEnemyFirstBaseFacility { get; set; }
        private ProcessingFacility FirstEnemyFirstProcessingFacility { get; set; }

        // Mis listas completas de instalaciones y unidades
        private List<BaseFacility> Facilities;
        private List<ProcessingFacility> PFacilities;
        private List<ExtractionUnit> UnitsExtractList;
        private List<ExplorationUnit> UnitsExploreList;
        private List<DestructionUnit> UnitsDestroyerList;

        // Las listas completas de instalaciones y unidades del enemigo
        private List<BaseFacility> EnemyFacilities;
        private List<ProcessingFacility> EnemyPFacilities;
        private List<ExtractionUnit> EnemyUnitsExtractList;
        private List<ExplorationUnit> EnemyUnitsExploreList;
        private List<DestructionUnit> EnemyUnitsDestroyerList;

        // Las listas completas de accesos limitados y torretas 
        private List<LimitedAccess> resourcesList;
        private List<Tower> towersList;

        // Número de paso de pensamiento 
        // (en realidad aquí se usa de forma tramposa: 0 es el inicio, 1 permanece durante toda la partida y 2 es parar)
        private int ThinkStepNumber { get; set; } = 0;

        // Última unidad creada
        private Unit LastUnit { get; set; }

        // Despierta el controlador y configura toda estructura interna que sea necesaria
        private void Awake()
        {
            Name = "Example 3";
            Author = "Alejandro Ansón";

            // Aumenta el tamaño y cambia el color de fuente de letra para OnGUI (amarillo para las IAs)
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 16;
            _labelStyle.normal.textColor = Color.yellow;

            _labelSmallStyle = new GUIStyle();
            _labelSmallStyle.fontSize = 11;
            _labelSmallStyle.normal.textColor = Color.yellow;
        }

        // El método de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {
            // Actualizo el mapa de influencia 
            // ...

            // Para decidir sobre las órdenes se comprueba que tengo dinero suficiente y que se dan las condiciones que hagan falta...
            // (Ojo: lo suyo siempre es comprobar que cada llamada tiene sentido y es posible hacerla)

            // Aquí se intenta elegir bien la acción a realizar.  
            switch (ThinkStepNumber)
            {
                case 0: // Al inicio, en el primer paso de pensamiento
                    // Lo primer es conocer el índice que me ha asignado el gestor del juego
                    MyIndex = RTSGameManager.Instance.GetIndex(this);

                    // Obtengo referencias a mis cosas (asumo que existen al comenzar, ojo! porque luego las puedo perder y que no estén)
                    MyFirstBaseFacility = RTSGameManager.Instance.GetBaseFacilities(MyIndex)[0];
                    MyFirstProcessingFacility = RTSGameManager.Instance.GetProcessingFacilities(MyIndex)[0];
                    // MyFirstBaseFacility
                    // ...

                    // Obtengo referencias a las cosas de mi enemigo
                    var indexList = RTSGameManager.Instance.GetIndexes();
                    indexList.Remove(MyIndex);
                    FirstEnemyIndex = indexList[0];
                    // Base Facility o otras cosas del enemigo, podría asumir que existen al inicio... pero luego pueden desaparecer
                    // ...

                    // Obtengo lista de accesos limitados
                    resourcesList = RTSScenarioManager.Instance.LimitedAccesses;

                    // Si en lugar de enumerados usásemos listas, se podrían barajar de forma aleatoria 
                    // tanto Moves como Objectives

                    ThinkStepNumber++;
                    // Construyo por primera vez el mapa de influencia (con las 'capas' que necesite)
                    // ...
                    break;

                case 1: // Durante toda la partida, en realidad

                    // Como no es demasiado costoso, vamos a tomar las listas completas en cada paso de pensamiento

                    Facilities = RTSGameManager.Instance.GetBaseFacilities(MyIndex);
                    PFacilities = RTSGameManager.Instance.GetProcessingFacilities(MyIndex);
                    UnitsExtractList = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
                    UnitsExploreList = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                    UnitsDestroyerList = RTSGameManager.Instance.GetDestructionUnits(MyIndex);

                    EnemyFacilities = RTSGameManager.Instance.GetBaseFacilities(FirstEnemyIndex);
                    EnemyPFacilities = RTSGameManager.Instance.GetProcessingFacilities(FirstEnemyIndex);
                    EnemyUnitsExtractList = RTSGameManager.Instance.GetExtractionUnits(FirstEnemyIndex);
                    EnemyUnitsExploreList = RTSGameManager.Instance.GetExplorationUnits(FirstEnemyIndex);
                    EnemyUnitsDestroyerList = RTSGameManager.Instance.GetDestructionUnits(FirstEnemyIndex);

                    towersList = RTSScenarioManager.Instance.Towers;

                    if (Facilities.Count > 0) // Si tengo alguna instalación base, me puedo plantear construir
                    {
                        // Intento crear una unidad en este orden: extractora, destructora y exploradora
                        if (UnitsExtractList.Count < PersonalMaxExtractor && UnitsExtractList.Count < RTSGameManager.Instance.ExtractionUnitsMax && RTSGameManager.Instance.GetMoney(MyIndex)>RTSGameManager.Instance.ExtractionUnitCost)
                        {
                            
                            RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                        }
                        if (UnitsDestroyerList.Count < PersonalMaxDestroyer && UnitsDestroyerList.Count < RTSGameManager.Instance.DestructionUnitsMax && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.DestructionUnitCost)
                        {
                            RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                        }
                        if (UnitsExploreList.Count < PersonalMaxExplorer && UnitsExploreList.Count < RTSGameManager.Instance.ExplorationUnitsMax && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExplorationUnitCost)
                        {
                            RTSGameManager.Instance.CreateUnit(this, MyFirstBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                        }
                        
                    }

                    // Variables auxiliares
                    int rand = 0;
                    int probability = 0;

                    // Escojo el enumerado de movimiento correspondiente a un índice que iré variando
                    // (esto habría sido más elegante hacerlo con una lista, pero nos interesaba que fueran enumerados por si alguien los quiere usar desde fuera)
                    switch (Moves[nextMove])
                    {
                        case PosibleMovement.MoveRandomExtraction:
                            if (UnitsExtractList != null && UnitsExtractList.Count > 0)
                            {
                                // Mover unidades extractoras suele ser muy mala idea, por eso sólo lo hago 1 de cada 10 veces
                                probability = Random.Range(0, 10);
                                if (probability == 0) { 
                                    rand =Random.Range(0, UnitsExtractList.Count);
                                    RTSGameManager.Instance.MoveUnit(this, UnitsExtractList[rand], chooseObjective(UnitsExtractList[rand].transform));
                                    movedUnit = UnitsExtractList[rand]; // Por indicar lo que estoy moviendo
                                }
                            }
                                break;
                        case PosibleMovement.MoveAllExtraction:
                            if (UnitsExtractList != null && UnitsExtractList.Count > 0)
                            {
                                // Mover unidades extractoras suele ser muy mala idea, por eso sólo lo hago 1 de cada 10 veces
                                probability = Random.Range(0, 10);
                                if (probability == 0)
                                { 
                                    foreach (Unit x in UnitsExtractList) { 
                                        RTSGameManager.Instance.MoveUnit(this, x, chooseObjective(x.transform));
                                        movedUnit = x; // Por indicar lo que estoy moviendo
                                    }
                                }
                            }
                            break;
                        case PosibleMovement.MoveLastExtraction:
                            if (UnitsExtractList != null && UnitsExtractList.Count > 0)
                            {
                                // Mover unidades extractoras suele ser muy mala idea, por eso sólo lo hago 1 de cada 10 veces
                                probability = Random.Range(0, 10);
                                if (probability == 0)
                                {
                                    RTSGameManager.Instance.MoveUnit(this, UnitsExtractList[UnitsExtractList.Count-1], chooseObjective(UnitsExtractList[UnitsExtractList.Count - 1].transform));
                                    movedUnit = UnitsExtractList[UnitsExtractList.Count - 1]; // Por indicar lo que estoy moviendo
                                }
                            }
                            break;
                        case PosibleMovement.MoveRandomExplorer:
                            if (UnitsExploreList != null && UnitsExploreList.Count > 0)
                            {
                                rand = Random.Range(0, UnitsExploreList.Count);
                                RTSGameManager.Instance.MoveUnit(this, UnitsExploreList[rand], chooseObjective(UnitsExploreList[rand].transform));
                                movedUnit = UnitsExploreList[rand]; // Por indicar lo que estoy moviendo
                            }
                            break;
                        case PosibleMovement.MoveAllExplorer:
                            if (UnitsExploreList != null && UnitsExploreList.Count > 0)
                            {
                                foreach (Unit x in UnitsExploreList)
                                {
                                    RTSGameManager.Instance.MoveUnit(this, x, chooseObjective(x.transform));
                                    movedUnit = x; // Por indicar lo que estoy moviendo
                                }
                            }
                            break;
                        case PosibleMovement.MoveLastExplorer:
                            if (UnitsExploreList != null && UnitsExploreList.Count > 0)
                            {
                                RTSGameManager.Instance.MoveUnit(this, UnitsExploreList[UnitsExploreList.Count-1], chooseObjective(UnitsExploreList[UnitsExploreList.Count-1].transform));
                                movedUnit = UnitsExploreList[UnitsExploreList.Count - 1]; // Por indicar lo que estoy moviendo
                            }
                            break;
                        case PosibleMovement.MoveRandomDestroyer:
                            if (UnitsDestroyerList != null && UnitsDestroyerList.Count > 0)
                            {
                                rand = Random.Range(0, UnitsDestroyerList.Count);
                                RTSGameManager.Instance.MoveUnit(this, UnitsDestroyerList[rand], chooseObjective(UnitsDestroyerList[rand].transform));
                                movedUnit = UnitsDestroyerList[rand]; // Por indicar lo que estoy moviendo
                            }
                            break;
                        case PosibleMovement.MoveAllDestroyer:
                            if (UnitsDestroyerList != null && UnitsDestroyerList.Count > 0)
                            {
                                foreach (Unit x in UnitsDestroyerList)
                                {
                                    RTSGameManager.Instance.MoveUnit(this, x, chooseObjective(x.transform));
                                    movedUnit = x; // Por indicar lo que estoy moviendo
                                }
                            }
                            break;
                        case PosibleMovement.MoveLastDestroyer:
                            if (UnitsDestroyerList != null && UnitsDestroyerList.Count > 0)
                            {
                                RTSGameManager.Instance.MoveUnit(this, UnitsDestroyerList[UnitsDestroyerList.Count - 1], chooseObjective(UnitsDestroyerList[UnitsDestroyerList.Count - 1].transform));
                                movedUnit = UnitsDestroyerList[UnitsDestroyerList.Count - 1]; // Por indicar lo que estoy moviendo
                            }
                            break;

                    }

                    // Nuestra política es muy tonta: voy recorriendo todos los tipos de movimiento que conozco, haciendo uno cada vez
                    nextMove = (nextMove + 1) % Moves.Count;
                    // Con los objetivos, la política es igual de estúpida
                    nextObjective = (nextObjective + 1) % Objectives.Count;

                    // Aquí se comprueba que hayamos acabado con absolutamente todo el ejército enemigo, para descansar
                    // A veces comprobábamos si EnemyFacilities[0].Health.Amount <=0 por si no había sido destruida por error
                    // Bastaría con que las EnemyFacilities estén destruidas, porque se supone que ahí acaba el juego
                    if ((EnemyFacilities == null ||  EnemyFacilities.Count == 0) &&
                        (EnemyPFacilities == null || EnemyPFacilities.Count == 0) &&
                        (EnemyUnitsExtractList == null || EnemyUnitsExtractList.Count == 0) &&
                        (EnemyUnitsExploreList == null || EnemyUnitsExploreList.Count == 0) &&
                        (EnemyUnitsDestroyerList == null || EnemyUnitsDestroyerList.Count == 0))
                    {
                        // Realmente aquí el juego habría acabado y no tiene sentido seguir haciendo cosas
                        Stopthinking();
                    }

                    break;
                case 2:
                    Stop = true;
                    break;

            }
        }

        // Recibe la transformada de un objeto y devuelve un objetivo RELATIVO a esa transformada
        protected Transform chooseObjective(Transform from)
        {
            Transform ObjectiveTrans=from;
            PosibleObjective ob= Objectives[nextObjective];
            GameObject help;

            // Variables auxiliares
            bool found = false;
            int rand = 0;

            // Escojo el enumerado de objetivo correspondiente a un índice que iré variando
            // (esto habría sido más elegante hacerlo con una lista, pero nos interesaba que fueran enumerados por si alguien los quiere usar desde fuera)
            switch (ob)
            {
                case PosibleObjective.FurthestEnemyBase:
                    if (EnemyFacilities != null && EnemyFacilities.Count > 0){
                        
                        help = getCloseOrFarObj(EnemyFacilities.ToArray(), from, false);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.FurthestEnemyProcesingFacility:
                    if (EnemyPFacilities != null && EnemyPFacilities.Count > 0){
                        
                        help = getCloseOrFarObj(EnemyPFacilities.ToArray(), from, false);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestEnemyBase:
                    if (EnemyFacilities != null && EnemyFacilities.Count > 0){
                        
                        help = getCloseOrFarObj(EnemyFacilities.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestEnemyProcesingFacility:
                    if (EnemyPFacilities != null && EnemyPFacilities.Count > 0){
                        
                        help = getCloseOrFarObj(EnemyPFacilities.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestResource:
                    // Enviar una unidad que no sea extractora a un recurso no suele ser habitual... podríamos reducir su probabilidad
                    if (resourcesList != null && resourcesList.Count > 0)
                    {
                        help = getCloseOrFarObj(resourcesList.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.RandomResource:
                    // Enviar una unidad que no sea extractora a un recurso no suele ser habitual... podríamos reducir su probabilidad
                    if (resourcesList != null && resourcesList.Count > 0)
                    {
                        rand = Random.Range(0, resourcesList.Count);
                        
                        ObjectiveTrans = resourcesList[rand].transform;
                    }
                    break;
                case PosibleObjective.FurthestResource:
                    // Enviar una unidad que no sea extractora a un recurso no suele ser habitual... podríamos reducir su probabilidad
                    if (resourcesList != null && resourcesList.Count > 0) { 
                        
                        help = getCloseOrFarObj(resourcesList.ToArray(), from, false);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestTower:
                    // Enviar una unidad extractora a una torre es muy mala idea... podríamos reducir su probabilidad
                    if (towersList != null && towersList.Count > 0){
                        
                        help = getCloseOrFarObj(towersList.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.FurthestTower:
                    // Enviar una unidad extractora a una torre es muy mala idea... podríamos reducir su probabilidad
                    if (towersList != null && towersList.Count > 0){
                        
                        help = getCloseOrFarObj(towersList.ToArray(), from, false);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.LastEnemyRandomUnit:
                    // Aquí se podría ver primero en qué lista NO quedan unidades... y de donde SÍ queden, es cuando elegiríamos al azar
                    found = false; 
                        rand = Random.Range(0, 3);
                        if (rand == 0)
                        {
                            if (EnemyUnitsDestroyerList != null && EnemyUnitsDestroyerList.Count > 0)
                            {
                                ObjectiveTrans = EnemyUnitsDestroyerList[EnemyUnitsDestroyerList.Count - 1].transform;
                                found = true;
                            }
                        }
                        if (!found || rand == 1 )
                        {
                            if (EnemyUnitsExploreList != null && EnemyUnitsExploreList.Count > 0)
                            {
                                ObjectiveTrans = EnemyUnitsExploreList[EnemyUnitsExploreList.Count - 1].transform;
                                found = true;
                            }
                        }
                        if (!found || rand == 2)
                        {
                            if (EnemyUnitsExtractList != null && EnemyUnitsExtractList.Count > 0)
                            {
                                ObjectiveTrans = EnemyUnitsExtractList[EnemyUnitsExtractList.Count - 1].transform;
                            }
                        }
                    
                    break;
                case PosibleObjective.LastEnemyDestroyer:
                    if (EnemyUnitsDestroyerList != null && EnemyUnitsDestroyerList.Count > 0)
                    {
                        ObjectiveTrans = EnemyUnitsDestroyerList[EnemyUnitsDestroyerList.Count - 1].transform;
                    }
                    break;
                case PosibleObjective.LastEnemyExplorer:
                    if (EnemyUnitsExploreList != null && EnemyUnitsExploreList.Count > 0)
                    {
                        ObjectiveTrans = EnemyUnitsExploreList[EnemyUnitsExploreList.Count - 1].transform;
                    }
                    break;
                case PosibleObjective.LastEnemyExtraction:
                    if (EnemyUnitsExtractList != null && EnemyUnitsExtractList.Count > 0)
                    {
                        ObjectiveTrans = EnemyUnitsExtractList[EnemyUnitsExtractList.Count - 1].transform;
                    }
                    break;
                case PosibleObjective.ClosestEnemyRandomUnit:
                    // Aquí se podría ver primero en qué lista NO quedan unidades... y de donde SÍ queden, es cuando elegiríamos al azar
                    found = false;
                        rand = Random.Range(0, 3);
                        if (rand == 0)
                        {
                            if (EnemyUnitsDestroyerList != null && EnemyUnitsDestroyerList.Count > 0)
                            {
                                help = getCloseOrFarObj(EnemyUnitsDestroyerList.ToArray(), from, true);
                                ObjectiveTrans = help.transform;
                                found = true;
                            }
                        }
                        if (!found || rand == 1)
                        {
                            if (EnemyUnitsExploreList != null && EnemyUnitsExploreList.Count > 0)
                            {
                                help = getCloseOrFarObj(EnemyUnitsExploreList.ToArray(), from, true);
                                ObjectiveTrans = help.transform;
                                found = true;
                            }
                        }
                        if (!found || rand == 2)
                        {
                            if (EnemyUnitsExtractList != null && EnemyUnitsExtractList.Count > 0)
                            {
                                help = getCloseOrFarObj(EnemyUnitsExtractList.ToArray(), from, true);
                                ObjectiveTrans = help.transform;
                            }
                        }
                    

                    break;
                case PosibleObjective.ClosestEnemyDestroyer:
                    if (EnemyUnitsDestroyerList != null && EnemyUnitsDestroyerList.Count > 0){
                        
                        help = getCloseOrFarObj(EnemyUnitsDestroyerList.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestEnemyExplorer:
                    if (EnemyUnitsExploreList != null && EnemyUnitsExploreList.Count > 0){
                        
                        help = getCloseOrFarObj(EnemyUnitsExploreList.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestEnemyExtraction:
                    if (EnemyUnitsExtractList != null && EnemyUnitsExtractList.Count > 0){
                        
                        help = getCloseOrFarObj(EnemyUnitsExtractList.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestBase:
                    if (Facilities != null && Facilities.Count > 0){
                        
                        help = getCloseOrFarObj(Facilities.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestProcesingFacility:
                    if (PFacilities != null && PFacilities.Count > 0){
                        
                        help = getCloseOrFarObj(PFacilities.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.FurthestBase:
                    if (Facilities != null && Facilities.Count > 0){
                        
                        help = getCloseOrFarObj(Facilities.ToArray(), from, false);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.FurthestProcesingFacility:
                    if (PFacilities != null && PFacilities.Count > 0){
                        
                        help = getCloseOrFarObj(PFacilities.ToArray(), from, false);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.LastRandomUnit:
                    // Aquí se podría ver primero en qué lista NO quedan unidades... y de donde SÍ queden, es cuando elegiríamos al azar
                    found = false;
                        rand = Random.Range(0, 3);
                        if (rand == 0)
                        {
                            if (UnitsDestroyerList != null && UnitsDestroyerList.Count > 0)
                            {
                                ObjectiveTrans = UnitsDestroyerList[UnitsDestroyerList.Count - 1].transform;
                                found = true;
                            }
                        }
                        if (!found || rand == 1)
                        {
                            if (UnitsExploreList != null && UnitsExploreList.Count > 0)
                            {
                                ObjectiveTrans = UnitsExploreList[UnitsExploreList.Count - 1].transform;
                                found = true;
                            }
                        }
                        if (!found || rand == 2)
                        {
                            if (UnitsExtractList != null && UnitsExtractList.Count > 0)
                            {
                                ObjectiveTrans = UnitsExtractList[UnitsExtractList.Count - 1].transform; 
                            }
                        }
                    
                    break;
                case PosibleObjective.LastDestroyer:
                    if (UnitsDestroyerList != null && UnitsDestroyerList.Count > 0)
                    {
                        ObjectiveTrans = UnitsDestroyerList[UnitsDestroyerList.Count - 1].transform;
                    }
                    break;
                case PosibleObjective.LastExplorer:
                    if (UnitsExploreList != null && UnitsExploreList.Count > 0)
                    {
                        ObjectiveTrans = UnitsExploreList[UnitsExploreList.Count - 1].transform;
                    }
                    break;
                case PosibleObjective.LastExtraction:
                    if (UnitsExtractList != null && UnitsExtractList.Count > 0)
                    {
                        ObjectiveTrans = UnitsExtractList[UnitsExtractList.Count - 1].transform;
                    }
                    break;
                case PosibleObjective.ClosestRandomUnit:
                    // Aquí se podría ver primero en qué lista NO quedan unidades... y de donde SÍ queden, es cuando elegiríamos al azar
                    found = false;
                        rand = Random.Range(0, 3);
                        if (rand == 0)
                        {
                            if (UnitsDestroyerList != null && UnitsDestroyerList.Count > 0)
                            {
                                help = getCloseOrFarObj(UnitsDestroyerList.ToArray(), from, true);
                                ObjectiveTrans = help.transform;
                                found = true;
                            }
                        }
                        if (!found || rand == 1)
                        {
                            if (UnitsExploreList != null && UnitsExploreList.Count > 0)
                            {
                                help = getCloseOrFarObj(UnitsExploreList.ToArray(), from, true);
                                ObjectiveTrans = help.transform;
                                found = true;
                            }
                        }
                        if (!found || rand == 2)
                        {
                            if (UnitsExtractList != null && UnitsExtractList.Count > 0)
                            {
                                help = getCloseOrFarObj(UnitsExtractList.ToArray(), from, true);
                                ObjectiveTrans = help.transform; 
                            }
                        }
                    
                    break;
                case PosibleObjective.ClosestDestroyer:
                    if (UnitsDestroyerList != null && UnitsDestroyerList.Count > 0){
                        
                        help = getCloseOrFarObj(UnitsDestroyerList.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestExplorer:
                    if (UnitsExploreList != null && UnitsExploreList.Count > 0){
                        
                        help = getCloseOrFarObj(UnitsExploreList.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;
                case PosibleObjective.ClosestExtraction:
                    if (UnitsExtractList != null && UnitsExtractList.Count > 0){
                        
                        help = getCloseOrFarObj(UnitsExtractList.ToArray(), from, true);
                        ObjectiveTrans = help.transform;
                    }
                    break;

            }

            return ObjectiveTrans;
        }

        // Dibuja la interfaz gráfica de usuario para que se vea la información en pantalla
        private void OnGUI()
        {
            // Abrimos un área de distribución arriba y a la izquierda (si el índice del controlador es par) o a la derecha (si el índice es impar), con contenido en vertical
            float areaWidth = 150;
            float areaHeight = 250;
            if (MyIndex % 2 == 0)
                GUILayout.BeginArea(new Rect(0, 0, areaWidth, areaHeight));
            else
                GUILayout.BeginArea(new Rect(Screen.width - areaWidth, 0, Screen.width, areaHeight));
            GUILayout.BeginVertical();

            // Lista las variables importantes como el índice del jugador y su cantidad de dinero
            GUILayout.Label("[ C" + MyIndex + " ] " + RTSGameManager.Instance.GetMoney(MyIndex) + " solaris", _labelStyle);

            // Aunque no exista el concepto de unidad seleccionada, podríamos mostrar cual ha sido la última en moverse
            if (movedUnit != null)
                // Una etiqueta para indicar la última unidad movida, si la hay
                GUILayout.Label(movedUnit.gameObject.name + " moved", _labelSmallStyle);

            // Cerramos el área de distribución con contenido en vertical
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public void Stopthinking()
        {
            ThinkStepNumber = 2;
        }

        // Método auxiliar que sirve para devolver tanto el objeto más cercano como el más lejano a una transformada 'from'
        protected GameObject getCloseOrFarObj(MonoBehaviour[] list, Transform from, bool close)
        {
            int it = 1;
            int maxIt = 0;
            int minIt = 0;
            float maxDistance = Vector3.Distance( list[0].transform.position,from.position);
            float minDistance = Vector3.Distance(list[0].transform.position, from.position);

            // No es una implementación muy eficiente porque mido distancias con respecto a todos los objetos de la lista
            // (y realmente no haría falta estar recalculando esto tantas veces... se debería cachear, o incluso recalcular sólo cada varios ciclos
            while (it<list.Length)
            {
                float distance= Vector3.Distance(list[it].transform.position, from.position);
                if(distance>maxDistance)
                {
                    maxIt = it;
                    maxDistance = distance;
                }
                if (distance < minDistance)
                {
                    minIt = it;
                    minDistance = distance;
                }
                it++;
            }
            if (close)
                return list[minIt].gameObject;
            else
                return list[maxIt].gameObject;
        }

    }
}