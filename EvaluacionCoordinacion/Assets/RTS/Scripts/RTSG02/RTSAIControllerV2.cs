using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace es.ucm.fdi.iav.rts.g02
{
    //Nueva versión de la inteligencia artificial
    public class RTSAIControllerV2 : RTSAIController
    {
        // El estilo para las etiquetas de la interfaz
        private GUIStyle _labelStyle;
        private GUIStyle _labelSmallStyle;

        //Contenedor de corrutinas, auxiliar
        private IEnumerator coroutine;

        private int MyIndex { get; set; }
        private ColorTeam myType;

        private BaseFacility myBase { get; set; }
        private ProcessingFacility myFactory { get; set; }
        private BaseFacility enemyBase { get; set; }
        private ProcessingFacility enemyFactory { get; set; }

        // Mis listas completas de instalaciones y unidades
        private List<BaseFacility> baseList;
        private List<ProcessingFacility> factoryList;
        private List<ExtractionUnit> extractorList;
        private List<ExplorationUnit> explorerList;
        private List<DestructionUnit> destructorList;

        // Las listas completas de instalaciones y unidades del enemigo
        private List<BaseFacility> enemyBaseList;
        private List<ProcessingFacility> enemyFactoryList;
        private List<ExtractionUnit> enemyExtractorList;
        private List<ExplorationUnit> enemyExplorerList;
        private List<DestructionUnit> enemyDestructorList;

        private List<CasillaPrioDefensa> prioDefensa;
        private List<CasillaPrioAtaque> prioMilitar;

        // Las listas completas de accesos limitados y torretas 

        //Estructura que representa a una melange
        [System.Serializable]
        struct Melange
        {
            //Recurso asociado a esta melange
            public LimitedAccess rec { get; set; }
            //Numero de extractores activos en esta melange
            public int extractores { get; set; }
        }

        //Número máximo de extractores por melange
        private const int MAX_EXTRACT_MELANGE = 2;
        private List<Melange> resources = new List<Melange>();
        private List<Tower> neutralTowers;

        //Indice correspondiente a mi enemigo
        int FirstEnemyIndex;

        // Número de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;

        // Última unidad creada
        private Unidad LastUnit { get; set; }

        //-----------UNITY_THINGS----------------//
        private void Awake()
        {
            Name = "IAV21G02V2";
            Author = "IAV21G02";

            // Aumenta el tamaño y cambia el color de fuente de letra para OnGUI (amarillo para las IAs)
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 16;
            _labelStyle.normal.textColor = Color.yellow;

            _labelSmallStyle = new GUIStyle();
            _labelSmallStyle.fontSize = 11;
            _labelSmallStyle.normal.textColor = Color.yellow;
        }

        //----------IA_STATES-------------------//
        // Metodo que gestiona los estados de la IA
        protected override void Think()
        {
            // Aquí se intenta elegir bien la acción a realizar.  
            switch (ThinkStepNumber)
            {
                case 0: // Al inicio, en el primer paso de pensamiento
                    FirstThink();
                    break;

                case 1: // Durante toda la partida, en realidad
                    AIGameLoop();
                    break;
                case 2:
                    Stop = true;
                    break;

            }
        }

        //Detiene el loop de pensamiento
        public void Stopthinking()
        {
            ThinkStepNumber = 2;
        }

        //Bucle principal de pensamiento
        private void AIGameLoop()
        {
            // Como no es demasiado costoso, vamos a tomar las listas completas en cada paso de pensamiento
            UpdateGameElements();

            //TO DO:Mirar el mapa de influencia y ver que tareas necesitamos hacer con nuestros batallones

            //TO DO:Actualizar tareas que estan realizando nuestros batallones y reasignarselas si es necesario


            //TO DO: Si nos han sobrado tareas por realizar, tratamos de comprar mas batallones para encargarles dichas tareas
            //ShoppingManagement();

            //El bucle de juego termina cuando una de las bases es destruida 
            if ((enemyBaseList == null || enemyBaseList.Count == 0 || baseList == null || baseList.Count == 0))
            {
                Stopthinking();
            }
        }


        //---------IA_FIRST_THINK------------------//

        ///<summary>
        ///Primer pensamiento de la IA al iniciar la ejecución
        ///</summary>
        private void FirstThink()
        {
            // Coger indice asignado por el gestor del juego
            MyIndex = RTSGameManager.Instance.GetIndex(this);

            //Referencia a mis cosas
            myBase = RTSGameManager.Instance.GetBaseFacilities(MyIndex)[0];
            myFactory = RTSGameManager.Instance.GetProcessingFacilities(MyIndex)[0];
            myType = myBase.GetComponent<UnitType>().getUnitType();

            //Quito mi indice de esa lista
            var indexList = RTSGameManager.Instance.GetIndexes();
            indexList.Remove(MyIndex);

            //Asumo que el primer indice es el de mi enemigo
            FirstEnemyIndex = indexList[0];

            // Obtengo lista de accesos limitados
            foreach (LimitedAccess rec in RTSScenarioManager.Instance.LimitedAccesses)
            {
#pragma warning disable IDE0059 // Asignación innecesaria de un valor
                Melange mel = new Melange
#pragma warning restore IDE0059 // Asignación innecesaria de un valor
                {
                    rec = rec,
                    extractores = 0
                };

                resources.Add(mel);
            }

            //Actualizo las listas de unidades
            UpdateGameElements();

            //Primero se compran extractores
            InitExtractores();
            //Segundo se compran exploradores
            InitExplorers();
            //Tercero se compran destructores
            InitDestructors();

            //Pasamos a AIGameLoop()
            ThinkStepNumber++;
        }

        /// <summary> 
        ///Inicia comprando extractores dependiendo del dinero inicial y de los exploradores iniciales
        /// </summary>
        private void InitExtractores()
        {
            //Solo me interesa en el inicio
            if (ThinkStepNumber != 0) return;

            //La multiplicación por 5 es para que sobre dinero. Este primer pensamiento trata de tener los 5 extractores desde el principio
            //en caso de tener bastante cantidad de dinero
            if (RTSGameManager.Instance.GetMoney(MyIndex) >= RTSGameManager.Instance.ExtractionUnitCost * 5 && extractorList.Count <= 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    BuyExtractor();
                }
            }
            //La multiplicación por 2 es porque quiero comprar 2 extractores para empezar si tengo menos de 2 extractores
            else if (RTSGameManager.Instance.GetMoney(MyIndex) >= RTSGameManager.Instance.ExtractionUnitCost * 2 && extractorList.Count < 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    BuyExtractor();
                }
            }
            //Al menos me interesa comprar 1
            else if (RTSGameManager.Instance.GetMoney(MyIndex) >= RTSGameManager.Instance.ExtractionUnitCost * 2 && extractorList.Count < 2)
            {
                BuyExtractor();
            }

            //Se les ordena ir a las melanges más cercanas
            foreach (ExtractionUnit ex in extractorList)
            {
                Transform melange = GetMelangeCercana();
                if (melange != null)
                {
                    coroutine = WaitToMoveExtractor(3.0f, ex, melange);
                    StartCoroutine(coroutine);                   
                }
            }
        }

        /// <summary> 
        ///Inicia comprando exploradores dependiendo del dinero inicial y de los exploradores iniciales
        /// </summary>
        private void InitExplorers()
        {
            //Solo me interesa en el inicio
            if (ThinkStepNumber != 0) return;

            //Primero, si tengo menos de 2 unidades y me sobra el dinero, trato de comprar 5 unidades
            if (RTSGameManager.Instance.GetMoney(MyIndex) >= RTSGameManager.Instance.ExplorationUnitCost * 4 && explorerList.Count < 2)
            {
                InvokeRepeating(nameof(BuyExplorer), 1.0f, 5.0f);
                for (int i = 0; i < 5; i++)
                {
                    BuyExplorer();
                }
            }
            //En caso de que no me sobre el dinero, pero tenga menos de 5 unidades, trato de comprarme 2 unidades
            else if (RTSGameManager.Instance.GetMoney(MyIndex) >= RTSGameManager.Instance.ExplorationUnitCost * 2 && explorerList.Count < 5)
            {
                InvokeRepeating(nameof(BuyExplorer), 1.0f, 2.0f);
                for (int i = 0; i < 2; i++)
                {
                    BuyExplorer();
                }
            }
            //Si no puedo comprar 2 trato de comprar 1
            else if (RTSGameManager.Instance.GetMoney(MyIndex) >= RTSGameManager.Instance.ExplorationUnitCost && explorerList.Count < 5)
            {
                BuyExplorer();
            }

            //Se les ordena moverse a un punto y que espere al siguiente pensamiento
        }
        
        /// <summary> 
        ///Inicia comprando destructores dependiendo del dinero inicial y de los destructores iniciales
        /// </summary>
        private void InitDestructors()
        {
            //Solo me interesa en el inicio
            if (ThinkStepNumber != 0) return;

            //Quiero tener al empezar al menos una unidad de destructor a ser posible
            if (RTSGameManager.Instance.GetMoney(MyIndex) >= RTSGameManager.Instance.DestructionUnitCost && destructorList.Count < 1)
            {
                BuyDestructor();
            }

            //Se le ordena moverse a un punto y que espere al siguiente pensamiento
        }

        //--------------------SHOP-----------------------//
        /// <summary>
        /// Compra un extractor
        /// </summary>
        private void BuyExtractor()
        {
            //Si no hay dinero o se ha alcanzado el límite, no se compra
            //Aunque previamente se hagan comprobaciones, es mejor poner esta por seguridad
            if (RTSGameManager.Instance.GetMoney(MyIndex) < RTSGameManager.Instance.ExtractionUnitCost ||
                extractorList.Count >= RTSGameManager.Instance.ExtractionUnitsMax)
            {
                return;
            }
            else
            {
                RTSGameManager.Instance.CreateUnit(this, myBase, RTSGameManager.UnitType.EXTRACTION);
            }
        }

        /// <summary>
        /// Compra un explorador
        /// </summary>
        private void BuyExplorer()
        {
            //Si no hay dinero, no se compra
            //Aunque previamente se hagan comprobaciones, es mejor poner esta por seguridad
            if (RTSGameManager.Instance.GetMoney(MyIndex) < RTSGameManager.Instance.ExplorationUnitCost ||
                explorerList.Count >= RTSGameManager.Instance.ExplorationUnitsMax)
            {
                return;
            }
            else
            {
                RTSGameManager.Instance.CreateUnit(this, myBase, RTSGameManager.UnitType.EXPLORATION);
            }
        }

        /// <summary>
        /// Compra un destructor
        /// </summary>
        private void BuyDestructor()
        {
            //Si no hay dinero, no se compra
            //Aunque previamente se hagan comprobaciones, es mejor poner esta por seguridad
            if (RTSGameManager.Instance.GetMoney(MyIndex) < RTSGameManager.Instance.DestructionUnitCost ||
                destructorList.Count >= RTSGameManager.Instance.DestructionUnitsMax)
            {
                return;
            }
            else
            {
                RTSGameManager.Instance.CreateUnit(this, myBase, RTSGameManager.UnitType.DESTRUCTION);
            }
        }

        //------------MISC---------------------//

        /// <summary>
        /// Actualiza las listas de las entidades existentes
        /// </summary>
        private void UpdateGameElements()
        {
            //Entidades propias
            baseList = RTSGameManager.Instance.GetBaseFacilities(MyIndex);
            myBase = baseList[0];
            factoryList = RTSGameManager.Instance.GetProcessingFacilities(MyIndex);
            myFactory = factoryList[0];
            extractorList = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
            explorerList = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
            destructorList = RTSGameManager.Instance.GetDestructionUnits(MyIndex);

            //Entidades enemigas
            enemyBaseList = RTSGameManager.Instance.GetBaseFacilities(FirstEnemyIndex);
            enemyBase = enemyBaseList[0];
            enemyFactoryList = RTSGameManager.Instance.GetProcessingFacilities(FirstEnemyIndex);
            enemyFactory = enemyFactoryList[0];
            enemyExtractorList = RTSGameManager.Instance.GetExtractionUnits(FirstEnemyIndex);
            enemyExplorerList = RTSGameManager.Instance.GetExplorationUnits(FirstEnemyIndex);
            enemyDestructorList = RTSGameManager.Instance.GetDestructionUnits(FirstEnemyIndex);

            neutralTowers = RTSScenarioManager.Instance.Towers;
        }

        /// <summary>
        /// Devuelve la melange más cercana de la factoría.
        /// Ya que los extractores irán yendo y viniendo de la factoría.
        /// otr extractor
        /// </summary>
        Transform GetMelangeCercana()
        {
#pragma warning disable IDE0059 // Asignación innecesaria de un valor
            Transform tr = null;
            //Para ir guardando la melange escogida
            Melange currMel = new Melange
            {
                rec = null,
                extractores = 0
            };
            int currIndex = 0;

            //Posiciones en 2D (XZ)
            Vector2 posFactory = new Vector2(myFactory.transform.position.x, myFactory.transform.position.z);
            Vector2 posMel = new Vector2(0, 0);
            Vector2 currdif = new Vector2(0, 0);

            int i = 0;
            while (i < resources.Count)
            {
                //Primera entrada
                if (!tr)
                {
                    currIndex = i;
                    currMel = resources[i];
                    currMel.extractores++;
                    tr = currMel.rec.transform;

                    posMel = new Vector2(currMel.rec.transform.position.x,
                                    currMel.rec.transform.position.z);
                    currdif = new Vector2(Math.Abs(posFactory.x - posFactory.x), Math.Abs(posFactory.y - posMel.y));

                    resources[i] = currMel;
                }
                else
                {
                    Vector2 posAux = new Vector2(resources[i].rec.transform.position.x, resources[i].rec.transform.position.z);
                    Vector2 newDif = new Vector2(Math.Abs(posFactory.x - posAux.x), Math.Abs(posFactory.y - posAux.y));

                    //Si tiene menos distancia y la melange aún no está llena
                    if (newDif.magnitude < posMel.magnitude && resources[i].extractores < MAX_EXTRACT_MELANGE)
                    {
                        //Actualizamos el recurso guardado anteriormente
                        currMel.extractores--;
                        resources[currIndex] = currMel;

                        //Se guarda el nuevo
                        currMel = resources[i];
                        //Aumenta el número de extractores
                        currMel.extractores++;
                        //Se guarda el transform
                        tr = currMel.rec.transform;
                        //Se actualiza la nueva posición
                        posMel = posAux;
                        //Se guarda la nueva diferencia
                        currdif = newDif;

                        currIndex = i;
                        resources[i] = currMel;
                    }
                }

                i++;
            }
            return tr;
#pragma warning restore IDE0059 // Asignación innecesaria de un valor
        }
        /// <summary> 
        ///Devuelve el color de la IA actual
        /// </summary>
        public ColorTeam GetMyType()
        {
            return myType;
        }

        /// <summary> 
        ///  Actualiza las prioridades de defensa y ataque del mapa de influencias
        /// </summary>
        public void ActualizaPrioridades()
        {
            //-------------MILICIA------------//
            prioMilitar = new List<CasillaPrioAtaque>();
            foreach (DestructionUnit unit in enemyDestructorList)
            {
                CasillaBehaviour currCasilla = MapManager.GetInstance().GetCasillaCercana(unit.transform);
                CasillaPrioAtaque atq = currCasilla.GetCasillaPrioMilitar();
                atq.ActualizaAtaque();

                if (!prioMilitar.Contains(atq))
                {
                    prioMilitar.Add(atq);
                }
            }

            foreach (ExplorationUnit unit in enemyExplorerList)
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
            foreach (ExtractionUnit unit in extractorList)
            {
                CasillaBehaviour currCasilla = MapManager.GetInstance().GetCasillaCercana(unit.transform);
                CasillaPrioDefensa def = currCasilla.getCasillaPrioDefensa();
                def.ActualizaDefensa();

                if (!prioDefensa.Contains(def))
                {
                    prioDefensa.Add(def);
                }
            }

            //Base
            CasillaPrioDefensa defAux = MapManager.GetInstance().GetCasillaCercana(baseList[0].transform).getCasillaPrioDefensa();
            defAux.ActualizaDefensa();

            if (!prioDefensa.Contains(defAux))
            {
                prioDefensa.Add(defAux);
            }

            //Factoria
            defAux = MapManager.GetInstance().GetCasillaCercana(factoryList[0].transform).getCasillaPrioDefensa();
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
        /// <summary> 
        /// Dibuja la interfaz gráfica de usuario para que se vea la información en pantalla
        /// </summary>
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
            //if (movedUnit != null)
            //    // Una etiqueta para indicar la última unidad movida, si la hay
            //    GUILayout.Label(movedUnit.gameObject.name + " moved", _labelSmallStyle);

            // Cerramos el área de distribución con contenido en vertical
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary> 
        ///Corrutina para empezar a mover la unidad de Extractores pasados unos segundos
        /// </summary>
        private IEnumerator WaitToMoveExtractor(float waitTime, ExtractionUnit ex, Transform target)
        {

            yield return new WaitForSeconds(waitTime);

            //Espera waitTime
            Debug.Log("Moviendo unidad: " + Time.time);
            ex.Move(this, target.position);
        }

    }
}