# Mechanical-Lab_UnityVR
Experience the welding precess in Unity VR
VR Welding Game

Name -> Kishore Kumar P

A fully interactive VR welding simulation built in Unity, emphasizing hands-on skill, feedback, and fun. This project is ideal for education, training, or personal learning—supporting both VR hardware and desktop testing via Unity’s Device Simulator.

Project Description
This project is a Unity-based VR simulation designed to replicate the core experience of manual arc welding, closely following the techniques and feedback style seen in the provided reference video. Using the XR Interaction Toolkit, interactive UI, and responsive visual/audio feedback, players can perform realistic welding tasks with accurate bead placement, heat control, and session scoring, either in full VR or via Device Simulator (for desktop play/testing).

Running & Testing
VR Headset Optimized For:
Meta Quest 2/3 (Android APK build)
Oculus Rift S/Quest Link, Valve Index, HTC Vive (PC build, OpenXR/SteamVR/Oculus Desktop)

Desktop Simulation (No Headset):
Fully playable with Unity’s XR Device Simulator—open the main scene and press Play (keyboard/mouse/touch supported).

To Run:

For PC builds, run the provided .exe file with a connected VR headset.

For Quest/Android builds, sideload the APK file using SideQuest or adb, then launch from your headset.

If working in Unity Editor, open the main scene and use Device Simulator for non-VR testing.

Approach to Replicating the Experiment
To match the welding procedure shown in the reference video, I:

Implemented a realistic welding gun with trigger and grip-based interaction, using VR controller inputs to simulate metalworking ergonomics and arc control.

Built a dynamic system for bead placement: Each bead’s position and material are determined by current tip distance and “temperature” (display value), visually mirroring the reference’s feedback for arc quality.

Added accurate weld-state visuals, including base colors and emission effects, so heating, cooling, and overheating are clear to the player during welding.

Developed a smart scoring system that requires the player to cover the entire seam with consistent, properly sized/colored beads to achieve “Good” or “Excellent”—just as in the instructional video.

Integrated on-device feedback: Visual score popups, audio cues, and safety interlocks for gas, matching the reference’s step-by-step instructional style.

This approach ensures that users experience a learning curve and result assessment closely aligned with the demonstration in the original video.

Credits:
Voice Over done by Presith.R.V