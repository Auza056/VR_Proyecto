# Planta de hospitalización

Abrir `Assets/Scenes/HospitalWard.unity` y pulsar Play. Se usan los controles de locomoción y el XR Device Simulator que ya tenía el proyecto. La escena original `HospitalRCP.unity` se conserva.

La planta contiene cuatro habitaciones con camilla, mobiliario y un paciente Remy por habitación, conectadas por un pasillo con colisiones. El jugador comienza en la primera habitación, donde está la estación de práctica de RCP (ver `Assets/VR/RCP/LEEME.md`).

## Recursos

- `Assets/Models/Remy.fbx`: modelo original, sin modificar su geometría ni sus huesos.
- `Assets/Models/RemyTextures`: texturas extraídas del FBX.
- `Assets/Models/RemyMaterials`: materiales externos URP/Lit. Cabello y pestañas usan recorte alfa.
- `PacienteRemyAcostado.prefab`: adaptación de escala y pose acostada con los brazos a los lados. Es una pose estática; el modelo no incluye animaciones adecuadas.
- `Assets/Sounds/Sonidos de acciones/fadingembersaudio-heart-monitor-hospital-ambience-430219.mp3`: ambiente global en bucle, volumen 0.08.
- `Assets/Sounds/Sonidos de acciones/mixkit-sick-man-coughing-2221.wav`: tos espacial en el segundo paciente, volumen 0.28, intervalos aleatorios de 18 a 40 segundos (`PatientCough.cs`).

Pendiente: ajuste de volumen con auriculares y medición de rendimiento con Oculus Rift físico. Los cuatro Remy suman aproximadamente 140 000 triángulos.
