# Mechanical-Lab_UnityVR
Experience the welding precess in Unity VR
VR Welding Game
A fully interactive VR welding simulation built in Unity, emphasizing hands-on skill, feedback, and fun. This project is ideal for education, training, or personal learning—supporting both VR hardware and desktop testing via Unity’s Device Simulator.

Overview
VR Welding Game is a Unity-based simulation that teaches the basics of manual arc welding through immersive, gamified practice:

Use a realistic welding gun to lay beads along marked seams on metal.

Control speed, distance, and stability—these actions affect bead quality and your score.

Dynamic visuals and feedback: Every bead you place changes its color and glow based on your welding quality and digital temperature display.

Playable with or without a VR headset! All mechanics can be tested with Unity’s Device Simulator (keyboard and mouse) if you don’t have VR hardware available.

Features
Welding Gun Interaction:

Grab the gun in VR, activate welding via controller (or simulated input).

See real-time sparks, audio, light, and bead placement as you move/trigger.

Smart Scoring System:

Evaluates bead quality and ensures you cover the whole seam—gaps or a single bead can’t get a good score!

Three result levels (“Excellent”, “Good”, “Bad”) with visual feedback and sound.

Weld Bead States:

Each weld bead gets a material/color based on your current simulated temperature (“moderate”, “optimal”, or “overheated”).

Teleportation Locomotion:

Industry-standard VR movement to safely navigate the workspace.

World-space UI:

Floating, VR-friendly UI cards present game rules and results in a modern onboarding/tutorial style.

Gas Safety Switch:

Welding is locked out unless the “gas” is switched on, with clear visual and UI feedback.

How to Build / Run
Clone or Download this repository.

Open in Unity (requires Unity 2021.3 LTS or newer; XR Interaction Toolkit package installed).

Load the Demo Scene (e.g., Assets/Scenes/Main.unity).

Set up XR Configuration:

For VR headset: Connect and configure via Unity XR settings (Oculus/SteamVR supported).

For desktop: Enable the XR Device Simulator (found in the XR Interaction Toolkit package).

Play the Scene:

Use VR controllers, or keyboard/mouse for Device Simulator.

Follow the onscreen instructions and UI panels for gameplay.

Controls (Default)
Grab Welding Gun: Press and hold controller grip (or mouse/keyboard in Device Simulator).

Weld: Press and hold controller trigger (or left mouse/keyboard).

Teleport: Use joystick/thumbstick (or simulated pointer for Device Simulator).

Toggle Gas: Interact with the tank handle or button.

Screenshots
(Add screenshots here! Use drag-and-drop in GitHub, or place .png/.jpg files in a /Screenshots folder and reference here for visual appeal.)

Requirements
Unity 2021.3 LTS (or newer)

XR Interaction Toolkit

Optional: VR headset (Oculus Rift/S, Quest Link, or SteamVR)

Credits
Programming & Design: [Your Name]

Assets: Unity primitives & custom materials for demo

XR Toolkit: Unity XR Interaction Toolkit

Special Thanks: [Any instructors, peers, or open-source contributors]

Notes
If you don’t have a VR headset, you can test the entire core gameplay with Unity’s Device Simulator.

Beads must be placed along the entire seam for a good score—this encourages careful and realistic practice.

Art assets and UI use basic/placeholder visuals for clarity and performance; feel free to customize for your needs.

License
This project was created for educational/non-commercial purposes.