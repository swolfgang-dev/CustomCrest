# Custom Crest

Custom Crest is a BepInEx/Harmony mod for Hollow Knight: Silksong.

This is a clean rebuild scaffold. It currently loads, compiles, and exposes attack-source config entries.

## Direction

The equipped crest should continue to control tool slots and binding type. This mod will layer attack-source overrides on top, so neutral, up, and down attacks can each copy behavior from a chosen crest regardless of the equipped crest.

## Config

Settings appear under `Custom Crest` in Configuration Manager.

Global settings apply to every save. Save-specific settings apply only to the active save through Save Scoped Config. A save-specific attack source overrides the global source when it is not `SelectedCrest`.

- `Global: Attack Sources > Neutral Attack`: source crest for neutral attacks. `SelectedCrest` keeps vanilla behavior.
- `Global: Attack Sources > Up Attack`: source crest for up attacks. `SelectedCrest` keeps vanilla behavior.
- `Global: Attack Sources > Down Attack`: source crest for down attacks. `SelectedCrest` keeps vanilla behavior.
- `Save X: Attack Sources > Neutral Attack`: source crest for neutral attacks on the active save.
- `Save X: Attack Sources > Up Attack`: source crest for up attacks on the active save.
- `Save X: Attack Sources > Down Attack`: source crest for down attacks on the active save.

## Build

From the game directory:

```powershell
powershell.exe -ExecutionPolicy Bypass -File .\ModDev\CustomCrest\build.ps1
```

The default build target is development mode:

```text
BepInEx/scripts/CustomCrest.dll
```

For a normal BepInEx plugin install:

```powershell
powershell.exe -ExecutionPolicy Bypass -File .\ModDev\CustomCrest\build.ps1 -Target Plugin
```
