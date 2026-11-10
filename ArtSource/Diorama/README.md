# Diorama Art Source

This folder keeps the source references and model handoff notes outside Unity's imported asset tree.

## Source assets

- `images/`: scene and individual landmark reference images. The individual images guide each model; `reference_scene_full.jpg` guides the overall diorama composition.
- `models_raw/`: reserved for editable source files and intermediate exports.
- Repository root: the five delivered GLB source models remain here as the original files.

## Unity runtime assets

The five supplied GLBs are copied byte-for-byte into:

`GameClient/Assets/Resources/Models/Diorama/`

They use the `.bytes` extension so Unity imports them as `TextAsset` and the home scene can load them through glTFast at runtime.

| Source GLB | Runtime resource | Home landmark |
|---|---|---|
| `01_Guild_Castle.glb` | `diorama_guild.bytes` | Guild castle |
| `02_Arena_Banner.glb` | `diorama_arena.bytes` | Arena banner |
| `03_Beast_Banner.glb` | `diorama_beast.bytes` | Beast banner |
| `04_Continent_Portal.glb` | `diorama_continent.bytes` | Continent portal |
| `05_Summon_Altar.glb` | `diorama_summon.bytes` | Summon altar |

The GLBs are authored with their vertical axis along +Z. `DioramaGlbRuntimeLoader` converts them to Unity's +Y-up orientation, fits each model to its landmark, and replaces the temporary fallback only after a successful import. Each supplied model contains its own base, so the temporary display pedestal is removed only after the GLB is ready.

Do not rename the resource files or change the `diorama_<landmark-id>` naming convention without updating the loader and the bindings in `DioramaHomeBuilder`. Keep the current procedural/FBX fallback so Home still opens when a model cannot load.
