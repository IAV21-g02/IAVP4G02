using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;


namespace es.ucm.fdi.iav.rts.g02
{
    class Mision : IComparable<Mision>
    {
        // misi�n que debe cumplir este batallon
        Comando actMision;
        //  Transform de la posici�n a la que ir
        Transform objetivo;
        //  Priridad que tiene esta misi�n
        int prio;
        // Determina si esta misi�n se ha completado
        public bool misionStatus = false;
        //  Tipo de estrateg�a que sigue este batall�n
        Estrategia estrategia;

        //  Crea una misi�n
        public Mision(Comando cmd_, Transform objetivo_, int prio_, Estrategia estrategia_)
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

        //  Asigna un objetivo a esta misi�n
        public void asignaObjetivo(Transform objetivo_)
        {
            objetivo = objetivo_;
        }

        //  Asigna a esta misi�n como completada
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
        //  N�mero de exploradores que necesita este batall�n
        public int numeroExploradores;
        //  N�mero de destructores que necesita este batall�n
        public int numeroDestructores;
        //  N�mero de extractores que necesita este batall�n
        public int numeroExtractores;

        //  Lista de extractores de este batallon
        public List<ExtractionUnit> extractores;
        //  Lista de extractores de este batallon
        public List<DestructionUnit> destructores;
        //  Lista de extractores de este batallon
        public List<ExplorationUnit> exploradores;

        //  Misi�n de este batallon
        Mision mision;
        //  Tipo de batallon
        TipoBatallon tipoBatallon;

        //  Determina si este batallon termino de construirse
        public bool completado;
        //  Determina si este batallon est� en una misi�n
        public bool enMision;
        //  Rango que determina si las unidades han llegado a su objetivo
        private float rangoPosicion;
        //  Determina si un batall�n est� en movimiento
        public bool enMovimiento;

        // Construye un batall�n con un tipo de batall�n y una misi�n
        public Batallon(TipoBatallon tipoBatallon_, Mision currMision)
        {
            tipoBatallon = tipoBatallon_;
            completado = false;
            enMision = false;
            rangoPosicion = 5;
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

        public bool batallonCompleto()
        {
            if (destructores.Count >= numeroDestructores &&
                exploradores.Count >= numeroExploradores &&
                extractores.Count >= numeroExtractores)
            {
                return true;
            }

            return false;
        }

        //  Construye un batallon en funci�n de un tipo de batall�n
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
            completado = batallonCompleto();
        }
        public void agregaUnidad(ExtractionUnit unidad)
        {
            extractores.Add(unidad);
            completado = batallonCompleto();

        }
        public void agregaUnidad(ExplorationUnit unidad)
        {
            exploradores.Add(unidad);
            completado = batallonCompleto();

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
            completado = batallonCompleto();

        }

        //  Asigna una nueva misi�n a un batall�n
        public void asignaMision(Mision nueMision)
        {
            mision = nueMision;
            enMision = true;
        }

        // desmonta un batall�n para agregarlo a una lista de unidades sin batall�n
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

        //  Mueve a todas las unidades de este batall�n a una posici�n
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

        //  Mueve a todas las unidades de este batall�n al objetivo que tiene asignado como misi�n
        public void movilizaBatallon(RTSAIControllerG02 rts)
        {

            foreach (ExplorationUnit explorador in exploradores)
            {
                if ((mision.getObjetivo().position - explorador.transform.position).magnitude > rangoPosicion)
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
                if ((extractor.transform.position - mision.getObjetivo().position).magnitude > rangoPosicion)
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
                if ((destructor.transform.position - mision.getObjetivo().position).magnitude > rangoPosicion)
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

        public TipoBatallon getTipoBatallon()
        {
            return tipoBatallon;
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
        //  Batall�n con todas las unidades
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
        //  Ataque a una concentraci�n de prioridad alta enemiga
        AtaqueMayorPrio,
        //  Ataque a las unidades neutrales (verdes)  
        AtaqueNeutral,
        //  Ataque a una concetraci�n de menor prioridad enemiga
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

    //  Diferentes estrategias que usar� la IA
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
        Emergencia,
        //  
        NONE
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
        [Tooltip("Prioridad m�xima para las misiones")]
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

        // N�mero de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;

        //  Actual estrateg�a que sigue la IA
        public Estrategia currEstrategia;
        //  Estrateg�a anterior
        private Estrategia estrategiaAnt;
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

        // �ltima unidad creada
        private Unidad LastUnit { get; set; }

        #endregion 

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

            estrategiaAnt = Estrategia.NONE;

            // Construyo por primera vez el mapa de influencia (con las 'capas' que necesite)
            // ...
            //�TO DO:?
        }

        //TO DO: pueeees, todo, pa que nos vamos a enga�ar :D
        private void AIGameLoop()
        {
            //  Actualizamamos las unidades que hay en juego
            ActualizeGameElements();

            //  Elegimos una estrateg�a 
            eligeEstrategia();
            //  Se ha detectado un cambio de estrateg�a
            if (currEstrategia != estrategiaAnt)
            {
                estrategiaAnt = currEstrategia;
                //  Creamos las misiones en funci�n de la nueva estrateg�a
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
                    gestionaFarm();
                    //gestionaExtractores();
                    break;
                    //.....
            }
            foreach (Batallon batallon in batallones)
            {
                Debug.Log(batallon.getTipoBatallon().ToString());
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

        //  Compra la unidad predefinida
        private void compraUnidad()
        {
            //switch (economia)
            //{
            //    case EstCompras.Destrucrotor
            //}

        }

        //  Devuelve la melange m�s cerca de una posici�n
        private LimitedAccess getMelangeCercana(Vector3 initPos)
        {
            LimitedAccess actMelange = null;
            float distance = 100000;
            foreach (LimitedAccess melange in Recursos)
            {
                float melangeDistance = (initPos - melange.transform.position).magnitude;
                if (melange.OccupiedBy == null && melangeDistance < distance)
                {
                    actMelange = melange;
                    distance = melangeDistance;
                }
            }
            //actMelange.GetComponent<Renderer>().material.color = Color.cyan;
            return actMelange;
        }

        //  Determina qu� unidad comprar
        private void gestionaCompra()   //TODO falta el modo emergencia
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            switch (currEstrategia)
            {
                case Estrategia.Ofensivo:   //Priorizamos la compra de destructores sobre los exploradores
                    if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                        MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
                    {
                        Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();
                        MisDestructores.Add(currUnit.GetComponent<DestructionUnit>());
                        unidadesSinBatallon.Add(currUnit);
                    }
                    else if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                        MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
                    {
                        Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();
                        MisExploradores.Add(currUnit.GetComponent<ExplorationUnit>());
                        unidadesSinBatallon.Add(currUnit);
                    }
                    break;
                case Estrategia.Defensivo:  //Priorizamos la compra de exploradores sobre los destructores
                    if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                            MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
                    {
                        Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();
                        MisExploradores.Add(currUnit.GetComponent<ExplorationUnit>());
                        unidadesSinBatallon.Add(currUnit);
                    }
                    else if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                            MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
                    {
                        Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();
                        MisDestructores.Add(currUnit.GetComponent<DestructionUnit>());
                        unidadesSinBatallon.Add(currUnit);
                    }
                    break;
                case Estrategia.Guerrilla:  //Priorizamos la compra de destructores sobre los exploradores
                    if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                            MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
                    {
                        Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();
                        MisExploradores.Add(currUnit.GetComponent<ExplorationUnit>());
                        unidadesSinBatallon.Add(currUnit);
                    }
                    else if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                            MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
                    {
                        Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();
                        MisDestructores.Add(currUnit.GetComponent<DestructionUnit>());
                        unidadesSinBatallon.Add(currUnit);
                    }
                    break;
                case Estrategia.Farming:
                    bool equilibrio = true;
                    if ((MisExtractores.Count) > (MisExploradores.Count) + (MisDestructores.Count))
                    {
                        equilibrio = false;
                    }
                    if (equilibrio && myMoney >= RTSGameManager.Instance.ExtractionUnitCost &&
                        MisExtractores.Count - 1 < RTSGameManager.Instance.ExtractionUnitsMax)
                    {
                        Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                        MisExtractores.Add(actExtractor);
                        RTSGameManager.Instance.MoveUnit(this, MisExtractores[MisExtractores.Count - 1].getExtractor(), getMelangeCercana(MiFactoria[0].transform.position).transform.position);
                    }
                    else if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                        MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
                    {
                        Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();
                        MisExploradores.Add(currUnit.GetComponent<ExplorationUnit>());
                        unidadesSinBatallon.Add(currUnit);
                    }

                    break;
                case Estrategia.Emergencia:

                    break;
            }

        }

        //  Moviliza a todos los batallones defensivos que tenga la IA con una misi�n asignada
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

        //  Moviliza a todos los batallones ofensivos que tenga la IA con una misi�n asignada
        private void gestionaAtaque()
        {
            if (batallones.Count == 0) return;
            foreach (Batallon batallon in batallones.ToArray())
            {
                if (batallon.getEstrategia().Equals(Estrategia.Ofensivo))
                {
                    if (!batallon.completado)
                    {
                        agregaUnidadesBatallon(batallon);
                    }

                    batallon.movilizaBatallon(this);
                }
            }
        }

        private void agregaUnidadesBatallon(Batallon batallon)
        {
            if (unidadesSinBatallon.Count == 0) return;
            int ind = 0;
            bool completo = false;
            while (!completo && ind < unidadesSinBatallon.Count)
            {
                if (batallon.numeroDestructores > batallon.destructores.Count)
                {
                    DestructionUnit dest = unidadesSinBatallon[ind].GetComponent<DestructionUnit>();
                    if (dest)
                    {
                        batallon.agregaUnidad(dest);
                        unidadesSinBatallon.Remove(dest);
                    }
                    else ind++;
                }
                if (batallon.numeroExploradores > batallon.exploradores.Count && ind < unidadesSinBatallon.Count)
                {
                    ExplorationUnit expl = unidadesSinBatallon[ind].GetComponent<ExplorationUnit>();
                    if (expl)
                    {
                        batallon.agregaUnidad(expl);
                        unidadesSinBatallon.Remove(expl);
                    }
                    else ind++;
                }
                completo = batallon.completado;
            }
        }

        private void gestionaFarm()
        {
            if (batallones.Count == 0) return;
            foreach (Batallon batallon in batallones.ToArray())
            {
                if (batallon.getEstrategia().Equals(Estrategia.Farming))
                {
                    if (!batallon.completado)
                    {
                        agregaUnidadesBatallon(batallon);
                    }
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
            if (MisExtractores.Count < minDesiredExtractors &&           //  Tengo menos extractores del m�nimo
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

        //  Determina si hay unidades enemigas cerca de base y te devuelve la unidad enemiga m�s cercana 
        public Unit amenazaBase()
        {
            Vector3 basePos = MiBase[0].transform.position;

            //  Distancia m�s cerca de un enemigo a nuestra base
            float enemigoCercano = 100000;
            //  Distancia de seguridad
            float distancia = 50;
            //  Enemigo m�s cercano a nuestra base
            Unit enemigo = null;

            //  Hay un explorador cerca de nuestra base ?
            foreach (ExplorationUnit exploradorEnemigo in ExploradoresEnemigos)
            {
                float enemigoDistancia = (exploradorEnemigo.transform.position - basePos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo est� cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = exploradorEnemigo;
                }
            }
            //  Hay un destructor cerca de nuestra factoria ?
            foreach (DestructionUnit destructorEnemigo in DestructoresEnemigos)
            {
                float enemigoDistancia = (destructorEnemigo.transform.position - basePos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo est� cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = destructorEnemigo;
                }
            }
            return enemigo;
        }

        //  Determina si hay unidades enemigas cerca de la factoria y te devuelve la unidad enemiga m�s cercana 
        public Unit amenazaFactoria()
        {
            Vector3 factPos = MiFactoria[0].transform.position;

            //  Distancia m�s cerca de un enemigo a nuestra base
            float enemigoCercano = 100000;
            //  Distancia de seguridad
            float distancia = 30;
            //  Enemigo m�s cercano a nuestra base
            Unit enemigo = null;

            //  Hay un explorador cerca de nuestra base ?
            foreach (ExplorationUnit exploradorEnemigo in ExploradoresEnemigos)
            {
                float enemigoDistancia = (exploradorEnemigo.transform.position - factPos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo est� cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = exploradorEnemigo;
                }
            }
            //  Hay un destructor cerca de nuestra factoria ?
            foreach (DestructionUnit destructorEnemigo in DestructoresEnemigos)
            {
                float enemigoDistancia = (destructorEnemigo.transform.position - factPos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo est� cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = destructorEnemigo;
                }
            }
            return enemigo;
        }

        //  Crea las misiones en funci�n de la estrategia que usa la IA (Este m�todo solo se llama cuando se detecta un cambio de estrateg�a)
        private void gestionaMisiones()
        {
            misMisiones.Clear();
            ActualizaPrioridades();
            Debug.Log("Estrateg�a elegida es " + currEstrategia.ToString());
            switch (currEstrategia)
            {
                case Estrategia.Ofensivo:
                    gestionaEstrategiaOfensiva();
                    break;
                case Estrategia.Farming:
                    gestionarEstrategiaFarm();
                    break;
                case Estrategia.Defensivo:
                    gestionaEstrategiaDefensiva();
                    break;
                case Estrategia.Guerrilla:
                    break;
                case Estrategia.Emergencia:
                    break;
                default:
                    break;

            }
        }

        //  Crea batall�n, lo a�ade a los batallones y devuelve el batall�n creado TODO: poner los batallones que faltan
        private Batallon creaBatallon(TipoBatallon tipoBatallon, Mision currMision)
        {
            Batallon currBatallon = new Batallon(tipoBatallon, currMision);

            switch (tipoBatallon)
            {
                case TipoBatallon.BatallonAurgar:
                    //Debug.Log("BATALLON AUGAR!");
                    int aux = 0;
                    int ind = 0;
                    while (aux < currBatallon.numeroExploradores && ind < unidadesSinBatallon.Count)
                    {
                        ExplorationUnit expl = unidadesSinBatallon[ind].GetComponent<ExplorationUnit>();
                        if (expl)
                        {
                            currBatallon.agregaUnidad(expl);
                            unidadesSinBatallon.Remove(expl);
                            aux++;
                        }
                        else ind++;
                    }

                    if (aux < currBatallon.numeroExploradores)
                    {
                        currBatallon.completado = false;
                        currBatallon.enMision = false;
                    }
                    else
                    {
                        currBatallon.completado = true;
                        currBatallon.enMision = false;
                    }
                    break;

                case TipoBatallon.BatallonPuerco:   //  Batall�n con todas nuestras unidades
                    //currBatallon.numeroExploradores = MisExploradores.Count - 1;
                    //currBatallon.numeroDestructores = MisDestructores.Count - 1;

                    foreach (Unit unidad in unidadesSinBatallon)
                    {
                        currBatallon.agregaUnidad(unidad);
                    }
                    unidadesSinBatallon.Clear();
                    currBatallon.completado = true;
                    currBatallon.enMision = false;
                    break;

                case TipoBatallon.BatallonTiwardo:  // Batall�n con dos destructores
                    //currBatallon.numeroExploradores = 0;
                    //currBatallon.numeroDestructores = 2;
                    int numDestructoresAsignados = 0;
                    int cont = 0;
                    while (cont < unidadesSinBatallon.Count && numDestructoresAsignados < currBatallon.numeroDestructores)
                    {
                        DestructionUnit dest = unidadesSinBatallon[cont].GetComponent<DestructionUnit>();
                        if (dest)
                        {
                            currBatallon.agregaUnidad(dest);
                            numDestructoresAsignados++;
                            unidadesSinBatallon.Remove(dest);
                        }
                        else cont++;
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

        private void eligeEstrategia()
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

            if (amenazaCercana)
            {
                Debug.Log("AMENAZA DETECTADA");
                currEstrategia = Estrategia.Defensivo;
            }
            else if (equilibrio && !supremaciaEconomica && !supremaciaMilitar)
            {
                Debug.Log("FARMING");
                currEstrategia = Estrategia.Farming;
            }
            else if (!supremaciaMilitar && supremaciaEconomica)
            {
                Debug.Log("GUERRILLA");
                currEstrategia = Estrategia.Guerrilla;
            }
            else if (supremaciaMilitar && supremaciaEconomica)  // TODO: supremacia militar
            {
                Debug.Log("OFENSIVA");
                currEstrategia = Estrategia.Ofensivo;
            }

            else if (currEstrategia.Equals(Estrategia.Farming))
            {

            }

            //return nuevaEstrategia;
        }

        private void gestionaEstrategiaOfensiva()
        {
            if (prioMilitar[0].GetCasilla().team_.Equals(ColorTeam.AMARILLO))
            {
                prioMilitar[0].GetCasilla().GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                prioMilitar[0].GetCasilla().GetComponent<Renderer>().material.color = Color.blue;
            }

            //  ataque a la casilla con mayor prioridad enemiga ya que sabemos que estamos en supremacia militar y con m�xima prioridad (100)
            Mision ataqueMayorPrio = new Mision(Comando.AtaqueMayorPrio, prioMilitar[0].GetCasilla().transform, 100, Estrategia.Ofensivo);
            misMisiones.Add(ataqueMayorPrio);

            // Agregamos ataque al nexo como misi�n secundaria con prioridad (95)
            Mision ataqueAlNexo = new Mision(Comando.AtaqueAlNexo, BaseEnemiga[0].transform, 95, Estrategia.Ofensivo);
            misMisiones.Add(ataqueAlNexo);

            desmonatarBatallones();

            //  Creamos un batall�n con todas nuestras unidades para un ataque en conjunto a la mayor prioridad enemiga
            creaBatallon(TipoBatallon.BatallonPuerco, ataqueMayorPrio);

            misMisiones.Sort();
        }

        //  Si tenemos unidades enemigas en cerca de la base o de la factoria -> ataque a estas unidades
        private void gestionaEstrategiaDefensiva()
        {
            if (prioDefensa[0].GetCasilla().team_.Equals(ColorTeam.AMARILLO))
            {
                prioDefensa[0].GetCasilla().GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                prioDefensa[0].GetCasilla().GetComponent<Renderer>().material.color = Color.blue;
            }

            Unit enemigoEnBase = amenazaBase();
            Unit enemigoEnFactoria = amenazaFactoria();

            if (enemigoEnBase != null)
            {
                Debug.Log("Enemigo cerca de la base detectado");
                Mision defensaBase = new Mision(Comando.DefiendeBase, enemigoEnBase.transform, 100, Estrategia.Defensivo);
                misMisiones.Add(defensaBase);
                desmonatarBatallones();
                creaBatallon(TipoBatallon.BatallonAurgar, defensaBase);
            }
            if (enemigoEnFactoria != null)
            {
                Debug.Log("Enemigo cerca de la factoria detectado");
                Mision defensaFactoria = new Mision(Comando.DefiedeFactoria, enemigoEnFactoria.transform, 80, Estrategia.Defensivo);
                misMisiones.Add(defensaFactoria);
                desmonatarBatallones();
                creaBatallon(TipoBatallon.BatallonAurgar, defensaFactoria);
            }


            misMisiones.Sort();

        }

        private void gestionarEstrategiaFarm()
        {
            desmonatarBatallones();
            if (MiFactoria.Count > 0)
            {
                Mision defiendeFactoria = new Mision(Comando.DefiedeFactoria, MiFactoria[0].transform, 100, Estrategia.Farming);
                creaBatallon(TipoBatallon.BatallonAurgar, defiendeFactoria);
                misMisiones.Add(defiendeFactoria);
            }

            if (MisExtractores.Count > 0)
            {
                Mision defiendeExtractor = new Mision(Comando.DefiendeExtractor, MisExtractores[0].getExtractor().transform, 80, Estrategia.Farming);
                creaBatallon(TipoBatallon.BatallonAurgar, defiendeExtractor);
                misMisiones.Add(defiendeExtractor);
            }
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

        private void desmonatarBatallones()
        {
            foreach (Batallon batallon in batallones.ToArray())
            {
                batallon.desmontarBatallon(unidadesSinBatallon, batallones);
            }
        }

        #region gesti�n

        #endregion
    }
}
