![Captura de pantalla 2021-06-03 130317](https://user-images.githubusercontent.com/48771457/120634939-24edac00-c46c-11eb-9591-08039fcc15eb.png)

# Descripción
Esta es la cuarta práctica de la asignatura **Inteligencia Artificial para Videojuegos** de la **Universidad Complutense de Madrid**. 

Consiste en la implementación de entidades con inteligencia artificial siguiendo el esquema y la estructura planteada en el enunciado **IAV-Práctica-4.pdf**.

La práctica plantea un ejercicio de creación de una IA para un RTS que toma como inspiración la novela **Dune (1965)** de Frank Herbert. En este podremos tomar el control de uno de los "bandos" de la novela, Fremen o Harkonnen, y jugar contra una IA diseñada por nosotros que sea capaz de gestionar la compra de unidades y la estrategia que deber de llevar a cabo las mismas. Por otro lado, también podemos dejar que dos IA distintas batallen entre sí para ver cual es la mejor de las dos cara a cara.


# Documentación técnica del proyecto

El proyecto está implementado con **Unity** y la documentación del código la generamos en español, así como la mayoría de variables, métodos y clases.

## 1.-Mecánicas
En el modo un jugador, en el que el usuario se enfrenta a una de las IAs, los controles son los siguientes:
- **Click izquierdo**: seleccionar unidad. Una vez seleccionada podemos enviarla a cualquier posición navegable de nuestro mapa haciendo click en la posición deseada.

- **Botones de compra**: sirven para comprar diferentes unidades, ya sean unidades dedicadas al ataque (destructores y exploradores) o unidades dedicadas a la obtención de recursos (extractores).

- **Botones de gestión de partida** :

      Botón ChangeSpeed -> Cambiar la velocidad a la que se están moviendo las tropas para agilizar las partidas.
      Botón Pausa -> Pausar el estado del juego.
      Botón Reset -> Volver a empezar la partida con las unidades establecidas al comienzo de la misma.
      Botón Quit -> Salir del juego.

Por otro lado, tanto en el modo un jugador como en el modo combate entre IAs, contamos con los siguientes controles:
- **Tecla M**: mostrar u ocultar el mapa de influencia del escenario.

## 2.-Mapa de influencia 
 
Para la creación del mapa de influencia del escenario hemos desarrollado los siguientes scripts y prefabs 

### 2.1- Scripts del Mapa de Influencia

- **MapManager**: define un **Singleton** cuya principal misión es encargarse de la creación del **mapa de influencia** tanto a nivel lógico como a nivel visual. Divide el escenario en casillas según el tamaño del **Terrain** del escenario. Dicha división de casillas es configurable y está establecida en el componente **Grid**, contenido en la entidad que poseé el terreno. Esta clase también cuenta con métodos para actualizar la prioridad de una casilla en concreto y todas sus adyacentes en un rango gracias a los métodos: **ActualizaPrioridadAlSalir** y **ActualizaPrioridadAlEntrar**. En la siguiente imagen podemos ver más en detalle como funciona la actualización de prioridades.


![Captura de pantalla 2021-06-03 133150](https://user-images.githubusercontent.com/48771457/120638269-35a02100-c470-11eb-94b7-382f41829d09.png)

Por otro lado, la actualización de dicha influencia se gestiona cada vez que una unidad de cualquier tipo sale o entra de dicha casilla. Nuestro primer acercamiento a la idea consistía en comprobar la colisión de las unidades mediante el uso de los métodos **OTriggerEnter()** y **OnTriggerExit()** de Unity. Sin embargo, esta implementación pese a funcionar correctamente a la hora de desplazar a las unidades nos generaba conflictos a la hora de atacar. Esto se debe a que el árbol de comportamiento interpretaba nuestras casillas como objetivos de ataque y al no contar con el script health ni ser una unidad se quedaba confuso y hacía que las tropas no disparasen. Finalmente conseguimos arreglar sin muchas dificultades el problema con la creación del método **GetCasillaCercana()** que podemos ver a continuación.

![Captura de pantalla 2021-06-04 100824](https://user-images.githubusercontent.com/48771457/120768867-df3cec00-c51c-11eb-8c1f-e2b144f16a09.png)

- **CasillaBehaviour**: script que contiene el prefab **Casilla**. Contiene información relacionada con la influencia que tienen los dos equipos sobre dicha casilla, además de la posible influencia que pudiesen tener los poblados Graben (representados por el color verde). Contiene un enum ColorTeam que representa el estado de dicha casilla o el equipo que tiene mayor influencia de ataque sobre la misma: Vacía, Azul, Amarillo, Verde o Neutral. En este script destacan los métodos para actualizar el color de la casilla (**CambiaCasillaColor()**), métodos para actualizar la influencia de la casilla cuando llega una unidad de un tipo concreto (**ModificaInfluenciaAlEntrar()** y **ModificaInfluenciaAlSalir()**) y métodos para obtener la fila y la columna de dicha casilla en el **MapManager**.

- **UnitType**: script añadido a **todas** las unidades que participan en la lógica del juego. Funciona a modo de contenedor de información para poder modificar cuanta influencia tienen las unidades, si son defensivas u ofensivas, cuanto rango de influencia tienen (es decir, a cuantos niveles de casillas adyacentes afectan), etc. También se encargan, en su método **Update()** de avisar al mapa de influencia si han cambiado de casilla para actualizarlo correctamente, además de definir el método **OnDestroy()** para cuando son destruidas, quitar la influencia de las casillas adyacentes. En la siguiente imagen podemos apreciar de forma más precisa la gestión que realizan estos dos últimos métodos mencionados.

![Captura de pantalla 2021-06-04 102753](https://user-images.githubusercontent.com/48771457/120771490-8d499580-c51f-11eb-9341-cf3803620ccd.png)

### 2.2- Prefabs del Mapa de Influencia

- **Prefab Casilla**: prefab que en sus comienzos se encargaba de controlar la entrada y salida de unidades de una casilla a otra del mapa de influencia puesto que contaba con el componente **Collider**. Tras los cambios explicados anteriormente, este prefab funciona a modo de contenedor del comportamiento **CasillaBehaviour** y como malla a renderizar del mapa de influencia en representación de una casilla.


## 3.-Controlador de tropas

Durante el transcurso de la práctica hemos desarrollado 3 versiones distintas de controlador de tropas. La gestión hecha mediante batallones fue finalmente descartada debido a la complejidad de su implementación. Sin embargo, consideramos relevante su mención pese a no ser funcional, puesto que partía de una base bastante razonada y trabajada.

### 3.1.- Gestión de tropas mediante Batallones

En este primer acercamiento proponíamos controlar a las unidades de manera grupal, repartíendolas en distintos batallones. Para ello, creamos un **struct Batallon** que contenía listas para referenciar a las unidades que formasen este batallón por tipo (extractoresBatallon, exploradoresBatallon y destructoresBatallon) además de variables de control para comprobar si el grupo se encontraba en movimiento, atacando, había completado la misión que se le había asignado etc.

También desarrollamos métodos para la gestión de estos batallones, puesto que en una situación especifica de la partida quisieses construir un batallón pero no contases con los suficientes recursos económicos para comprar todas las unidades (y no tuvieses en tu lista de unidades sin batallón la unidad que te falte para completar dicho grupo). Destacan en este aspecto métodos como **agregaUnidad()** o **desmontaBatallon()**.

Dicha implementación de batallones contaba también con un enum **Tipo Batallon** gracias al cual establecíamos cuantas unidades de cada tipo conformaba dicha agrupación como se puede apreciar en la siguiente imagen:

![Captura de pantalla 2021-06-04 111335](https://user-images.githubusercontent.com/48771457/120777778-f16f5800-c525-11eb-98f3-f3531e6f9c21.png)

Las dificultades para la implementación de este controlador llegaron a la hora de comprobar cuando la misión asignada al mismo estaba completa, puesto que eran necesarias muchas modificaciones para comprobar cuando todas las unidades habían llegado a un objetivo o habían derrotado al rival que se les había asignado. Por tanto, al desconocer cuando una misión había sido completada, nos era imposible reestructurar las tropas para crear nuevos batallones que realizasen otras tareas o simplemente cambiar la misión de un grupo ya formado. Estos hechos sumados a que se acercaba la fecha de entrega del controlador nos hicieron tomar la decisión de modificar nuestro planteamiento inicial y simplificarlo, de forma que controlabamos cuantas unidades realizaban una acción y cuantas otra en base a porcentajes.

### 3.2.- Versión final del controlador 

Nuestro controlador basa su estrategia de manejo y compra de unidades en base a una serie de estados. Los cambios entre estados vienen dictaminados por la situación actual de la  partida: si contamos con **supremacía militar** sobre el enemigo (es decir, nuestra influencia total en el mapa es mayor que la del enemigo más un porcentaje), si contamos con **supremacía económica** (más dinero que el enemigo), si hay algún enemigo atacando nuestra base etc. Estos estados son los siguientes:

- **Ofensivo**: estado en el que entramos cuando superamos claramente los recursos actuales del enemigo (tanto militares como económicos) o cuando este cuenta con pocas unidades ofensivas con las que defenderse de un ataque potente y organizado con todas nuestras tropas. En este estado, nuestro objetivo principal es atacar las instalaciones enemigas, dándole prioridad a los ataques sobre la base (60% de las unidades ofensivas) mientras que enviaremos a las tropas restantes (40% unidades ofensivas) a atacar la zona con mayor influencia rival para debilitar aun más a nuestro enemigo.A no ser que no quedes tropas enemigas, en cuyo caso iran todas las tropas a atacar el nucleo. Por otro lado, en el apartado de la gestión de la compra de las unidades, se prioriza la compra de unidades militares, y especialmente la de destructores puesto que contamos también con supremacía económica sobre el enemigo.

- **Defensivo**: en el momento en el que nuestra base o la factoría se encuentren bajo las hordas del equipo rival pasaremos al estado defensa. En este estado enviaremos a nuestras tropas ofensivas a defender nuestras instalaciones priorizado la defensa de la base (50% de nuestras tropas ofensivas frente al 20% que se encargaran de la defensa de la factoría) en caso de que ambas instalaciones estén siendo atacadas. Puesto que es más sencillo entrar en este estado que en el de ataque, no vamos a enviar al 100% de nuestras tropas a defender y estas continuaran con la tarea que les fue asignada en el estado anterior (defender exploradores para tratar de conseguir mas recursos, atacar a la base rival etc.) En cuanto a la gestión de la compra durante este estado, priorizaremos la compra de exploradores, las unidades ofensivas mas baratas para contar con el mayor numero de tropas posibles para defender nuestras infraestructuras, seguido de los destructores y finalmente los extractores.

- **Farming**: es el estado en el que comenzaremos por defecto cualquier partida. El objetivo durante este estado consiste en conseguir recursos económicos suficientes que luego pudiesemos invertir en mejores tropas para comenzar una estrategia más ofensiva como es el caso de **Guerrilla** o **Ofensiva**. Por lo tanto, una de las prioridades de este estado consiste en defender a nuestras unidades extractoras para así conseguir rápidamente la mayor cantidad de Solaris posibles. En este estado también las unidades se dedicarán a defender la factoria puesto que si esta es destruida seremos incapaces de ahorrar mas Solaris y todas nuestras unidades extractoras se volveran inútiles. La estrategia de compra a seguir se basará en priorizar la compra de extractores, para asi acelerar la produccion de solaris, y en caso de tener suficientes extractores, seguir comprando exploradores para defender y no quedarnos indefensos

- **Guerilla**: pasaremos a este estado en el momento en el que tengamos tropas suficientes pero no el dinero suficiente como para tener garantias de hacer un ataque directo,pasaremos en ese momento a atacar los extractores del enemigo para disminuir su capacidad de conseguir solaris y lograr una ventaja economica sobre él.En cuanto a la compra, en este estado nos centraremos en comprar primero destructores para aumentar nuestra fuerza de ataque rapidamente, y cuando tengamos suficientes seguiremos con la compra de exploradores.

- **Emergencia**: si nos encontramos en una situación en la que nuestras tropas sean demasiado escasas,consideraremos que pasamos a un estado de emergencia, y que de forma urgente debemos aumentar nuestras tropas.En cuanto a la estrategia sera igual que la del farming, nos dedicaremos a defender los extractores y la factoria para asegurarnos el tener dinero suficiente como para comprar nuevas tropas. Sin embargo en la compra priorizaremos los destructores para ampliar nuestra fuerza de ataque lo mas rapido posible.

## 4.-Pruebas realizadas
### 4.1.- Pruebas iniciales

En un primer momento, para probar el funcionamiento de nuestra IA realizamos una serie de pruebas basicas y simples, la mayoria sin resultado ganador pero que nos permitian ver el avance de la IA y su funcionamiento.
Consistian en probar de forma individual cada una de las estrategias(ofensiva, defensiva ,farming, guerrilla y emergencia) es decir, una partida en la que solo fueramos ofensivos, otra en la que solo fueramos defensivos, etc... para asi comprobar que los distintos algoritmos de compra y de movimiento de tropas funcionaban correctamente y hacían lo que queriamos en cada momento.
Cuando ya funcionaban de forma individual, pasamos a ir combinarlas para conseguir el resultado final.

### 4.2.- Comenzar la partida sin unidades

Puesto que uno de los apartados de la práctica especifica concretamente realizar pruebas en base al estado inicial del escenario y el número inicial de unidades con las que cuenta nuestro controlador nos dedicamos a añadir una serie de restricciones al código para que no se diesen situaciones extrañas que pudiesen confundir a nuestra IA.

Una de ellas es no poder empezar la partida con dinero negativo o 0 y no contar con extractores inicialmente. En ese caso la única opción con la que contamos para ganar dicha partida es mandar a todas nuestras tropas a atacar la base enemiga para tratar de destruirla y ganarla de la forma más rápida posible, puesto que no podemos conseguir dinero para comprar más recursos. Como esta situación también puede darse durante el transcurso de una partida, añadimos una comprobación extra en el método que se encarga de cambiar el estado del controlador. En la siguiente imagen mostramos el fragmento de código correspondiente a esta comprobación

AÑADIR IMAGEN (TO DO)

Por otro lado, no se puede jugar en el caso de que no contemos con base inicial puesto que esto supone una derrota inmediata en nuestra contra. Para gestionar esto, añadimos una excepción cuando esto ocurre en el método **InitializeController()**. También comprobamos en dicho método que al principio contemos o con dinero o con alguna unidad inicial para conseguirlo puesto que en dicho caso solo nos tocaría esperar a que las unidades enemigas destruyesen nuestra base y perderíamos sin poder hacer nada para evitarlo. En la siguiente imagen podemos ver la comprobación realizada:

![devenv_MmvHFYBHQv](https://user-images.githubusercontent.com/48771457/120842168-a6c4fe80-c56c-11eb-9c55-6fe0344c1621.png)

Finalmente, haciendo click en la siguiente imagen mostramos un fragmento de las pruebas realizadas para comprobar que se puede inicial la partida sin unidades propias dispuestas por el mapa y como el controlador comienza comprando unidades por su cuenta sin intervención externa.

HACER VIDEO CON LA VERSIÓN ACTUALIZADA (TO DO)

### 4.3.- Combate contra IA simple (RTSControllerExample3)

Esta fue una de las primeras pruebas que realizamos para comprobar que en situaciones simples nuestra IA era capaz de defenderse de los ataques de otra IA simple. Pudimos confirmar que,a grandes rasgos, el cambio de estados funcionaba como deseabamos: comenzando en estado **farming**, cambiando ente este y **defensivo** cuando las unidades enemigas se acercaban peligrosamente al núcleo y cambiando al estado **Ofensivo** cuando el enemigo apenas contaba con unidades ofensivas para atacar su base y ganar la partida. 

También nos sirvió para darnos cuenta de que había cosas en el diseño de nuestro controlador que no acababan de cuadrar. Por ejemplo la estrategia que habíamos ideado para el movimiento de los extractores no estaba funcionando como debería. Todos trataban de ir a la zona de extracción más cercana a la base sin comprobar si esta estaba libre y por lo tanto estaban esperando al que el primero acabase para entrar el siguiente. Nos dimos cuenta que obviamente deberíamos distribuirlos de otra forma para que la obtención de recursos fuese mucho más rápida e hicimos los cambios pertinentes para mejorarlo.

Haciendo click en la siguiente imagen podrás ver un video con un fragmento de las pruebas realizadas para este apartado:

[![image](https://user-images.githubusercontent.com/48771457/120833169-c4409b00-c561-11eb-807f-a55cd7eb74f1.png)](https://youtu.be/ysJjr7nLXO0)

### 4.4.- Combate contra nuestra propia IA 
Una de las ultimas pruebas que realizamos, cuando la IA ya estaba casi terminada fue enfrentar la IA contra si misma. Al principio la partida empieza bastante igualada y lenta, ambos equipos comienzan farmeando, extrayendo recursos. Una vez que una de ellas obtiene los recursos suficientes empieza a atacar, y se empiezan a movilizar todas las tropas,la otra entonces comienza la defensa y asi empieza la lucha entre tropas hasta que uno de los dos equipos consigue la superioridad militar suficiente como para lanzarse a la ofensiva y atacar la base del contrario, con lo que gana la partida.

Casi todas las partidas que hemos probado de este tipo siguen el modelo anterior,(farming, una ataca el otro se defiende y siguen en conflicto hasta que uno consigue tener una superioridad en numero de tropas suficiente como para atacar la base del contrario y asi ganar la partida)

### 4.5.- Combate en un escenario distinto al inicial
Por ultimo probamos nuestra IA en un escenario diferente al que se deba por defecto, y funcionaba bastante bien, sin cambios destacables ni problemas.

## 5.-Recursos de terceros empleados
- Pseudocódigo del libro: [**AI for Games, Third Edition**](https://ebookcentral.proquest.com/lib/universidadcomplutense-ebooks/detail.action?docID=5735527) de **Millington**

- Código de [**Federico Peinado**](https://github.com/federicopeinado) habilitado para la asignatura.

- [**Bolt**](https://unity.com/es/products/unity-visual-scripting) y [**Behaviour Designer**](https://opsive.com/assets/behavior-designer/) para inteligencia artificial en **Unity**
