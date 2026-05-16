
# MaterialAssetRestorerCore
Moon and interior devs are likely familiar with how poorly the certain assets rip when trying to design their levels. I made this tool to attempt to restore some at runtime. It can access materials both from within the vanilla prefabs and scenes. ([My thread in the LC Modding Discord](https://discord.com/channels/1168655651455639582/1245084720614604873))

This mod works by searching through a moon after it is loaded to replace the broken ones from modded moons/interiors with the corresponding one from vanilla. API specs and example useage below.

NOTE: IN ORDER TO ACCESS VANILLA MATERIALS FROM SCENES, THEY ARE BRIEFLY LOADED DURING `StartOfRound`. FROM MY TESTING THIS DID NOT CAUSE MAJOR PERMANENT ISSUES, BUT A FEW ERRORS LIKE `[Error  : Unity Log] Cascade Shadow atlasing has failed, only one directional light can cast shadows at a time` AND OTHER WARNINGS MAY BRIEFLY LOG. AGAIN, I DO NOT BELIEVE THIS CAUSES ANY MAJOR ISSUES LONG TERM.

## API Docs For Devs

Create a json file (or multiple) with the format and in the direcotry shown below. Each json contains a list of `MaterialSets`. A `MaterialSet` is made up of a `BaseMaterial`, which is the name of the material in the vanilla game, and a `ReplaceMaterial`, which is the name of the material in your mod. Often these will likely be the same name, but they don't have to be. You must also specify either a `PrefabName` or a `SceneName` so that this tool knows where to look to find the `BaseMaterial`. 
```
{
    "MaterialSets": [
        {
            "BaseMaterial" : "gamematerial",
            "ReplaceMaterial" : "yourmaterial",
            "PrefabName" : "gameprefab"
        },
        {
            "BaseMaterial" : "gamematerial",
            "ReplaceMaterial" : "yourmaterial",
            "SceneName" : "gamescene"
        }
    ]
}
```

Project Heirarchy (NOTE: YOU MUST PLACE THE JSON FILE IN THE `plugins/MaterialReplacements/` FOLDER FOR THIS TO WORK):
```
YourMod/
├── plugins/
│   └── MaterialReplacements/
|       └── YourJSON.json
├── README.md
├── manifest.json
├── icon.png
├── CHANGELOG.md (optional)
└── LICENSE.md (optional)

```

## Example Usecase

[WaterAssetRestorer](https://thunderstore.io/c/lethal-company/p/Sniper1_1/WaterAssetRestorer/)

### Before 

![Without](https://imgur.com/Z06aUeJ.png)


### After 

![with](https://imgur.com/FLS6tVR.png)

## Credits

- The developers of this mod's dependencies as it literally could not exist without them.
- [Audio Knight](https://www.youtube.com/@knightofaudio) on YouTube for a handy starting tutorial.
- [Nomnomab's project patcher](https://github.com/nomnomab/lc-project-patcher) to access vanilla LC through Unity.
- Those that helped provide information on the modding Discord, mainly through the [Dev-general channel on Discord](https://discord.com/channels/1168655651455639582/1168656318345777313).
- Debugging tools like [Imperium](https://thunderstore.io/c/lethal-company/p/giosuel/Imperium/) and [UnityExplorer](https://thunderstore.io/c/lethal-company/p/LethalCompanyModding/Yukieji_UnityExplorer/).
- Xilophor's [modding template](https://github.com/Xilophor/Lethal-Company-Mod-Templates) for a handy way to start.
- Thunderstore for hosting this mod as I wouldn't know how to distribute without it.