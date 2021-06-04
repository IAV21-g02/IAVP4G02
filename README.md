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

### 3.2.- Versión Stiwi/Paula
Igual las mezclamos asi que esto pa luego :D

### 3.3.- Versión Amaro/Aurora/Leyre
Igual las mezclamos asi que esto pa luego :D

## 4.-Pruebas realizadas

Añadir pruebas realizadas :D

## 5.-Recursos de terceros empleados
- Pseudocódigo del libro: [**AI for Games, Third Edition**](https://ebookcentral.proquest.com/lib/universidadcomplutense-ebooks/detail.action?docID=5735527) de **Millington**

- Código de [**Federico Peinado**](https://github.com/federicopeinado) habilitado para la asignatura.

- [**Bolt**](https://unity.com/es/products/unity-visual-scripting) y [**Behaviour Designer**](https://opsive.com/assets/behavior-designer/) para inteligencia artificial en **Unity**
