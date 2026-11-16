# Presentación final — revisión y checklist

Escena: `Assets/Scenes/HospitalWard.unity` (única en el build). Revisión del 15 de noviembre de 2026.

## Qué se corrigió en esta revisión

| Problema detectado | Cambio |
|---|---|
| El jugador quedaba ~1.65 m más alto con el Rift y había que agacharse para la RCP, hasta perder los Touch | `PlayerHeightCalibrator` solicita espacio de suelo y quita el desplazamiento fijo del `Camera Offset`. Ajuste manual ±5 cm y ALTURA REAL |
| Cortes de compresión al superponer las manos frente a los sensores | El detector tolera pérdidas de seguimiento de hasta 0.2 s (`trackingGraceTime`) |
| Cada participante aparecía donde estuviera en su sala real | Recentrado automático al detectar el visor, botón CENTRAR VISTA y recentrado al confirmar REINICIAR RECORRIDO |
| Demasiadas interfaces | Portada + un único menú con pestañas. Se eliminaron el letrero de pulso (ahora en la pantalla RCP) y los carteles flotantes del pasillo |
| No había menú principal | Portada: COMENZAR RECORRIDO, PRÁCTICA LIBRE, CÓMO SE JUEGA, altura, CENTRAR VISTA y SALIR |
| Aspecto plano y poco legible | Paneles redondeados opacos con borde, tipografía Inter en todos los paneles, botones con estados de color |
| Bordes y texto dentados en el visor | MSAA 4x. HDR desactivado (no hay postprocesado) para ganar rendimiento |
| Posible parpadeo entre capas de los paneles | Plano cercano de la cámara de 0.01 a 0.05 m y lejano de 1000 a 120 m |
| Botones sin respuesta táctil | Vibración breve en el Touch que pulsa (`RcpTouchButton.hapticAmplitude`) |
| Sin visor no se podía usar | `DesktopSimulatorGate` estaba desactivado. Ahora activa el simulador de escritorio solo si a los 3 s no hay visor |

Verificado en el Editor: Edit Mode **21/21**, Play Mode **16/16**, consola sin errores ni advertencias, 0 scripts faltantes, 0 botones sin manos asignadas. Arranque comprobado en Play: portada visible, simulador activado sin visor y recentrado correcto.

## Antes del día de la presentación (pendiente, requiere el Rift)

1. **Build listo.** Windows 64, solo HospitalWard, generado el 15/11/2026 con todos estos cambios. Descárgalo de [Releases](https://github.com/Auza056/VR_Proyecto/releases) (`HospitalVR_RCP_Windows.zip`), descomprime toda la carpeta y abre `VR_Proyecto.exe` con el software del Rift abierto y OpenXR activo.
2. **Probar con Rift y Touch físicos:**
   - Al ponerte el visor apareces frente al menú, a tu altura real.
   - Llegas al tórax de la camilla **de pie**, sin agacharte.
   - Las compresiones se cuentan sin cortes. El indicador de seguimiento perdido debe quedar en ~0 s.
   - El texto de la portada y de los resultados se lee bien.
   - La vibración al tocar botones resulta cómoda.
3. **Sensores del Rift:** colócalos a la altura de la cabeza o por encima, apuntando hacia la zona de la camilla. Si un sensor queda bajo, las manos lo tapan durante las compresiones.
4. Si la camilla queda muy alta o muy baja para alguien, usa SUBIR/BAJAR en la portada.

## Guion sugerido (5–7 min)

1. Portada: explicar el objetivo y abrir CÓMO SE JUEGA.
2. AJUSTES → DEMO · 30 s.
3. COMENZAR RECORRIDO → manual → INICIAR CASO → reconocer en la recepción → teléfono.
4. Práctica guiada de 30 s en la camilla, con el pulso visible en la esquina de la pantalla RCP.
5. INICIAR RETO (sin ayudas) → RESULTADOS para comparar.
6. REINICIAR RECORRIDO (tocar dos veces) para el siguiente participante: vuelve a la portada, recentrado y con la altura restablecida.

## Plan B

- **Si el Rift falla:** abre el ejecutable o el Editor sin visor. A los 3 s se activa el simulador de escritorio (controles en el README).
- **Si alguien queda desorientado:** CENTRAR VISTA en la portada o en Ajustes.

## Mejoras opcionales (no necesarias para presentar)

- Narración o audio guía en cada etapa.
- Fundido al iniciar y al reiniciar.
- Pruebas automáticas específicas para la calibración de altura y el recentrado (hoy se verifican a mano en Play).
- Unificar también la fuente de los letreros del pasillo y de los nombres de pacientes (siguen en Liberation Sans).
