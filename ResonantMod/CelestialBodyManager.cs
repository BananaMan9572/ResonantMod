using System.Collections.Generic;
using UnityEngine;

namespace ResonantMod
{
    internal class CelestialBodyManager
    {
        public List<CelestialBody> Planets { get; } = new List<CelestialBody>();
        public List<CelestialBody> Moons { get; } = new List<CelestialBody>();

        public CelestialBody SelectedBody;

        public CelestialBody SelectedMoon;
        public bool IsMoon { get; set; }

        public void PopulatePlanets(out string errorMessage)
        {
            Planets.Clear();

            if (FlightGlobals.Bodies == null)
            {
                errorMessage = "FlightGlobals.Bodies is null.";
                return;
            }

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body.referenceBody != null && body.referenceBody.isStar)
                {
                    Planets.Add(body);
                }
            }

            if (Planets.Count > 0)
            {
                PopulateMoons();
                errorMessage = string.Empty;
            } else
            {
                errorMessage = "No planets found (how did you manage that?).";
            }
        }

        public void PopulateMoons()
        {
            Moons.Clear();
            if (SelectedBody != null)
            {
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    if (body.referenceBody == SelectedBody)
                    {
                        Moons.Add(body);
                    }
                }
            }
            SelectedMoon = (Moons.Count > 0) ? Moons[0] : null;
        }

        public CelestialBody GetTargetBody()
        {
            return IsMoon ? SelectedMoon : SelectedBody;
        }
    }
}