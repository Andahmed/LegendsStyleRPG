# Diorama Art Source

This folder keeps source artwork and references outside Unity's imported asset tree.

## Folders

- `images/`: visual reference images used to guide modeling. `reference_scene_full.jpg` is a scene-wide reference only; the individual landmark images are modeling references.
- `models_raw/`: original source files and working exports kept before final Unity export (for example, Meshy source downloads and intermediate formats).
- `GameClient/Assets/Resources/Models/Diorama/`: final Unity-ready FBX models and their required textures.

## Naming

Final FBX files use the pattern `diorama_<id>.fbx`, for example:

- `diorama_guild.fbx`
- `diorama_summon.fbx`
- `diorama_arena.fbx`

Keep each model's textures alongside its FBX in the Unity Resources folder. Do not put reference-only images in `GameClient/Assets`.
