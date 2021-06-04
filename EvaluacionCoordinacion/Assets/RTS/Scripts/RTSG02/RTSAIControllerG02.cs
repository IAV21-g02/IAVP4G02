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
        //  Ofensivo consiste en jugar de forma agresiva, atacando directamente a la base enemiga y a las zonas con mayor influencias del enemigo
        Ofensivo,
        //  Guerrilla consiste en ataques de pocas unidades a estructuras, extractores o anidades enemigas y luego reagruparse.
        Guerrilla,
        //  Entra en estado de emergencia cuando quedan pocas unidades ofensivas
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

        public LimitedAccess getMelange()
        {
            return melange;
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

        // El estilo para las etiquetas de la interfaz
        private GUIStyle _labelStyle;
        private GUIStyle _labelSmallStyle;

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
        //  Estrategía anterior
        private Estrategia estrategiaAnt;
        //  Lista de misiones que tiene la IA actualmente
        private List<Mision> misMisiones;
        //  Siguiente unidad que la IA quiere comprar
        private UnidadAComprar unidadAComprar;
        //  Lista de misiones a completar
        private Priority_Queue<Mision> misiones;
        //  Unidades que no tienen un batallon asignado
        private List<Unit> unidadesSinBatallon;
        [Tooltip("Dinero que se considera suficiente para la IA")]
        public int dineroSuficiente = 60000;

        // Última unidad creada
        private Unidad LastUnit { get; set; }

        #endregion 

        public AudioClip ataqueAliado;
        public AudioClip ataquePosicion;
        public AudioClip defiendePosicion;
        public AudioClip ganaste;
        public AudioClip perdiste;
        public AudioClip gameOver;
        public AudioClip getRdy;
        public AudioClip enemigoVisto;
        public AudioClip peligro;
        public AudioClip farm;


        AudioSource audioSource;

        private void Awake()
        {
            Name = "IAV21G02";
            Author = "IAV21G02";

            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 16;
            _labelStyle.normal.textColor = Color.yellow;

            _labelSmallStyle = new GUIStyle();
            _labelSmallStyle.fontSize = 11;
            _labelSmallStyle.normal.textColor = Color.yellow;
        }

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

            //// Aunque no exista el concepto de unidad seleccionada, podríamos mostrar cual ha sido la última en moverse
            //if (movedUnit != null)
            // Una etiqueta para indicar la última unidad movida, si la hay
            GUILayout.Label("Usando la estrategía " + currEstrategia.ToString(), _labelSmallStyle);



            // Cerramos el área de distribución con contenido en vertical
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // El método de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {

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
            Debug.Log("Partida Terminada");
        }

        //  Inicializa el controlador
        private void InitController()
        {

            audioSource = MapManager.GetInstance().gameObject.GetComponent<AudioSource>();
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

            MisExtractores = new List<Extractor>();

            //Inicializamos a parte la lista de los extractores para gestionar mejor su movimiento porque
            //si no, cuando hay varios que van a la misma casilla para extraer recursos se suelen quedar
            //"pillados"
            ActualizeGameElements();

            //Envíamos a los estractores que ya tenemos en juego a su objetivo de extraer recursos
            //teniendo en cuenta lo previamente mentado.
            gestionaExtractores();

            estrategiaAnt = Estrategia.NONE;

            if ((RTSGameManager.Instance.GetMoney(MyIndex) <= 0
                && MiFactoria.Count <= 0
                && MisExtractores.Count <= 0
                && MisExploradores.Count <= 0
                && MisDestructores.Count <= 0)
                || MiBase.Count <= 0)
            {
                throw new Exception("No hay condiciones suficientes para jugar");
            }
            //Pasamos a AIGameLoop()
            ThinkStepNumber++;

            audioSource.clip = getRdy;
            audioSource.Play();
        }

        //TO DO: pueeees, casi todo, pa que nos vamos a engañar :D
        private void AIGameLoop()
        {
            //  Actualizamamos las unidades que hay en juego
            ActualizeGameElements();

            //  Elegimos una estrategía 
            eligeEstrategia();
            //  Se ha detectado un cambio de estrategía
            if (currEstrategia != estrategiaAnt)
            {
                estrategiaAnt = currEstrategia;
                playSound();
            }

            //currEstrategia = Estrategia.Farming;

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
                    break;
                case Estrategia.Guerrilla:
                    gestionaGuerrilla();
                    break;
                case Estrategia.Emergencia:
                    gestionaEmergencia();
                    break;
            }


            gestionaCompra();


            //El bucle de juego termina cuando una de las bases es destruida 
            if ((BaseEnemiga == null || BaseEnemiga.Count == 0 || MiBase == null || MiBase.Count == 0))
            {
                audioSource.clip = gameOver;
                audioSource.Play();
                Stopthinking();
                if (BaseEnemiga.Count == 0 || BaseEnemiga == null)
                {
                    audioSource.clip = ganaste;
                }
                else audioSource.clip = perdiste;
                audioSource.PlayDelayed(gameOver.length + 1);
            }

        }

        //  Reproduce un sonido en función de la estrategía que tenga
        private void playSound()
        {
            switch (currEstrategia)
            {
                case Estrategia.Defensivo:
                    audioSource.clip = enemigoVisto;
                    audioSource.Play();
                    break;
                case Estrategia.Ofensivo:
                    audioSource.clip = ataqueAliado;
                    audioSource.Play();
                    break;
                case Estrategia.Guerrilla:
                    audioSource.clip = ataquePosicion;
                    audioSource.Play();
                    break;
                case Estrategia.Emergencia:
                    audioSource.clip = peligro;
                    audioSource.Play();
                    break;
                case Estrategia.Farming:
                    audioSource.clip = farm;
                    audioSource.Play();
                    break;
            }
        }

        //  Actualiza la lista tanto de enemigos como de aliados
        private void ActualizeGameElements()
        {
            MiBase = RTSGameManager.Instance.GetBaseFacilities(MyIndex);
            MiFactoria = RTSGameManager.Instance.GetProcessingFacilities(MyIndex);

            MisExtractores.Clear();
            foreach (ExtractionUnit extractor in RTSGameManager.Instance.GetExtractionUnits(MyIndex))
            {
                MisExtractores.Add(new Extractor(extractor));
            }

            MisExploradores = RTSGameManager.Instance.GetExplorationUnits(MyIndex);

            MisDestructores = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
            //MisExtractores = RTSGameManager.Instance.GetExtractionUnits(MyIndex);

            BaseEnemiga = RTSGameManager.Instance.GetBaseFacilities(FirstEnemyIndex);
            FactoriaEnemiga = RTSGameManager.Instance.GetProcessingFacilities(FirstEnemyIndex);
            ExtractoresEnemigos = RTSGameManager.Instance.GetExtractionUnits(FirstEnemyIndex);
            ExploradoresEnemigos = RTSGameManager.Instance.GetExplorationUnits(FirstEnemyIndex);
            DestructoresEnemigos = RTSGameManager.Instance.GetDestructionUnits(FirstEnemyIndex);

            Torretas = RTSScenarioManager.Instance.Towers;
        }

        //  Devuelve la melange más cercana y que no este siendo ocupada por otro extractor
        private LimitedAccess getMelangeToFarm(Vector3 initPos)
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
        //  Controla que los extractores tengan una melange que farmear
        private void gestionaExtractores()
        {
            foreach (Extractor extractor in MisExtractores)
            {
                if (extractor.getExtractor().Resources > 0) return;

                LimitedAccess currMelange = extractor.getMelange();
                if (currMelange && currMelange.OccupiedBy == null)
                {
                    extractor.getExtractor().Move(this, currMelange.transform);
                }
                else
                {
                    LimitedAccess nuevaMelange = getMelangeToFarm(extractor.getExtractor().transform.position);
                    if (nuevaMelange)
                    {
                        extractor.getExtractor().Move(this, nuevaMelange.transform);
                        extractor.extrayendoRecurso(nuevaMelange);
                    }
                }

            }
        }

        #region herramientas
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

        public ColorTeam getMyType()
        {
            return myType;
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

        public Unit amenazaExtractor()
        {
            foreach (Extractor ext in MisExtractores)
            {
                Vector3 extPos = ext.getExtractor().transform.position;
                //  Distancia de seguridad
                float distancia = 30;
                //  Hay un destructor amenazando un extractor?
                foreach (DestructionUnit destructorEnemigo in DestructoresEnemigos)
                {
                    float enemigoDistancia = (destructorEnemigo.transform.position - extPos).magnitude;
                    if (enemigoDistancia < distancia)
                    {
                        return destructorEnemigo;
                    }
                }
                //  Hay un explorador amenazando un extractor?
                foreach (ExplorationUnit exploradorEnemigo in ExploradoresEnemigos)
                {
                    float enemigoDistancia = (exploradorEnemigo.transform.position - extPos).magnitude;
                    if (enemigoDistancia < distancia)
                    {
                        return exploradorEnemigo;
                    }
                }
            }
            return null;
        }

        //  Determina si hay unidades enemigas cerca de la factoria y te devuelve la unidad enemiga más cercana 
        public Unit amenazaFactoria()
        {
            Vector3 factPos = MiFactoria[0].transform.position;
            //  Distancia más cerca de un enemigo a nuestra base
            float enemigoCercano = 100000;
            //  Distancia de seguridad
            float distancia = 30;
            //  Enemigo más cercano a nuestra base
            Unit enemigo = null;

            //  Hay un explorador cerca de nuestra base ?
            foreach (ExplorationUnit exploradorEnemigo in ExploradoresEnemigos)
            {
                float enemigoDistancia = (exploradorEnemigo.transform.position - factPos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo está cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = exploradorEnemigo;
                }
            }
            //  Hay un destructor cerca de nuestra factoria ?
            foreach (DestructionUnit destructorEnemigo in DestructoresEnemigos)
            {
                float enemigoDistancia = (destructorEnemigo.transform.position - factPos).magnitude;
                if (enemigoDistancia < distancia && enemigoDistancia < enemigoCercano) //Un enemigo está cerca de la base
                {
                    enemigoCercano = enemigoDistancia;
                    enemigo = destructorEnemigo;
                }
            }
            return enemigo;
        }
        #endregion

        #region gestion_de_estrategias
        //  Determina que estrategía usar
        private void eligeEstrategia()
        {
            //TO DO: ajustar valores de dinero y tal :)
            int money = RTSGameManager.Instance.GetMoney(MyIndex);
            bool supremaciaEconomica = money > 30000 + RTSGameManager.Instance.GetMoney(FirstEnemyIndex);
            bool supremaciaMilitar = false;

            //TO DO: Ajustar aqui tb
            if (getTotalPrioAliada() > getTotalPrioEnemiga() + 6)
            {
                supremaciaMilitar = true;
            }

            Unit enemigoEnBase = amenazaBase();
            Unit enemigoEnFactoria = amenazaFactoria();
            bool amenazaCercana = enemigoEnBase != null || enemigoEnFactoria != null;

            //  Si no podemos farmear la única opción es un ataque final 
            if (money < RTSGameManager.Instance.ExtractionUnitCost && MisExtractores.Count <= 0)
            {
                currEstrategia = Estrategia.Ofensivo;
            }
            //  Si estamos atacando y nos quedamos sin unidades ofensivas
            else if (currEstrategia == Estrategia.Ofensivo && MisDestructores.Count + MisExploradores.Count == 0)
            {
                currEstrategia = Estrategia.Farming;
            }
            //  En el caso de que estemos en emergencia y nuestro enemigo no tenga unidades -> atacamos el nexo
            else if (currEstrategia == Estrategia.Emergencia && (DestructoresEnemigos.Count + ExploradoresEnemigos.Count) == 0
                && (MisDestructores.Count > 0 || MisExploradores.Count > 0))
            {
                currEstrategia = Estrategia.Ofensivo;
            }
            //Prioridad absoluta al estado de emergencia cuando no contamos con pocas unidades de unidades
            else if (MisDestructores.Count + MisExploradores.Count < 3 && currEstrategia != Estrategia.Ofensivo)
            {
                //Si además contamos con pocos exploradores, priorizamos su compra pasando al estado farming
                if (MisExtractores.Count <= 1) currEstrategia = Estrategia.Farming;
                //Si no, entramos directamente en emergencia
                else
                {
                    currEstrategia = Estrategia.Emergencia;
                }
            }
            //amenazas cerca de la base o de la factoria
            else if (amenazaCercana)
            {
                currEstrategia = Estrategia.Defensivo;
            }
            //Si mi enemigo se ha quedado sin unidades o tiene pocas ofensivas pasamos a modo full ofensivo
            else if (ExploradoresEnemigos.Count + DestructoresEnemigos.Count <= 2)
            {
                currEstrategia = Estrategia.Ofensivo;
            }
            //Cuando no contamos ni con las unidades ni con los recursos necesarios
            //para un ataque decente, pero no nos encontramos bajo amenaza
            else if (!supremaciaMilitar && (!supremaciaEconomica || MisExtractores.Count <= 1))
            {
                currEstrategia = Estrategia.Farming;
            }
            //En Guerrilla se prioriza atacar a los extractores y a la factoria enemiga, por lo tanto si no hay extractores
            //o factoria enemiga no tiene sentido atacarla
            else if (supremaciaMilitar && !supremaciaEconomica &&
                FactoriaEnemiga.Count > 0 && ExtractoresEnemigos.Count > 0)
            {
                currEstrategia = Estrategia.Guerrilla;
            }
            //En el caso que tengamos ventaja en todo pasamos al modo full ofensivo
            else if (supremaciaMilitar)
            {
                currEstrategia = Estrategia.Ofensivo;
            }

        }
        //  Gestiona la estrategía de defensa
        //  Defiende en el orden de la base > factoria
        private void gestionaDefensa()
        {
            Unit enemigoEnFactoria = amenazaFactoria();
            Unit enemigoEnBase = amenazaBase();

            //  Ambas están en peligro
            if (enemigoEnBase && enemigoEnFactoria)
            {
                if (MisDestructores.Count > 0)
                {
                    int numdest = MisDestructores.Count;
                    int porcentaje = (int)(numdest * 0.5);
                    //  Envío el 50% de los destructores a defender la base
                    if (MiBase.Count > 0)
                    {
                        for (int i = 0; i < porcentaje; i++)
                        {
                            Transform obj = enemigoEnBase.transform;
                            Vector3 dist = MisDestructores[i].transform.position - obj.position;
                            if (MisDestructores[i].Radius < dist.magnitude)
                            {
                                MisDestructores[i].Move(this, enemigoEnBase.transform);
                            }
                            else MisDestructores[i].Attack(this, enemigoEnBase.transform);
                        }
                    }
                    //  Envío el otro 50% a desfender la amenaza a la factoria
                    if (MiFactoria.Count > 0)
                    {
                        for (int j = porcentaje; j < (int)(numdest * 0.5); j++)
                        {
                            Vector3 dist = MisDestructores[j].transform.position - enemigoEnFactoria.transform.position;
                            if (MisDestructores[j].Radius < dist.magnitude)
                                MisDestructores[j].Move(this, enemigoEnFactoria.transform);
                            else MisDestructores[j].Attack(this, enemigoEnFactoria.transform);
                        }
                    }
                }

                if (MisExploradores.Count > 0)
                {
                    int numdest = MisExploradores.Count;
                    int porcentaje = (int)(numdest * 0.3); //30%
                    if (MiBase.Count > 0)
                    {
                        for (int i = 0; i < porcentaje; i++)
                        {
                            Transform obj = enemigoEnBase.transform;
                            Vector3 dist = MisExploradores[i].transform.position - obj.position;
                            if (MisExploradores[i].Radius < dist.magnitude)
                            {
                                MisExploradores[i].Move(this, obj);
                            }
                            else MisExploradores[i].Stop(this);
                        }
                    }

                    if (MiFactoria.Count > 0)
                    {
                        for (int j = porcentaje; j < (int)(numdest * 0.5); j++) //20% de los exploradores
                        {
                            Vector3 dist = MisExploradores[j].transform.position - enemigoEnFactoria.transform.position;
                            if (MisExploradores[j].Radius < dist.magnitude)
                                MisExploradores[j].Move(this, enemigoEnFactoria.transform);
                            else MisExploradores[j].Stop(this);

                        }
                    }
                }
            }
            //  Solo la factoria está en peligro
            else if (enemigoEnFactoria)
            {
                // Envío el 50% de los destructores
                if (MisDestructores.Count > 0 && MiFactoria.Count > 0)
                {
                    int numdest = MisDestructores.Count;
                    int porcentaje = (int)(numdest * 0.5);
                    for (int i = 0; i < porcentaje; i++)
                    {
                        Transform obj = enemigoEnFactoria.transform;
                        Vector3 dist = MisDestructores[i].transform.position - obj.position;
                        if (MisDestructores[i].Radius < dist.magnitude)
                        {
                            MisDestructores[i].Move(this, obj);
                        }
                        else MisDestructores[i].Attack(this, obj);
                    }
                }
                // Envío el 50% de los exploradores
                if (MisExploradores.Count > 0 && MiFactoria.Count > 0)
                {
                    int numdest = MisExploradores.Count;
                    int porcentaje = (int)(numdest * 0.5);

                    for (int i = 0; i < porcentaje; i++)
                    {
                        Transform obj = enemigoEnFactoria.transform;
                        Vector3 dist = MisExploradores[i].transform.position - obj.position;
                        if (MisExploradores[i].Radius < dist.magnitude)
                        {
                            MisExploradores[i].Move(this, obj);
                        }
                        else MisExploradores[i].Stop(this);

                    }
                }
            }
            //Solo la base está en peligro
            else if (enemigoEnBase)
            {
                //  Envío el 50% de los destructores a atacar al enemigo en base aliada
                if (MisDestructores.Count > 0 && MiBase.Count > 0)
                {
                    int numdest = MisDestructores.Count;
                    int porcentaje = (int)(numdest * 0.5);
                    for (int i = 0; i < porcentaje; i++)
                    {
                        Transform obj = enemigoEnBase.transform;
                        Vector3 dist = MisDestructores[i].transform.position - obj.position;
                        if (MisDestructores[i].Radius < dist.magnitude)
                        {
                            MisDestructores[i].Move(this, obj);
                        }
                        else MisDestructores[i].Attack(this, obj);
                    }
                }

                //  Envío el 50% de los exploradores a atacar al enemigo en base aliada
                if (MisExploradores.Count > 0 && MiBase.Count > 0)
                {
                    int numdest = MisExploradores.Count;
                    int porcentaje = (int)(numdest * 0.5);

                    for (int i = 0; i < porcentaje; i++)
                    {
                        Transform obj = enemigoEnBase.transform;
                        Vector3 dist = MisExploradores[i].transform.position - obj.position;
                        if (MisExploradores[i].Radius < dist.magnitude)
                        {
                            MisExploradores[i].Move(this, obj);
                        }
                        //else MisExploradores[i].Stop(this);

                    }

                }
            }

        }
        //  Gestiona la estrategía de ataque
        private void gestionaAtaque()
        {
            if (MisDestructores.Count > 0)
            {
                int numdest = MisDestructores.Count;
                int porcentaje = (int)(numdest * 0.6);
                //  Si existe una prioridad enemiga -> enviamos 60% de los destructores a esa prio
                if (MapManager.GetInstance().getEnemyMaxPrio(myType)
                    || MapManager.GetInstance().getEnemyPrio(myType) > 0)
                {
                    for (int i = 0; i < porcentaje; i++)
                    {
                        if (MapManager.GetInstance().getEnemyMaxPrio(myType))
                        {
                            Transform obj = MapManager.GetInstance().getEnemyMaxPrio(myType).transform;
                            Vector3 dist = MisDestructores[i].transform.position - obj.position;
                            if (MisDestructores[i].Radius < dist.magnitude)
                            {
                                MisDestructores[i].Move(this, obj);
                            }
                            else MisDestructores[i].Attack(this, obj);

                        }
                    }
                    //  Enviamos el 40% a la base enemiga de los destructores
                    for (int j = porcentaje; j < numdest; j++)
                    {
                        if (BaseEnemiga.Count > 0)
                        {
                            Vector3 dist = MisDestructores[j].transform.position - BaseEnemiga[0].transform.position;
                            if (MisDestructores[j].Radius / 2 < dist.magnitude)
                                MisDestructores[j].Move(this, BaseEnemiga[0].transform);
                            else MisDestructores[j].Attack(this,BaseEnemiga[0].transform);
                        }
                        else continue;

                    }
                }
                else
                {   //  Enviamos a todos los destructores a la base enemiga
                    for (int j = 0; j < numdest; j++)
                    {
                        if (BaseEnemiga.Count > 0)
                        {
                            Vector3 dist = MisDestructores[j].transform.position - BaseEnemiga[0].transform.position;
                            if (MisDestructores[j].Radius / 2 < dist.magnitude)
                                MisDestructores[j].Move(this, BaseEnemiga[0].transform);
                            else MisDestructores[j].Attack(this,BaseEnemiga[0].transform);
                        }
                        else continue;

                    }
                }
            }

            if (MisExploradores.Count != 0)
            {
                int numdest = MisExploradores.Count;
                int porcentaje = (int)(numdest * 0.6);
                //  Si existe una prioridad enemiga -> enviamos 60% de los exploradores a esa prio
                if (MapManager.GetInstance().getEnemyMaxPrio(myType)
                    || MapManager.GetInstance().getEnemyPrio(myType) > 0)
                {
                    for (int i = 0; i < porcentaje; i++)
                    {
                        if (MapManager.GetInstance().getEnemyMaxPrio(myType))
                        {
                            Transform obj = MapManager.GetInstance().getEnemyMaxPrio(myType).transform;
                            Vector3 dist = MisExploradores[i].transform.position - obj.position;
                            if (MisExploradores[i].Radius < dist.magnitude)
                                MisExploradores[i].Move(this, obj);
                            else MisExploradores[i].Stop(this);

                        }
                    }
                    //  Enviamos el 40% de los exploradores a la base enemiga
                    if (BaseEnemiga.Count > 0)
                    {
                        for (int j = porcentaje; j < numdest; j++)
                        {
                            Vector3 dist = MisExploradores[j].transform.position - BaseEnemiga[0].transform.position;
                            if (MisExploradores[j].Radius < dist.magnitude)
                                MisExploradores[j].Move(this, BaseEnemiga[0].transform);
                            else MisExploradores[j].Stop(this);
                        }
                    }
                }
                else
                {
                    //  Enviamos a todos los exploradores a la base enemiga
                    if (BaseEnemiga.Count > 0)
                    {
                        for (int j = 0; j < numdest; j++)
                        {
                            Vector3 dist = MisExploradores[j].transform.position - BaseEnemiga[0].transform.position;
                            if (MisExploradores[j].Radius < dist.magnitude)
                                MisExploradores[j].Move(this, BaseEnemiga[0].transform);
                            else MisExploradores[j].Stop(this);
                        }
                    }
                }

            }
        }
        //  Gestiona la estrategía de farmeo
        private void gestionaFarm()
        {
            //Destructores
            if (MisDestructores.Count != 0)
            {
                int numdest = MisDestructores.Count;
                int porcentaje = (int)(numdest * 0.5);
                for (int i = 0; i < porcentaje; i++)
                {
                    //  Se destina el 50% de los destructores para defender la factoria
                    Transform obj = MiFactoria[0].transform;
                    Vector3 dist = MisDestructores[i].transform.position - obj.position;
                    if (MisDestructores[i].Radius * 2 < dist.magnitude)
                    {
                        //MisDestructores[i].Move(this, obj);
                        RTSGameManager.Instance.MoveUnit(this, MisDestructores[i], obj);
                    }
                    //  Estoy en la factoria aliada
                    else
                    {
                        //  Hay una amenaza enemiga cerca de la factoria
                        Unit enemigoEnfactoria = amenazaFactoria();
                        if (enemigoEnfactoria)
                        {
                            Debug.Log("Amenaza en en la factoria");
                            Transform obj1 = enemigoEnfactoria.transform;
                            Vector3 dist1 = MisDestructores[i].transform.position - obj1.position;
                            if (MisDestructores[i].Radius < dist1.magnitude)
                            {
                                RTSGameManager.Instance.MoveUnit(this, MisDestructores[i], obj);
                            }
                            else
                            {
                                MisDestructores[i].Stop(this);
                                MisDestructores[i].Attack(this, enemigoEnfactoria.transform);
                            }
                        }
                        else MisDestructores[i].Stop(this);
                    }
                }
                int e = 0;
                //  Proteger mis extractores con destructores
                if (MisExtractores.Count > 0)
                {
                    for (int j = porcentaje; j < numdest; j++)
                    {
                        if (e >= MisExtractores.Count) e = 0;
                        Transform obj = MisExtractores[e].getExtractor().transform;
                        Vector3 dist = MisDestructores[j].transform.position - obj.position;
                        if (MisDestructores[j].Radius * 10 < dist.magnitude)
                        {
                            //MisDestructores[j].Move(this, dist);
                            RTSGameManager.Instance.MoveUnit(this, MisDestructores[j], obj);

                        }
                        //  Amenazas cerca de un extractor
                        else
                        {
                            //Unit enemigoEnExtractor = amenazaExtractor();
                            //if (enemigoEnExtractor)
                            //{
                            //    Debug.Log("Enemigo cerca de un extractor");
                            //    Transform obj1 = enemigoEnExtractor.transform;
                            //    Vector3 dist1 = MisDestructores[j].transform.position - obj1.position;
                            //    //  Me muevo a la posición de esa amenaza
                            //    if (MisDestructores[j].Radius < dist1.magnitude)
                            //    {
                            //        RTSGameManager.Instance.MoveUnit(this, MisDestructores[j], obj);
                            //        //MisDestructores[j].Move(this, obj1);
                            //    }
                            //    //  Estoy a rango de ataque de esa amenaza y ataco
                            //    else MisDestructores[j].Attack(this, enemigoEnExtractor.transform);
                            //}
                            ////  No hay amenazas cercanas al extractor
                            //else MisDestructores[j].Stop(this);
                            MisDestructores[j].Stop(this);
                        }
                        e++;
                    }
                }
                //No tengo extractores que defender -> defiendo base
                else
                {
                    for (int j = porcentaje; j < numdest; j++)
                    {
                        if (e >= MisExtractores.Count) e = 0;
                        Transform obj = MiBase[0].transform;
                        Vector3 dist = MisDestructores[j].transform.position - obj.position;
                        //  Protegiendo a los extractores
                        if (MisDestructores[j].Radius * 2 < dist.magnitude)
                        {
                            RTSGameManager.Instance.MoveUnit(this, MisDestructores[j], obj);

                            //MisDestructores[j].Move(this, obj);
                        }
                        else
                        {   // Hay alguna amenaza cerca de los extractores?
                            Unit enemigoEnBase = amenazaBase();
                            Transform obj1 = enemigoEnBase.transform;
                            Vector3 dist1 = MisDestructores[j].transform.position - obj1.position;
                            if (enemigoEnBase)
                            {
                                if (MisDestructores[j].Radius < dist1.magnitude)
                                {
                                    //MisDestructores[j].Move(this, obj1);
                                    RTSGameManager.Instance.MoveUnit(this, MisDestructores[j], obj);

                                }
                                else
                                    MisDestructores[j].Attack(this, enemigoEnBase.transform);
                            }
                            else MisDestructores[j].Stop(this);
                        }
                        e++;
                    }
                }

            }
            //  Exploradores
            if (MisExploradores.Count != 0)
            {
                int numdest = MisExploradores.Count;
                int porcentaje = (int)(numdest * 0.5);
                for (int i = 0; i < porcentaje; i++)
                {
                    MisExploradores[i].Move(this, MiFactoria[0].transform);
                }
                int e = 0;
                if (MisExtractores.Count > 0)
                {
                    for (int j = porcentaje; j < numdest; j++)
                    {
                        if (e >= MisExtractores.Count) e = 0;
                        Transform obj = MisExtractores[e].getExtractor().transform;
                        Vector3 dist = MisExploradores[j].transform.position - obj.position;
                        Debug.Log(MisExploradores[j].Radius);
                        if (MisExploradores[j].Radius * 10 < dist.magnitude)
                        {
                            RTSGameManager.Instance.MoveUnit(this, MisExploradores[j], obj);
                            //MisExploradores[j].move(this, obj);
                        }
                        else
                        {
                            Unit enemigoEnExtractor = amenazaExtractor();
                            if (enemigoEnExtractor)
                            {
                                if (e >= MisExtractores.Count) e = 0;
                                Transform obj1 = enemigoEnExtractor.transform;
                                Vector3 dist1 = MisExploradores[j].transform.position - obj1.position;
                                if (MisExploradores[j].Radius < dist1.magnitude)
                                {
                                    MisExploradores[j].Move(this, enemigoEnExtractor.transform);
                                }
                                else MisExploradores[j].Stop(this);
                            }
                            else MisExploradores[j].Stop(this);
                        }
                        e++;
                    }
                }
                //  Defender la base
                else
                {
                    for (int j = porcentaje; j < numdest; j++)
                    {
                        if (e >= MisExtractores.Count) e = 0;
                        Transform obj = MiBase[0].transform;
                        Vector3 dist = MisExploradores[j].transform.position - obj.position;
                        if (MisExploradores[j].Radius * 2 < dist.magnitude)
                        {
                            MisExploradores[j].Move(this, obj);
                        }
                        else
                        {
                            Unit enemigoEnExtractor = amenazaBase();
                            if (enemigoEnExtractor)
                            {
                                if (e >= MisExtractores.Count) e = 0;
                                Transform obj1 = enemigoEnExtractor.transform;
                                Vector3 dist1 = MisExploradores[j].transform.position - obj1.position;
                                if (MisExploradores[j].Radius < dist1.magnitude)
                                {
                                    MisExploradores[j].Move(this, enemigoEnExtractor.transform);
                                }
                                else MisExploradores[j].Stop(this);
                            }
                            else MisExploradores[j].Stop(this);
                        }
                        e++;
                    }
                }

            }
        }
        //  Gestiona la estrategía de guerrilla
        private void gestionaGuerrilla()
        {
            if (MisDestructores.Count != 0)
            {
                int numdest = MisDestructores.Count;
                int porcentaje = (int)(numdest * 0.5);
                if (FactoriaEnemiga.Count > 0)
                {
                    for (int i = 0; i < porcentaje; i++)
                    {
                        MisDestructores[i].Move(this, FactoriaEnemiga[0].transform.position);
                    }
                }
                int e = 0;

                if (ExtractoresEnemigos.Count > 0)
                {
                    for (int j = porcentaje; j < numdest; j++)
                    {
                        if (e >= ExtractoresEnemigos.Count) e = 0;
                        MisDestructores[j].Move(this, ExtractoresEnemigos[e].transform.position);
                        e++;
                    }
                }
            }

            if (MisExploradores.Count != 0)
            {
                int numdest = MisExploradores.Count;
                int porcentaje = (int)(numdest * 0.5);
                if (FactoriaEnemiga.Count > 0)
                {
                    for (int i = 0; i < porcentaje; i++)
                    {
                        MisExploradores[i].Move(this, FactoriaEnemiga[0].transform.position);
                    }
                }
                if (ExtractoresEnemigos.Count > 0)
                {
                    int e = 0;
                    for (int j = porcentaje; j < numdest; j++)
                    {
                        if (e >= ExtractoresEnemigos.Count) e = 0;
                        MisExploradores[j].Move(this, ExtractoresEnemigos[e].transform.position);
                        e++;
                    }
                }
            }
        }

        //  A nivel de unidades emergencia hace lo mismo que farming
        private void gestionaEmergencia()
        {
            gestionaFarm();
        }
        #endregion

        #region gestion_de_compras
        //  Determina qué unidad comprar
        private void gestionaCompra()   //TODO falta el modo emergencia
        {
            switch (currEstrategia)
            {
                case Estrategia.Ofensivo:
                    compraOfensiva();
                    break;

                case Estrategia.Defensivo:
                    compraDefensiva();
                    break;

                case Estrategia.Guerrilla:
                    compraGuerrilla();
                    break;

                case Estrategia.Farming:
                    compraFarming();
                    break;

                case Estrategia.Emergencia:
                    compraEmergencia();
                    break;
            }

        }

        //  Priorizamos la compra de exploradores ante el bajo número de unidades
        private void compraEmergencia()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);

            if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                MisDestructores.Count < RTSGameManager.Instance.DestructionUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();
            }
            else if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                MisExploradores.Count < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();
            }
        }

        private void compraOfensiva()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);

            bool equilibrio = true;
            if ((MisDestructores.Count) > (MisExploradores.Count) + 2)
            {
                equilibrio = false;
            }
            if (equilibrio && myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                MisDestructores.Count < RTSGameManager.Instance.DestructionUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();

            }
            else if (!equilibrio && myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                MisExploradores.Count < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();

            }
            else if (myMoney >= RTSGameManager.Instance.ExtractionUnitCost &&
                MisExtractores.Count < RTSGameManager.Instance.ExtractionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                RTSGameManager.Instance.MoveUnit(this, actExtractor.getExtractor(), getMelangeToFarm(MiFactoria[0].transform.position).transform.position);
            }
        }

        private void compraFarming()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            bool equilibrio = true;

            bool exploradoresSuficientes = false;

            exploradoresSuficientes = MisExploradores.Count >= RTSGameManager.Instance.ExplorationUnitsMax / 2;
            equilibrio = MisExtractores.Count > 3;


            if ((!equilibrio || exploradoresSuficientes) && myMoney >= RTSGameManager.Instance.ExtractionUnitCost &&
                MisExtractores.Count < RTSGameManager.Instance.ExtractionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                RTSGameManager.Instance.MoveUnit(this, actExtractor.getExtractor(), getMelangeToFarm(MiFactoria[0].transform.position).transform.position);
            }
            else if ((equilibrio || !exploradoresSuficientes) && myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                MisExploradores.Count < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();

            }
        }

        private void compraDefensiva()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            bool pocosExtractores = MisExtractores.Count < minDesiredExtractors;
            bool muchosExploradores = MisExploradores.Count >= RTSGameManager.Instance.ExplorationUnitsMax / 3;
            bool muchosDestructores = MisDestructores.Count >= RTSGameManager.Instance.DestructionUnitsMax / 3;

            //Priorizamos la compra de exploradores sobre los destructores
            if (!pocosExtractores && !muchosExploradores && myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                    MisExploradores.Count < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();

            }
            else if (!pocosExtractores && !muchosDestructores && myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                    MisDestructores.Count < RTSGameManager.Instance.DestructionUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();

            }
            else if (pocosExtractores && myMoney >= RTSGameManager.Instance.ExtractionUnitCost &&
                MisExtractores.Count < RTSGameManager.Instance.ExtractionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                RTSGameManager.Instance.MoveUnit(this, actExtractor.getExtractor(), getMelangeToFarm(MiFactoria[0].transform.position).transform.position);
            }
        }

        private void compraGuerrilla()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            //Priorizamos la compra de destructores sobre los exploradores
            if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                    MisExploradores.Count < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();

            }
            else if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                    MisDestructores.Count < RTSGameManager.Instance.DestructionUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();

            }
            else if (myMoney >= RTSGameManager.Instance.ExtractionUnitCost &&
                MisExtractores.Count < RTSGameManager.Instance.ExtractionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                RTSGameManager.Instance.MoveUnit(this, actExtractor.getExtractor(), getMelangeToFarm(MiFactoria[0].transform.position).transform.position);
            }
        }
        #endregion

    }
}
