using System.Collections.Generic;
using System.IO;
using System.Linq;
using HospitalVR.Interaction;
using HospitalVR.Learning;
using HospitalVR.Presentation;
using HospitalVR.RCP;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using MenuCommand = HospitalVR.Presentation.MenuCommand;

namespace HospitalVR.Editor
{
    /// <summary>
    /// Rediseño visual: portada (menú principal), menú único con pestañas, paneles redondeados,
    /// calibración de altura y eliminación de letreros redundantes. Ejecutar sobre HospitalWard fuera de Play.
    /// </summary>
    public static class MenuRedesignSetup
    {
        const string UiFolder = "Assets/VR/Presentation/UI";
        const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Inter-Regular SDF.asset";

        // Paleta
        public static readonly Color Bg = new Color(.043f, .078f, .110f, .97f);
        public static readonly Color Surface = new Color(.075f, .135f, .185f, 1f);
        public static readonly Color Border = new Color(.18f, .77f, .71f, .55f);
        public static readonly Color Accent = new Color(.18f, .77f, .71f);
        public static readonly Color Primary = new Color(.12f, .56f, .52f);
        public static readonly Color Ready = new Color(.11f, .36f, .42f);
        public static readonly Color Hover = new Color(.22f, .66f, .70f);
        public static readonly Color Pressed = new Color(.96f, .70f, .30f);
        public static readonly Color Muted = new Color(.09f, .14f, .19f);
        public static readonly Color TextMuted = new Color(.62f, .70f, .78f);

        static Sprite panelSprite, buttonSprite, circleSprite, shadowSprite;
        static Material spriteMat, solidMat;
        static TMP_FontAsset font;
        static Transform[] hands;
        static TrainingMenu menu;
        static List<TrainingMenuButton> commands;
        static AudioSource clickSource;
        static AudioClip clickClip;

        [MenuItem("Hospital VR/Rediseñar menús y altura")]
        public static void ApplyFromMenu() => Debug.Log(Apply());

        public static string Apply()
        {
            RcpFinalSetup.Guard();
            LoadAssets();
            var flow = Object.FindAnyObjectByType<TrainingFlow>();
            hands = new[] { flow.session.detector.leftHand, flow.session.detector.rightHand };
            clickSource = flow.rcpPanel.feedbackSource;
            clickClip = Object.FindObjectsByType<RcpTouchButton>(FindObjectsInactive.Include).Select(b => b.pressClip).FirstOrDefault(c => c);

            var calibrator = SetupHeight(flow);
            BuildMenu(flow, calibrator);
            StationPulse(flow);
            Declutter(flow);
            SkinPanels(flow);
            SkinButtons(flow);
            UnifyFonts(flow);
            VrQuality();

            var rig = flow.session.detector.rigRoot;
            rig.SetPositionAndRotation(new Vector3(-1.02f, 0, -1.22f), Quaternion.Euler(0, 90, 0));
            EditorUtility.SetDirty(rig);
            flow.Refresh(); menu.RefreshNow();
            EditorUtility.SetDirty(flow); EditorUtility.SetDirty(menu);
            PrefabUtility.SaveAsPrefabAsset(menu.gameObject, "Assets/VR/Presentation/MenuAprendizaje.prefab");
            EditorSceneManager.MarkSceneDirty(flow.gameObject.scene);
            EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
            return "Portada, menú unificado, calibración de altura y estilo redondeado aplicados.";
        }

        // ---------- Recursos ----------
        static void LoadAssets()
        {
            if (!AssetDatabase.IsValidFolder(UiFolder)) AssetDatabase.CreateFolder("Assets/VR/Presentation", "UI");
            panelSprite = MakeSprite("PanelRedondeado", 128, 40, 0, 1000f, 44);
            buttonSprite = MakeSprite("BotonRedondeado", 64, 22, 0, 1100f, 24);
            circleSprite = MakeSprite("Circulo", 64, 32, 0, 64f, 0);
            shadowSprite = MakeSprite("Sombra", 128, 30, 30, 1000f, 62);
            string matPath = UiFolder + "/SpriteUI.mat";
            spriteMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!spriteMat)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                spriteMat = new Material(shader); AssetDatabase.CreateAsset(spriteMat, matPath);
            }
            string solidPath = UiFolder + "/PanelSolido.mat";
            solidMat = AssetDatabase.LoadAssetAtPath<Material>(solidPath);
            if (!solidMat) { solidMat = new Material(Shader.Find("HospitalVR/Panel UI")); AssetDatabase.CreateAsset(solidMat, solidPath); }
            solidMat.enableInstancing = true;
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        }

        static Sprite MakeSprite(string name, int size, int radius, int feather, float ppu, int border)
        {
            string path = UiFolder + "/" + name + ".png";
            if (!File.Exists(path))
            {
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                float half = size * .5f;
                float inset = feather;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(Mathf.Abs(x + .5f - half), Mathf.Abs(y + .5f - half));
                    Vector2 b = new Vector2(half - inset - radius, half - inset - radius);
                    Vector2 q = p - b;
                    float d = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - radius;
                    float a = feather > 0 ? .55f * Mathf.Pow(1f - Mathf.Clamp01(d / feather), 2f) * (d <= 0 ? 1f : 1f) : Mathf.Clamp01(.5f - d);
                    if (feather > 0 && d <= 0) a = .55f;
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
            }
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = ppu;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = true;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            var s = new TextureImporterSettings(); imp.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect; s.spriteBorder = new Vector4(border, border, border, border);
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ---------- Primitivas visuales ----------
        public static SpriteRenderer Plate(string name, Transform parent, Vector3 pos, Vector2 size, Color color, Sprite sprite = null, int order = -2)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite ? sprite : panelSprite;
            // Opaco con profundidad salvo sombras y velos translúcidos.
            sr.sharedMaterial = sr.sprite == shadowSprite || color.a < .6f ? spriteMat : solidMat; sr.color = color; sr.sortingOrder = order;
            if (sr.sprite != circleSprite) { sr.drawMode = SpriteDrawMode.Sliced; sr.size = size; }
            else go.transform.localScale = new Vector3(size.x, size.y, 1);
            sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; sr.receiveShadows = false;
            return sr;
        }

        public static TextMeshPro Label(Transform parent, string name, Vector3 pos, string value, float size, Vector2 bounds, Color? color = null, TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            t.font = font; t.text = value; t.fontSize = size; t.color = color ?? Color.white; t.alignment = align;
            t.rectTransform.sizeDelta = bounds; t.textWrappingMode = TextWrappingModes.Normal;
            t.enableAutoSizing = true; t.fontSizeMin = size * .75f; t.fontSizeMax = size;
            t.overflowMode = TextOverflowModes.Ellipsis; t.richText = true;
            if (bold) t.fontStyle = FontStyles.Bold;
            t.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return t;
        }

        static void Palette(RcpTouchButton b)
        {
            b.normalColor = Ready; b.hoverColor = Hover; b.pressedColor = Pressed; b.disabledColor = Muted;
        }

        static RcpTouchButton TouchButton(Transform parent, string caption, Vector3 pos, Vector2 size, float textSize)
        {
            var root = new GameObject("Tocar_" + caption).transform; root.SetParent(parent, false); root.localPosition = pos;
            var visual = Plate("Visual", root, new Vector3(0, 0, -.012f), size, Ready, buttonSprite, -1);
            var label = Label(visual.transform, "Texto", new Vector3(0, 0, -.004f), caption, textSize, size - new Vector2(.03f, .012f), bold: true);
            var b = root.gameObject.AddComponent<RcpTouchButton>();
            b.hands = hands; b.triggerCenter = new Vector3(0, 0, -.035f); b.triggerSize = new Vector3(size.x, size.y, .08f);
            b.pressVisual = visual.transform; b.pressDepth = .006f; b.pressDirection = Vector3.forward;
            b.feedbackRenderer = visual; b.feedbackSource = clickSource; b.pressClip = clickClip;
            Palette(b);
            label.name = "Texto";
            return b;
        }

        static TrainingMenuButton Cmd(Transform parent, MenuCommand command, string caption, Vector3 pos, Vector2 size, float textSize = .30f, string objectName = null)
        {
            var b = TouchButton(parent, caption, pos, size, textSize);
            if (objectName != null) b.name = objectName;
            var link = b.gameObject.AddComponent<TrainingMenuButton>();
            link.menu = menu; link.command = command; link.touch = b; link.caption = caption; link.label = b.GetComponentInChildren<TMP_Text>();
            UnityEventTools.AddPersistentListener(b.onPressed, link.Execute);
            commands.Add(link); return link;
        }

        static Transform Group(Transform parent, string name)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); return t;
        }

        // ---------- Altura ----------
        static PlayerHeightCalibrator SetupHeight(TrainingFlow flow)
        {
            var origin = Object.FindAnyObjectByType<XROrigin>();
            var cal = origin.GetComponent<PlayerHeightCalibrator>();
            if (!cal) cal = origin.gameObject.AddComponent<PlayerHeightCalibrator>();
            cal.origin = origin;
            var gate = Object.FindAnyObjectByType<DesktopSimulatorGate>(FindObjectsInactive.Include);
            if (gate) { gate.enabled = true; gate.autoEnableWithoutHeadset = true; EditorUtility.SetDirty(gate); }
            cal.desktopSimulator = gate && gate.simulator ? gate.simulator
                : origin.gameObject.scene.GetRootGameObjects().FirstOrDefault(g => g.name == "XR Device Simulator");
            EditorUtility.SetDirty(cal);
            return cal;
        }

        // ---------- Menú ----------
        static void BuildMenu(TrainingFlow flow, PlayerHeightCalibrator calibrator)
        {
            var p = flow.transform.Find("Recorrido");
            foreach (var child in p.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
            foreach (var r in p.GetComponents<Renderer>()) r.enabled = false;
            menu = p.GetComponent<TrainingMenu>(); if (!menu) menu = p.gameObject.AddComponent<TrainingMenu>();
            menu.flow = flow; menu.height = calibrator; menu.startOnMainPage = true;
            menu.readyColor = Ready; menu.mutedColor = Muted; menu.currentColor = Accent; menu.primaryColor = Primary;
            commands = new List<TrainingMenuButton>();
            p.position = new Vector3(-.43f, 1.40f, -1.22f); p.rotation = Quaternion.Euler(0, 90, 0);

            const float W = 1.30f, H = 1.22f;
            Plate("Sombra", p, new Vector3(0, -.01f, .01f), new Vector2(W + .16f, H + .16f), new Color(0, 0, 0, .8f), shadowSprite, -4);
            Plate("Borde", p, new Vector3(0, 0, .003f), new Vector2(W + .012f, H + .012f), Border, panelSprite, -3);
            Plate("Fondo", p, Vector3.zero, new Vector2(W, H), Bg, panelSprite, -3);
            Plate("Cabecera", p, new Vector3(0, .555f, -.002f), new Vector2(W - .03f, .085f), Surface, buttonSprite, -2);
            Label(p, "Marca", new Vector3(-.30f, .555f, -.006f), "HOSPITAL VR · RCP", .30f, new Vector2(.62f, .06f), TextMuted, TextAlignmentOptions.Left, true);
            menu.subtitle = Label(p, "Seccion", new Vector3(.33f, .555f, -.006f), "", .28f, new Vector2(.55f, .06f), Accent, TextAlignmentOptions.Right, true);
            Label(p, "Alcance", new Vector3(0, -.585f, -.006f), "Simulación educativa · No mide fuerza, profundidad real ni competencia clínica.", .19f, new Vector2(1.2f, .035f), TextMuted);

            // Portada
            var main = Group(p, "Portada"); menu.mainPage = main.gameObject;
            var icon = Group(main, "Icono"); icon.localPosition = new Vector3(0, .38f, -.004f);
            Plate("Aro", icon, Vector3.zero, new Vector2(.16f, .16f), new Color(.18f, .77f, .71f, .18f), circleSprite, -2);
            Plate("CruzV", icon, new Vector3(0, 0, -.001f), new Vector2(.035f, .10f), Accent, buttonSprite, -1);
            Plate("CruzH", icon, new Vector3(0, 0, -.001f), new Vector2(.10f, .035f), Accent, buttonSprite, -1);
            Label(main, "Titulo", new Vector3(0, .245f, -.006f), "APRENDE A ACTUAR", .95f, new Vector2(1.2f, .12f), Color.white, bold: true);
            Label(main, "Lema", new Vector3(0, .16f, -.006f), "Reconoce · Pide ayuda · Practica el ritmo de la RCP", .32f, new Vector2(1.15f, .06f), TextMuted);
            Cmd(main, MenuCommand.BeginTour, "COMENZAR RECORRIDO", new Vector3(0, .045f, 0), new Vector2(.66f, .105f), .40f);
            Cmd(main, MenuCommand.Practice, "PRÁCTICA LIBRE", new Vector3(-.19f, -.085f, 0), new Vector2(.36f, .09f), .32f, "Tocar_PRÁCTICA LIBRE");
            Cmd(main, MenuCommand.HowTo, "CÓMO SE JUEGA", new Vector3(.19f, -.085f, 0), new Vector2(.36f, .09f), .32f, "Tocar_CÓMO SE JUEGA (portada)");
            Plate("Separador", main, new Vector3(0, -.175f, -.002f), new Vector2(.9f, .004f), new Color(1, 1, 1, .08f), buttonSprite, -2);
            Label(main, "AlturaTitulo", new Vector3(0, -.215f, -.006f), "ALTURA DEL JUGADOR", .24f, new Vector2(.8f, .04f), TextMuted, bold: true);
            menu.heightText = Label(main, "AlturaEstado", new Vector3(0, -.255f, -.006f), "", .26f, new Vector2(.9f, .04f), Color.white);
            Cmd(main, MenuCommand.HeightDown, "BAJAR", new Vector3(-.30f, -.32f, 0), new Vector2(.26f, .075f), .28f, "Tocar_BAJAR (portada)");
            Cmd(main, MenuCommand.HeightReset, "ALTURA REAL", new Vector3(0, -.32f, 0), new Vector2(.26f, .075f), .28f, "Tocar_ALTURA REAL (portada)");
            Cmd(main, MenuCommand.HeightUp, "SUBIR", new Vector3(.30f, -.32f, 0), new Vector2(.26f, .075f), .28f, "Tocar_SUBIR (portada)");
            Label(main, "AlturaAyuda", new Vector3(0, -.39f, -.006f), "Juega de pie. Si la camilla queda muy baja o muy alta, ajusta aquí.", .22f, new Vector2(1.1f, .04f), TextMuted);
            Cmd(main, MenuCommand.Recenter, "CENTRAR VISTA", new Vector3(-.15f, -.485f, 0), new Vector2(.28f, .07f), .26f, "Tocar_CENTRAR VISTA (portada)");
            Cmd(main, MenuCommand.Quit, "SALIR", new Vector3(.15f, -.485f, 0), new Vector2(.28f, .07f), .26f);

            // Marco común (pestañas, progreso, pie)
            var chrome = Group(p, "Marco"); menu.chrome = chrome.gameObject;
            Cmd(chrome, MenuCommand.MainMenu, "MENÚ", new Vector3(-.4725f, .445f, 0), new Vector2(.29f, .075f), .28f);
            Cmd(chrome, MenuCommand.Home, "RECORRIDO", new Vector3(-.1575f, .445f, 0), new Vector2(.29f, .075f), .28f, "Tocar_INICIO");
            Cmd(chrome, MenuCommand.Settings, "AJUSTES", new Vector3(.1575f, .445f, 0), new Vector2(.29f, .075f), .28f);
            Cmd(chrome, MenuCommand.Results, "RESULTADOS", new Vector3(.4725f, .445f, 0), new Vector2(.29f, .075f), .28f);
            var progress = Group(chrome, "Progreso"); progress.localPosition = new Vector3(0, .345f, -.004f);
            Plate("Linea", progress, new Vector3(0, .018f, .001f), new Vector2(1.0f, .006f), new Color(1, 1, 1, .10f), buttonSprite, -2);
            menu.steps = new TMP_Text[6];
            for (int i = 0; i < 6; i++)
            {
                float x = -.50f + i * .20f;
                Plate("Punto" + i, progress, new Vector3(x, .018f, 0), new Vector2(.03f, .03f), Muted, circleSprite, -1);
                menu.steps[i] = Label(progress, "Paso" + i, new Vector3(x, -.03f, -.002f), "", .21f, new Vector2(.19f, .06f));
            }
            menu.messageText = Label(chrome, "Mensaje", new Vector3(0, -.415f, -.006f), "", .23f, new Vector2(1.18f, .06f), new Color(.96f, .82f, .55f));
            Cmd(chrome, MenuCommand.RecoverManual, "RECUPERAR MANUAL", new Vector3(-.30f, -.505f, 0), new Vector2(.54f, .075f), .27f);
            Cmd(chrome, MenuCommand.Reset, "REINICIAR RECORRIDO", new Vector3(.30f, -.505f, 0), new Vector2(.54f, .075f), .27f);

            // Recorrido
            var home = Group(p, "Inicio"); menu.homePage = home.gameObject;
            Plate("Tarjeta", home, new Vector3(0, .17f, -.002f), new Vector2(1.18f, .14f), Surface, buttonSprite, -2);
            flow.flowText = Label(home, "PasoActual", new Vector3(0, .17f, -.006f), "", .32f, new Vector2(1.12f, .12f));
            Cmd(home, MenuCommand.StartCase, "INICIAR CASO", new Vector3(-.295f, .02f, 0), new Vector2(.56f, .10f), .34f);
            Cmd(home, MenuCommand.Practice, "PRÁCTICA DIRECTA", new Vector3(.295f, .02f, 0), new Vector2(.56f, .10f), .34f);
            Cmd(home, MenuCommand.Challenge, "INICIAR RETO", new Vector3(-.295f, -.11f, 0), new Vector2(.56f, .10f), .34f);
            Cmd(home, MenuCommand.Finish, "FINALIZAR INTENTO", new Vector3(.295f, -.11f, 0), new Vector2(.56f, .10f), .34f);
            Label(home, "AyudaInicio", new Vector3(0, -.255f, -.006f),
                "<b>Caso:</b> recepción del pasillo.   <b>Práctica:</b> camilla de esta habitación.\nEl reto se habilita al terminar la práctica y se hace sin ayudas.",
                .25f, new Vector2(1.15f, .10f), TextMuted);

            // Ajustes
            var settings = Group(p, "Ajustes"); menu.settingsPage = settings.gameObject;
            menu.settingsText = Label(settings, "EstadoAjustes", new Vector3(0, .212f, -.006f), "", .28f, new Vector2(1.18f, .075f));
            Cmd(settings, MenuCommand.Duration30, "DEMO · 30 s", new Vector3(-.295f, .125f, 0), new Vector2(.56f, .075f), .28f);
            Cmd(settings, MenuCommand.Duration60, "PRÁCTICA · 60 s", new Vector3(.295f, .125f, 0), new Vector2(.56f, .075f), .28f);
            Cmd(settings, MenuCommand.Audio, "AUDIO", new Vector3(-.40f, .035f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.Haptics, "VIBRACIÓN", new Vector3(0, .035f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.Visual, "PULSO VISUAL", new Vector3(.40f, .035f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.VolumeDown, "VOLUMEN −", new Vector3(-.40f, -.055f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.Hand, "CAMBIAR TOUCH", new Vector3(0, -.055f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.VolumeUp, "VOLUMEN +", new Vector3(.40f, -.055f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.IntensityDown, "INTENSIDAD −", new Vector3(-.40f, -.145f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.Marker, "MARCADOR", new Vector3(0, -.145f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.IntensityUp, "INTENSIDAD +", new Vector3(.40f, -.145f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.HeightDown, "ALTURA −", new Vector3(-.40f, -.24f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.HeightReset, "ALTURA REAL", new Vector3(0, -.24f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.HeightUp, "ALTURA +", new Vector3(.40f, -.24f, 0), new Vector2(.36f, .075f), .26f);
            Cmd(settings, MenuCommand.Recenter, "CENTRAR VISTA", new Vector3(-.20f, -.33f, 0), new Vector2(.36f, .065f), .24f);
            Cmd(settings, MenuCommand.HowTo, "CÓMO SE JUEGA", new Vector3(.20f, -.33f, 0), new Vector2(.36f, .065f), .24f);

            // Resultados
            var results = Group(p, "Resultados"); menu.resultsPage = results.gameObject;
            Plate("TarjetaPractica", results, new Vector3(-.295f, .02f, -.002f), new Vector2(.57f, .42f), Surface, buttonSprite, -2);
            Plate("TarjetaReto", results, new Vector3(.295f, .02f, -.002f), new Vector2(.57f, .42f), Surface, buttonSprite, -2);
            Label(results, "TituloPractica", new Vector3(-.295f, .19f, -.006f), "PRÁCTICA GUIADA", .28f, new Vector2(.52f, .05f), Accent, bold: true);
            Label(results, "TituloReto", new Vector3(.295f, .19f, -.006f), "RETO SIN AYUDAS", .28f, new Vector2(.52f, .05f), Accent, bold: true);
            menu.practiceText = Label(results, "DatosPractica", new Vector3(-.295f, -.005f, -.006f), "", .24f, new Vector2(.52f, .32f), align: TextAlignmentOptions.TopLeft);
            menu.challengeText = Label(results, "DatosReto", new Vector3(.295f, -.005f, -.006f), "", .24f, new Vector2(.52f, .32f), align: TextAlignmentOptions.TopLeft);
            menu.comparisonText = Label(results, "Comparacion", new Vector3(0, -.29f, -.006f), "", .22f, new Vector2(1.15f, .08f), TextMuted);

            // Cómo se juega
            var help = Group(p, "ComoJugar"); menu.helpPage = help.gameObject;
            Plate("TarjetaControles", help, new Vector3(-.295f, -.04f, -.002f), new Vector2(.57f, .52f), Surface, buttonSprite, -2);
            Plate("TarjetaRcp", help, new Vector3(.295f, -.04f, -.002f), new Vector2(.57f, .52f), Surface, buttonSprite, -2);
            Label(help, "TituloControles", new Vector3(-.295f, .185f, -.006f), "CONTROLES", .28f, new Vector2(.52f, .05f), Accent, bold: true);
            Label(help, "Controles", new Vector3(-.295f, -.06f, -.006f),
                "<b>Stick izquierdo</b>  moverse\n<b>Stick derecho</b>  girar\n<b>Grip</b>  agarrar y soltar objetos\n<b>Botones</b>  tócalos con la mano, sin gatillo\n<b>Retira la mano</b> para volver a pulsar\n\nJuega <b>de pie</b> y frente a los sensores.",
                .31f, new Vector2(.52f, .42f), align: TextAlignmentOptions.TopLeft);
            Label(help, "TituloRcp", new Vector3(.295f, .185f, -.006f), "COMPRESIONES", .28f, new Vector2(.52f, .05f), Accent, bold: true);
            Label(help, "Rcp", new Vector3(.295f, -.06f, -.006f),
                "1. Colócate de pie junto a la camilla.\n2. Superpón ambos mandos sobre la zona azul del tórax.\n3. Mantén 1,5 s para empezar.\n4. Baja y sube con el pulso: 100–120 por minuto.\n\nNo hace falta agacharse: si cuesta llegar, usa <b>ALTURA</b> en el menú.",
                .31f, new Vector2(.52f, .42f), align: TextAlignmentOptions.TopLeft);

            menu.buttons = commands.ToArray();
            foreach (var r in p.GetComponentsInChildren<TMP_Text>(true)) EditorUtility.SetDirty(r);
        }

        // ---------- Pantalla RCP con pulso integrado ----------
        static void StationPulse(TrainingFlow flow)
        {
            var station = flow.session.transform.parent;
            var oldSign = station.Find("PulsoRitmo"); if (oldSign) Object.DestroyImmediate(oldSign.gameObject);
            var panel = station.Find("PantallaRCP/Panel");
            var old = panel.Find("Pulso"); if (old) Object.DestroyImmediate(old.gameObject);
            var g = Group(panel, "Pulso"); g.localPosition = new Vector3(.43f, .37f, -.03f);
            var dot = Plate("Indicador", g, Vector3.zero, new Vector2(.07f, .07f), Accent, circleSprite, 0);
            Label(g, "Texto", new Vector3(0, -.06f, -.002f), "PULSO", .17f, new Vector2(.14f, .03f), TextMuted, bold: true);
            flow.metronome.visualIndicator = dot.transform;
            EditorUtility.SetDirty(flow.metronome);
        }

        // ---------- Menos interfaces ----------
        static void Declutter(TrainingFlow flow)
        {
            // Letreros flotantes redundantes: la portada y el menú ya explican la ruta.
            var demo = flow.transform.Find("OrientacionDemo");
            if (demo) Object.DestroyImmediate(demo.gameObject);
            var adjust = flow.session.transform.parent.Find("AjustesRitmo");
            if (adjust) Object.DestroyImmediate(adjust.gameObject);
        }

        // ---------- Estilo de los demás paneles ----------
        static void SkinPanels(TrainingFlow flow)
        {
            var station = flow.session.transform.parent;
            var targets = new List<Transform>
            {
                station.Find("PantallaRCP/Panel/Fondo"),
                flow.transform.Find("Reconocer/Panel"),
                flow.transform.Find("TelefonoSimulado/Telefono"),
                flow.transform.Find("CasoB_Interacciones/Panel"),
                flow.transform.Find("DEA_Simulado/Indicaciones/Fondo"),
            };
            foreach (var t in targets.Where(x => x))
            {
                var r = t.GetComponent<MeshRenderer>(); if (!r) continue;
                var parent = t.parent;
                var old = parent.Find(t.name + "_UI"); if (old) Object.DestroyImmediate(old.gameObject);
                // El frente es el lado en el que están los textos.
                float side = -1f;
                var texts = parent.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0) side = Mathf.Sign(texts.Average(x => parent.InverseTransformPoint(x.transform.position).z - t.localPosition.z));
                if (side == 0) side = -1f;
                var s = t.localScale;
                var ui = Group(parent, t.name + "_UI");
                ui.localPosition = t.localPosition + new Vector3(0, 0, side * (s.z * .5f + .002f)); ui.localRotation = t.localRotation;
                Plate("Borde", ui, new Vector3(0, 0, -side * .003f), new Vector2(s.x + .012f, s.y + .012f), Border, panelSprite, -3);
                Plate("Fondo", ui, Vector3.zero, new Vector2(s.x, s.y), Bg, panelSprite, -3);
                r.enabled = false;
                EditorUtility.SetDirty(r);
            }
        }

        // ---------- Tipografía única ----------
        static void UnifyFonts(TrainingFlow flow)
        {
            var roots = new[] { flow.transform, flow.session.transform.parent };
            foreach (var root in roots)
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.font == font) continue;
                t.font = font; t.fontSharedMaterial = font.material;
                if (!t.enableAutoSizing) { t.enableAutoSizing = true; t.fontSizeMax = t.fontSize; t.fontSizeMin = t.fontSize * .7f; }
                EditorUtility.SetDirty(t);
            }
        }

        // ---------- Calidad para VR ----------
        static void VrQuality()
        {
            foreach (var path in new[] { "Assets/Settings/PC_RPAsset.asset" })
            {
                var urp = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(path);
                if (!urp) continue;
                urp.msaaSampleCount = 4;      // bordes y texto nítidos en el visor
                urp.supportsHDR = false;      // sin postprocesado: HDR solo cuesta rendimiento
                EditorUtility.SetDirty(urp);
            }
            var cam = Object.FindAnyObjectByType<XROrigin>().Camera;
            cam.nearClipPlane = .05f; cam.farClipPlane = 120f; // más precisión de profundidad: sin parpadeo entre capas
            EditorUtility.SetDirty(cam);
        }

        static void SkinButtons(TrainingFlow flow)
        {
            foreach (var b in Object.FindObjectsByType<RcpTouchButton>(FindObjectsInactive.Include))
            {
                if (b.transform.IsChildOf(menu.transform) || !b.pressVisual || b.pressVisual.name != "Confirmacion") continue;
                var cube = b.pressVisual.GetComponent<MeshRenderer>(); if (!cube) continue;
                var old = b.pressVisual.Find("Skin"); if (old) Object.DestroyImmediate(old.gameObject);
                var s = b.pressVisual.localScale;
                var skin = Plate("Skin", b.pressVisual, new Vector3(0, 0, -.52f), new Vector2(s.x, s.y), Ready, buttonSprite, -1);
                skin.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
                skin.transform.localPosition = new Vector3(0, 0, -.5f - .0015f / s.z);
                cube.enabled = false;
                b.feedbackRenderer = skin;
                Palette(b);
                var label = b.GetComponentInChildren<TMP_Text>(true);
                if (label) { label.fontStyle |= FontStyles.Bold; EditorUtility.SetDirty(label); }
                EditorUtility.SetDirty(b); EditorUtility.SetDirty(cube);
            }
        }
    }
}
