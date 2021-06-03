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

        private void Awake()
        {
            Name = "IAV21G02";
            Author = "IAV21G02";
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

            //Inicializamos a parte la lista de los extractores para gestionar mejor su movimiento porque
            //si no, cuando hay varios que van a la misma casilla para extraer recursos se suelen quedar
            //"pillados"
            MisExtractores = new List<Extractor>();
            ActualizeGameElements();

            //Envíamos a los estractores que ya tenemos en juego a su objetivo de extraer recursos
            //teniendo en cuenta lo previamente mentado.
            gestionaExtractores();

            estrategiaAnt = Estrategia.NONE;

            //Pasamos a AIGameLoop()
            ThinkStepNumber++;

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

            }

            //currEstrategia = Estrategia.Ofensivo;

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
            }


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
            //MisExtractores = RTSGameManager.Instance.GetExtractionUnits(MyIndex);

            BaseEnemiga = RTSGameManager.Instance.GetBaseFacilities(FirstEnemyIndex);
            FactoriaEnemiga = RTSGameManager.Instance.GetProcessingFacilities(FirstEnemyIndex);
            ExtractoresEnemigos = RTSGameManager.Instance.GetExtractionUnits(FirstEnemyIndex);
            ExploradoresEnemigos = RTSGameManager.Instance.GetExplorationUnits(FirstEnemyIndex);
            DestructoresEnemigos = RTSGameManager.Instance.GetDestructionUnits(FirstEnemyIndex);

            Torretas = RTSScenarioManager.Instance.Towers;
        }


        //  Devuelve la melange más cerca de una posición
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

        //  Determina qué unidad comprar
        private void gestionaCompra()   //TODO falta el modo emergencia
        {
            Debug.Log(RTSGameManager.Instance.GetMoney(MyIndex));
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

                    break;
            }

        }

        private void compraOfensiva()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);

            //Priorizamos la compra de destructores sobre los exploradores

            bool equilibrio = true;
            if ((MisDestructores.Count) > (MisExploradores.Count) + 2)
            {
                equilibrio = false;
            }
            if (equilibrio && myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();

            }
            else if (!equilibrio && myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();

            }
            else if (myMoney >= RTSGameManager.Instance.ExtractionUnitCost && MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax &&
                MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                MisExtractores.Add(actExtractor);
                RTSGameManager.Instance.MoveUnit(this, MisExtractores[MisExtractores.Count - 1].getExtractor(), getMelangeCercana(MiFactoria[0].transform.position).transform.position);
            }
        }

        private void compraFarming()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
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
            else if (!equilibrio && myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();

            }
        }

        private void compraDefensiva()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            //Priorizamos la compra de exploradores sobre los destructores
            if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                    MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();

            }
            else if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                    MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
            {
                Unit currUnit = RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();

            }
            else if (myMoney >= RTSGameManager.Instance.ExtractionUnitCost && MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax &&
                MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                MisExtractores.Add(actExtractor);
                RTSGameManager.Instance.MoveUnit(this, MisExtractores[MisExtractores.Count - 1].getExtractor(), getMelangeCercana(MiFactoria[0].transform.position).transform.position);
            }
        }

        private void compraGuerrilla()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            //Priorizamos la compra de destructores sobre los exploradores
            if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost &&
                    MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();

            }
            else if (myMoney >= RTSGameManager.Instance.DestructionUnitCost &&
                    MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();

            }
            else if (myMoney >= RTSGameManager.Instance.ExtractionUnitCost && MisExploradores.Count - 1 < RTSGameManager.Instance.ExplorationUnitsMax &&
                MisDestructores.Count - 1 < RTSGameManager.Instance.DestructionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                MisExtractores.Add(actExtractor);
                RTSGameManager.Instance.MoveUnit(this, MisExtractores[MisExtractores.Count - 1].getExtractor(), getMelangeCercana(MiFactoria[0].transform.position).transform.position);
            }
        }

        private void gestionaDefensa()
        {
            Unit enemigoEnFactoria = amenazaFactoria();
            Unit enemigoEnBase = amenazaBase();


            //Ambas están en peligro
            if (enemigoEnBase && enemigoEnFactoria)
            {
                if (MisDestructores.Count > 0)
                {
                    int numdest = MisDestructores.Count;
                    int porcentaje = (int)(numdest * 0.5); //50% de las tropas destructoras a la base
                    if (MiBase.Count > 0)
                    {
                        for (int i = 0; i < porcentaje; i++)
                        {

                            Transform obj = MiBase[0].transform;
                            Vector3 dist = MisDestructores[i].transform.position - obj.position;
                            if (MisDestructores[i].Radius < dist.magnitude)
                            {
                                MisDestructores[i].Move(this, obj);
                            }
                            else MisDestructores[i].Stop(this);

                        }
                    }

                    if (MiFactoria.Count > 0)
                    {
                        for (int j = porcentaje; j < (int)(numdest * 0.7); j++) //20% de las tropas destructoras
                        {
                            Vector3 dist = MisDestructores[j].transform.position - MiFactoria[0].transform.position;
                            if (MisDestructores[j].Radius < dist.magnitude)
                                MisDestructores[j].Move(this, MiBase[0].transform);
                            else MisDestructores[j].Stop(this);

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
                            Transform obj = MiBase[0].transform;
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
                            Vector3 dist = MisExploradores[j].transform.position - MiFactoria[0].transform.position;
                            if (MisExploradores[j].Radius < dist.magnitude)
                                MisExploradores[j].Move(this, MiBase[0].transform);
                            else MisExploradores[j].Stop(this);

                        }
                    }
                }
            }
            //Solo la factoria está en peligro
            else if (enemigoEnFactoria)
            {
                if (MisDestructores.Count > 0 && MiFactoria.Count > 0)
                {
                    int numdest = MisDestructores.Count;
                    int porcentaje = (int)(numdest * 0.5);
                    for (int i = 0; i < porcentaje; i++)
                    {

                        Transform obj = MiFactoria[0].transform;
                        Vector3 dist = MisDestructores[i].transform.position - obj.position;
                        if (MisDestructores[i].Radius < dist.magnitude)
                        {
                            MisDestructores[i].Move(this, obj);
                        }
                        else MisDestructores[i].Stop(this);

                    }
                }

                if (MisExploradores.Count > 0 && MiFactoria.Count > 0)
                {
                    int numdest = MisExploradores.Count;
                    int porcentaje = (int)(numdest * 0.5);

                    for (int i = 0; i < porcentaje; i++)
                    {
                        Transform obj = MiFactoria[0].transform;
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
                if (MisDestructores.Count > 0 && MiBase.Count > 0)
                {
                    int numdest = MisDestructores.Count;
                    int porcentaje = (int)(numdest * 0.5);
                    for (int i = 0; i < porcentaje; i++)
                    {
                        Transform obj = MiBase[0].transform;
                        Vector3 dist = MisDestructores[i].transform.position - obj.position;
                        if (MisDestructores[i].Radius < dist.magnitude)
                        {
                            MisDestructores[i].Move(this, obj);
                        }
                        else MisDestructores[i].Stop(this);
                    }
                }

                if (MisExploradores.Count > 0 && MiBase.Count > 0)
                {
                    int numdest = MisExploradores.Count;
                    int porcentaje = (int)(numdest * 0.5);

                    for (int i = 0; i < porcentaje; i++)
                    {
                        Transform obj = MiBase[0].transform;
                        Vector3 dist = MisExploradores[i].transform.position - obj.position;
                        if (MisExploradores[i].Radius < dist.magnitude)
                        {
                            MisExploradores[i].Move(this, obj);
                        }
                        else MisExploradores[i].Stop(this);

                    }

                }
            }

        }

        private void gestionaAtaque()
        {
            if (MisDestructores.Count > 0)
            {
                int numdest = MisDestructores.Count;
                int porcentaje = (int)(numdest * 0.6);
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

                for (int j = porcentaje; j < numdest; j++)
                {
                    Vector3 dist = MisDestructores[j].transform.position - BaseEnemiga[0].transform.position;
                    if (MisDestructores[j].Radius < dist.magnitude)
                        MisDestructores[j].Move(this, BaseEnemiga[0].transform);
                    else MisDestructores[j].Attack(this, BaseEnemiga[0].transform);

                }
            }

            if (MisExploradores.Count != 0)
            {
                int numdest = MisExploradores.Count;
                int porcentaje = (int)(numdest * 0.6);
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

                for (int j = porcentaje; j < numdest; j++)
                {
                    Vector3 dist = MisExploradores[j].transform.position - BaseEnemiga[0].transform.position;
                    if (MisExploradores[j].Radius < dist.magnitude)
                        MisExploradores[j].Move(this, BaseEnemiga[0].transform);
                    else MisExploradores[j].Stop(this);
                }
            }
        }

        private void gestionaFarm() // TO DO parar las tropas cuando lleguen al objetivo
        {
            //si no tengo unidades no puedo hacer nada
            //if (MisDestructores.Count == 0 && MisExploradores.Count == 0) return;
            //Destructores
            if (MisDestructores.Count != 0)
            {
                int numdest = MisDestructores.Count;
                int porcentaje = (int)(numdest * 0.5);
                for (int i = 0; i < porcentaje; i++)
                {
                    MisDestructores[i].Move(this, MiFactoria[0].transform.position);
                }
                int e = 0;
                for (int j = porcentaje; j < numdest; j++)
                {
                    if (e >= MisExtractores.Count) e = 0;
                    MisDestructores[j].Move(this, MisExtractores[e].getExtractor().transform.position);
                    e++;
                }
            }

            if (MisExploradores.Count != 0)
            {
                int numdest = MisExploradores.Count;
                int porcentaje = (int)(numdest * 0.5);
                for (int i = 0; i < porcentaje; i++)
                {
                    MisExploradores[i].Move(this, MiFactoria[0].transform.position);
                }
                int e = 0;
                for (int j = porcentaje; j < numdest; j++)
                {
                    if (e >= MisExtractores.Count) e = 0;
                    MisExploradores[j].Move(this, MisExtractores[e].getExtractor().transform.position);
                    e++;
                }
            }
        }

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
        }//TO DO revisar

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

        private void eligeEstrategia()
        {
            //TO DO: ajustar valores de dinero y tal :)
            bool supremaciaEconomica = RTSGameManager.Instance.GetMoney(MyIndex) > 30000 + RTSGameManager.Instance.GetMoney(FirstEnemyIndex);
            bool supremaciaMilitar = false;

            //TO DO: Ajustar aqui tb
            if (getTotalPrioAliada() > getTotalPrioEnemiga() + 6)
            {
                supremaciaMilitar = true;
            }

            Unit enemigoEnBase = amenazaBase();
            Unit enemigoEnFactoria = amenazaFactoria();
            bool amenazaCercana = enemigoEnBase != null || enemigoEnFactoria != null;

            //Prioridad absoluta al estado de emergencia cuando no contamos con pocas unidades de unidades
            if (MisDestructores.Count + MisExploradores.Count < 3)
            {
                //Si además contamos con pocos exploradores, priorizamos su compra pasando al estado farming
                if (MisExploradores.Count <= 1) currEstrategia = Estrategia.Farming;
                //Si no, entramos directamente en emergencia
                else currEstrategia = Estrategia.Emergencia;
            }
            //amenazas cerca de la base o de la factoria
            else if (amenazaCercana)
            {
                Debug.Log("AMENAZA DETECTADA");
                currEstrategia = Estrategia.Defensivo;
            }
            //Si mi enemigo se ha quedado sin unidades o tiene pocas ofensivas pasamos a modo full ofensivo
            else if (ExploradoresEnemigos.Count + DestructoresEnemigos.Count <=2 )
            {
                Debug.Log("OFENSIVA");
                currEstrategia = Estrategia.Ofensivo;
            }
            //Cuando no contamos ni con las unidades ni con los recursos necesarios
            //para un ataque decente, pero no nos encontramos bajo amenaza
            else if (!supremaciaMilitar && (!supremaciaEconomica || MisExtractores.Count <= 1))
            {
                Debug.Log("FARMING");
                currEstrategia = Estrategia.Farming;
            }
            //En Guerrilla se prioriza atacar a los extractores y a la factoria enemiga, por lo tanto si no hay extractores
            //o factoria enemiga no tiene sentido atacarla
            else if (supremaciaMilitar && !supremaciaEconomica &&
                FactoriaEnemiga.Count > 0 && ExtractoresEnemigos.Count > 0)
            {
                Debug.Log("GUERRILLA");
                currEstrategia = Estrategia.Guerrilla;
            }
            //En el caso que tengamos ventaja en todo pasamos al modo full ofensivo
            else if (supremaciaMilitar)
            {
                Debug.Log("OFENSIVA");
                currEstrategia = Estrategia.Ofensivo;
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


    }
}
