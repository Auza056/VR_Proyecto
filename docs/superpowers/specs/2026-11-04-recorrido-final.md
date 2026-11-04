# Implementar la versión final de VR_Proyecto con Unity MCP

Actúa como desarrollador senior de Unity XR. Trabaja sobre mi copia local de [Auza056/VR_Proyecto](https://github.com/Auza056/VR_Proyecto). Quiero que **implementes y verifiques los cambios en el proyecto**, incluida la integración en escenas y prefabs mediante Unity MCP.

El producto final es un simulador educativo en español para público general. Debe permitir aprender a reconocer una emergencia simulada, pedir ayuda y practicar el ritmo de las compresiones en VR. La plataforma objetivo es **Windows PC VR con Oculus Rift y mandos Touch**. Conserva el XR Device Simulator para pruebas de escritorio.

## 1. Conecta y comprueba el estado real del proyecto

- Identifica la raíz del repositorio local y revisa `git status`, la rama, el commit y las instrucciones del proyecto. Preserva el trabajo local y evita sobrescribir cambios ajenos. Trabaja en una rama de implementación si puedes hacerlo sin interferir con esos cambios. Deja los cambios locales para revisión.
- Descubre las herramientas de Unity MCP disponibles en la sesión. Haz una consulta de solo lectura para confirmar el proyecto conectado, la escena activa y el estado de la consola. Verifica que el Editor abierto corresponde al repositorio antes de modificar escenas o prefabs.
- Lee `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/EditorBuildSettings.asset`, `Packages/manifest.json`, los scripts propios bajo `Assets/VR`, la documentación de la estación y `VERIFICACION_HOSPITALWARD.md`. La revisión previa encontró Unity `6000.5.11f1`, URP, XR Interaction Toolkit `3.5.1` y `HospitalWard.unity` como escena habilitada para la compilación. El estado local puede ser más reciente.
- Inspecciona en el Editor el XR Origin, los dos mandos, la cámara, las acciones de entrada, los interactores, el proveedor XR activo y las referencias reales de la estación. Reutiliza la locomoción, las manos, la camilla, la consola, los audios y los portapapeles existentes.
- Si falla MCP, diagnostica la conexión con las herramientas que realmente estén disponibles. Si sigue bloqueada, avanza con el código que puedas verificar localmente y deja identificadas las acciones de Editor pendientes. Distingue en el informe qué quedó escrito, qué se integró y qué se probó.

## 2. Añade un metrónomo táctil, visual y sonoro

Parte de `RcpMetronome.cs`. Actualmente el clic se programa con el reloj DSP. Usa **una sola cadencia compartida**, de 110 pulsos/min por defecto, para que los tres canales marquen el mismo instante.

- Emite **un impulso háptico breve por pulso programado** durante la sesión guiada activa. Usa como punto inicial 45 ms y amplitud 0,25, ajustables. Valida la comodidad en el Rift antes de fijar el valor final.
- Envía el impulso al llegar el instante del pulso. El audio puede programarse con anticipación para el DSP. Evita adelantar la vibración por ese margen de programación, duplicar pulsos o disparar una ráfaga tras una caída de FPS.
- Empieza con un mando configurable y permite elegir izquierdo, derecho o ambos. Verifica el binding y la API contra la versión de XRI instalada y el proveedor XR activo. Reutiliza `HapticImpulsePlayer` si encaja con la configuración real. Si hace falta usar `UnityEngine.XR.InputDevice`, comprueba que el dispositivo sea válido y admita impulsos, y distingue claramente ese tipo del `InputDevice` de Input System.
- Añade controles separados para **audio, vibración y pulso visual**. Conserva el volumen del metrónomo y añade intensidad háptica ajustable. La vibración y la señal visual deben seguir funcionando con el audio apagado o sin un clip asignado. Usa una animación breve y discreta en el indicador visual.
- El pulso marca el **momento objetivo de iniciar la compresión**, independientemente de `CompressionDetected`. Para indicar una compresión registrada, usa el contador o un icono distinto. Evita superponer una segunda vibración por cada compresión.
- El metrónomo debe cesar al finalizar, cancelar, reiniciar, desactivar la estación, salir del área de práctica o perder el foco. Cancela los clics pendientes y limpia el estado del reloj. Si el mando se desconecta o carece de háptica, mantén los canales restantes y recupera el mando al volver a conectarse.
- En el simulador, verifica el despacho de impulsos mediante un contador o receptor de prueba. Reserva la comprobación de vibración, sincronía percibida e intensidad para los Touch físicos.

## 3. Mejora la detección y el informe de RCP

Reutiliza `RcpCompressionDetector`, `RcpSession`, `RcpRateEstimator` y `RcpPanelView`.

- Revisa el caso en que los mandos quedan a lados opuestos del marcador pero su punto medio cae dentro. Ajusta la zona y el criterio de proximidad para admitir la superposición razonable de los Touch, incluso con una mano algo más alta. Valida la tolerancia en el visor. Mantén la protección existente contra falsos conteos al caminar o girar.
- Separa **manos fuera de la zona** de **seguimiento perdido**. Si hubo interrupciones de seguimiento, indícalo en el resumen. Con pocos datos, muestra «Datos insuficientes» para las métricas afectadas.
- Presenta compresiones detectadas, ritmo estimado, porcentaje de compresiones evaluadas dentro de 100–120/min, pausa más larga y retorno del mando. Revisa qué pasa con el último retorno pendiente al terminar y define qué muestras se incluyen en cada porcentaje.
- El resumen debe dar una sugerencia concreta para el próximo intento. Por ejemplo, «Practica con el metrónomo» o «Revisa las pausas». Las métricas describen la simulación y el movimiento de los mandos. La interfaz debe explicar que el sistema no mide fuerza, profundidad real del tórax ni competencia clínica.

## 4. Integra un manual ilustrado dentro del hospital

Convierte uno de los portapapeles existentes en un **manual agarrable y legible en VR**. Añade un atril o soporte junto a la estación para consultarlo con las manos libres. Usa páginas reutilizables, texto grande, contraste alto y botones táctiles con confirmación visual. Permite volver a consultar el manual y recuperarlo si cae fuera del alcance.

Crea ilustraciones vectoriales originales o gráficos simples, con una acción por página. Implementa estas cinco páginas.

1. **Controles VR.** Dibujo de los Touch y una acción de prueba para agarrar, soltar y tocar un botón.
2. **Reconocer la emergencia.** Viñetas para observar el entorno y comprobar respuesta y respiración del adulto virtual.
3. **Pedir ayuda.** Dibujo del teléfono y de un testigo. Explica cómo indicar la ubicación y pedir que traigan un DEA.
4. **Colocar las manos.** Vista superior de la referencia en el tórax y vista lateral de la postura. Vincula la ilustración con el marcador de la estación.
5. **Seguir el ritmo.** Dos viñetas para descenso y retorno, y una demostración de los pulsos del metrónomo.

Mantén los textos de la simulación editables desde el proyecto. Usa las [guías AHA 2025 de soporte vital básico adulto](https://cpr.heart.org/en/resuscitation-science/cpr-and-ecc-guidelines/adult-basic-life-support/) como referencia y deja indicado qué contenido debe revisar un instructor local. Incluye una nota breve en el manual y el resultado sobre el alcance educativo de la simulación.

## 5. Completa el recorrido de aprendizaje

Implementa el flujo **Manual → Reconocer → Pedir ayuda → Practicar → Reto → Resultados**. Conserva un acceso directo a la estación para repetir el ejercicio sin recorrer todo el escenario.

- **Reconocimiento.** Crea un caso de un adulto que acaba de colapsar fuera del hospital, por ejemplo en una recepción universitaria simulada. Reutiliza el espacio y recursos existentes. Usa un paciente virtual sobre una superficie firme y diferencia visualmente este caso del maniquí de la estación. Registra las acciones relevantes y da una pista si se omite un paso en el modo guiado.
- **Llamada simulada.** Integra un teléfono interactivo o un panel accesible desde el mismo objeto. Permite indicar la ubicación, describir la situación y solicitar que traigan un DEA. Usa «Emergencias — simulación» mientras el número local no esté verificado. La interacción funciona íntegramente dentro del juego.
- **Práctica guiada.** Activa marcador, mensajes y metrónomo configurable. Reutiliza la duración actual de 60 segundos y permite configurarla para la demo desde el Inspector.
- **Reto.** Usa la misma duración que la práctica al comparar intentos. Arranca con marcador y metrónomo apagados por defecto, conserva los avisos técnicos necesarios y muestra los resultados al final. Si el usuario activa ayudas, indica ese estado en el resumen.
- **Reinicio y comodidad.** Restablece objetos, estados y métricas al repetir el caso. Si la rúbrica exige una interacción del `CharacterController`, usa la entrada a la zona de práctica para mostrar la orientación de la estación y verifica que el disparador funciona con el rig real. Mantén la locomoción y el giro actuales.

## 6. Añade las ampliaciones después de estabilizar el recorrido principal

Implementa estas fases en este orden, con cambios separados y verificables.

1. **DEA simulado.** Crea un equipo agarrable con dos parches y puntos de colocación. Simula los mensajes de encendido, colocación, análisis, apartarse y seguir la indicación del equipo. Incluye una página ilustrada en el manual. Marca qué partes del guion necesitan revisión por un instructor.
2. **Caso de contraste.** Añade una persona que sí responde, aprovechando otra habitación. El usuario debe observar el caso y elegir pedir ayuda de forma adecuada sin iniciar automáticamente compresiones.
3. **Ficha del piloto.** Deja en el repositorio un protocolo breve para probar con 5–8 compañeros. Registra de forma anónima si reconocen la situación, piden ayuda y dónde se traban. Deja los resultados para cuando se realice el piloto.

Si un bloque de ampliación pone en riesgo la estabilidad de la entrega, conserva el flujo principal funcional y documenta con precisión lo pendiente.

## 7. Verifica y prepara la entrega

- Integra los cambios por etapas en Unity MCP. Tras cada etapa, espera la recompilación, revisa la consola, comprueba referencias y guarda los prefabs y la escena. Verifica que siga habiendo un solo XR Origin, cámara, AudioListener y simulador. Evita actualizar paquetes o cambiar de proveedor XR salvo que un bloqueo comprobado lo exija.
- Ejecuta pruebas de Play Mode con entradas del XR Device Simulator cuando estén disponibles. Comprueba el recorrido completo, agarre y lectura del manual, llamada, ritmo lento/en rango/rápido, manos separadas, movimiento del rig, pérdida de seguimiento, sesión sin compresiones, reinicio y funcionamiento independiente de los tres canales del metrónomo. Para el metrónomo, verifica también que no haya pulsos duplicados ni posteriores a la finalización.
- Si el Rift está disponible, prueba seguimiento con manos superpuestas, altura de camilla, alcance, lectura del manual, vibración de ambos Touch, sincronía con el clic, mezcla de audio y rendimiento. Si no tienes acceso físico al visor, deja una lista breve de comprobaciones manuales.
- Genera una compilación Windows fuera de las carpetas versionadas si el entorno lo permite. Abre la compilación y comprueba la escena de inicio si puedes. Documenta cualquier bloqueo observado.
- Actualiza el README con la escena principal real, requisitos, controles, conexión del Rift, uso del simulador, recorrido de la demo y limitaciones. Revisa la procedencia de modelos, sonidos e ilustraciones y documenta las licencias o permisos pendientes de confirmar. Mantén los recursos de terceros y la configuración ajena a esta tarea fuera de cambios innecesarios.
- Entrega un resumen con **cambios implementados, escenas y archivos modificados, evidencia de pruebas, ruta de la compilación, pendientes de Rift físico y guion de demo de 3–4 minutos**. Ejecuta las verificaciones antes de declarar una fase terminada.

Empieza por conectar Unity MCP e inspeccionar la estación. Luego implementa y verifica el metrónomo háptico antes de pasar al manual y al caso de emergencia.