using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;


namespace es.ucm.fdi.iav.rts.g02
{
    class Mision : IComparable<Mision>
    {
        // misión que debe cumplir este batallon
        Comando actMision;
        //  Transform de la posición a la que ir
        Transform objetivo;
        //  Priridad que tiene esta misión
        int prio;
        // Determina si esta misión se ha completado
        public bool misionStatus = false;
        //  Tipo de estrategía que sigue este batallón
        Estrategia estrategia;

        //  Crea una misión
        public Mision(Comando cmd_ , Transform objetivo_ ,int prio_, Estrategia estrategia_)
        {
            this.actMision = cmd_;
            this.objetivo = objetivo_;
            this.prio = prio_;
            this.estrategia = estrategia_;
        }

        public int CompareTo(Mision other)
        {
            int result = prio - other.prio;

            if (this.actMision.Equals(other.actMision) && this.objetivo.Equals(other.objetivo) && result == 0)
                return 0;
            else return result;
        }

        public bool Equals(Mision other)
        {
            return (this.actMision.Equals(other.actMision) && this.objetivo.Equals(other.objetivo) && this.prio.Equals(other.prio));
        }

        //public override int GetHashCode()
        //{           
        //    return this
        //}

        public override bool Equals(object obj)
        {
            Mision other = (Mision)obj;
            return Equals(other);
        }

        //  Asigna un objetivo a esta misión
        public void asignaObjetivo(Transform objetivo_)
        {
            objetivo = objetivo_;
        }

        //  Asigna a esta misión como completada
        public bool misionCompletada()
        {
            return misionStatus;
        }

        public Transform getObjetivo()
        {
            return objetivo;
        }

        public Estrategia getEstrategia()
        {
            return estrategia;
        }

    }
    struct Batallon
    {
        //  Número de exploradores que necesita este batallón
        public int numeroExploradores;
        //  Número de destructores que necesita este batallón
        public int numeroDestructores;
        //  Número de extractores que necesita este batallón
        public int numeroExtractores;

        //  Lista de extractores de este batallon
        List<ExtractionUnit> extractores;
        //  Lista de extractores de este batallon
        List<DestructionUnit> destructores;
        //  Lista de extractores de este batallon
        List<ExplorationUnit> exploradores;

        //  Misión de este batallon
        Mision mision;
        //  Tipo de batallon
        TipoBatallon tipoBatallon;

        //  Determina si este batallon termino de construirse
        public bool completado;
        //  Determina si este batallon está en una misión
        public bool enMision;
        //  Rango que determina si las unidades han llegado a su objetivo
        private float rangoPosicion;
        //  Determina si un batallón está en movimiento
        public bool enMovimiento;

        // Construye un batallón con un tipo de batallón y una misión
        public Batallon(TipoBatallon tipoBatallon_,Mision currMision)
        {
            tipoBatallon = tipoBatallon_;
            completado = false;
            enMision = false;
            rangoPosicion = 8;
            enMovimiento = false;

            extractores = new List<ExtractionUnit>();
            destructores = new List<DestructionUnit>();
            exploradores = new List<ExplorationUnit>();

            numeroDestructores = 0;
            numeroExploradores = 0;
            numeroExtractores = 0;

            mision = currMision;

            switch (tipoBatallon_)
            {
                case TipoBatallon.BatallonTiwardo:
                    numeroDestructores = 2;
                    numeroExploradores = 0;
                    numeroExtractores = 0;
                    break;
                case TipoBatallon.BatallonDobleDesayuno:
                    numeroDestructores = 1;
                    numeroExploradores = 2;
                    numeroExtractores = 0;
                    break;
                case TipoBatallon.BatallonAurgar:
                    numeroDestructores = 0;
                    numeroExploradores = 2;
                    numeroExtractores = 0;
                    break;
                default:
                    break;
            }
        }

        //  Construye un batallon en función de un tipo de batallón
        public void creaBatallon(TipoBatallon tipoBatallon_)
        {
            tipoBatallon = tipoBatallon_;
            completado = false;
            enMision = false;

            extractores = new List<ExtractionUnit>();
            destructores = new List<DestructionUnit>();
            exploradores = new List<ExplorationUnit>();

            switch (tipoBatallon_)
            {
                case TipoBatallon.BatallonTiwardo:
                    numeroDestructores = 2;
                    numeroExploradores = 0;
                    numeroExtractores = 0;
                    break;
                case TipoBatallon.BatallonDobleDesayuno:
                    numeroDestructores = 1;
                    numeroExploradores = 2;
                    numeroExtractores = 0;
                    break;
                case TipoBatallon.BatallonAurgar:
                    numeroDestructores = 0;
                    numeroExploradores = 2;
                    numeroExtractores = 0;
                    break;
                default:
                    break;
            }
        }
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
        public void agregaUnidad(Unit unidad)
        {
            ExplorationUnit expl = unidad.GetComponent<ExplorationUnit>();
            if (expl)
            {
                exploradores.Add(expl);
                return;
            }
            DestructionUnit dest = unidad.GetComponent<DestructionUnit>();
            if (dest)
            {
                destructores.Add(dest);
                return;
            }
            ExtractionUnit ext = unidad.GetComponent<ExtractionUnit>();
            if (ext)
            {
                extractores.Add(ext);
                return;
            }
        }

        //  Asigna una nueva misión a un batallón
        public void asignaMision(Mision nueMision)
        {
            mision = nueMision;
            enMision = true;
        }

        // desmonta un batallón para agregarlo a una lista de unidades sin batallón
        public void desmontarBatallon(List<Unit> unidadesSinBatallon, List<Batallon> batallones)
        {
            foreach (ExtractionUnit unit_ in extractores.ToArray())
            {
                unidadesSinBatallon.Add(unit_);
                extractores.Remove(unit_);
            }

            foreach (DestructionUnit unit_ in destructores.ToArray())
            {
                unidadesSinBatallon.Add(unit_);
                destructores.Remove(unit_);
            }

            foreach (ExplorationUnit unit_ in exploradores.ToArray())
            {
                unidadesSinBatallon.Add(unit_);
                exploradores.Remove(unit_);
            }

            batallones.Remove(this);
        }

        //  Mueve a todas las unidades de este batallón a una posición
        //public void movilizaBatallon(Transform pos)
        //{
        //    foreach (ExplorationUnit explorador in exploradores)
        //    {
        //        explorador.GetComponent<NavMeshAgent>().SetDestination(pos.position);
        //    }

        //    foreach (ExtractionUnit extractor in extractores)
        //    {
        //        extractor.GetComponent<NavMeshAgent>().SetDestination(pos.position);
        //    }

        //    foreach (DestructionUnit destructor in destructores)
        //    {
        //        destructor.GetComponent<NavMeshAgent>().SetDestination(pos.position);
        //    }
        //}

        //  Mueve a todas las unidades de este batallón al objetivo que tiene asignado como misión
        public void movilizaBatallon(RTSAIControllerG02 rts)
        {

            foreach (ExplorationUnit explorador in exploradores)
            {
                if ((mision.getObjetivo().position - explorador.transform.position).magnitude < rangoPosicion)
                {
                    RTSGameManager.Instance.MoveUnit(rts, explorador, mision.getObjetivo().position);
                }
                else
                {
                    RTSGameManager.Instance.StopUnit(rts, explorador);
                }
            }

            foreach (ExtractionUnit extractor in extractores)
            {
                if ((extractor.transform.position - mision.getObjetivo().position).magnitude < rangoPosicion)
                {
                    RTSGameManager.Instance.MoveUnit(rts, extractor, mision.getObjetivo().position);
                }
                else
                {
                    RTSGameManager.Instance.StopUnit(rts, extractor);
                }
            }

            foreach (DestructionUnit destructor in destructores)
            {
                if ((destructor.transform.position - mision.getObjetivo().position).magnitude < rangoPosicion)
                {
                    RTSGameManager.Instance.MoveUnit(rts, destructor, mision.getObjetivo().position);

                }
                else
                {
                    RTSGameManager.Instance.StopUnit(rts, destructor);
                }
            }
        }

        public Mision getMision()
        {
            return mision;
        }

        public Estrategia getEstrategia()
        {
            return mision.getEstrategia();
        }

    }

    // Tipos de batallones que se pueden crear
    public enum TipoBatallon
    {
        //  Dos destructores
        BatallonTiwardo,
        //  Dos exploradores y un destructor
        BatallonDobleDesayuno,
        //  Dos exploradores
        BatallonAurgar, 
        //  Batallón con todas las unidades
        BatallonPuerco
    }

    //  Estado que se encarga de gestionar la ofensiva de la IA
    public enum Comando
    {

        #region Comandos_Ofensivos
        //  Ataque directo al nexo
        AtaqueAlNexo,
        //  Ataque a todo lo que este cercano a una melange(mina) con mayor prio
        AtaqueMelangeMayorPrio,
        //  Ataque a todo lo que este cercano a una melange(mina) con menor prio
        AtaqueMelangeMenorPrio,
        //  Ataque directo a la factoria
        AtaqueFactoria,
        //  Ataque a una concentración de prioridad alta enemiga
        AtaqueMayorPrio,
        //  Ataque a las unidades neutrales (verdes)  
        AtaqueNeutral,
        //  Ataque a una concetración de menor prioridad enemiga
        AtaqueMenorPrio,
        //Ataque a un extractor
        ataqueExtractor,

        #endregion 

        #region Comanddos_Defensivos
        DefiendeRecurso,
        //  Defiende a la base
        DefiendeBase,
        //  Defiende a la factoria
        DefiedeFactoria,
        //  Defiende a un extractor en concreto
        DefiendeExtractor,
        //  Defiende de una pos a otra
        Patrulla,
        #endregion

        //  No hacer nada
        Festivo,
    }

    //  Estado que se encarga de gestionar las compras de la IA
    public enum UnidadAComprar
    {
        Extractor,
        Exploradores,
        Destructores,
        //no comprar nada
        Ahorrar,
        Emergencia
    }
    
    //  Diferentes estrategias que usará la IA
    public enum Estrategia
    {
        //  Farming consiste en priorizar la compra de extractores y con las unidades militares que se tenga defender estos extractores
        Farming,
        //  Defensivo consiste en jugar de forma defensiva ante los ataque enemigos, colocando unidades defendiendo estructuras 
        Defensivo,
        //  Ofensivo consiste en jugar de forma agresiva, atacando directamente
        Ofensivo,
        //  Guerrilla consiste en ataques de pocas unidades a estructuras, extractores o anidades enemigas y luego reagruparse.
        Guerrilla,
        //  El estado de emergencia es cuando hay posibilidades de perder el juego (ataque enemigo a la base o factoria) y no tengo unidades defendiendo
        Emergencia
    }

    struct Extractor
    {
        ExtractionUnit extractor;
        public bool extrayendo;
        LimitedAccess melange;
        public Extractor(ExtractionUnit ext)
        {
            extractor = ext;
            extrayendo = false;
            melange = null;
        }

        public void extrayendoRecurso(LimitedAccess melange_)
        {
            melange = melange_;
            extrayendo = true;
        }

        public ExtractionUnit getExtractor()
        {
            return extractor;
        }
    }

    public class RTSAIControllerG02 : RTSAIController
    {
        #region variables
        private int MyIndex { get; set; }
        private ColorTeam myType;
        private ColorTeam enemyType;
        [Tooltip("Prioridad máxima para las misiones")]
        public int maximaPrioridad = 100;

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
        private List<Extractor> MisExtractores;
        private List<ExplorationUnit> MisExploradores;
        private List<DestructionUnit> MisDestructores;

        // Las listas completas de instalaciones y unidades del enemigo
        private List<BaseFacility> BaseEnemiga;
        private List<ProcessingFacility> FactoriaEnemiga;
        private List<ExtractionUnit> ExtractoresEnemigos;
        private List<ExplorationUnit> ExploradoresEnemigos;
        private List<DestructionUnit> DestructoresEnemigos;

        //  
        private List<CasillaPrioDefensa> prioDefensa;
        private List<CasillaPrioAtaque> prioMilitar;        //TODO : son las aliadas o enemigas? o  ambas?

        // Las listas completas de accesos limitados y torretas 
        private List<LimitedAccess> Recursos;
        private List<Tower> Torretas;

        //Indice correspondiente a mi enemigo
        int FirstEnemyIndex;

        // Número de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;

        //  Actual estrategía que sigue la IA
        public Estrategia currEstrategia;
        //  Lista de misiones que tiene la IA actualmente
        private List<Mision> misMisiones;
        //  Siguiente unidad que la IA quiere comprar
        private UnidadAComprar unidadAComprar;
        //  Lista de batallones disponibles
        private List<Batallon> batallones;
        //  Lista de misiones a completar
        private Priority_Queue<Mision> misiones;
        //  Unidades que no tienen un batallon asignado
        private List<Unit> unidadesSinBatallon;
        [Tooltip("Dinero que se considera suficiente para la IA")]
        public int dineroSuficiente = 60000;

        // Última unidad creada
        private Unidad LastUnit { get; set; }

        #endregion 

        private void Awake()
        {
            Name = "IAV21G02";
            Author = "IAV21G02";
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

            if (myType.Equals(ColorTeam.AMARILLO))
            {
                enemyType = ColorTeam.AZUL;
            }
            else
            {
                enemyType = ColorTeam.AMARILLO;
            }

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

            batallones = new List<Batallon>();
            unidadesSinBatallon = new List<Unit>();
            misiones = new Priority_Queue<Mision>();
            misMisiones = new List<Mision>();
            MisExtractores = new List<Extractor>();

            ActualizeGameElements();

            foreach (ExplorationUnit explorador in MisExploradores.ToArray())
            {
                unidadesSinBatallon.Add(explorador);
            }

            foreach (DestructionUnit destuctores in MisDestructores.ToArray())
            {
                unidadesSinBatallon.Add(destuctores);
            }

            foreach (Extractor extractor in MisExtractores.ToArray())
            {
                RTSGameManager.Instance.MoveUnit(this, extractor.getExtractor(), RTSGameManager.Instance.GetExtractionUnits(MyIndex)[0].transform);
            }

            gestionaExtractores();

            // Construyo por primera vez el mapa de influencia (con las 'capas' que necesite)
            // ...
            //¿TO DO:?
        }

        //TO DO: pueeees, todo, pa que nos vamos a engañar :D
        private void AIGameLoop()
        {
            //TO DO:Mirar el mapa de influencia y ver que tareas necesitamos hacer con nuestros batallones

            // Como no es demasiado costoso, vamos a tomar las listas completas en cada paso de pensamiento
            ActualizeGameElements();

            Estrategia nuevaEstrategia = eligeEstrategia();
            //  Se ha detectado un cambio de estrategia
            if (nuevaEstrategia != currEstrategia)
            {
                currEstrategia = nuevaEstrategia;
                gestionaMisiones();
            }

            switch (currEstrategia)
            {
                case Estrategia.Defensivo:
                    gestionaDefensa();
                    break;
                case Estrategia.Ofensivo:
                    gestionaAtaque();
                    break;
                case Estrategia.Farming:
                    //gestionaExtractores();
                    break;
                    //.....
            }

            //TO DO:Actualizar tareas que estan realizando nuestros batallones y reasignarselas si es necesario
            //eligeEstrategia();


            gestionaCompra();


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

            foreach (ExtractionUnit extractor in RTSGameManager.Instance.GetExtractionUnits(MyIndex))
            {
                MisExtractores.Add(new Extractor(extractor));
            }

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
            // Si tengo alguna instalación base y dinero para comprar la unidad que menos cuesta me puedo plantear comprar
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
                //De estas, priorizaremos tener mas Exploradores, despues más destructores y por ultimo mas Extractores
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

        //  Compra la unidad predefinida
        private void compraUnidad()
        {
            //switch (economia)
            //{
            //    case EstCompras.Destrucrotor
            //}

        }

        //  Devuelve la melange más cerca de una posición
        private LimitedAccess getMelangeCercana(Vector3 initPos)
        {
            LimitedAccess actMelange = null;
            float distance = 100000;
            foreach (LimitedAccess melange in Recursos) 
            {
                float melangeDistance = (initPos - melange.transform.position ).magnitude;
                if (melange.OccupiedBy == null && melangeDistance < distance) 
                {
                    actMelange = melange;
                    distance = melangeDistance;
                }
            }
            //actMelange.GetComponent<Renderer>().material.color = Color.cyan;
            return actMelange;
        }

        //  Determina qué unidad comprar
        private void gestionaCompra()   //TODO falta el modo emergencia
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            switch (currEstrategia)
            {
                case Estrategia.Ofensivo:   //Priorizamos la compra de destructores sobre los exploradores
                    if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                        MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
                    {
                        MisDestructores.Add(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>());
                    }
                    else if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                        MisDestructores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax) 
                    {
                        MisExploradores.Add(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>());
                    }
                    break;
                case Estrategia.Defensivo:  //Priorizamos la compra de exploradores sobre los destructores
                    if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                            MisDestructores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
                    {
                        MisExploradores.Add(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>());
                    }
                    else if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                            MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax) 
                    {
                        MisDestructores.Add(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>());
                    }
                    break;
                case Estrategia.Guerrilla:  //Priorizamos la compra de destructores sobre los exploradores
                    if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                            MisDestructores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
                    {
                        MisExploradores.Add(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>());
                    }
                    else if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                            MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
                    {
                        MisDestructores.Add(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>());
                    }
                    break;
                case Estrategia.Farming:
                    if (myMoney >= RTSGameManager.Instance.ExtractionUnitCost &&
                        MisExtractores.Count - 1 < RTSGameManager.Instance.ExtractionUnitsMax)  
                    {
                        Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                        MisExtractores.Add(actExtractor);
                        RTSGameManager.Instance.MoveUnit(this,MisExtractores[MisExtractores.Count - 1].getExtractor(), getMelangeCercana(MiFactoria[0].transform.position).transform.position);
                    }
                    break;
                case Estrategia.Emergencia:

                    break;
            }

        }

        //  Moviliza a todos los batallones defensivos que tenga la IA con una misión asignada
        private void gestionaDefensa()
        {
            if (batallones.Count == 0) return;
            foreach (Batallon batallon in batallones)
            {
                if (batallon.getEstrategia().Equals(Estrategia.Defensivo))
                {
                    batallon.movilizaBatallon(this);
                }
            }
        }

        //  Moviliza a todos los batallones ofensivos que tenga la IA con una misión asignada
        private void gestionaAtaque()
        {
            if (batallones.Count == 0) return;
            foreach(Batallon batallon in batallones.ToArray())
            {
                if (batallon.getEstrategia().Equals(Estrategia.Ofensivo))
                {
                    batallon.movilizaBatallon(this);
                }
            }
        }

        private void gestionaExtractores()
        {
            foreach (Extractor extractor in MisExtractores)
            {
                if (!extractor.extrayendo)
                {
                    LimitedAccess melange = getMelangeCercana(MiFactoria[0].transform.position);
                    extractor.getExtractor().Move(this, melange.transform.position);
                    extractor.extrayendoRecurso(melange);
                }
            }
        }

        private void enEmergencia()
        {
            //Si tenemos menos extractores del minimo deseado priorizamos el construirlos
            if (MisExtractores.Count < minDesiredExtractors &&           //  Tengo menos extractores del mínimo
                RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExtractionUnitCost)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION);
            }

            //Lo mismo con los destructores
            else if (MisDestructores.Count < minDesiredDestructors && 
                RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.DestructionUnitCost)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION);
            }

            //Lo mismo con los Exploradores
            else if (MisExploradores.Count < minDesiredExplorers && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExplorationUnitCost)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION);
            }
        }

        public ColorTeam getMyType()
        {
            return myType;
        }

        //  Actualiza las prioridades de defensa y ataque del mapa de influencias
        public void ActualizaPrioridades()
        {
            //-------------MILICIA------------//
            prioMilitar = new List<CasillaPrioAtaque>();
            foreach (DestructionUnit unit in DestructoresEnemigos.ToArray())
            {
                CasillaBehaviour currCasilla = MapManager.GetInstance().GetCasillaCercana(unit.transform);
                CasillaPrioAtaque atq = currCasilla.GetCasillaPrioMilitar();
                atq.ActualizaAtaque();

                if (!prioMilitar.Contains(atq))
                {
                    prioMilitar.Add(atq);
                }
            }

            foreach (ExplorationUnit unit in ExploradoresEnemigos)
            {
                CasillaBehaviour currCasilla = MapManager.GetInstance().GetCasillaCercana(unit.transform);
                CasillaPrioAtaque atq = currCasilla.GetCasillaPrioMilitar();
                atq.ActualizaAtaque();

                if (!prioMilitar.Contains(currCasilla.GetCasillaPrioMilitar()))
                {
                    prioMilitar.Add(currCasilla.GetCasillaPrioMilitar());
                }
            }

            //------------DEFENSA---------------//
            prioDefensa = new List<CasillaPrioDefensa>();
            foreach (Extractor unit in MisExtractores)
            {
                CasillaBehaviour currCasilla = MapManager.GetInstance().GetCasillaCercana(unit.getExtractor().transform);
                CasillaPrioDefensa def = currCasilla.getCasillaPrioDefensa();
                def.ActualizaDefensa();

                if (!prioDefensa.Contains(def))
                {
                    prioDefensa.Add(def);
                }
            }

            //Base
            CasillaPrioDefensa defAux = MapManager.GetInstance().GetCasillaCercana(MiBase[0].transform).getCasillaPrioDefensa();
            defAux.ActualizaDefensa();

            if (!prioDefensa.Contains(defAux))
            {
                prioDefensa.Add(defAux);
            }

            //Factoria
            defAux = MapManager.GetInstance().GetCasillaCercana(MiFactoria[0].transform).getCasillaPrioDefensa();
            defAux.ActualizaDefensa();

            if (!prioDefensa.Contains(defAux))
            {
                prioDefensa.Add(defAux);
            }

            //Ordenamiento de prioridades
            ComparerAtaque compATQ = new ComparerAtaque();
            prioMilitar.Sort(compATQ);
            ComparerDef compDEF = new ComparerDef();
            prioDefensa.Sort(compDEF);
        }

        //  Determina si hay unidades enemigas cerca de base y te devuelve la unidad enemiga más cercana 
        public Unit amenazaBase()
        {
            Vector3 basePos = MiBase[0].transform.position;

            //  Distancia más cerca de un enemigo a nuestra base
            float enemigoCercano = 100000;
            //  Distancia de seguridad
            float distancia = 50;
            //  Enemigo más cercano a nuestra base
            Unit enemigo = null;

            //  Hay un explorador cerca de nuestra base ?
            foreach (ExplorationUnit exploradorEnemigo in ExploradoresEnemigos)
            {
                float enemigoDistancia = (exploradorEnemigo.transform.position - basePos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo está cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = exploradorEnemigo;
                }
            }
            //  Hay un destructor cerca de nuestra factoria ?
            foreach (DestructionUnit destructorEnemigo in DestructoresEnemigos)
            {
                float enemigoDistancia = (destructorEnemigo.transform.position - basePos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo está cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = destructorEnemigo;
                }
            }
            return enemigo;
        }

        //  Determina si hay unidades enemigas cerca de la factoria y te devuelve la unidad enemiga más cercana 
        public Unit amenazaFactoria()
        {
            Vector3 basePos = MiFactoria[0].transform.position;

            //  Distancia más cerca de un enemigo a nuestra base
            float enemigoCercano = 100000;
            //  Distancia de seguridad
            float distancia = 50;
            //  Enemigo más cercano a nuestra base
            Unit enemigo = null;

            //  Hay un explorador cerca de nuestra base ?
            foreach (ExplorationUnit exploradorEnemigo in ExploradoresEnemigos)
            {
                float enemigoDistancia = (exploradorEnemigo.transform.position - basePos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo está cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = exploradorEnemigo;
                }
            }
            //  Hay un destructor cerca de nuestra factoria ?
            foreach (DestructionUnit destructorEnemigo in DestructoresEnemigos)
            {
                float enemigoDistancia = (destructorEnemigo.transform.position - basePos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo está cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = destructorEnemigo;
                }
            }
            return enemigo;
        }

        //  Crea las misiones en función de la estrategia que usa la IA
        private void gestionaMisiones()
        {
            Debug.Log("Estrategía elegida es " + currEstrategia.ToString());
            switch (currEstrategia)
            {
                case Estrategia.Ofensivo:
                    gestionaEstrategiaOfensiva();
                    break;
                case Estrategia.Farming:

                    break;
                case Estrategia.Defensivo:
                    break;
                case Estrategia.Guerrilla:
                    break;
                case Estrategia.Emergencia:
                    break;
                default:
                    break;
                
            }
        }

        //  Crea batallón, lo añade a los batallones y devuelve el batallón creado TODO: poner los batallones que faltan
        private Batallon creaBatallon(TipoBatallon tipoBatallon,Mision currMision)
        {
            Batallon currBatallon = new Batallon(tipoBatallon, currMision);

            switch (tipoBatallon)
            {
                case TipoBatallon.BatallonPuerco:   //  Batallón con todas nuestras unidades
                    currBatallon.numeroExploradores = MisExploradores.Count - 1;
                    currBatallon.numeroDestructores = MisDestructores.Count - 1;

                    foreach (Unit unidad in unidadesSinBatallon)
                    {
                        currBatallon.agregaUnidad(unidad);
                    }
                    currBatallon.completado = true;
                    currBatallon.enMision = false;
                    break;

                case TipoBatallon.BatallonTiwardo:  // Batallón con dos destructores
                    currBatallon.numeroExploradores = 0;
                    currBatallon.numeroDestructores = 2;
                    int numDestructoresAsignados = 0;
                    int cont = 0;
                    while (cont < unidadesSinBatallon.Count - 1 &&  numDestructoresAsignados < currBatallon.numeroDestructores)
                    {
                        DestructionUnit dest = unidadesSinBatallon[cont].GetComponent<DestructionUnit>();
                        if (dest)
                        {
                            currBatallon.agregaUnidad(dest);
                            numDestructoresAsignados++;
                            cont++;
                        }
                    }
                    if (numDestructoresAsignados == currBatallon.numeroDestructores)
                    {
                        currBatallon.completado = true;
                        currBatallon.enMision = false;
                    }
                    else
                    {
                        currBatallon.completado = false;
                        currBatallon.enMision = false;
                    }
                    break;
            }

            batallones.Add(currBatallon);
            return batallones[batallones.Count - 1];
        }

        private Estrategia eligeEstrategia()
        {
            bool supremaciaEconomica = RTSGameManager.Instance.GetMoney(MyIndex) > 100000 + RTSGameManager.Instance.GetMoney(FirstEnemyIndex);
            bool supremaciaMilitar = false;
            if (getTotalPrioAliada() > getTotalPrioEnemiga())
            {
                supremaciaMilitar = true;
            }

            bool equilibrio = true;
            if ((MisExtractores.Count - 1) >= (MisExploradores.Count - 1) + (MisDestructores.Count - 1)) 
            {
                equilibrio = false;
            }

            bool amenazaCercana;
            Unit enemigoEnBase = amenazaBase();
            Unit enemigoEnFactoria = amenazaFactoria();
            amenazaCercana = enemigoEnBase != null || enemigoEnFactoria != null;
            Estrategia nuevaEstrategia = currEstrategia;

            if (amenazaCercana)
            {
                nuevaEstrategia = Estrategia.Defensivo;
            }
            if (equilibrio && !supremaciaEconomica && !supremaciaMilitar)
            {
                nuevaEstrategia = Estrategia.Farming;
            }
            else if (!equilibrio && !supremaciaEconomica)
            {
                nuevaEstrategia = Estrategia.Defensivo;
            }
            else if (!supremaciaEconomica && !supremaciaMilitar)
            {
                nuevaEstrategia = Estrategia.Farming;
            }
            else if (supremaciaMilitar && supremaciaEconomica)  // TODO: supremacia militar
            {
                nuevaEstrategia = Estrategia.Ofensivo;
            }
            else if (!supremaciaMilitar && supremaciaEconomica) 
            {
                nuevaEstrategia = Estrategia.Guerrilla;
            }
            else if (!supremaciaEconomica && supremaciaMilitar) // TODO : revisar
            {
                nuevaEstrategia = Estrategia.Guerrilla;
            }

            return nuevaEstrategia;
        }

        private void gestionaEstrategiaOfensiva() 
        {
            ActualizaPrioridades();
            if (prioMilitar[0].GetCasilla().team_.Equals(ColorTeam.AMARILLO))
            {
                prioMilitar[0].GetCasilla().GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                prioMilitar[0].GetCasilla().GetComponent<Renderer>().material.color = Color.blue;
            }

            //  ataque a la casilla con mayor prioridad enemiga ya que sabemos que estamos en supremacia militar y con máxima prioridad (100)
            Mision ataqueMayorPrio = new Mision(Comando.AtaqueMayorPrio, prioMilitar[0].GetCasilla().transform, 100, Estrategia.Ofensivo);
            misMisiones.Add(ataqueMayorPrio);

            // Agregamos ataque al nexo como misión secundaria con prioridad (95)
            Mision ataqueAlNexo = new Mision(Comando.AtaqueAlNexo, BaseEnemiga[0].transform, 95, Estrategia.Ofensivo);
            misMisiones.Add(ataqueAlNexo);

            foreach (Batallon batallon in batallones.ToArray())
            {
                //  Desmontamos todos los batallones para volver a crearlos
                batallon.desmontarBatallon(unidadesSinBatallon, batallones);
            }
            //  Creamos un batallón con todas nuestras unidades para un ataque en conjunto a la mayor prioridad enemiga
            creaBatallon(TipoBatallon.BatallonPuerco, ataqueMayorPrio);

            misMisiones.Sort();
        }

        //  Devuelve la cantidad de prio de todas las unidades enemigas
        private int getTotalPrioEnemiga()
        {
            int totalPrioEnemiga = 0;
            foreach (ExplorationUnit explorador in ExploradoresEnemigos)
            {
                totalPrioEnemiga += explorador.GetComponent<UnitType>().influencia;
            }
            foreach (DestructionUnit destructor in DestructoresEnemigos)
            {
                totalPrioEnemiga += destructor.GetComponent<UnitType>().influencia;
            }

            return totalPrioEnemiga;
        }

        //  Devuelve la cantidad de prio aliada
        private int getTotalPrioAliada()
        {
            int totalPrioAliada = 0;
            foreach (ExplorationUnit explorador in MisExploradores)
            {
                totalPrioAliada += explorador.GetComponent<UnitType>().influencia;
            }
            foreach (DestructionUnit destructor in MisDestructores)
            {
                totalPrioAliada += destructor.GetComponent<UnitType>().influencia;
            }

            return totalPrioAliada;
        }

        #region gestión

        #endregion
    }
}
