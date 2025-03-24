# Resonant Orbit Calculator

Resonant Orbit Calculator is a mod for KSP that is designed to help calculate resonant target orbits for satellite constellations. It provides a tool for determining the required orbital paramters for a resonant orbit.

- **Orbit Calculation** Computes paramaters for satellite constellations
- **Dynamic Body Selection** Allows you to choose whatever planet or moon, no matter the star system.

---

## Installation

### Prerequisites
- **Kerbal Space Program**: Ensure the game is installed.

### Steps
1. Download the latest release of **Resonant Orbit Calculator** from the [Releases page](https://github.com/BananaMan9572/ResonantOrbitCalculator/releases).
2. Extract the contents of the ZIP files.
3. Copy the `ResonantMod` folder into your KSP's `GameData` directory.
4. Launch KSP and the mod will be available in the flight scene.
5. 
## Usage
1. **Open the Mod**:
- In the KSP flight scene, click the mod's button in the application launcher toolbar *(looks like a planet with a constellation orbiting around it)*.


<div align="center">
  <img src="https://github.com/user-attachments/assets/87614a2b-8aca-42e2-bd34-c55e6171c9ce" alt="Icon Screenshot" width="64">
</div>


2. **Set Parameters**:
- Select a planet or moon.
- Enter the desired altitude (in kilometers) and number of satellites.

3. **Calculate**:
- Click the **Calculate** button to view the results:
  - **Periapsis**: The lowest point of the orbit.
  - **Apoapsis**: The highest point of the orbit.
  - **Injection ΔV**: The delta-V required to achieve the orbit.

---

## What it should look like _(as of v1.0)_

![example](https://github.com/user-attachments/assets/04335942-e753-4f72-a3c7-308dab594118)

---

## Contributing
Contributions are asked for! Here's how you can help:
1. **Reporting Issues**: If you find a bug or want a new feature, open an issue on the [Issues page](https://github.com/BananaMan9572/ResonantOrbitCalculator/issues)
2. **Submit Pull Requests**: Fork the repository, make changes, and submit a pull request.
3. **Share Feedback**: Let me know how I can improve the mod.

---

## Building from Source

If you want to build the mod from source, follow these steps:

### Prerequisites
- **Visual Studio 2022**: Download and install [Visual Studio](https://visualstudio.microsoft.com/).
- **.NET Framework**: Ensure you have the .NET Framework installed.
- **KSP Assembly References**: You’ll need references to KSP’s core assemblies. These can be found in your KSP installation directory under `KSP_Data/Managed/`.

### Steps
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/BananaMan9572/ResonantOrbitCalculator.git
   cd ResonantMod
   ```
2. Open the `ResonantMod.sln` file in Visual Studio.
3. Add references to the required assemblies:
   - `Assembly-CSharp.dll`
   - `UnityEngine.dll`
   - `UnityEngine.AnimationModule.dll`
   - `UnityEngine.CoreModule.dll`
   - `UnityEngine.IMGUIModule.dll`
4. Build the Project

--- 

# License
Resonant Orbit Calculator is licensed under the **MIT License.** See the [LICENSE](https://github.com/BananaMan9572/ResonantOrbitCalculator/blob/master/LICENSE.txt) file for details.

---

# Credits

- Developer: Matthew Muth (Banana_Man)


