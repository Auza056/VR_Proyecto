# Práctica de RCP (habitación 1)

Primera estación educativa de compresiones torácicas en `Assets/Scenes/HospitalWard.unity`. Los pacientes de las habitaciones 2 a 4 son solo ambientación.

## Cómo se usa

1. Entra en la habitación 1 (señal «HABITACIÓN 1 · Práctica de RCP») y colócate en la zona azul del suelo, junto a la camilla marcada como «CAMILLA DE PRÁCTICA · RCP».
2. En la consola acoplada a la camilla, toca **INICIAR** con la mano. Los botones se activan por contacto: no hace falta pulsar grip, gatillo ni otros botones del mando.
3. **Preparación**: coloca ambas manos superpuestas sobre el marcador azul del tórax (mitad inferior del esternón) y mantenlas 1,5 s.
4. **Sesión activa** (60 s por defecto): baja y sube las manos. Cada ciclo de descenso y retorno cuenta una compresión. La pantalla muestra tiempo, compresiones, ritmo estimado e indicaciones.
5. Al terminar el tiempo, o al tocar **FINALIZAR**, aparece el resumen. **REINICIAR** comienza un nuevo intento.
6. **METRÓNOMO** activa o desactiva un pulso a 110/min; **−** y **+** ajustan su volumen, independiente del ambiente.

El desplazamiento y el giro del jugador siguen funcionando en todo momento; mientras el jugador se desplaza o gira, no se cuentan compresiones.

## Componentes (`Scripts`)

| Script | Responsabilidad |
| --- | --- |
| `RcpChestReference` | Punto de referencia del tórax (hijo de `Paciente_01`), con el tamaño de la zona válida ajustable. |
| `RcpCompressionDetector` | Lee la posición de los mandos ya seguidos, la expresa respecto al tórax, filtra y detecta ciclos. |
| `RcpRateEstimator` | Ritmo con las últimas compresiones (búfer circular, sin asignaciones por frame). |
| `RcpSession` | Estados Inactiva → Preparación → Activa → Finalizada y métricas del resumen. |
| `RcpPanelView` | Pantalla, marcador del tórax, botones visibles y sonidos de inicio y fin. |
| `RcpTouchButton` | Botón físico activado por contacto de la mano. |
| `RcpMetronome` | Metrónomo con fuente de audio propia, programado con el reloj de audio. |

`Prefabs/EstacionRCP.prefab` contiene la lógica, la pantalla, la consola, la señal y la marca del suelo. Las referencias a los mandos, al XR Origin y al tórax se asignan en la escena.

### Detección

- Altura de las manos en el marco del tórax, con filtro paso bajo (10 Hz) y validación de seguimiento y de saltos de posición.
- Ambas manos dentro de la zona del tórax y a menos de 20 cm entre sí.
- Ciclo con histéresis: descenso de 3,5 cm desde el punto alto para iniciar y subida de 2,5 cm para cerrar; intervalo mínimo de 0,27 s entre conteos; un apoyo de más de 1,5 s no cuenta.
- La referencia alta sigue cambios lentos de postura (2 cm/s), por lo que una deriva lenta no cuenta.
- Retorno «completo» si el mando vuelve a menos de 1,2 cm del punto alto anterior.
- Detección suspendida mientras el XR Origin se mueve (> 0,15 m/s) o gira (> 20 °/s), y 0,5 s después.

Todos estos valores se ajustan en el Inspector de `EstacionRCP_01/Logica`.

## Criterios educativos y limitaciones

Objetivos utilizados para las indicaciones: ritmo de 100 a 120 compresiones por minuto, retorno completo del tórax tras cada compresión y mínimas interrupciones. Proceden de:

- European Resuscitation Council Guidelines 2025: Adult Basic Life Support. *Resuscitation* 2025;215 Suppl 1:110771. https://doi.org/10.1016/j.resuscitation.2025.110771 (texto consultado en https://www.resus.org.uk/professional-library/2025-resuscitation-guidelines/adult-basic-life-support-guidelines): «Compress the chest at a rate of 100–120 min-1 with as few interruptions as possible», «Compress to a depth of at least 5 cm, but not more than 6 cm», «Allow the chest to recoil completely after each compression; avoid leaning on the chest».
- 2025 American Heart Association Guidelines for CPR and ECC, Part 7: Adult Basic Life Support. *Circulation*. https://doi.org/10.1161/CIR.0000000000001369 (mismos objetivos de ritmo, profundidad y retorno).

Limitaciones de esta simulación:

- No hay maniquí ni resistencia física. El recorrido del mando **no es** la profundidad torácica ni la fuerza aplicada, por lo que no se evalúa la profundidad de 5–6 cm.
- El «retorno» se refiere a que el mando vuelve a su altura previa, no al retroceso real del tórax.
- No se evalúan posición exacta de las manos, brazos rectos, ventilaciones ni desfibrilación.
- Es una herramienta de práctica educativa; no está validada clínicamente ni sustituye un curso acreditado de soporte vital.

## Escenario

- Accesos de las cuatro habitaciones ensanchados (hueco libre 1,30 m) para que el jugador pueda salir al pasillo; hoja de puerta abatida contra la pared.
- Colisionador de la camilla de práctica recortado 28 cm en el lado del reanimador para poder situarse junto a ella.
- Materiales propios de la planta (`Materials/Planta_*`) sin metalicidad ni brillo excesivo, techo mate; los materiales originales del paquete Hospital no se modifican.
- Una sola luz direccional sin sombras como relleno; las luces puntuales de cada sala se mantienen.
- Señalización de habitaciones en el pasillo y señal de la camilla de práctica.

Pendiente con Oculus Rift: comprobar alcance real de los brazos con la altura de la camilla, desplazamiento del punto de agarre del mando (`contactOffset`), volumen del metrónomo con auriculares y rendimiento en el visor.
