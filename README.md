# Simulador educativo VR de reconocimiento y ritmo de RCP

Escena principal: **`Assets/Scenes/HospitalWard.unity`**, única escena habilitada para compilar. Abrir con **Unity 6000.5.11f1**. Windows PC VR, Oculus Rift y mandos Touch; URP 17.5.0, XR Interaction Toolkit 3.5.1, Input System 1.20.0 y OpenXR. Las escenas HospitalRCP y MainScene se conservan como antecedentes.

## Iniciar con Rift

1. Conecta el Rift y ambos Touch y comprueba su seguimiento en el software de escritorio del visor. Usa el runtime OpenXR compatible con tu instalación de Rift.
2. Abre HospitalWard o el ejecutable Windows. El proyecto conserva OpenXRLoader y el perfil Oculus Touch de Windows. No se cambió de proveedor ni se actualizaron paquetes.
3. Si se detecta un visor XR activo, el simulador de escritorio permanece desactivado. Sin visor, `DesktopSimulatorGate` activa el XR Device Simulator existente, sin crear otro rig.
4. Ajusta posición, alcance y altura antes de practicar. La vibración parte de .25 y 45 ms, pendiente de validación de comodidad con Touch físicos.

## Controles

- Se mantienen desplazamiento continuo con el stick izquierdo y giro por pasos con el derecho, según las acciones existentes del rig.
- **Grip:** agarrar manual, DEA o parches. Liberar grip para soltar.
- **Tocar botones:** introducir una mano en su superficie; no requiere gatillo. Retirar la mano para volver a pulsar. El botón confirma con movimiento visual.
- Manual: Anterior / Siguiente / PRUEBA. El atril permite leer sin sostenerlo. RECUPERAR lo devuelve cuando está suelto; también se recupera si cae o queda lejos.
- Consola RCP original: INICIAR / FINALIZAR / REINICIAR, interruptor general del metrónomo y volumen.
- Panel adicional: audio, vibración y señal visual separados; intensidad −/+ y selección de Touch izquierdo, derecho o ambos. El pulso marca el **inicio objetivo del descenso**; el contador registra ciclos detectados.

## Escritorio con XR Device Simulator

Pulsar Play y dar foco a **Game**. Las pruebas automatizadas también requieren mantener la ventana de Unity en primer plano: salir de foco finaliza el intento por diseño.

- U: cuerpo/visor; botón derecho y ratón: mirar; WASD/Q/E: mover según el modo seleccionado.
- T/Y: seleccionar mando izquierdo/derecho; Shift izquierdo / Espacio permiten manipularlos mientras se mantienen pulsados.
- 3: modo Position; Ctrl izquierdo: rotación según las acciones del simulador importado.
- G: grip del mando seleccionado; botón izquierdo: gatillo. Las posiciones de los mandos activan los botones táctiles.

El panel del simulador muestra las asignaciones importadas. El simulador comprueba solicitudes de háptica, **no vibración física**.

## Recorrido de aprendizaje

1. **Manual:** junto a la camilla de la habitación 1. Seis páginas: controles, reconocimiento, ayuda, manos, ritmo y DEA. Probar agarrar, soltar y tocar.
2. **Reconocer:** INICIAR CASO en el panel del recorrido. En la recepción universitaria simulada del pasillo, observar entorno, respuesta y respiración del adulto sobre suelo firme. El caso se diferencia de la estación de camilla.
3. **Pedir ayuda:** completar ubicación, situación y petición de un DEA a un testigo en el teléfono. CONTINUAR activa la preparación en la habitación 1. El teléfono muestra «Emergencias — simulación» y no realiza llamadas.
4. **Practicar:** mantener las manos cerca del marcador 1.5 s y practicar 60 s por defecto. Duración ajustable en `RcpSession.sessionDuration` para demos.
5. **Reto:** consultar el primer resultado y tocar INICIAR RETO. Usa la misma duración; marcador y metrónomo apagados. Si se activan ayudas, queda registrado. Sin ayudas se conservan los avisos técnicos y se omiten consejos de colocación/ritmo.
6. **Resultados:** conteo, ritmo estimado, porcentaje evaluado en 100–120/min, retorno del mando, pausas y pérdidas de seguimiento. Con pocas muestras aparece «Datos insuficientes». Una sugerencia orienta el siguiente intento.

**PRÁCTICA DIRECTA** permite repetir sin rehacer el caso. REPETIR CASO limpia progreso y métricas y devuelve los objetos sueltos. Salir del área, desactivar la estación o perder foco interrumpe el intento y cancela los pulsos.

## Ampliaciones

- **DEA simulado:** equipo junto al teléfono. Encender, agarrar los dos parches, colocarlos sobre sus puntos numerados en el esquema de tórax descubierto y soltarlos. Apartar ambas manos durante el análisis y antes de tocar DESCARGA. El guion indica reanudar RCP. No hay análisis fisiológico ni descarga real. REINICIAR DEA restablece equipo y parches.
- **Caso B:** panel en la habitación 3, escenario de sala de descanso. La persona responde y respira normalmente. Observar y pedir ayuda sin iniciar compresiones automáticamente.
- **Piloto:** [protocolo anónimo para 5–8 compañeros](docs/PILOTO.md). Aún no realizado.

## Contenido y mantenimiento

- Textos del manual, ubicación y alcance: `Assets/VR/Learning/ContenidoEducativo.asset`.
- Mensajes del recorrido: `TrainingFlow.cs`; guion del DEA: `TrainingAed.cs`. Editables desde el código del proyecto.
- Estación: `Assets/VR/RCP/Prefabs/EstacionRCP.prefab`; manual: `Assets/VR/Learning/ManualIlustrado.prefab`. Las referencias a rig/tórax/atril pertenecen a la escena y deben asignarse al reutilizar un prefab.
- Configuración reproducible en Editor: `RcpFinalSetup.Apply()`, `TrainingSetup.Apply()`, `TrainingExtensionsSetup.Apply()` y `FinalDeliverySetup.SavePrefabs()`, en ese orden sobre HospitalWard. No ejecutar repetidamente para reemplazar ajustes manuales del diseñador.
- Pruebas en `Assets/VR/Tests/Editor` y `Assets/VR/Tests/PlayMode`, ejecutables desde Unity Test Runner o MCP. La suite Play Mode inyecta estados al XR Device Simulator; no sustituye una prueba con Rift.

## Alcance y entrega

Simulación educativa para público general. Mide movimiento de mandos: **no mide fuerza, profundidad real del tórax ni competencia clínica**. La camilla enseña movimiento en VR; el caso de reconocimiento representa un adulto sobre suelo firme. No sustituye formación acreditada.

Referencia: [AHA 2025, Adult Basic Life Support](https://cpr.heart.org/en/resuscitation-science/cpr-and-ecc-guidelines/adult-basic-life-support/). Un instructor local debe revisar los textos, reconocimiento, llamada y guion del DEA, además del número local, antes de usar la demo en enseñanza.

Consultar [evidencia, compilación, pendientes de Rift y guion de demo](docs/ENTREGA_XR.md) y [procedencia y permisos pendientes](docs/PROCEDENCIA.md). La documentación histórica `VERIFICACION_HOSPITALWARD.md` describe el estado anterior y se conserva.
