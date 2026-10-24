// WiRR W04: deterministyczny demonstrator dydaktyczny, NIE odczyt AR/IMU.
// Unity 6000.6.5f1: dodaj do pustego GameObject w nowej scenie i uruchom Play.
// Bez AR Foundation, urządzeń, starego Input Managera i płatnych usług.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public sealed class WiRRW04UnityDemo : MonoBehaviour
{
    private const float MaxPoseAgeSeconds = 0.25f;
    private const float ReferenceToleranceMeters = 0.05f;
    private const float FocalPx = 800f;
    private const float BaselineMeters = 0.10f;
    private const float DisparitySigmaPx = 0.5f;

    private GameObject physicalTarget;
    private GameObject diagnosticOverlay;
    private GameObject approvedOverlay;
    private GameObject realDepthProxy;
    private GameObject physicalSurfaceOutline; // niezależna referencja, nie mapa depth
    private GameObject virtualObject;
    private GameObject stereoRig;
    private GameObject stereoTarget;
    private GameObject stereoLeftRay;
    private GameObject stereoRightRay;
    private readonly Vector3 stereoLeftCamera = new Vector3(-0.80f, 1.30f, 0.75f);
    private readonly Vector3 stereoRightCamera = new Vector3(0.80f, 1.30f, 0.75f);

    private GameObject trackingPanel;
    private GameObject stereoPanel;
    private GameObject occlusionPanel;
    private Text trackingStatus;
    private Text stereoStatus;
    private Text occlusionStatus;
    private Slider stereoSlider;
    private Slider realSlider;
    private Slider virtualSlider;
    private Font labelFont;
    private float nextUiRefreshAt;

    private int mode; // 0: tracking, 1: stereo, 2: okluzja
    private bool tracking = true;
    private bool mapMatched = true;
    private bool referenceConfirmed = true;
    private float errorMeters;
    private float lastPoseTime;

    private float disparityPx = 40f;
    private float zReal = 1.5f;
    private float zVirtual = 2.5f;
    private bool depthValid = true;
    private bool depthSynchronized = true;
    private bool showUnsafeFailure;

    private bool PoseFresh =>
        tracking && Time.unscaledTime - lastPoseTime <= MaxPoseAgeSeconds;

    // Realne XR wymaga danych dostawcy, spójnych ramek i niezależnej referencji.
    private bool IsOverlayApproved =>
        PoseFresh && mapMatched && referenceConfirmed;

    private void Start()
    {
        if (Camera.main == null)
        {
            GameObject cameraObject = new GameObject("W04 Demo Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 1.3f, -3f);
            camera.transform.LookAt(new Vector3(0f, 1.3f, 2f));
        }

        if (FindFirstObjectByType<Light>() == null)
        {
            GameObject lightObject = new GameObject("W04 Demo Light");
            Light lamp = lightObject.AddComponent<Light>();
            lamp.type = LightType.Directional;
            lightObject.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
        }

        physicalTarget = Cube("Fizyczna referencja (symulowana)", 
            new Vector3(0f, 1.3f, 2f), new Vector3(1.4f, 0.9f, 0.12f),
            new Color(0.35f, 0.40f, 0.47f));
        diagnosticOverlay = Cube("DEBUG: niedopuszczona poza kotwicy",
            new Vector3(0f, 1.3f, 1.86f), new Vector3(0.92f, 0.56f, 0.04f),
            new Color(0.95f, 0.26f, 0.22f));
        approvedOverlay = Cube("Nakładka dopuszczona do użycia",
            new Vector3(0f, 1.3f, 1.80f), new Vector3(0.55f, 0.32f, 0.04f),
            new Color(0.14f, 0.82f, 0.45f));
        realDepthProxy = Cube("Zrekonstruowana realna powierzchnia (proxy)",
            new Vector3(0f, 1.3f, zReal), new Vector3(0.8f, 0.8f, 0.08f),
            new Color(0.16f, 0.59f, 0.96f));
        virtualObject = Cube("Wirtualny obiekt",
            new Vector3(0f, 1.3f, zVirtual), new Vector3(0.62f, 0.62f, 0.08f),
            new Color(1f, 0.70f, 0.17f));

        BuildStereoVisualization();
        BuildPhysicalSurfaceOutline();
        lastPoseTime = Time.unscaledTime;
        BuildUI();
    }

    private static GameObject Cube(string title, Vector3 position, Vector3 scale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = title;
        obj.transform.position = position;
        obj.transform.localScale = scale;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader != null)
        {
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            obj.GetComponent<Renderer>().material = material;
        }
        return obj;
    }


    // Schemat przestrzenny stereo, celowo narysowany poza skalą fizyczną.
    // Szerokość pary kamer w tej wizualizacji NIE jest BaselineMeters.
    private void BuildStereoVisualization()
    {
        stereoRig = new GameObject("Stereo: wizualizacja schematyczna");
        Cube("Lewa kamera - schemat", stereoLeftCamera,
            new Vector3(0.38f, 0.32f, 0.25f), new Color(0.15f, 0.70f, 0.95f))
            .transform.SetParent(stereoRig.transform, true);
        Cube("Prawa kamera - schemat", stereoRightCamera,
            new Vector3(0.38f, 0.32f, 0.25f), new Color(0.10f, 0.80f, 0.68f))
            .transform.SetParent(stereoRig.transform, true);
        stereoTarget = Cube("Wspólny punkt sceny", Vector3.zero,
            new Vector3(0.32f, 0.32f, 0.32f), new Color(0.98f, 0.69f, 0.15f));
        stereoTarget.transform.SetParent(stereoRig.transform, true);
        stereoLeftRay = Cube("Promień lewy - wizualizacja", Vector3.zero,
            Vector3.one, new Color(0.15f, 0.70f, 0.95f));
        stereoLeftRay.transform.SetParent(stereoRig.transform, true);
        stereoRightRay = Cube("Promień prawy - wizualizacja", Vector3.zero,
            Vector3.one, new Color(0.10f, 0.80f, 0.68f));
        stereoRightRay.transform.SetParent(stereoRig.transform, true);
        UpdateStereoVisualization();
        stereoRig.SetActive(false);
    }

    private static void SetRay(GameObject ray, Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        ray.transform.position = (from + to) * 0.5f;
        ray.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        ray.transform.localScale = new Vector3(0.025f, 0.025f, delta.magnitude);
    }

    private void UpdateStereoVisualization()
    {
        if (stereoRig == null)
            return;
        float zMeters = FocalPx * BaselineMeters / disparityPx;
        // Skala sceniczna 1.5...4.3 jest tylko pomocą wizualną.
        float visualZ = Mathf.Lerp(1.5f, 4.3f, Mathf.InverseLerp(1f, 8f, zMeters));
        Vector3 target = new Vector3(0f, 1.3f, visualZ);
        stereoTarget.transform.position = target;
        SetRay(stereoLeftRay, stereoLeftCamera, target);
        SetRay(stereoRightRay, stereoRightCamera, target);
    }


    // Kontur znanej, fizycznej przeszkody pozostaje na ekranie również wtedy,
    // gdy czujnik głębi nie dostarcza próbki. Same krawędzie nie zasłaniają
    // centralnego obszaru: można zobaczyć błędny hologram przez przeszkodę.
    private void BuildPhysicalSurfaceOutline()
    {
        physicalSurfaceOutline = new GameObject("Realna przeszkoda: kontur referencyjny");
        float half = 0.49f;
        float thickness = 0.045f;
        Color outlineColor = new Color(0.85f, 0.90f, 0.95f);
        Cube("Krawędź górna", new Vector3(0f, 1.3f + half, 0f),
            new Vector3(2f * half, thickness, thickness), outlineColor)
            .transform.SetParent(physicalSurfaceOutline.transform, true);
        Cube("Krawędź dolna", new Vector3(0f, 1.3f - half, 0f),
            new Vector3(2f * half, thickness, thickness), outlineColor)
            .transform.SetParent(physicalSurfaceOutline.transform, true);
        Cube("Krawędź lewa", new Vector3(-half, 1.3f, 0f),
            new Vector3(thickness, 2f * half, thickness), outlineColor)
            .transform.SetParent(physicalSurfaceOutline.transform, true);
        Cube("Krawędź prawa", new Vector3(half, 1.3f, 0f),
            new Vector3(thickness, 2f * half, thickness), outlineColor)
            .transform.SetParent(physicalSurfaceOutline.transform, true);
        physicalSurfaceOutline.SetActive(false);
    }

    private void Update()
    {
        // Symulowany estymator publikuje nową pozę tylko w stanie tracking.
        if (tracking)
            lastPoseTime = Time.unscaledTime;

        bool anchorMode = mode == 0;
        physicalTarget.SetActive(anchorMode);
        diagnosticOverlay.SetActive(anchorMode);
        approvedOverlay.SetActive(anchorMode && IsOverlayApproved);

        // Czerwony element: tylko diagnostyka. Nigdy nie jest "zatwierdzony".
        diagnosticOverlay.transform.position =
            physicalTarget.transform.position + new Vector3(errorMeters, 0f, -0.14f);

        if (stereoRig != null)
        {
            stereoRig.SetActive(mode == 1);
            if (mode == 1)
                UpdateStereoVisualization();
        }

        bool depthMode = mode == 2;
        // Brak pomiaru NIE oznacza zReal = nieskończoność ani pustego świata.
        bool depthUsable = depthValid && depthSynchronized;
        realDepthProxy.SetActive(depthMode && depthUsable);
        virtualObject.SetActive(depthMode && (depthUsable || showUnsafeFailure));
        physicalSurfaceOutline.SetActive(depthMode);
        // Kontur niezależnej referencji przesuwamy razem z przeszkodą.
        // Nie zastępuje on brakujących danych z czujnika głębi.
        physicalSurfaceOutline.transform.position = new Vector3(0f, 0f, zReal);
        realDepthProxy.transform.position = new Vector3(0f, 1.3f, zReal);
        virtualObject.transform.position = new Vector3(0f, 1.3f, zVirtual);

        // Aktualizacja tekstu UI 10 Hz zamiast alokowania napisów w każdej klatce.
        if (Time.unscaledTime >= nextUiRefreshAt)
        {
            DrawTracking();
            DrawStereo();
            DrawOcclusion();
            nextUiRefreshAt = Time.unscaledTime + 0.1f;
        }
    }

    private void ResetTracking()
    {
        tracking = true;
        mapMatched = true;
        referenceConfirmed = true;
        errorMeters = 0f;
        lastPoseTime = Time.unscaledTime;
    }

    private void LoseTracking()
    {
        tracking = false;
        mapMatched = false;
        referenceConfirmed = false;
    }

    private void RecoverOnlyTracking()
    {
        tracking = true;
        mapMatched = false;
        referenceConfirmed = false;
        lastPoseTime = Time.unscaledTime;
    }

    private void SimulateWrongRelocalization()
    {
        tracking = true;
        mapMatched = true; // zgłoszone dopasowanie, lecz pozycja jest błędna
        referenceConfirmed = false;
        errorMeters = 0.35f;
        lastPoseTime = Time.unscaledTime;
    }

    private void CheckIndependentReference()
    {
        // Ground truth jest dostępny TYLKO w tej symulacji. W AR trzeba
        // zastosować zewnętrzny pomiar, marker lub zatwierdzenie operatora.
        referenceConfirmed = PoseFresh && mapMatched &&
            Mathf.Abs(errorMeters) <= ReferenceToleranceMeters;
    }

    private void CorrectRegistration()
    {
        errorMeters = 0f;
        referenceConfirmed = false; // poprawka wymaga nowej weryfikacji
    }


    // Interfejs tworzony przez uGUI, więc działa z Input System-only.
    // Testowany kontrakt projektu WiRR: com.unity.ugui i com.unity.inputsystem.
    private void BuildUI()
    {
        if (EventSystem.current == null)
        {
            GameObject events = new GameObject("W04 EventSystem");
            events.AddComponent<EventSystem>();
            // Dodany moduł otrzymuje domyślne UI Input Actions dostawcy Input System.
            events.AddComponent<InputSystemUIInputModule>();
        }
        else if (EventSystem.current.GetComponent<InputSystemUIInputModule>() == null)
        {
            // Własna scena dydaktyczna może istnieć w starszym projekcie z
            // StandaloneInputModule. Używamy jawnie Input System UI.
            var legacy = EventSystem.current.GetComponent<StandaloneInputModule>();
            if (legacy != null)
                legacy.enabled = false;
            EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (labelFont == null)
        {
            Debug.LogError("WiRR W04: brak standardowej czcionki Unity LegacyRuntime.ttf.");
            return;
        }

        GameObject canvasObject = new GameObject("W04 Runtime UI", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0f;

        GameObject frame = CreateUIObject(canvas.transform, "W04 Panel",
            typeof(Image), typeof(VerticalLayoutGroup));
        var frameRect = frame.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0f, 1f);
        frameRect.anchorMax = new Vector2(0f, 1f);
        frameRect.pivot = new Vector2(0f, 1f);
        frameRect.anchoredPosition = new Vector2(22f, -22f);
        frameRect.sizeDelta = new Vector2(690f, 920f);
        frame.GetComponent<Image>().color = new Color(0.06f, 0.10f, 0.17f, 0.96f);
        var stack = frame.GetComponent<VerticalLayoutGroup>();
        stack.padding = new RectOffset(16, 16, 16, 16);
        stack.spacing = 9f;
        stack.childAlignment = TextAnchor.UpperLeft;
        stack.childControlWidth = true;
        stack.childControlHeight = false;
        stack.childForceExpandHeight = false;

        AddLabel(frame.transform, "WiRR W04  |  Tracking · stereo · okluzja", 29, 45);
        AddLabel(frame.transform, "Eksperyment syntetyczny. Bez kamery i gogli.", 20, 37);

        GameObject tabs = CreateUIObject(frame.transform, "Tryby",
            typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        tabs.GetComponent<LayoutElement>().preferredHeight = 55f;
        var tabsLayout = tabs.GetComponent<HorizontalLayoutGroup>();
        tabsLayout.spacing = 8f;
        tabsLayout.childForceExpandWidth = true;
        tabsLayout.childControlWidth = true;
        tabsLayout.childControlHeight = true;
        AddButton(tabs.transform, "1. Kotwica", () => SelectMode(0));
        AddButton(tabs.transform, "2. Stereo", () => SelectMode(1));
        AddButton(tabs.transform, "3. Okluzja", () => SelectMode(2));

        trackingPanel = AddSection(frame.transform, "Tracking");
        AddLabel(trackingPanel.transform,
            "Czerwony: estymator (debug). Zielony: zatwierdzona nakładka.", 21, 72);
        trackingStatus = AddLabel(trackingPanel.transform, "", 23, 140);
        AddButton(trackingPanel.transform, "A. Położenie początkowe", ResetTracking);
        AddButton(trackingPanel.transform, "B. Utrata śledzenia", LoseTracking);
        AddButton(trackingPanel.transform, "C. TRACKING bez relokalizacji", RecoverOnlyTracking);
        AddButton(trackingPanel.transform, "D. Błędna relokalizacja: +35 cm",
            SimulateWrongRelocalization);
        AddButton(trackingPanel.transform, "E. Test niezależnej referencji (5 cm)",
            CheckIndependentReference);
        AddButton(trackingPanel.transform, "F. Skoryguj położenie", CorrectRegistration);

        stereoPanel = AddSection(frame.transform, "Stereo");
        AddLabel(stereoPanel.transform,
            "Rektyfikowane stereo: f=800 px, B=0,10 m, sigma_d=0,5 px.", 21, 65);
        stereoStatus = AddLabel(stereoPanel.transform, "", 26, 110);
        stereoSlider = AddSlider(stereoPanel.transform, 10f, 80f, disparityPx,
            v => disparityPx = v);
        AddButton(stereoPanel.transform, "Przykład: d = 40 px",
            () => { stereoSlider.value = 40f; });
        AddButton(stereoPanel.transform, "Zwiększ głębię: d = 20 px",
            () => { stereoSlider.value = 20f; });
        AddLabel(stereoPanel.transform,
            "Kolorowe promienie łączą schematyczne kamery z punktem 3D.", 20, 65);
        AddLabel(stereoPanel.transform,
            "Rysunek poza skalą. Z = fB/d; brak błędów kalibracji.", 20, 65);

        occlusionPanel = AddSection(frame.transform, "Okluzja");
        AddLabel(occlusionPanel.transform,
            "Jasna ramka: realny cel. Niebieski: depth proxy. Żółty: AR.", 21, 72);
        occlusionStatus = AddLabel(occlusionPanel.transform, "", 23, 150);
        AddLabel(occlusionPanel.transform, "Głębia powierzchni realnej [m]", 20, 35);
        realSlider = AddSlider(occlusionPanel.transform, 0.6f, 4f, zReal,
            v => zReal = v);
        AddLabel(occlusionPanel.transform, "Głębia obiektu wirtualnego [m]", 20, 35);
        virtualSlider = AddSlider(occlusionPanel.transform, 0.6f, 4f, zVirtual,
            v => zVirtual = v);
        AddButton(occlusionPanel.transform, "Włącz / wyłącz pomiar głębi",
            () => depthValid = !depthValid);
        AddButton(occlusionPanel.transform, "Włącz / wyłącz synchronizację",
            () => depthSynchronized = !depthSynchronized);
        AddButton(occlusionPanel.transform, "Pokaż / ukryj błędną implementację",
            () => showUnsafeFailure = !showUnsafeFailure);
        AddLabel(occlusionPanel.transform,
            "Ramka pozostaje widoczna bez danych depth; obiekt AR nie powinien przechodzić przez jej środek.", 19, 65);
        AddLabel(occlusionPanel.transform,
            "Z-buffer Unity, nie kamera głębi ani shader AR.", 20, 48);

        SelectMode(0);
    }

    private void SelectMode(int next)
    {
        mode = next;
        trackingPanel.SetActive(mode == 0);
        stereoPanel.SetActive(mode == 1);
        occlusionPanel.SetActive(mode == 2);
    }

    private static GameObject CreateUIObject(Transform parent, string name,
        params System.Type[] components)
    {
        GameObject go = new GameObject(name, components);
        go.transform.SetParent(parent, false);
        return go;
    }

    private GameObject AddSection(Transform parent, string name)
    {
        GameObject section = CreateUIObject(parent, name,
            typeof(VerticalLayoutGroup), typeof(LayoutElement));
        var layout = section.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;
        // Cała sekcja zajmuje przewidywalne miejsce w panelu.
        section.GetComponent<LayoutElement>().preferredHeight = 690f;
        return section;
    }

    private Text AddLabel(Transform parent, string value, int fontSize, float height)
    {
        GameObject line = CreateUIObject(parent, "Tekst", typeof(Text),
            typeof(LayoutElement));
        var element = line.GetComponent<LayoutElement>();
        element.preferredHeight = height;
        var label = line.GetComponent<Text>();
        label.font = labelFont;
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleLeft;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.text = value;
        return label;
    }

    private void AddButton(Transform parent, string title, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreateUIObject(parent, title, typeof(Image),
            typeof(Button), typeof(LayoutElement));
        buttonObject.GetComponent<LayoutElement>().preferredHeight = 55f;
        buttonObject.GetComponent<Image>().color = new Color(0.04f, 0.36f, 0.61f);
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);
        Text label = AddLabel(buttonObject.transform, title, 21, 45);
        var textRect = label.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 0f);
        textRect.offsetMax = new Vector2(-10f, 0f);
        label.alignment = TextAnchor.MiddleCenter;
    }

    private Slider AddSlider(Transform parent, float min, float max, float value,
        UnityEngine.Events.UnityAction<float> onChange)
    {
        GameObject sliderObject = CreateUIObject(parent, "Suwak",
            typeof(Image), typeof(Slider), typeof(LayoutElement));
        sliderObject.GetComponent<LayoutElement>().preferredHeight = 42f;
        sliderObject.GetComponent<Image>().color = new Color(0.24f, 0.30f, 0.40f);
        GameObject fillObject = CreateUIObject(sliderObject.transform, "Wypełnienie",
            typeof(Image));
        fillObject.GetComponent<Image>().color = new Color(0.10f, 0.77f, 0.75f);
        var fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);
        GameObject handleObject = CreateUIObject(sliderObject.transform, "Uchwyt",
            typeof(Image));
        var handle = handleObject.GetComponent<RectTransform>();
        handle.sizeDelta = new Vector2(24f, 42f);
        handleObject.GetComponent<Image>().color = Color.white;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.fillRect = fillObject.GetComponent<RectTransform>();
        slider.handleRect = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        slider.onValueChanged.AddListener(onChange);
        return slider;
    }

    private void DrawTracking()
    {
        if (trackingStatus == null)
            return;
        trackingStatus.text =
            "Tracking: " + tracking + " | Mapa: " + mapMatched +
            " | Referencja: " + referenceConfirmed +
            "\nWiek pozy: " + (Time.unscaledTime - lastPoseTime).ToString("F2") + " s" +
            " | Błąd: " + errorMeters.ToString("F2") + " m" +
            "\nDecyzja: " + (IsOverlayApproved ? "DOPUSZCZONA" : "ZABLOKOWANA");
    }

    private void DrawStereo()
    {
        if (stereoStatus == null)
            return;
        float z = FocalPx * BaselineMeters / disparityPx;
        float sigmaZ = z * z * DisparitySigmaPx / (FocalPx * BaselineMeters);
        stereoStatus.text = "Dysparycja: " + disparityPx.ToString("F1") + " px" +
            "\nGłębia Z: " + z.ToString("F2") + " m" +
            "\nModelowa niepewność σZ: " + (sigmaZ * 100f).ToString("F1") + " cm";
    }

    private void DrawOcclusion()
    {
        if (occlusionStatus == null)
            return;
        bool usable = depthValid && depthSynchronized;
        bool realOccludes = usable && zReal < zVirtual;
        occlusionStatus.text =
            "Z real: " + zReal.ToString("F2") + " m | Z virtual: " +
            zVirtual.ToString("F2") + " m" +
            "\nGłębia: " + depthValid + " | Synchronizacja: " +
            depthSynchronized + " | Błędny wariant: " + showUnsafeFailure +
            "\nWynik: " + (usable
                ? (realOccludes ? "REAL zasłania" : "VIRTUAL z przodu")
                : "BRAK WIARYGODNEJ GŁĘBI");
    }
}
