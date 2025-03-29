using System.Collections.Generic;

namespace ResonantMod {
    internal class CelestialBodyManager {
        public List<CelestialBody> Planets { get; } = new List<CelestialBody>();
        public List<CelestialBody> Moons { get; } = new List<CelestialBody>();

        public CelestialBody SelectedBody;

        public CelestialBody SelectedMoon;
        public bool IsMoon { get; set; }

        public void PopulatePlanets(out string errorMessage) {
            this.Planets.Clear();

            if(FlightGlobals.Bodies == null) {
                errorMessage = "FlightGlobals.Bodies is null.";
                return;
            }

            foreach(CelestialBody body in FlightGlobals.Bodies) {
                if(body.referenceBody != null && body.referenceBody.isStar) {
                    this.Planets.Add(body);
                }
            }

            if(this.Planets.Count > 0) {
                this.PopulateMoons();
                errorMessage = string.Empty;
            } else {
                errorMessage = "No planets found (how did you manage that?).";
            }
        }

        public void PopulateMoons() {
            this.Moons.Clear();
            if(this.SelectedBody != null) {
                foreach(CelestialBody body in FlightGlobals.Bodies) {
                    if(body.referenceBody == this.SelectedBody) {
                        this.Moons.Add(body);
                    }
                }
            }
            this.SelectedMoon = (this.Moons.Count > 0) ? this.Moons[0] : null;
        }

        public CelestialBody GetTargetBody() {
            return this.IsMoon ? this.SelectedMoon : this.SelectedBody;
        }
    }
}