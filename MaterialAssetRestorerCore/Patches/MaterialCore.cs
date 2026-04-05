using HarmonyLib;
using UnityEngine;


namespace MaterialAssetRestorerCore.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class MaterialInit
    {
        public static Material WAR_company_flooded = null;
        public static Material WAR_cave = null;
        public static Material WAR_pool = null;
        public static Material WAR_adamance_march_vow = null;

        //after StartOfRound, initialize the materials
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void InitializeWaterMaterials()
        {
            MaterialAssetRestorerCore.Logger.LogInfo("Initializing water materials...");
            WAR_company_flooded = MaterialGet.GET_material("Water_mat_04");
            WAR_cave = MaterialGet.GET_material("CaveWater", "CaveWaterTile");
            WAR_pool = MaterialGet.GET_material("PoolWater", "PoolTile");
            //WAR_adamance_march_vow = WaterGet.GET_material("VowWater");

        }                
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
    public class WaterSwap
    {
        [HarmonyPostfix]
        public static void SwapWaterMaterials()
        {
            MaterialAssetRestorerCore.Logger.LogInfo("Swapping water materials...");
            MaterialSet.SET_material("Flooded&GordionWater", MaterialInit.WAR_company_flooded);
            MaterialSet.SET_material("CaveWater", MaterialInit.WAR_cave);
            MaterialSet.SET_material("PoolWater", MaterialInit.WAR_pool);
            //MaterialSet.SET_material("VowWater", MaterialInit.WAR_adamance_march_vow);
        }
    }
}