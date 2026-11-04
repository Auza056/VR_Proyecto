# Preparación para presentación (sin build)

Base: d018e99. Rama implementacion/presentacion-casi-final.

- Menú Inicio/Ajustes/Resultados con progreso, accesos contextuales y confirmación de reinicio.
- Canales con estado explícito, duración 30/60 s antes de practicar y misma duración objetivo para el reto.
- Comparación lado a lado; intentos interrumpidos no desbloquean falsamente la siguiente etapa.
- Botones con respuesta de contacto, confirmación y estado deshabilitado; pantalla RCP y paneles con contraste y marcos adecuados.
- Guardar escena/prefabs, verificar texto, referencias y pruebas en Editor. No ejecutar BuildPipeline ni manage_build.

## Uso para la exposición

Abrir `Assets/Scenes/HospitalWard.unity` y pulsar Play. El jugador aparece frente al menú.

- **INICIO:** recorrido completo o práctica directa. INICIAR RETO se desbloquea tras la práctica; FINALIZAR solo está disponible al preparar o ejecutar un intento.
- **AJUSTES:** antes de practicar, elegir 30 s para una demo breve o mantener 60 s. La misma duración objetivo se conserva para el reto y queda bloqueada durante la comparación. Audio, vibración y visual muestran estado. La intensidad y el volumen se ajustan por separado; el alcance de ambos se mantiene en 0–100 %.
- **RESULTADOS:** práctica y reto en columnas, sin inventar métricas si faltan datos. Se avisa si los tiempos observados difieren. Una interrupción técnica conserva el resultado parcial y permite repetir sin avanzar falsamente de etapa.
- **REINICIAR RECORRIDO:** tocar, retirar la mano y volver a tocar en cuatro segundos. Cambiar de acción o dejar vencer ese plazo cancela la confirmación.
- Pasos de reconocimiento y llamada: color de confirmación y texto LISTO. No se pueden repetir acciones ya registradas ni continuar la llamada antes de completarla.
- Todos los menús se tocan con los mandos seguidos. Los botones oscuros no aceptan acciones. Si se pierde el seguimiento no se pulsan botones con una pose obsoleta.

### Demo breve sugerida

1. Inicio frente al menú; mostrar sus tres pestañas y el manual.
2. Elegir 30 s; iniciar caso, reconocer y completar teléfono.
3. Práctica guiada 30 s, reto 30 s; mostrar comparación y sugerencia en estación.
4. Mostrar DEA y caso B si el tiempo lo permite. Reiniciar para el siguiente participante.

Para la presentación larga, mantener 60 s. Los cambios nuevos se prueban en el Editor; **el ejecutable de la entrega anterior no contiene estas mejoras**. Generar el nuevo build en casa desde esta escena.

## Evidencia final (4 de noviembre de 2026)

- Edit Mode: job `583cdb99e8b0451db255f41103f771af`, **21/21**, cero fallos.
- Play Mode después de guardar escena y prefabs: job `6d9d76b62d064e75a166ead6c4c3c3ee`, **16/16**, cero fallos (58.06 s).
- Pruebas nuevas: botones bloqueados no invocan eventos; reto interrumpido se puede reintentar; pestañas por contacto real de poses del XR Device Simulator; confirmación de reinicio; cambios de pestaña no borran progreso; duración bloqueada y conservada al entrar al reto; resultados parciales visibles sin desbloquear falsamente etapas.
- Se conservan las pruebas de compresiones lentas/en rango/rápidas, manual agarrable, teléfono táctil, DEA, salida de área, tracking y foco. La primera prueba de teléfono señaló una ambigüedad del receptor de pruebas entre botones homónimos del caso A y B; el receptor ahora elige el botón activo y disponible, como debe hacerlo el participante.
- Consola final: **0 errores, 0 advertencias**. El Starter Kit conserva advertencias de API obsoleta que pueden reaparecer al recompilar todos sus scripts; no se modificaron paquetes para silenciarlas.
- Inspección: **1 XR Origin, 1 cámara, 1 AudioListener, 1 simulador**, cero scripts faltantes, cero botones con manos sin asignar y cero vínculos inválidos del menú. Duración guardada: **60 s**; 30 s es opcional. Spawn: (-1.80, 0, -1.22), mirando al menú. No se cambiaron las entradas ni los proveedores de locomoción/giro.
- Texto de las tres páginas: **sin desbordamientos**. También se inspeccionó la comparación sin compresiones y la pantalla RCP. Capturas en `Assets/Screenshots/Presentacion`. La captura de comparación usa intentos detenidos manualmente sin compresiones; no son resultados de usuarios ni del piloto.
- Prefabs actualizados: `Assets/VR/RCP/Prefabs/EstacionRCP.prefab`, `Assets/VR/Learning/ManualIlustrado.prefab`; nuevo `Assets/VR/Presentation/MenuAprendizaje.prefab`. Sus vínculos externos a flow, manos, tórax o atril se asignan en la escena al reutilizarlos.
- Configuración reproducible: `Hospital VR > Preparar presentación` ejecuta `PresentationSetup.Apply()` sobre HospitalWard fuera de Play. Reconstruye la presentación y reposiciona el punto de inicio: no usar sobre cambios manuales sin revisarlos.
- Se restauró la opción temporal del Test Runner. TMP repobló su caché dinámica de glifos al inspeccionar el texto nuevo; se conserva esa actualización de datos de fuente, sin cambiar tipografía ni paquetes. Cambios locales en `implementacion/presentacion-casi-final`, sin commit ni push.
- **No se llamó a herramientas de build ni se generó/actualizó ningún ejecutable Windows en esta revisión.** El build anterior permanece como antecedente.

## Aún requiere comprobación en casa

Rift/Touch físicos: legibilidad estereoscópica del menú y del resumen, alcance de botones, comodidad de vibración, tracking al superponer manos y rendimiento. El piloto de 5–8 personas y revisión del instructor siguen pendientes, tal como se documentó en la entrega anterior. No se inventaron resultados de esas validaciones.
