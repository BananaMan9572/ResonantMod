using UnityEngine;
using KSP.IO;
using System;
using System.Collections.Generic;

namespace ResonantMod.GhostMarker {
    internal class GhostShipManager {
        public bool CreateGhostShip(Orbit targetOrbit, out string errorMessage) {
            errorMessage = string.Empty; 
            try {
                // Try to get the InvisiblePart
                AvailablePart getPart = PartLoader.getPartInfoByName("strutConnector");

                if(getPart == null) {
                    Debug.LogError("InvisiblePart not found!");
                    errorMessage = "InvisiblePart not found!";
                    return false;
                }

                if(getPart.partPrefab == null) {
                    Debug.LogError("getPart.partPrefab is null!");
                    errorMessage = "Part prefab is null!";
                    return false;
                }

                Part invisiblePart = UnityEngine.Object.Instantiate(getPart.partPrefab) as Part;
                UnityEngine.Object.DontDestroyOnLoad(invisiblePart);

                if(invisiblePart == null) {
                    Debug.LogError("InvisiblePart instantiation failed!");
                    errorMessage = "InvisiblePart instantiation failed!";
                    return false;
                }

                // Create the ghost ship
                GameObject ghostObject = new GameObject("GhostShip");
                Vessel vessel = ghostObject.AddComponent<Vessel>();

                vessel.parts = new List<Part>();
                vessel.parts.Add(invisiblePart);
                invisiblePart.vessel = vessel;
                vessel.rootPart = invisiblePart;
                vessel.SetReferenceTransform(invisiblePart);
                invisiblePart.transform.parent = vessel.transform;

                if(vessel.rootPart == null) {
                    Debug.LogError("Vessel rootPart is null!");
                    errorMessage = "Vessel rootPart is null!";
                    return false;
                }

                // Set the orbit by adding an orbitDriver
                vessel.orbitDriver = ghostObject.AddComponent<OrbitDriver>();
                vessel.orbitDriver.orbit = targetOrbit;
                vessel.orbitDriver.updateFromParameters();

                if(vessel.orbitDriver == null) {
                    Debug.LogError("Vessel orbitDriver is null!");
                    errorMessage = "Vessel orbitDriver is null!";
                    return false;
                }

                if(targetOrbit == null) {
                    Debug.LogError("targetOrbit is null!");
                    errorMessage = "Target orbit is null!";
                    return false;
                }

                // Set the vesselType and situation and uncontrollable
                vessel.vesselType = VesselType.Probe;
                vessel.situation = Vessel.Situations.ORBITING;
                vessel.DiscoveryInfo.SetLevel(DiscoveryLevels.Owned);
                vessel.DiscoveryInfo.SetLastObservedTime(Planetarium.GetUniversalTime());

                FlightGlobals.Vessels.Add(vessel);
                GameEvents.onVesselCreate.Fire(vessel);

                // Position the ghost ship, set the velocity and update the orbit
                Vector3d position = targetOrbit.getPositionAtUT(Planetarium.GetUniversalTime());
                vessel.SetPosition(position);

                Vector3d velocity = targetOrbit.getOrbitalVelocityAtUT(Planetarium.GetUniversalTime());
                vessel.SetWorldVelocity(velocity);

                vessel.orbitDriver.updateFromParameters();

                vessel.protoVessel = null;
                vessel.id = Guid.NewGuid();
                vessel.GoOnRails();
                vessel.Landed = false;

                GameEvents.onVesselChange.Add(this.PreventSwitch);

            } catch(Exception e) {
                errorMessage = string.IsNullOrEmpty(errorMessage) ? e.Message : errorMessage;
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        public void PreventSwitch(Vessel attemptedSwitch) {
            if(attemptedSwitch != null && attemptedSwitch.vesselType == VesselType.Unknown && attemptedSwitch.rootPart?.partInfo?.name == "strutConnector") {
                if(FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel != attemptedSwitch) {
                    FlightGlobals.ForceSetActiveVessel(FlightGlobals.ActiveVessel);
                }
            }
        }
    }
}
