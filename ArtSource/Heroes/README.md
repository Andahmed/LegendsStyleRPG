# Hero Art Source

This folder stores original hero character source images and intermediate 3D exports outside Unity's imported asset tree.

## Folders

- `images/`: original source images intended for conversion to 3D in Meshy. The references use an A-pose, have no weapon, and use a plain background. These source images are art references only and must not be placed inside Unity's `GameClient/Assets` import tree.
- `models_raw/`: raw FBX/GLB files and intermediate exports before rigging and final Unity export.
- `GameClient/Assets/Resources/Models/Heroes/`: final rigged Unity-ready FBX files, named `hero_<id>.fbx`.

## Hero IDs

Current IDs:

- `warrior`
- `paladin`
- `berserker`
- `sharpshooter`
- `healer`

Planned for later:

- `mage`
- `ranger`
- `rogue`
- `samurai`

## Original artwork and licensing

The source images in this collection are original generated artwork and are not taken from any existing game. The project owner is responsible for checking the commercial-use license and terms of any tools or services used to generate, convert, rig, or otherwise process these assets.
