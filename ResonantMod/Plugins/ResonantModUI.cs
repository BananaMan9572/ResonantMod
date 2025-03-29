using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine;

namespace ResonantMod.Plugins {
    [KSPAddon(KSPAddon.Startup.FlightEditorAndKSC, false)]
    internal class ResonantModUI : MonoBehaviour {
        private readonly ResonantOrbitCalculator calculator = new ResonantOrbitCalculator();
        private readonly CelestialBodyManager bodyManager = new CelestialBodyManager();

        // UI Elements
        private ApplicationLauncherButton appButton;
        private bool showGUI = false;
        private Rect windowRect = new Rect(300, 200, 600, 300);
        private bool isGUIHidden = false;
        private bool showDebug = false;
        public string ErrorMessage { get; set; } = string.Empty;

        // Input Fields to Parse
        private string altitudeText = string.Empty;
        private string numberOfSatsText = string.Empty;

        // Dropdown Controls
        private bool showPlanetDropdown = false;
        private bool showMoonDropdown = false;
        private Vector2 scrollPosition = Vector2.zero;

        void Start() {
            GameEvents.onGUIApplicationLauncherReady.Add(this.AddAppButton);
            GameEvents.onHideUI.Add(this.OnHideGUI);
            GameEvents.onShowUI.Add(this.OnShowGUI);
            this.bodyManager.PopulatePlanets(out string error);
            if(!string.IsNullOrEmpty(error))
                this.ErrorMessage = error;
        }

        void OnDestroy() {
            GameEvents.onHideUI.Remove(this.OnHideGUI);
            GameEvents.onShowUI.Remove(this.OnShowGUI);
            GameEvents.onGUIApplicationLauncherReady.Remove(this.AddAppButton);
            if(this.appButton != null) {
                ApplicationLauncher.Instance.RemoveModApplication(this.appButton);
            }
        }

        private void OnHideGUI() => this.isGUIHidden = true;
        private void OnShowGUI() => this.isGUIHidden = false;
        private void ToggleGUI() => this.showGUI = !this.showGUI;

        void AddAppButton() {
            if(ApplicationLauncher.Instance != null && this.appButton == null) {
                Texture2D iconTexture = GameDatabase.Instance.GetTexture("ResonantMod/icon", false);
                this.appButton = ApplicationLauncher.Instance.AddModApplication(
                    this.ToggleGUI,
                    this.ToggleGUI,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.ALWAYS,
                    iconTexture
                );
            }
        }

        void OnGUI() {
            if(this.showGUI && !this.isGUIHidden) {
                this.windowRect = GUI.Window(9572, this.windowRect, this.DrawGUI, "ResonantMod");

                if(this.showPlanetDropdown) {
                    this.DrawDropdown(this.bodyManager.Planets, ref this.bodyManager.SelectedBody, ref this.showPlanetDropdown,
                               this.windowRect.x + 10, this.windowRect.y + this.windowRect.height);
                }

                if(this.showMoonDropdown) {
                    this.DrawDropdown(this.bodyManager.Moons, ref this.bodyManager.SelectedMoon, ref this.showMoonDropdown,
                               this.windowRect.x + (this.windowRect.width / 2) + 10, this.windowRect.y + this.windowRect.height);
                }
            }
        }

        void DrawGUI(int windowID) {
            if(GUI.Button(new Rect(this.windowRect.width - 25, 2, 20, 15), "x")) {
                this.showGUI = false;
            }

            if(GUI.Button(new Rect(this.windowRect.width - 50, 2, 20, 15), this.showDebug ? "D" : "d")) {
                this.showDebug = !this.showDebug;
            }

            this.DrawMainContent();
            GUI.DragWindow();
        }

        void DrawMainContent() {
            float width = (this.windowRect.width / 2) - 15;
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            this.DrawLeftSection(width);
            this.DrawRightSection(width);
            GUILayout.EndHorizontal();

            if(!string.IsNullOrEmpty(this.ErrorMessage)) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUIStyle errorStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } };
                GUILayout.Label(this.ErrorMessage, errorStyle, GUILayout.Width(580));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        void DrawRightSection(float width) {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width));

            GUILayout.Label("Calculation Results:", GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Periapsis: {this.calculator.Periapsis} km");
            GUILayout.Label($"Apoapsis: {this.calculator.Apoapsis} km");
            GUILayout.Label($"Injection ΔV: {this.calculator.Injection} ms⁻¹");
            GUILayout.EndVertical();

            if(this.showDebug) {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Debug Info:", GUILayout.ExpandWidth(false));
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"Current Body: {this.bodyManager.GetTargetBody()?.bodyName ?? "None"}");
                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }

        void DrawLeftSection(float width) {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width));
            GUILayout.Label("Parameters");
            this.DrawCelestialBodySelection(width);
            this.DrawInputFields(width);
            if(GUILayout.Button("Calculate", GUILayout.Width(70))) {
                this.CalculateOrbit();
            }
            GUILayout.EndVertical();
        }

        void DrawCelestialBodySelection(float width) {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Select a body:");
            GUILayout.BeginHorizontal();

            if(GUILayout.Button(this.bodyManager.SelectedBody?.bodyName ?? "Select Body", GUILayout.Width((width / 2) - 10))) {
                this.showPlanetDropdown = !this.showPlanetDropdown;
                this.showMoonDropdown = false;
                this.ErrorMessage = string.Empty;
            }

            if(this.bodyManager.IsMoon) {
                string buttonText = this.bodyManager.Moons.Count > 0
                    ? this.bodyManager.SelectedMoon?.bodyName ?? "Select Moon"
                    : "No moons";

                if(GUILayout.Button(buttonText, GUILayout.Width((width / 2) - 10))) {
                    this.showMoonDropdown = !this.showMoonDropdown;
                    this.showPlanetDropdown = false;
                    this.ErrorMessage = string.Empty;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            bool previousIsMoon = this.bodyManager.IsMoon;
            this.bodyManager.IsMoon = GUILayout.Toggle(this.bodyManager.IsMoon, "Is a moon of this body");

            if(this.bodyManager.IsMoon != previousIsMoon) {
                this.showPlanetDropdown = false;
                this.showMoonDropdown = false;
                if(this.bodyManager.IsMoon) {
                    this.bodyManager.PopulateMoons();
                }
            }

            GUILayout.Space(8);
        }

        void DrawInputFields(float width) {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal(GUILayout.Width(width - 10));
            GUILayout.Label("Altitude:", GUILayout.Width(100));
            this.altitudeText = GUILayout.TextField(this.altitudeText, GUILayout.Width(100));
            GUILayout.Label("km");
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal(GUILayout.Width(width - 10));
            GUILayout.Label("Satellites:", GUILayout.Width(100));
            this.numberOfSatsText = GUILayout.TextField(this.numberOfSatsText, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void DrawDropdown(List<CelestialBody> bodies, ref CelestialBody selected, ref bool showDropdown, float x, float y) {
            if(showDropdown) {
                Rect dropdownRect = new Rect(x, y, 200, 150);
                GUI.Box(dropdownRect, "");
                GUILayout.BeginArea(dropdownRect);
                this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, GUILayout.Height(140));

                foreach(CelestialBody body in bodies) {
                    if(GUILayout.Button(body.bodyName)) {
                        selected = body;
                        showDropdown = false;
                        this.ErrorMessage = string.Empty;

                        if(bodies == this.bodyManager.Planets) {
                            this.bodyManager.PopulateMoons();
                        }
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }

        void CalculateOrbit() {
            if(!float.TryParse(this.altitudeText, out float altitude) || altitude < 0) {
                this.ErrorMessage = "Invalid altitude value.";
                return;
            }

            if(!int.TryParse(this.numberOfSatsText, out int numberOfSats) || numberOfSats < 3) {
                this.ErrorMessage = "At least 3 satellites required.";
                return;
            }

            CelestialBody targetBody = this.bodyManager.GetTargetBody();
            if(targetBody == null) {
                this.ErrorMessage = "No celestial body selected.";
                return;
            }

            if(!this.calculator.CalculateOrbit(targetBody, altitude, numberOfSats, out string error)) {
                this.ErrorMessage = error;
            }
        }
    }
}