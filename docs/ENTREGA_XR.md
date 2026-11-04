# Evidencia de trabajo XR — 4 de noviembre de 2026

Base: `main`, commit `3526db2`. Rama de trabajo: `implementacion/recorrido-final-xr`. Ocho archivos de configuración ya estaban modificados; copia externa `../VR_Proyecto-cambios-previos.patch` y huellas `../VR_Proyecto-config-previa.json`.

MCP verificó `C:/Users/ALUMNO/Downloads/VR_Proyecto`, Unity 6000.5.11f1 y Windows64. Editor inicialmente con escena vacía no modificada; se cargó HospitalWard. URP 17.5, XRI 3.5.1, Input System 1.20. OpenXRLoader y perfil Oculus Touch Windows habilitados; no se actualizó ningún paquete.

## Metrónomo

- Job RED `51295147d0d64aecb33ad70733f30ea8`: 5 fallos esperados por ausencia del reloj.
- Job GREEN `2d5cd8be6758488ea5603adc5815dd17`: 5/5. Cadencia 110, margen inicial 50 ms, no anticipación, duplicado, ráfaga ni pulsos tras Stop.
- Play por MCP: con audio apagado y clip null, 19 pulsos y 19 solicitudes hápticas. Finish dejó ambos contadores en 19 en consulta posterior. Éxitos físicos=0, esperado sin Touch.
- XRI HapticImpulsePlayer verificado por reflexión y código del paquete. Binding separado `<XRController>{LeftHand}/haptic` y `{RightHand}/haptic`, propio ciclo de habilitación XRI.
- Valores iniciales provisionales: amplitud .25, duración .045 s, mando derecho. Validación física pendiente.

## Detección y lógica

- RED métricas: pausa anterior contaminaba ventana (22.86/min en vez de datos insuficientes), ausencia de validación individual.
- Job GREEN `7ef061d21f3c471c964f04d2ce7b071a`: 12/12 (reloj y métricas).
- Job GREEN `9be4c1c5c24e44cdaf0b725dc337b6cf`: 15/15, añade orden de reconocimiento/llamada, reto/resultados, reinicio y bloqueo de compresiones en caso que responde.

## Definición de muestras

- Conteo: ciclos de descenso y retorno detectados con poses válidas y rig quieto.
- Ritmo visible: al menos tres marcas en el tramo reciente, sin cruzar pausas >2.5 s. Ventana de seis por defecto.
- Ritmo medio: intervalos consecutivos ≤2.5 s, sin cruzar pérdida técnica de seguimiento; mínimo dos intervalos válidos. Las pausas largas se informan aparte.
- En rango: compresiones con una estimación reciente válida; primera y segunda del tramo se excluyen. Se muestra numerador/denominador.
- Retorno: se evalúa al siguiente descenso o a los .9 s; al finalizar se incluye el pendiente solo si ya alcanzó la altura de retorno. Ventanas truncadas o perdidas se excluyen, nunca se inventa un retorno negativo. Se muestra numerador/denominador.
- Pausa más larga: intervalos sin ciclos, incluido inicio→primero y último→fin; sin compresiones equivale a duración observada.
- Manos fuera y seguimiento perdido se acumulan por separado. El resumen indica interrupciones; no interpola movimiento perdido.
- Tolerancias iniciales Touch: separación horizontal .22 m, altura relativa .16 m, margen individual .04 m respecto a la zona; ajustar en Rift.

## Seguimiento de integración

Las primeras ejecuciones de Play Mode detectaron aislamiento insuficiente entre escenas y acción de grip persistente. Se identificó el singleton de simulador de XRI en DontDestroyOnLoad; se evita duplicarlo y las pruebas descargan la escena anterior antes de activar acciones nuevas. Se corrigió la cápsula inicial (antes altura 2 m centrada en 0, atravesaba el piso): altura 1.65, centro y .905, radio .30. Se conservan proveedores, acciones de movimiento y giro.

Los resultados finales de Play Mode y las capturas se detallan más abajo. La compilación se registra al terminar.

## Verificación final

- Edit Mode, suite completa: job `3e38e8ab60704013ac205a5b78312982`, **19/19**, 0 fallos. Incluye último retorno confirmado/incompleto, sin duplicación al cerrar.
- Play Mode: job `4cc5b424ba9b4f4f84f5914859cce3d5`, **12/12**, 0 fallos, 49.97 s.
- Cobertura Play: pulso con clip nulo/audio apagado y parada; ritmo 110 y rechazo de manos separadas; 80 y 140/min; giro/traslación del rig sin conteos; tracking separado de manos fuera; foco; salida por CharacterController; cero compresiones; agarre, soltado y recuperación del manual; reconocimiento y teléfono mediante volúmenes táctiles de los mandos; flujo y ayudas en reto; pulsaciones repetidas; DEA con dos parches y manos apartadas.
- Entradas: estados inyectados en XR Device Simulator → Input System → TrackedPoseDriver/XRI y detector. El recorrido lógico también usa llamadas directas en su prueba específica; otra prueba verifica los botones de reconocimiento/llamada por contacto simulado. La colocación de parches en la prueba de DEA usa las posiciones de sus Rigidbody; no acredita destreza de agarre físico de ambos parches.
- Se verificó una pérdida real de foco durante ejecuciones anteriores: el intento terminó con `Se perdió el foco de la aplicación`. Las pruebas esperan seguimiento válido y conservan temporalmente entrada en segundo plano; restauran ese ajuste. Para ejecutar la suite conviene mantener Unity/Game al frente.
- Inspección MCP en Edit Mode: escena guardada, 1 XR Origin, 1 cámara, 1 AudioListener, 1 XR Device Simulator, 0 scripts faltantes; referencias de manos, poses, tórax, háptica, indicador, atril, trigger y todos los botones válidas. Manual: 6 páginas. Duración guardada: 60 s.
- Revisión independiente de código: se corrigieron consejos educativos no declarados en reto y cambio involuntario de etapa al reiniciar. Pruebas de regresión incluidas.

## Archivos y escena

- `Assets/Scenes/HospitalWard.unity`: escena integrada; conserva hospital, locomoción, manos, mobiliario, pacientes, ambiente y control de luces.
- `Assets/VR/RCP/Prefabs/EstacionRCP.prefab`: controles de canales, indicador y área; referencias externas guardadas como overrides de escena.
- `Assets/VR/Learning/ManualIlustrado.prefab`, `ContenidoEducativo.asset`, materiales de trazos/piel de referencia y componentes de aprendizaje.
- Scripts RCP: metrónomo/reloj, detector, sesión, estimador y panel. Scripts nuevos: manual, flujo, progreso, DEA, recuperación y activación de simulador.
- Configuración reproducible: `Assets/VR/Editor/RcpFinalSetup.cs`, `TrainingSetup.cs`, `TrainingExtensionsSetup.cs`, `ManualIllustrations.cs`, `FinalDeliverySetup.cs`.
- Tests, README, este informe, `PILOTO.md`, `PROCEDENCIA.md` y especificación/plan.
- Se preservó exactamente el diff semántico previo de URP global. Los otros siete archivos inicialmente marcados no tenían diferencias semánticas contra HEAD (normalización de finales de línea); no se alteraron deliberadamente. No hay cambios de paquetes, proveedor ni perfil Touch.

## Pendientes con Rift físico

1. Conectar Rift/Touch, confirmar proveedor en ejecución y seguimiento de ambos mandos al superponerlos; ajustar margen horizontal/vertical y offset de contacto sin relajar el bloqueo por locomoción.
2. Probar vibración izquierda/derecha/ambas, reconexión y comodidad de .25 / 45 ms; ajustar con usuarios antes de fijar intensidad. Confirmar sincronía percibida con clic y señal visual, mezcla con ambiente y ausencia de doble vibración al comprimir.
3. Comprobar altura de camilla, alcance de todos los botones, seguridad del espacio, agarre, atril, tamaño/contraste y lectura estereoscópica del manual.
4. Medir rendimiento y estabilidad en el Rift; no se midieron tiempos de GPU ni fluidez con hardware físico.
5. Instructor local: revisar reconocimiento, guion de llamada y DEA, ubicación/número local y lenguaje para público general. El teléfono sigue rotulado simulación. Confirmar licencias pendientes antes de redistribuir.

## Demo de 3–4 minutos

| Tiempo | Acción |
|---|---|
| 0:00–0:25 | Presentar alcance educativo; agarrar/soltar manual y tocar PRUEBA. Mostrar páginas de reconocimiento y ritmo. |
| 0:25–0:55 | INICIAR CASO; observar entorno, respuesta y respiración en recepción; indicar ubicación/situación y pedir DEA en teléfono. CONTINUAR. |
| 0:55–1:55 | Práctica guiada de 60 s: colocar manos, seguir 110/min y mostrar controles separados. |
| 1:55–2:05 | Consultar primer resumen; INICIAR RETO. |
| 2:05–3:05 | Reto de 60 s con marcador y metrónomo apagados. |
| 3:05–3:40 | Leer métricas y sugerencia; explicar tracking/datos insuficientes y límites. Mostrar acceso directo y, si queda tiempo, página del DEA. |

El DEA completo y el caso B se muestran aparte si se dispone de más tiempo. Para acortar una demostración, cambiar una sola vez `sessionDuration` desde Inspector; práctica y reto seguirán usando el mismo valor. El piloto aún no tiene resultados.

## Capturas y comprobación visual

En `Assets/Screenshots/EntregaXR/`: `manual-final-controles.png`, `manual-final-manos.png`, `manual-final-dea.png` y `telefono-final.png`. Se inspeccionaron imágenes del Editor; no acreditan lectura estereoscópica. Las seis páginas se comprobaron con TMP tras abreviar ritmo/DEA y adaptar el título largo: 0 desbordamientos de título o cuerpo. Los paneles del pasillo se adelantaron para evitar quedar ocultos por la pared. La prueba de contacto del teléfono se repitió después, job `0942cd92a97540088246e84fc4e17889`: 1/1.

## Compilación Windows y arranque

- Resultado MCP: **Succeeded**, job `build-6403a06198`, Windows64 / Mono, HospitalWard como única escena; **0 errores y 71 advertencias**, 218.74 MB, 1136.37 s. La mayor parte del tiempo fue compilación inicial de variantes de URP.
- Ejecutable: `C:/Users/ALUMNO/Downloads/VR_Proyecto_Entrega_Windows/VR_Proyecto.exe`. Distribuir la carpeta completa, incluido `VR_Proyecto_Data` y las bibliotecas acompañantes.
- Se abrió el ejecutable en ventana 1280×720. Proceso `VR_Proyecto` respondió; captura `arranque-windows.png` en esa misma carpeta confirma estación de HospitalWard, manos y XR Device Simulator. Log: `Player-smoke.log`.
- Bloqueo de VR físico observado: el entorno devuelve `XR_ERROR_RUNTIME_UNAVAILABLE` al iniciar OpenXR. La aplicación continuó en escritorio con el simulador. Esto no acredita conexión ni funcionamiento del Rift; requiere runtime OpenXR instalado/activo y visor conectado para esa prueba.
- No se observaron excepciones administradas de los componentes educativos en el log de arranque. La comprobación del ejecutable fue de inicio y visualización; el recorrido automatizado completo se comprobó en el Editor.
- Las advertencias incluyen APIs obsoletas/campos de XRI Starter Kit preexistente, avisos de ScriptableSingleton de XRI y desconexiones/reconexiones WebSocket de MCP mientras el Editor estaba ocupado construyendo. Texto completo: `BuildWarnings.txt` junto al ejecutable. No se actualizaron esos paquetes para silenciar advertencias.

Cambios dejados sin commit en `implementacion/recorrido-final-xr`, listos para revisión. No se hizo push ni merge.

Advertencias del build verificadas: **23** del compilador sobre XRI Starter Kit, **4** sobre ScriptableSingleton de XRI y **44** de transporte MCP/WebSocket. Se restauraron las listas temporales que Unity rellenó al compilar (URP runtime settings y preloadedAssets), conservando el diff previo del usuario. También se retiró una sincronización incidental de `_Color` del material RCP rojo; su `_BaseColor` no cambió. El ejecutable fue cerrado al terminar la comprobación.
