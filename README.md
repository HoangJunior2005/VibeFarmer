# Thôn An Lúa

A Unity game project built with `Unity 2024.1.0f1`, featuring a Vietnamese-style farming village theme.

## Overview

The project includes the main scenes:
- `MainMenu.unity` — the game main menu
- `ShopScene.unity` — the shop scene for buying and selling items
- `TownScene.unity` — the main village area for interaction and gameplay

Key project features:
- A variety of assets: animations, models, audio, textures, UI, shaders, and terrain
- Universal Render Pipeline (URP) for improved visuals
- Cinemachine for camera control, Timeline for cutscenes, and Visual Scripting/Built-in scripting for gameplay

## Requirements

- Unity Editor: `2024.1.0f1`
- Universal Render Pipeline: `17.3.0`
- Other packages listed in `Packages/manifest.json`

## Project Structure

- `Assets/` — game assets and source content
- `Packages/manifest.json` — Unity package manifest
- `ProjectSettings/` — Unity project configuration files
- `Assembly-CSharp.csproj` / `Thôn An Lúa.slnx` — C# project and solution files for IDE support

## How to Open the Project

1. Open Unity Hub.
2. Click `Add` and select the project folder at `c:\Users\Administrator\Desktop\Thôn An Lúa`.
3. Open the project in Unity Editor using version `2024.1.0f1`.

## Development Notes

- Keep packages in sync with `Packages/manifest.json`.
- Test the `MainMenu`, `ShopScene`, and `TownScene` after gameplay updates.
- Back up `ProjectSettings` before changing graphics or build configuration.

## Notes

For sharing or continuing development, include `Assets/`, `Packages/`, and `ProjectSettings/` so Unity can restore the project configuration correctly. 
