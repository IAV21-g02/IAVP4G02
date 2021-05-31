using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace es.ucm.fdi.iav.rts.g02
{
    class Mision : IComparable<Mision>
    {
        // misión que debe cumplir este batallon
        Comando actMision;
        //  Transform de la posición a la que ir
        Transform objetivo;
        //  Priridad
        int prio;
        // 
        bool misionStatus = false;
        public Mision(Comando cmd_ , Transform objetivo_ ,int prio_)
        {
            this.actMision = cmd_;
            this.objetivo = objetivo_;
            this.prio = prio_;
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

        public override bool Equals(object obj)
        {
            Mision other = (Mision)obj;
            return Equals(other);
        }

        public bool misionCompletada()
        {
            return misionStatus;
        }

        //public override int GetHashCode()
        //{           
        //    return this
        //}
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
        //  Determina si este batallon está en producción
        public bool construyendo;
        //  Determina si este batallon está en una misión
        public bool enMision;

        public Batallon(TipoBatallon tipoBatallon_,Mision currMision)
        {
            tipoBatallon = tipoBatallon_;
            construyendo = true;
            completado = false;
            enMision = false;

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
            construyendo = true;
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

        //  Asigna una nueva misión a un batallón
        public void asignaMision(Mision nueMision)
        {
            mision = nueMision;
            enMision = true;
        }

        // desmonta un batallón para agregarlo a una lista de unidades sin batallón
        private void desmontarBatallon(List<Unit> unidadesSinBatallon)
        {
            foreach (ExtractionUnit unit_ in extractores)
            {
                unidadesSinBatallon.Add(unit_);
                extractores.Remove(unit_);
            }

            foreach (DestructionUnit unit_ in destructores)
            {
                unidadesSinBatallon.Add(unit_);
                destructores.Remove(unit_);
            }

            foreach (ExplorationUnit unit_ in exploradores)
            {
                unidadesSinBatallon.Add(unit_);
                exploradores.Remove(unit_);
            }
        }
    }

    public enum TipoBatallon
    {
        //  Dos destructores
        BatallonTiwardo,
        //  Dos exploradores y un destructor
        BatallonDobleDesayuno,
        //  Dos exploradores
        BatallonAurgar, //...
    }


    //  Estado que se encarga de gestionar la ofensiva de la IA
    public enum Comando
    {   
        //  Comandos de caracter ofensivo
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
        //  No hacer nada
        Festivo,

        //  Comandos de caracter defensivo
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
        Farming,Defensivo,Ofensivo
    }

    public class RTSAIControllerG02 : RTSAIController
    {
        private int MyIndex { get; set; }
        private Type myType;
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

        //  
        private List<CasillaPrioDefensa> prioDefensa;
        private List<CasillaPrioMilitar> prioMilitar;

        // Las listas completas de accesos limitados y torretas 
        private List<LimitedAccess> Recursos;
        private List<Tower> Torretas;

        //Indice correspondiente a mi enemigo
        int FirstEnemyIndex;

        // Número de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;

        public Estrategia currEstrategia;

        private List<Mision> misMisiones;

        private UnidadAComprar unidadAComprar;
        //  Lista de batallones disponibles
        private List<Batallon> batallones;
        //  Lista de misiones a completar
        private Priority_Queue<Mision> misiones;
        
        //  Unidades que no tienen un batallon asignado
        private List<Unit> unidadesSinBatallon;

        private int dineroSuficiente = 60000;

        // Última unidad creada
        private Unidad LastUnit { get; set; }

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
            
            //TO DO:Actualizar tareas que estan realizando nuestros batallones y reasignarselas si es necesario


            //TO DO: Si nos han sobrado tareas por realizar, tratamos de comprar mas batallones para encargarles dichas tareas
            ShoppingManagement();

            #region CONTROLLER 3
            //// Variables auxiliares
            //int rand = 0;
            //int probability = 0;

            //// Escojo el enumerado de movimiento correspondiente a un índice que iré variando
            //// (esto habría sido más elegante hacerlo con una lista, pero nos interesaba que fueran enumerados por si alguien los quiere usar desde fuera)
            //switch (Moves[nextMove])
            //{
            //    case PosibleMovement.MoveRandomExtraction:
            //        if (MisExtractores != null && MisExtractores.Count > 0)
            //        {
            //            // Mover unidades extractoras suele ser muy mala idea, por eso sólo lo hago 1 de cada 10 veces
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
            //            // Mover unidades extractoras suele ser muy mala idea, por eso sólo lo hago 1 de cada 10 veces
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
            //            // Mover unidades extractoras suele ser muy mala idea, por eso sólo lo hago 1 de cada 10 veces
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

            //// Nuestra política es muy tonta: voy recorriendo todos los tipos de movimiento que conozco, haciendo uno cada vez
            //nextMove = (nextMove + 1) % Moves.Count;
            //// Con los objetivos, la política es igual de estúpida
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

        //  Determina qué unidad comprar
        private void gestionaCompra()
        {
            enEmergencia();

            if (MisExploradores.Count < ExploradoresEnemigos.Count + 2 && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExplorationUnitCost
                && MisExploradores.Count < RTSGameManager.Instance.ExplorationUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.EXPLORATION);
            }
            else if (MisDestructores.Count < DestructoresEnemigos.Count + 2 && RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.DestructionUnitCost
                && MisDestructores.Count < RTSGameManager.Instance.DestructionUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, MiBase[0], RTSGameManager.UnitType.DESTRUCTION);
            }

        }

        private void gestionaDefensa()
        {
            if (batallones.Count == 0) return;

        }

        private void gestionaAtaque()
        {
            if (batallones.Count == 0) return;
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

        public Type getMyType()
        {
            return myType;
        }

        //  Actualiza las prioridades de defensa y ataque del mapa de influencias
        public void gestionaPrioridades()
        {
            prioMilitar = new List<CasillaPrioMilitar>();
            foreach (DestructionUnit unit in DestructoresEnemigos)
            {
                CasillaBehaviour currCasilla = MapManager.getInstance().getCasillaCercana(unit.transform);
                if (!prioMilitar.Contains(currCasilla.getCasillaPrioMilitar()))
                {
                    prioMilitar.Add(currCasilla.getCasillaPrioMilitar());
                }
            }

            foreach (ExplorationUnit unit in ExploradoresEnemigos)
            {
                CasillaBehaviour currCasilla = MapManager.getInstance().getCasillaCercana(unit.transform);
                if (!prioMilitar.Contains(currCasilla.getCasillaPrioMilitar()))
                {
                    prioMilitar.Add(currCasilla.getCasillaPrioMilitar());
                }
            }

            ///////////////////////////
            prioDefensa = new List<CasillaPrioDefensa>();
            foreach (ExtractionUnit unit in MisExtractores)
            {
                CasillaBehaviour currCasilla = MapManager.getInstance().getCasillaCercana(unit.transform);
                if (!prioDefensa.Contains(currCasilla.getCasillaPrioDefensa()))
                {
                    prioDefensa.Add(currCasilla.getCasillaPrioDefensa());
                }
            }
            prioDefensa.Add(MapManager.getInstance().getCasillaCercana(MiBase[0].transform).getCasillaPrioDefensa());
            prioDefensa.Add(MapManager.getInstance().getCasillaCercana(MiFactoria[0].transform).getCasillaPrioDefensa());

            prioMilitar.Sort();
            prioDefensa.Sort();
        }

        private void creaMisiones()
        {
            bool estoyPuerco = RTSGameManager.Instance.GetMoney(MyIndex) > 100000 + RTSGameManager.Instance.GetMoney(FirstEnemyIndex);
            bool estoyMasao = MisDestructores.Count + MisExploradores.Count >= DestructoresEnemigos.Count + ExploradoresEnemigos.Count;

            switch (currEstrategia)
            {
                case Estrategia.Ofensivo:
                    Mision currMision;
                    if (estoyPuerco && estoyMasao)
                    {
                        currMision = new Mision(Comando.AtaqueAlNexo, BaseEnemiga[0].transform, 99);
                        misMisiones.Add(currMision);
                    }
                    else if (estoyPuerco && !estoyMasao) //Tengo más dinero pero tengo menos unidades
                    {

                        currMision = new Mision(Comando.AtaqueMenorPrio, prioMilitar[prioMilitar.Count - 1].getCasilla().transform, 60);
                        misMisiones.Add(currMision);

                    }
                    else if (!estoyPuerco && estoyMasao)
                    {
                        currMision = new Mision(Comando.ataqueExtractor, ExtractoresEnemigos[0].transform, 55);
                        misMisiones.Add(currMision);

                    }
                    else
                    {
                        currMision = new Mision(Comando.AtaqueMenorPrio, prioMilitar[prioMilitar.Count - 1].getCasilla().transform, 60);
                        misMisiones.Add(currMision);
                        currMision = new Mision(Comando.DefiendeRecurso, MisExtractores[0].transform, 54);
                        misMisiones.Add(currMision);
                    }
                    misMisiones.Sort();
                    break;
                case Estrategia.Farming:
                    break;
                case Estrategia.Defensivo:
                    break;
                default:
                    break;
                
            }
        }

        private Batallon creaBatallon(TipoBatallon tipoBatallon,Mision currMision)
        {
            return new Batallon(tipoBatallon, currMision);
        }

        private void eligeMision()
        {
            if (misMisiones.Count > 0)
            {

            }
        }
    }
}
