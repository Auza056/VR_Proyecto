# Simulador hospitalario VR

Abrir con Unity 6000.5.11f1 y cargar `Assets/Scenes/HospitalRCP.unity`. El proyecto utiliza URP y XR Interaction Toolkit 3.5.1. Esta presentación permite visualizar el hospital y las manos; todavía no incluye mecánica de RCP.

## Probar con XR Device Simulator

Pulsar Play y dar foco a Game. El visor y ambas manos funcionan sin mantener grip ni trigger. Cada mano es hija del mando que ya utiliza TrackedPoseDriver.

- U: seleccionar cuerpo/visor; botón derecho y ratón: mirar; WASD/Q/E: desplazarse en el simulador.
- T / Y: seleccionar el mando izquierdo / derecho; también Shift izquierdo / Espacio mientras se mantienen pulsados.
- 3: activar el modo Position para trasladar los mandos con WASD/Q/E.
- R: alternar rotación/traslación con ratón, según el panel del simulador.
- G: grip del mando seleccionado; botón izquierdo del ratón: trigger.
- Grip flexiona pulgar, medio, anular y meñique. Trigger flexiona el índice de forma independiente. Soltar ambos devuelve la pose abierta.

Las manos conservan escala humana (unos 18.4 cm entre muñeca y punta del dedo medio), con un ajuste local de muñeca de (0, -0.03, -0.025) metros. Los valores analógicos se leen de `<XRController>{LeftHand/RightHand}/grip` y `/trigger`, compatibles con el simulador y controles Touch. No dependen de seguimiento óptico ni de XR Hands. La orientación y comodidad deben validarse también con un Rift físico.

## Adaptaciones propias

`Assets/VR/Hands` contiene las dos mallas separadas, prefabs y material URP. `ControllerHandAnimator` interpola poses de los huesos existentes. `HospitalHandsSetup`, accesible en **Hospital VR / Integrar manos en el jugador existente**, regenera los prefabs propios sin sustituir el jugador ni duplicar sus manos. Requiere el prefab fuente en la ruta original de PolyOne; no modifica los recursos importados. El clip Idle original se inspeccionó, pero no se utiliza porque anima las dos manos juntas en espacio de presentación.

El sistema de poses del XRI Starter Kit se inspeccionó, pero sus poses de robot no corresponden al esqueleto PolyOne. No se utiliza ni se publica su escena de demostración. Su importación local presentó cuatro errores de compilación en Keychain.cs por GetInstanceID retirado; se corrigió la copia local y se proporciona el cambio mínimo en `docs/compatibilidad-starter-kit.patch`. La integración de manos no requiere ese paquete.

## Recursos y atribuciones

- **Free Pack – VR Hands**, PolyOne Studio, versión 1.0: https://assetstore.unity.com/packages/3d/props/free-pack-vr-hands-402280. Su licencia es la Standard Unity Asset Store EULA: https://unity.com/legal/as-terms. Las mallas adaptadas proceden de este recurso y conservan esa licencia, que no se reemplaza por una licencia del código propio.
- **XR Interaction ToolKit Starter Kit**, MikeNspired: inspeccionado localmente; sus demos y sistema de poses no forman parte de la integración.
- **Hospital**: modelo y texturas importados previamente por el autor del proyecto, quien indica procedencia de Asset Store. No se aportó el nombre/editor del recurso; debe conservarse la licencia y atribución de su proveedor. No se atribuye autoría propia sobre estos modelos.
- Los ejemplos Unity que ya existían conservan sus atribuciones originales.

La licencia estándar permite incorporar assets a un producto, pero no concede permiso general para redistribuirlos como fuentes públicas. El propietario solicitó publicar los recursos necesarios en el remoto actual y gestionar posteriormente su privacidad. La visibilidad del repositorio no se cambia automáticamente.

## Verificación

Se probaron ambas manos en Play con entradas de teclado/ratón al Input System y XR Device Simulator: gestos independientes, retorno a abierto, traslación, rotación y seguimiento sin grip/trigger. El arnés selecciona explícitamente el dispositivo del simulador para aislar cada prueba; no escribe los transforms de las manos. Las mediciones se guardan en `Evidence/VRHands`. Los ajustes temporales de foco y segundo plano se restauraron fuera de Play.

No se modificaron distribución, materiales ni iluminación del hospital durante la integración. Se conservan un solo jugador, cámara, AudioListener y simulador. La validación física de Rift/Touch y el rendimiento en hardware siguen pendientes. El Oculus XR Plugin importado por el usuario muestra una advertencia de deprecación en esta versión del Editor; no se actualizó ni sustituyó el proveedor XR.
