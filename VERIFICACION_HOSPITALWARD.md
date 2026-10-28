# Estado actualizado: pasos, control y placas hospitalarias

Actualización del 28 de octubre de 2026, posterior a la entrega registrada más abajo. Esta sección describe el estado actual y reemplaza las descripciones anteriores del diseño del control, placas y criterio de audio.

- **Pasos:** `PlayerFootsteps`, en el XR Origin, reproduce la grabación existente `Assets/Sounds/Sonidos de acciones/freesound_community-indoor-footsteps-6385.mp3` mediante `SonidoPasos`. Lee desplazamiento horizontal real y estado grounded del Character Controller. El sonido se pausa en reposo con un fundido de 0.12 s. Rechaza saltos de posición y no usa el movimiento de manos como señal de caminar. No modifica la locomoción.
- **Clic del control:** `click_001.ogg`, de la biblioteca Kenney ya importada, duración aproximada 0.1 s. AudioSource 3D en el propio control, volumen 0.4. Suena en el evento activated al estar agarrado, haya o no luz válida; el clic no indica éxito de apagado.
- **Diseño del control:** carcasa clínica blanca con esquinas redondeadas y bisel, base oscura, aro metálico y un solo botón verde con símbolo de apagado, etiqueta LUZ y emisor frontal. Se preservan el collider, Rigidbody, XRGrabInteractable y raycast frontal de 12 m. La malla nueva se guarda como `Assets/VR/Interaction/CarcasaRedondeada.asset`.
- **Placas:** cuatro tarjetas de identificación hospitalaria sujetas visualmente al pie de las camas, con marco de acero, fondo blanco, banda verde, nombre y número de habitación/cama. Centro a 0.52 m, dimensiones 56 × 23.5 cm; ya no están suspendidas por encima de la cama. Se mantienen Samuel, Alejandro, Vicente y Alexander.
- Configuración reproducible en `HospitalPresentationSetup.Apply()`, también integrada en el configurador anterior.

## Verificación de esta actualización

1. Compilación en Unity completada sin errores. Las consultas posteriores a las pruebas no mostraron errores ni advertencias de los scripts; la consulta final mostró una advertencia intermitente de transporte de Unity MCP (`WebSocket is not initialised`). El guardado se completó correctamente. No se generó un build.
2. En reposo, pasos: IsWalking=false, PlaybackStarts=0, AudioSource.isPlaying=false, volumen=0.
3. Con desplazamiento del simulador, una observación temporal registró 107 muestras con IsWalking=true y fuente reproduciendo con volumen positivo; un inicio de reproducción. Después de detenerse: IsWalking=false, isPlaying=false, volumen=0. El rig se desplazó desde x=-1.15 hasta aproximadamente x=-2.05. Se verificó estado de reproducción; no se escuchó ni se evaluó la mezcla sonora.
4. El intento inicial de pulsación perdió la selección del control entre operaciones del simulador y no produjo activación. Se repitió una secuencia continua con eventos del Input System enviados a los mandos virtuales creados por XR Device Simulator. Se suspendió temporalmente su lectura de teclado/ratón en Play Mode para evitar interferencias; no se cambió su configuración guardada.
5. Resultado de la secuencia repetida: grabbed=true, lifted=true, activations=2, lampClickPlaying=true, missClickPlaying=true, lampOff=true, released=true. Se observó AudioSource.isPlaying en el evento de activación tanto apuntando a Luminaria_1 como al piso. Se apagó RellenoClinico_1 al apuntar a la lámpara. El grip, el transporte y la suelta del control rediseñado funcionaron. No se forzó la selección con SelectEnter ni se invocó directamente el evento activated.
6. Se inspeccionaron en capturas las cuatro placas desde altura de ojos de 1.65 m: nombres completos visibles, al pie de la cama y sin recorte. La lectura estereoscópica y comodidad siguen pendientes de visor físico.
7. Fuera de Play Mode se comprobó XR Device Simulator habilitado, 0 scripts faltantes y referencias de ambos audios asignadas. Escena guardada con control sobre Commode y estados iniciales de luces restaurados.

**Criterios actualizados:** audios condicionales **Cumple** por configuración: pasos ligados al desplazamiento, clic RCP por pulsación y metrónomo por sesión activa; además hay clic propio del control. Se probaron las activaciones nuevas, no una sesión RCP completa. Interacciones específicas del Character Controller mediante triggers/OnControllerColliderHit: **No cumple**, sin cambio; los pasos son audio, no se cuentan como una de esas interacciones. Los demás criterios conservan el resultado de la revisión inicial.

Capturas actuales:

- [Control rediseñado](Assets/Screenshots/screenshot-20261028-103932.png)
- [Samuel](Assets/Screenshots/screenshot-20261028-104529.png)
- [Alejandro](Assets/Screenshots/screenshot-20261028-104530.png)
- [Vicente](Assets/Screenshots/screenshot-20261028-104531.png)
- [Alexander](Assets/Screenshots/screenshot-20261028-104531_1.png)

## Registro histórico de la entrega anterior

Escena guardada: `Assets/Scenes/HospitalWard.unity` (esa es la capitalización real del archivo).
Fecha: 28 de octubre de 2026. Unity 6000.5.11f1, XR Interaction Toolkit 3.5.1, Input System 1.20.0.
Inspección y modificación de la escena realizadas con Unity MCP, incluyendo ejecución C# dentro del Editor.

## Cambios implementados

- Se conservó el XR Origin, Character Controller, seguimiento, locomoción, dos manos y sus componentes `ControllerHandAnimator`. No se reemplazaron modelos, animaciones ni assets de acciones existentes.
- Se añadieron `XR Interaction Manager` y un hijo `AgarreDirecto` en cada mando, con `XRDirectInteractor`, SphereCollider trigger de 10 cm, selección mediante grip y activación mediante gatillo. Utilizan lectores de Input System de XRI 3.5.1 con bindings para cada mano.
- `Habitacion_01/ControlRemoto` está sobre el tablero del objeto Commode, aproximadamente en (-0.95, 0.56, 1.95). Tiene carcasa de 6.5 × 16 cm, un único botón central visible, emisor frontal, BoxCollider, Rigidbody y `XRGrabInteractable`.
- `LightRemote` escucha el evento `activated` de XRI solamente mientras el control está seleccionado. El rayo sale de `OrigenRayo`, delante de la carcasa, en su eje local +Z y tiene 12 m de alcance. Ordena impactos por distancia, ignora los colliders del propio control y los triggers, y se detiene ante el primer obstáculo sólido. Un impacto válido desactiva exclusivamente el componente Light asociado. Una pulsación sin blanco válido no modifica luces. No hay encendido, alternancia ni otras funciones.
- `RemoteLightTarget` enlaza cada lámpara con una sola luz. Las ocho luminarias existentes se asociaron a los ocho rellenos clínicos. Se añadieron cuatro luminarias centrales y cuatro de pasillo para ofrecer superficies visibles a las demás fuentes existentes. Sus colliders tienen en cuenta escala y rotación. La lente cambia a material oscuro al apagarse.
- Las luces principales de las habitaciones 2, 3 y 4 ya estaban deshabilitadas y sus GameObjects inactivos. Se conservaron así; sus nuevas luminarias aparecen oscuras. Las 16 fuentes existentes son Realtime.
- Se añadieron tres `PortapapelesClinico`, sobre las Commode de las habitaciones 2–4, con Rigidbody, BoxCollider y `XRGrabInteractable`. Los muebles, pacientes y equipos médicos permanecen estáticos.
- Se añadieron cuatro placas `NombrePaciente` al pie de cada cama: habitación 1 Samuel, 2 Alejandro, 3 Vicente y 4 Alexander. Tienen fondo oscuro de 72 × 22 cm, texto blanco TextMeshPro, altura aproximada de 1.02 m y orientación hacia el acceso de cada habitación.
- Código nuevo en `Assets/VR/Interaction/`; configuración reproducible e idempotente en `Assets/VR/Editor/HospitalInteractionSetup.cs`, método `Apply()`.

## Pruebas ejecutadas y resultados

Se utilizó Play Mode y el XR Device Simulator existente. MCP modificó programáticamente las poses y estados de sus mandos simulados, que el simulador entregó al Input System y a XRI. No se sustituyó el agarre por llamadas a SelectEnter ni se invocó manualmente el evento de activación del control en estas pruebas de entrada. No fue una prueba manual de teclado/ratón ni una prueba con visor.

| Prueba | Procedimiento y evidencia | Resultado |
|---|---|---|
| Compilación | Importación, compilación y recarga del dominio en el Editor; consulta de consola posterior. | Sin errores de compilación. Última consulta de consola: 0 errores y 0 advertencias. No se generó un build de aplicación. |
| Agarrar el control | Mando derecho simulado en contacto, grip de 0 a 1. `interactablesHovered` incluyó ControlRemoto; después `isSelected` fue true. | Correcto. |
| Transportarlo | Se elevó la pose del mando 0.8 m manteniendo grip. El control pasó de y≈0.56 a y≈1.36 y continuó seleccionado. | Correcto. |
| Apuntar y apagar | Se orientó el mando hacia la luminaria existente de la habitación 1. El primer impacto fue `Habitacion_01/04_Iluminacion/Luminaria_1`; se pulsó el gatillo simulado. | `ActivationCount` pasó de 0 a 1; se apagó solo `RellenoClinico_1`. Luces activas: 13 → 12. |
| Activación sin luz válida | Se soltó el gatillo, se apuntó al piso y se volvió a pulsar. | `ActivationCount` pasó a 2, `LastSwitchedOff` quedó null y siguieron 12 luces activas. |
| Soltar el control | Grip volvió a 0. | `isSelected=false`, Rigidbody con gravedad y sin isKinematic; el objeto cayó y quedó apoyado sobre Commode, y≈0.63, en la orientación de la prueba. |
| Mano izquierda | Se acercó el mando izquierdo al portapapeles de la habitación 2 y se pulsó grip. | El interactor izquierdo seleccionó `PortapapelesClinico`; posteriormente se comprobó sin selección y con gravedad. No se documentó un transporte elevado estable de ese portapapeles. |
| Asociación de todas las lámparas | Prueba directa de `TrySwitchOff()` en Play Mode, con el control colocado bajo cada blanco. Se compararon los estados de las 16 luces, incluyendo GameObjects inactivos. | Las 16 asociaciones cambiaron exactamente un componente Light y siempre el asociado. Para las tres luces principales previamente inactivas se habilitó temporalmente solo el componente durante la prueba: esto verifica el enlace y cambio de estado, no su iluminación visible. |
| Obstáculo | Se interpuso un cubo temporal entre el rayo y una luminaria encendida. | La luz permaneció encendida; el cubo se retiró después. |
| Distancia | Durante la prueba se redujo temporalmente el alcance a 0.1 m con la lámpara a mayor distancia. | No se apagó; al restablecer 12 m sí se apagó. Alcance final guardado: 12 m. |
| Locomoción existente | Se inyectó eje primario izquierdo en el simulador. Se observaron resolución del LocomotionMediator, XRBodyTransformer y XROrigin, acción Move habilitada y vinculada al mando simulado. | El XR Origin pasó de (-1.15, 0, 0.65) a aproximadamente (-2.36, -0.05, 0.65). Desplazamiento horizontal ≈1.21 m. No se recorrió todo el hospital. |
| Cuatro nombres | Capturas posicionadas dentro de las cuatro habitaciones, desde altura de ojos 1.65 m y aproximadamente 1.5 m de la placa. | Samuel, Alejandro, Vicente y Alexander visibles, sin recorte y al pie de sus respectivas camas. Esto verifica la imagen del Editor, no legibilidad estereoscópica con visor. |
| Integridad y guardado | Inspección final de componentes, escena fuera de Play Mode y guardada. | 0 scripts faltantes, dos ControllerHandAnimator conservados, cuatro nombres diferentes. El control vuelve a su posición inicial sobre Commode y las luces recuperan sus estados originales al salir de las pruebas. |

Durante la sesión hubo mensajes intermitentes de reconexión de Unity MCP y un aviso de assets de paquetes inmutables. La conexión permitió completar el trabajo. También se corrigieron advertencias de APIs obsoletas del script de configuración. La consulta final no mostró mensajes pendientes; no se modificó el paquete MCP. Se restauraron cambios incidentales que Unity realizó en materiales RCP preexistentes durante las pruebas.

## Revisión de criterios: solo información

No se añadieron interacciones del Character Controller, disparadores de audio ni cambios de locomoción para satisfacer estos criterios.

| Criterio | Estado | Evidencia |
|---|---|---|
| Tema del escenario libre | **Cumple** | Hospital con cuatro habitaciones, pacientes, equipamiento clínico y pasillo. |
| Uso de assets propios o descargados permitido | **Cumple** | Se usan modelos, materiales y sonidos ya presentes en Assets/Hospital, Assets/PolyOne, Assets/Sounds y recursos XRI. La procedencia o autoría exacta de cada recurso no se auditó; el criterio permite ambas opciones. |
| Sistema de locomoción para recorrer el escenario | **Cumple** | XR Origin con Character Controller; `DynamicMoveProvider`, `SnapTurnProvider`, `LocomotionMediator` y `XRBodyTransformer`. Se comprobó desplazamiento con entrada simulada. No se probó todo el recorrido ni hardware físico. |
| Espacio adecuado para exploración | **Cumple** | Cuatro habitaciones de aproximadamente 6 × 5.5 m, separadas 6.5 m, accesos abiertos y pasillo longitudinal. Se inspeccionaron geometría y colliders; no se hizo una navegación exhaustiva de todos los accesos. |
| Al menos tres interacciones de entorno mediante Character Controller con triggers u OnControllerColliderHit, independientes de audios | **No cumple** | No se encontró ninguna interacción elegible. Los únicos callbacks de trigger presentes en scripts activos de la escena son de los XRDirectInteractor de las manos; no corresponden al Character Controller. Los scripts RCP comparan posiciones de manos. |
| Al menos tres audios activados por necesidad o posición, independientes del recuento de interacciones anterior | **No cumple**, con interpretación estricta de la condición de activación | Hay cuatro sonidos distintos, pero solo clic de botones y metrónomo tienen una condición explícita por acción/estado. TosOcasional usa temporizador aleatorio; AmbienteHospital inicia automáticamente. No se encontró una tercera activación condicionada por posición o necesidad del jugador. La atenuación espacial de la tos no es un trigger de activación por posición. |

### Interacciones encontradas

Elegibles para el requisito específico del Character Controller: **0**.

Las siguientes existen, pero no se suman a ese requisito:

1. Iniciar práctica RCP: `Boton_Iniciar`, `RcpTouchButton` → `RcpSession.StartPractice`.
2. Finalizar práctica: `Boton_Finalizar` → `RcpSession.Finish`.
3. Reiniciar práctica: `Boton_Reiniciar` → `RcpSession.ResetPractice`.
4. Activar/desactivar metrónomo: `Boton_Metronomo` → `RcpMetronome.Toggle`.
5. Reducir volumen: `Boton_VolumenMenos` → `RcpMetronome.VolumeDown`.
6. Aumentar volumen: `Boton_VolumenMas` → `RcpMetronome.VolumeUp`.
7. Compresiones RCP: `EstacionRCP_01/Logica`, `RcpCompressionDetector`, según seguimiento y desplazamiento relativo de manos sobre `ReferenciaTorax_RCP`.
8. Agarrar/soltar el control y tres portapapeles: `XRDirectInteractor` + `XRGrabInteractable`, mediante mandos.
9. Apagar luz con control: `LightRemote` + `RemoteLightTarget`, mediante activación XRI y raycast.

Los seis botones RCP se activan por comprobación de posiciones de manos dentro de un volumen calculado en `RcpTouchButton.Update`; no usan el Character Controller, pese al nombre triggerSize de su campo.

### Audios encontrados

1. **Clic** `click_002`: `EstacionRCP_01/Logica`, AudioSource compartido por los seis `RcpTouchButton`; `Press()` ejecuta PlayOneShot. Seis botones con el mismo clip cuentan como un sonido distinto.
2. **Metrónomo** `tick_001`: `EstacionRCP_01/Metronomo`, AudioSource + `RcpMetronome`. Se programa con PlayScheduled solo si la sesión está Activa y el metrónomo habilitado.
3. **Tos** `mixkit-sick-man-coughing-2221`: `Habitacion_02/Paciente_02/TosOcasional`, AudioSource 3D + `PatientCough`; PlayOneShot cada 18–40 s. El clip está asignado en PatientCough.clip, aunque AudioSource.clip esté vacío. No lee posición del jugador.
4. **Ambiente** `fadingembersaudio-heart-monitor-hospital-ambience-430219`: `AmbienteHospital`, AudioSource con playOnAwake y loop, mezcla 2D. Se observó isPlaying=true durante Play Mode. No tiene activación condicional por proximidad o estado.

La configuración y código de los cuatro audios se inspeccionaron. No se escucharon ni se midieron audibilidad o niveles. No se ejecutaron sesiones RCP completas para probar todos sus eventos sonoros.

## Uso y comprobaciones pendientes con hardware

En visor: acercar la mano al control sobre Commode de la habitación 1, mantener grip, levantarlo, orientar el extremo emisor hacia una luminaria y pulsar gatillo. Soltar grip para dejarlo. El botón solo apaga. Para repetir con luces encendidas, reiniciar Play Mode, que recupera el estado guardado.

En el XR Device Simulator importado, las asignaciones actuales incluyen Shift izquierdo para manipular la izquierda, Espacio para la derecha, G para grip y botón izquierdo del ratón para gatillo; Ctrl izquierdo permite rotación. Estas asignaciones se leyeron de los inputactions. Las pruebas automatizadas anteriores usaron los estados del simulador directamente, no estas teclas.

**Requiere visor y mandos físicos:** comprobar comodidad del agarre y alineación de la palma, precisión de apuntado con movimientos reales, seguimiento de ambas manos, sensación y animación bajo entradas físicas, legibilidad estereoscópica a distintas distancias, accesibilidad de Commode (tablero a unos 54 cm), comodidad de locomoción y audibilidad espacial. No se afirma que esas pruebas hayan sido realizadas.

Limitaciones preexistentes observadas: ausencia de tres interacciones del Character Controller y ausencia de tres audios con condición explícita de activación según el criterio estricto anterior. Se informan sin corregirse, conforme al alcance solicitado. No quedan errores de compilación detectados ni fallos pendientes de las pruebas de control y raycast ejecutadas.

## Capturas conservadas

- [Samuel](Assets/Screenshots/screenshot-20261028-101423.png)
- [Alejandro](Assets/Screenshots/screenshot-20261028-101943.png)
- [Vicente](Assets/Screenshots/screenshot-20261028-102011.png)
- [Alexander](Assets/Screenshots/screenshot-20261028-101953.png)
- [Control sobre Commode](Assets/Screenshots/screenshot-20261028-101953_1.png)
