
---

**<details><summary>Version 1.1.2</summary>**

 - Fixed `Cascade Shadow atlasing has failed, only one directional light can cast shadows at a time` error that was occuring when trying to load a scene (made lighting flash briefly) to cache materials. Thanks [Lunxara](https://www.twitch.tv/lunxara) for noting the epilepsy risk and [Pacoito](https://thunderstore.io/c/lethal-company/p/pacoito/) for suggesting a fix.
 
 </details>
 
---

**<details><summary>Version 1.1.1</summary>**

 - Fixed accidentally applying scene load/unload event patches early.
 
 </details>
 
---

**<details><summary>Version 1.1.0</summary>**

 - Lock ship lever while a player is caching materials.
 - Suppress scene load/unload events while caching incase other mods hook into these.
 - Slightly optimized scene searches by only searching targetted scene (ie don't also search ScampleSceneRelay, the ship scene, unless it is the target).
 - Switched to patch ship lever's Start() instead of StartOfRound.Start() to try to reduce the chances of errors from other mods breaking the process and keeping lever locked indefinitely.
 
 </details>
 
---

**<details><summary>Version 1.0.2</summary>**

 - Fixed an incompatability with [Wesley's Interiors](https://thunderstore.io/c/lethal-company/p/Magic_Wesley/WesleysInteriors/).
 - Still working on a solution to lock the ship lever until all materials are loaded for everyone. In the meantime, just wait a couple of seconds before pulling the lever after someone joins.
 
 </details>
 
---

**<details><summary>Version 1.0.1</summary>**

 - Fixed typo in README.
 
 </details>
 
---

**<details><summary>Version 1.0.0</summary>**

 - Initial release.
 
 </details>
 
---
