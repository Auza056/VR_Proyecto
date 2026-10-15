# HospitalRCP

Abrir `Assets/Scenes/HospitalRCP.unity` y pulsar Play. La escena original `Assets/Scene.unity` conserva el estado abierto del usuario, incluido lo que estaba sin guardar. `Scene-unsaved-backup.unity` contiene además una copia de ese estado anterior al montaje.

## Prueba con XR Device Simulator

Dar foco a Game. Usar botón derecho del ratón y movimiento del ratón para mirar hacia la camilla. Para mover un mando, pulsar `3` (Position), mantener Shift izquierdo para el izquierdo o Espacio para el derecho y usar WASD / Q / E. T e Y alternan la selección de los mandos; U devuelve el control del cuerpo/visor. Los controles completos aparecen en el panel del simulador. No hay lógica de RCP ni conteo de compresiones.

## Reparación

El pipeline activo es URP (`PC_RPAsset`); los 30 materiales integrados del Collada usaban Standard, sin texturas resueltas. Se crearon materiales URP/Lit externos y 30 remapeos en `Assets/Hospital/HospitalScene.dae.meta`.

El Collada referencia nombres antiguos ausentes, enumerados exactamente en `collada-texture-references.json` (por ejemplo Bed -> Image_0.png). Se inspeccionaron las imágenes JPG de cada material y su aspecto aplicado sobre las UV: cama, monitor, muebles y estructura recuperan sus atlas correspondientes. No se usaron colores planos para sustituir texturas. Wall4 tiene imágenes, pero no corresponde a ningún material o pieza del modelo y se conservó sin asignar.

Los JPG originales quedan intactos. Los mapas de datos son grises separados de 2048 x 2048, no mapas ORM ya empaquetados. En `Materials/Generated`, cada MetallicAOGloss contiene R=metallic, G=AO, B=0, A=255-roughness. Se importa como dato lineal y alimenta MetallicGlossMap y OcclusionMap; Smoothness=1. Los cuatro mapas de opacidad se integran como alpha del albedo en BaseOpacity, con transparencia URP. También se conservan las dos emisiones y se importan las diez normales usadas como NormalMap. Se modificaron únicamente sus archivos `.meta`, no los JPG. El piso se repite por módulos de malla porque su textura es un atlas.

## Escenario

Cuarto de 6 x 5.5 x 2.8 metros. Camilla de 2.2 m de largo; área libre lateral de 1.4 x 2 m. Piso, paredes, ventana y acceso reutilizan las mallas importadas. El techo es la única geometría primitiva añadida. Mobiliario y equipo se agrupan por función. Hay 28 colliders, sin Rigidbody estáticos; el acceso conserva el hueco con MeshCollider y puerta abierta. Una luz direccional y dos luces de relleno sin sombras adicionales; geometría marcada para static batching. Aproximadamente 24 717 triángulos hospitalarios, sin contar XR. No se ha medido rendimiento en Oculus Rift.

No hay paciente ni maniquí entre los assets del hospital. Se dejó `PatientAnchor` sobre el colchón, cuya altura se comprobó por raycast sobre la malla. Falta aportar un modelo adecuado de maniquí de RCP o paciente.

XR Origin comienza al costado oeste, orientado hacia la cama. Se conservaron mandos, TrackedPoseDriver, acciones y simulador. Se conectaron las referencias de cámara, objeto origen y Camera Offset que estaban vacías en XROrigin, con altura de visor de 1.65 m. En esta escena se desactivaron los dos objetos LeftHandQuestVisual/RightHandQuestVisual: sus visuales y affordances carecían de interactores y producían cuatro advertencias. Los objetos siguen en la jerarquía y no se modificaron los prefabs ni assets XR originales.

## Verificación y límites

Inspección visual por MCP en Scene y Game, incluidos arranques en Play. Última ejecución: cero errores y cero advertencias de consola. Prueba automatizada con entradas de teclado al Input System y el XR Device Simulator: ambos mandos cambiaron su posición seguida; ver `xr-final-test.json`. No se escribieron directamente sus transforms para pasar la prueba. Los ajustes temporales de foco y ejecución en segundo plano usados para automatizar se restauraron. Esto no constituye una prueba con hardware Rift.

`scene-validation.json`: todos los materiales hospitalarios tienen URP/Lit compatible y albedo, 30 remapeos, ningún solapamiento de AABB entre conjuntos independientes de mobiliario/equipo, área de práctica libre y raycast de acceso despejado. Los contactos internos de ensamblajes (puertas, vidrio, accesorios) se mantienen. Se comprobaron las escalas mundiales unitarias de las cajas de colisión, incluso bajo paredes escaladas y rotadas. `preservation-audit.json` comprueba los archivos existentes; `xr-action-reference-audit.json` compara las referencias de acciones de las jerarquías originales y nuevas cargadas en Unity: 78 referencias en cada una, ninguna cambiada ni perdida.

La consola inicial contenía excepciones del Inspector de texturas `Could not find the requested Platform Texture Settings ('WebGL')`. Su stack procede de TextureImporterInspector; no es la causa del magenta. Dejaron de aparecer al cambiar la selección y no se reprodujeron en la última prueba; si regresan al abrir ese Inspector, requieren revisar el Editor por separado.

El registro anterior `Logs/Editor.log` identifica la advertencia de paquete inmutable en `Packages/com.unity.render-pipelines.core/Runtime/Debugging/Runtime UI Resources/RuntimeDebugWindow_PanelSettings.asset`. El registro no permite atribuir quién lo alteró. Se dejó intacto: recuperar ese recurso desde una copia limpia de la misma versión de URP Core sería una reparación separada. No se modificó PackageCache, ni se actualizó ningún paquete o cambió el pipeline.

Los cambios previos de Git permanecen. No se hizo commit. Las capturas `HospitalRCP-room.png`, `HospitalRCP-scene-1.png` y `HospitalRCP-game-start.png` documentan el resultado.
