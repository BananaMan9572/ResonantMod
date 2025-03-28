using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class ResonantMod : MonoBehaviour
{
    private ApplicationLauncherButton appButton;
    private bool showGUI = false;
    private Rect windowRect = new Rect(300, 200, 600, 300);

    private string errorMessage = string.Empty;
    private bool isMoon = false;
    private bool showDebug = false;

    private string altitudeText = string.Empty;
    private float altitude;
    private string numberOfSatsText = string.Empty;
    private int numberOfSats;

    private float periapsis;
    private float apoapsis;
    private float injection;


    // For calculations
    private double gm;
    double smaResonant;
    CelestialBody bodyToUse;

    private List<CelestialBody> planets = new List<CelestialBody>();
    private List<CelestialBody> moons = new List<CelestialBody>();
    private CelestialBody selectedBody = null;
    private CelestialBody selectedMoon = null;

    private bool showPlanetDropdown = false;
    private bool showMoonDropdown = false;
    private Vector2 scrollPosition = Vector2.zero;

    void Start()
    {
        GameEvents.onGUIApplicationLauncherReady.Add(AddAppButton);
        PopulatePlanets();
    }

    void OnDestroy()
    {
        GameEvents.onGUIApplicationLauncherReady.Remove(AddAppButton);
        if (appButton != null)
        {
            ApplicationLauncher.Instance.RemoveModApplication(appButton);
            appButton = null;
        }
    }

    void AddAppButton()
    {
        if (ApplicationLauncher.Instance != null && appButton == null)
        {
            Texture2D iconTexture = GameDatabase.Instance.GetTexture("ResonantMod/icon", false);
            appButton = ApplicationLauncher.Instance.AddModApplication(
                ToggleGUI, 
                ToggleGUI, 
                null, 
                null, 
                null, 
                null,
                ApplicationLauncher.AppScenes.FLIGHT,
                iconTexture
            );
        }
    }

    void PopulatePlanets()
    {
        planets.Clear();
        
        if(FlightGlobals.Bodies == null)
        {
            Debug.LogWarning("[ResonantMod] FlightGlobals.Bodies is null. (main menu?)");
            return;
        }

        foreach (CelestialBody body in FlightGlobals.Bodies)
        {
            if (body.referenceBody != null && body.referenceBody.isStar)
            {
                planets.Add(body);
            }
        }

        if (planets.Count > 0)
        {
            selectedBody = planets[0];
            PopulateMoons();
        }
    }

    void PopulateMoons()
    {
        moons.Clear();
        if (selectedBody != null)
        {
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body.referenceBody == selectedBody)
                {
                    moons.Add(body);
                }
            }
        }

        selectedMoon = (moons.Count > 0) ? moons[0] : null;
    }

    bool HasMoon(CelestialBody body)
    {
        foreach (CelestialBody moon in moons)
        {
            if (moon.referenceBody == body)
            {
                return true;
            }
        }
        return false;
    }

    void ToggleGUI() => showGUI = !showGUI;

    void OnGUI()
    {
        if (showGUI)
        {
            windowRect = GUI.Window(9572, windowRect, DrawGUI, "ResonantMod");

            if (showPlanetDropdown)
            {
                DrawDropdown(planets, ref selectedBody, ref showPlanetDropdown, windowRect.x + 10, windowRect.y + windowRect.height);
            }

            if (showMoonDropdown)
            {
                DrawDropdown(moons, ref selectedMoon, ref showMoonDropdown, windowRect.x + windowRect.width / 2 + 10, windowRect.y + windowRect.height);
            }
        }
    }

    void DrawGUI(int windowID)
    {
        if (GUI.Button(new Rect(windowRect.width - 25, 2, 20, 15), "x"))
        {
            showGUI = false;
        }

        if (GUI.Button(new Rect(windowRect.width - 50, 2, 20, 15), showDebug ? "D" : "d"))
        {
            showDebug = !showDebug;
        }

        DrawMainContent();
        GUI.DragWindow();
    }

    void DrawMainContent()
    {
        float width = windowRect.width / 2 - 15;
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        DrawLeftSection(width);
        DrawRightSection(width);
        GUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(errorMessage))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUIStyle errorStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } };
            GUILayout.Label(errorMessage, errorStyle, GUILayout.Width(580));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    void DrawRightSection(float width)
    {
        // Start main vertical group
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width));

        // Calculation results section
        GUILayout.Label("Calculation Results:", GUILayout.ExpandWidth(false));
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label($"Periapsis: {periapsis} km");
        GUILayout.Label($"Apoapsis: {apoapsis} km");
        GUILayout.Label($"Injection ΔV: {injection} ms⁻¹");
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        // Debug section (only if enabled)
        if (showDebug)
        {

            if (bodyToUse != null)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Space(5);
                GUILayout.Label("Debug Info:", GUILayout.ExpandWidth(false));
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"Selected body: {bodyToUse.bodyName}");
                GUILayout.Label($"μ: {(float)bodyToUse.gravParameter:E3} m³s⁻²");
                GUILayout.Label($"Body Mass: {(float)bodyToUse.Mass:E3} kg");
                GUILayout.Label($"Body Radius: {(float)bodyToUse.Radius:E3} m");
                GUILayout.Label($"Semi-major axis: {(float)smaResonant:E3} m");
                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("Do a calculation first!");
            }

        }
        GUILayout.EndVertical();
    }

    void DrawLeftSection(float width)
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width));
        GUILayout.Label("Parameters");

        DrawCelestialBodySelection(width);
        DrawInputFields(width);

        if (GUILayout.Button("Calculate", GUILayout.Width(70))) CalculateOrbit();

        GUILayout.EndVertical();
    }

    void DrawCelestialBodySelection(float width)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Select a body:");
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(selectedBody?.bodyName ?? "Select Body", GUILayout.Width(width / 2 - 10)))
        {
            showPlanetDropdown = !showPlanetDropdown;
            showMoonDropdown = false;
            errorMessage = string.Empty;
        }

        if (isMoon)
        {
            string buttonText;

            if(selectedMoon == null && HasMoon(selectedBody))
            {
                buttonText = "Select Moon";
            }
            else if (HasMoon(selectedBody))
            {
                buttonText = selectedMoon.bodyName;
            }
            else
            {
                buttonText = "No moons";
            }

            if (GUILayout.Button(buttonText, GUILayout.Width(width / 2 - 10)))
            {
                showMoonDropdown = !showMoonDropdown;
                showPlanetDropdown = false;
                errorMessage = string.Empty;
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        bool previousIsMoon = isMoon;
        isMoon = GUILayout.Toggle(isMoon, "Is a moon of this planet");

        if (isMoon != previousIsMoon)
        {
            showPlanetDropdown = false;
            showMoonDropdown = false;

            if (isMoon)
            {
                PopulateMoons();
            }
            else
            {
                selectedMoon = null;
            }
        }

        GUILayout.Space(8);
    }

    void DrawInputFields(float width)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal(GUILayout.Width(width - 10));
        GUILayout.Label("Altitude:", GUILayout.Width(100));
        altitudeText = GUILayout.TextField(altitudeText, GUILayout.Width(100));
        GUILayout.Label("km");
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal(GUILayout.Width(width - 10));
        GUILayout.Label("Satellites:", GUILayout.Width(100));
        numberOfSatsText = GUILayout.TextField(numberOfSatsText, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    void DrawDropdown(List<CelestialBody> bodies, ref CelestialBody selected, ref bool showDropdown, float x, float y)
    {
        if (showDropdown)
        {
            Rect dropdownRect = new Rect(x, y, 200, 150);
            GUI.Box(dropdownRect, "");
            GUILayout.BeginArea(dropdownRect);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(140));

            foreach (CelestialBody body in bodies)
            {
                if (GUILayout.Button(body.bodyName))
                {
                    selected = body;
                    showDropdown = false;

                    if (bodies == planets)
                    {
                        selectedBody = selected;
                        selectedMoon = null;
                        PopulateMoons();
                    }
                    else if (bodies == moons)
                    {
                        selectedMoon = selected;
                    }

                    errorMessage = string.Empty;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }

    void CalculateOrbit()
    {
        errorMessage = string.Empty;

        if (!float.TryParse(altitudeText, out altitude) || altitude < 0)
        {
            errorMessage = altitude < 0 ? "Altitude must be a non-negative number." : "Invalid altitude value.";
            return;
        }

        if (!int.TryParse(numberOfSatsText, out numberOfSats) || numberOfSats < 1)
        {
            errorMessage = numberOfSats < 3 ? "At least 3 satellites are required." : "Invalid number of satellites.";
            return;
        }

        bodyToUse = isMoon ? selectedMoon : selectedBody;
        if (bodyToUse == null)
        {
            errorMessage = "No celestial body selected.";
            return;
        }

        gm = bodyToUse.gravParameter;
        double rTarget = bodyToUse.Radius + altitude * 1000;
        double tTarget = 2 * Math.PI * Math.Sqrt(Math.Pow(rTarget, 3) / gm);
        double tResonant = tTarget * (numberOfSats + 1) / numberOfSats;
        smaResonant = Math.Pow((tResonant * tResonant) * gm / (4 * Math.PI * Math.PI), 1.0 / 3.0);

        double rPeriapsis = rTarget;
        double rApoapsis = 2 * smaResonant - rTarget;

        if (rApoapsis <= rPeriapsis)
        {
            errorMessage = "Resonant orbit calculation failed. Adjust parameters.";
            return;
        }

        periapsis = (float)((rPeriapsis - bodyToUse.Radius) / 1000);
        apoapsis = (float)((rApoapsis - bodyToUse.Radius) / 1000);

        double vPeriapsis = Math.Sqrt(gm * (2 / rPeriapsis - 1 / smaResonant));
        double vCircular = Math.Sqrt(gm / rPeriapsis);
        injection = (float)(vPeriapsis - vCircular);
    }
}
