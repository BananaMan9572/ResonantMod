using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine;

namespace ResonantMod
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    internal class ResonantModUI : MonoBehaviour
    {
        private readonly ResonantOrbitCalculator calculator = new ResonantOrbitCalculator();
        private readonly CelestialBodyManager bodyManager = new CelestialBodyManager();

        // UI Elements
        private ApplicationLauncherButton appButton;
        private bool showGUI = false;
        private Rect windowRect = new Rect(300, 200, 600, 300);
        private bool isGUIHidden = false;
        private bool showDebug = false;

        // Input Fields to Parse
        private string altitudeText = string.Empty;
        private string numberOfSatsText = string.Empty;

        // Dropdown Controls
        private bool showPlanetDropdown = false;
        private bool showMoonDropdown = false;
        private Vector2 scrollPosition = Vector2.zero;

        void Start()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(AddAppButton);
            GameEvents.onHideUI.Add(OnHideGUI);
            GameEvents.onShowUI.Add(OnShowGUI);
            bodyManager.PopulatePlanets();
        }

        void OnDestroy()
        {
            GameEvents.onHideUI.Remove(OnHideGUI);
            GameEvents.onShowUI.Remove(OnShowGUI);
            GameEvents.onGUIApplicationLauncherReady.Remove(AddAppButton);
            if (appButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
            }
        }

        private void OnHideGUI() => isGUIHidden = true;
        private void OnShowGUI() => isGUIHidden = false;
        private void ToggleGUI() => showGUI = !showGUI;

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
                    ApplicationLauncher.AppScenes.ALWAYS,
                    iconTexture
                );
            }
        }

        void OnGUI()
        {
            if (showGUI && !isGUIHidden)
            {
                windowRect = GUI.Window(9572, windowRect, DrawGUI, "ResonantMod");

                if (showPlanetDropdown)
                {
                    DrawDropdown(bodyManager.Planets, ref bodyManager.SelectedBody, ref showPlanetDropdown,
                               windowRect.x + 10, windowRect.y + windowRect.height);
                }

                if (showMoonDropdown)
                {
                    DrawDropdown(bodyManager.Moons, ref bodyManager.SelectedMoon, ref showMoonDropdown,
                               windowRect.x + windowRect.width / 2 + 10, windowRect.y + windowRect.height);
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

            if (!string.IsNullOrEmpty(bodyManager.ErrorMessage))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUIStyle errorStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } };
                GUILayout.Label(bodyManager.ErrorMessage, errorStyle, GUILayout.Width(580));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        void DrawRightSection(float width)
        {

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width));


            GUILayout.Label("Calculation Results:", GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Periapsis: {calculator.Periapsis} km");
            GUILayout.Label($"Apoapsis: {calculator.Apoapsis} km");
            GUILayout.Label($"Injection ΔV: {calculator.Injection} ms⁻¹");
            GUILayout.EndVertical();

            if (showDebug)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Debug Info:", GUILayout.ExpandWidth(false));
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"Current Body: {bodyManager.GetTargetBody()?.bodyName ?? "None"}");
                GUILayout.EndVertical();
                GUILayout.EndVertical();
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

            if (GUILayout.Button(bodyManager.SelectedBody?.bodyName ?? "Select Body",
                GUILayout.Width(width / 2 - 10)))
            {
                showPlanetDropdown = !showPlanetDropdown;
                showMoonDropdown = false;
                bodyManager.ErrorMessage = string.Empty;
            }

            if (bodyManager.IsMoon)
            {
                string buttonText = bodyManager.Moons.Count > 0
                    ? bodyManager.SelectedMoon?.bodyName ?? "Select Moon"
                    : "No moons";

                if (GUILayout.Button(buttonText, GUILayout.Width(width / 2 - 10)))
                {
                    showMoonDropdown = !showMoonDropdown;
                    showPlanetDropdown = false;
                    bodyManager.ErrorMessage = string.Empty;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            bool previousIsMoon = bodyManager.IsMoon;
            bodyManager.IsMoon = GUILayout.Toggle(bodyManager.IsMoon, "Is a moon of this body");

            if (bodyManager.IsMoon != previousIsMoon)
            {
                showPlanetDropdown = false;
                showMoonDropdown = false;
                if (bodyManager.IsMoon) bodyManager.PopulateMoons();
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
                        bodyManager.ErrorMessage = string.Empty;

                        if (bodies == bodyManager.Planets)
                        {
                            bodyManager.PopulateMoons();
                        }
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }

        void CalculateOrbit()
        {
            if (!float.TryParse(altitudeText, out float altitude) || altitude < 0)
            {
                bodyManager.ErrorMessage = "Invalid altitude value.";
                return;
            }

            if (!int.TryParse(numberOfSatsText, out int numberOfSats) || numberOfSats < 3)
            {
                bodyManager.ErrorMessage = "At least 3 satellites required.";
                return;
            }

            var targetBody = bodyManager.GetTargetBody();
            if (targetBody == null)
            {
                bodyManager.ErrorMessage = "No celestial body selected.";
                return;
            }

            if (!calculator.CalculateOrbit(targetBody, altitude, numberOfSats, out string error))
            {
                bodyManager.ErrorMessage = error;
            }
        }
    }
}