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

- **MapManager**: define un **Singleton** cuya principal misión es encargarse de la creación del mapa de influencia tanto a nivel lógico como a nivel visual. Divide el escenario en casillas según el tamaño del **Terrain** del escenario. Dicha división de casillas es configurable y está establecida en el componente **Grid**, contenido en la entidad que poseé el terreno. Esta clase también cuenta con métodos para actualizar la prioridad de una casilla en concreto y todas sus adyacentes en un rango gracias a los métodos: **ActualizaPrioridadAlSalir** y **ActualizaPrioridadAlEntrar**. En la siguiente imagen podemos ver más en detalle como funciona la actualización de prioridades.


![Captura de pantalla 2021-06-03 133150](https://user-images.githubusercontent.com/48771457/120638269-35a02100-c470-11eb-94b7-382f41829d09.png)

Por otro lado, la actualización de dicha influencia se gestiona cada vez que una unidad de cualquier tipo sale o entra de dicha casilla. Para ello, la entidad que contega el componente **CasillaBehaviour** debe estar marcado como **Trigger** puesto que esta gestión se realiza en los métodos **OnTriggerEnter()** y **OnTriggerExit()**. A continuación mostramos la gestión realizada en el OnCollisionEnter de forma más ilustrativa.

![Captura de pantalla 2021-06-03 162030](https://user-images.githubusercontent.com/48771457/120661272-6724e680-c488-11eb-80ba-c07233938170.png)

- **CasillaBehaviour**: script que contiene el prefab **Casilla**. Contiene información relacionada con la influencia que tienen los dos equipos sobre dicha casilla, además de la posible influencia que pudiesen tener los poblados Graben (representados por el color verde). Contiene un enum ColorTeam que representa el estado de dicha casilla o el equipo que tiene mayor influencia de ataque sobre la misma: Vacía, Azul, Amarillo, Verde o Neutral. En este script destacan los métodos para actualizar el color de la casilla (**CambiaCasillaColor()**), métodos para actualizar la influencia de la casilla cuando llega una unidad de un tipo concreto (**ModificaInfluenciaAlEntrar()** y **ModificaInfluenciaAlSalir()**) y métodos para obtener la fila y la columna de dicha casilla en el **MapManager**.

- **UnitType**: script añadido a **todas** las unidades que participan en la lógica del juego. Funciona a modo de contenedor de información para poder modificar cuanta influencia tienen las unidades, si son defensivas u ofensivas, cuanto rango de influencia tienen (es decir, a cuantos niveles de casillas adyacentes afectan), etc.

![Captura de pantalla 2021-06-03 160744](https://user-images.githubusercontent.com/48771457/120658647-e6fd8180-c485-11eb-9e06-2242ce7a41d4.png)

### 2.2- Prefabs del Mapa de Influencia

- **Prefab Casilla**: consta de un boxCollider que es trigger. Cada vez que una entidad entra en una casilla actualizará los datos almacenados en el script **CasillaBehaviour** y llamará a los métodos correspondientes para actualizar su color. A continuación mostramos en detalle los componentes que contiene este prefab:

![Captura de pantalla 2021-06-03 161601](https://user-images.githubusercontent.com/48771457/120660169-5627a580-c487-11eb-8198-331f82f7553c.png)


## 3.-Controlador de tropas


## 4.-Pruebas realizadas



## 5.-Recursos de terceros empleados
- Pseudocódigo del libro: [**AI for Games, Third Edition**](https://ebookcentral.proquest.com/lib/universidadcomplutense-ebooks/detail.action?docID=5735527) de **Millington**

- Código de [**Federico Peinado**](https://github.com/federicopeinado) habilitado para la asignatura.

- [**Bolt**](https://unity.com/es/products/unity-visual-scripting) y [**Behaviour Designer**](https://opsive.com/assets/behavior-designer/) para inteligencia artificial en **Unity**
