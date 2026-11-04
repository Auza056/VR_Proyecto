# Recorrido educativo XR — Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans. Conservar cambios sin commit para revisión, según el encargo.

**Goal:** Implementar y comprobar el recorrido educativo en español con una estación RCP reutilizable.
**Architecture:** Extender los componentes RCP existentes; reloj DSP compartido y salida háptica XRI. Manual y caso como componentes separados, contenido editable y configuración de Editor reproducible ejecutada por MCP.
**Tech Stack:** Unity 6000.5.11f1, URP 17.5, XRI 3.5.1, Input System 1.20, OpenXR; Windows64.
**Spec:** `docs/superpowers/specs/2026-11-04-recorrido-final.md` (encargo del usuario).

## Global Constraints

- Preservar ocho configuraciones modificadas antes de empezar; copia externa `../VR_Proyecto-cambios-previos.patch`.
- Mantener HospitalWard, un XR Origin, una cámara/AudioListener y el XR Device Simulator.
- Sin actualizaciones de paquetes ni cambios de proveedor XR. Sin llamadas externas del teléfono.
- RCP: 60 s por defecto y misma duración en reto; 110 pulsos/min, 45 ms y amplitud 0,25 ajustables.
- Educación: movimiento de mandos, sin medición de fuerza, profundidad torácica o competencia clínica.

## Review Focus

- Caída de FPS: omitir pulsos vencidos, sin ráfagas ni vibración anticipada.
- Desactivar audio o quitar clip: mantener visual y háptica; finalizar cancela audio programado.
- Seguimiento perdido, manos separadas y movimiento del rig: no contar falsos ciclos.
- Último retorno: excluir una ventana incompleta, conservar retorno ya confirmado.
- Reinicios y salida de zona/foco: limpiar sesión y objetos sin ayudas inadvertidas en reto.

### Task 1: Metrónomo

**Files:** RcpMetronome.cs; nuevo RcpBeatClock.cs; Tests/Editor/RcpClockTests.cs; Editor/RcpFinalSetup.cs.
**Interfaces:** RcpBeatClock.Start/Stop/Tick; RcpMetronome.BeatDispatched, HapticDispatchCount, StopClock; salidas HapticImpulsePlayer.
- [x] Pruebas de cadencia, margen DSP, duplicados, salto temporal y cancelación; ejecutar RED.
- [x] Implementar reloj/salidas independientes y controles. Integrar en estación y prefab con MCP.
- [x] Compilar, ejecutar pruebas y comprobar contadores en Play Mode antes del manual.

### Task 2: Detección e informe

**Files:** RcpCompressionDetector.cs, RcpChestReference.cs, RcpSession.cs, RcpRateEstimator.cs, RcpPanelView.cs; pruebas de métricas.
**Interfaces:** clasificación de dos manos, trackingLostTime/interruptions, ayudas y modo en RcpResults.
- [x] Probar ambos puntos fuera con punto medio dentro, distinta altura, reinicio de estimador tras pausa y retorno pendiente.
- [x] Implementar tolerancias y denominadores explícitos, sugerencia y datos insuficientes.
- [x] Verificar ciclos lentos/en rango/rápidos y protección al mover rig con entradas simuladas.

### Task 3: Manual y recorrido

**Files:** nuevos Learning/TrainingContent.cs, TrainingManual.cs, TrainingFlow.cs, PracticeArea.cs y Editor/TrainingSetup.cs.
**Interfaces:** contenido ScriptableObject; cinco páginas originales; pasos Manual→Reconocer→Pedir ayuda→Practicar→Reto→Resultados.
- [x] Pruebas de orden de reconocimiento/llamada, reinicio y ayudas en reto.
- [x] Convertir portapapeles, soporte, recuperación, ilustraciones de líneas; integrar caso y teléfono.
- [x] Verificar textos, lectura en capturas, agarre por simulador y recorrido completo en Play Mode.

### Task 4: Ampliaciones

**Files:** Learning/TrainingAed.cs; segunda escena de caso dentro del hospital; docs/PILOTO.md.
- [x] DEA con secuencia, parches y comprobación de apartarse; página seis.
- [x] Caso que responde y protocolo anónimo de 5–8 personas. Verificar o dejar pendientes precisos si compromete estabilidad.

### Task 5: Entrega

**Files:** README.md, docs/ENTREGA_XR.md, docs/PROCEDENCIA.md, escena y prefabs.
- [x] Consola, referencias, conteo de rig y pruebas Edit/Play Mode; guardar.
- [x] Compilar Windows fuera del repositorio y abrir si es viable.
- [x] Registrar evidencia real, pendientes físicos Rift y guion de 3–4 min.

## Ledger

- Inspección: MCP conectado a la ruta correcta. Escena inicialmente vacía sin cambios; HospitalWard cargada. Referencias RCP válidas. OpenXR configurado.
- Decisión: rama en copia existente, sin worktree separado, para usar el Editor conectado. Mantener sin commits por solicitud de cambios locales revisables.
- Decisión: el encargo especifica alcance, orden y tecnología; se ejecuta directamente sin añadir aprobaciones de diseño.

- Task 1–4: implementadas e integradas; 19 Edit Mode y 12 Play Mode sin fallos. UI final sin desbordamientos y prueba táctil posterior 1/1. Compilación Windows job build-6403a06198 en curso.

- Task 5: completada. Build Windows64/Mono: 218.74 MB, 0 errores, 71 advertencias documentadas. Ejecutable abierto y arranque comprobado en escritorio; runtime OpenXR no disponible en este equipo durante la prueba. Informe y guion de 3:40 en docs/ENTREGA_XR.md.
