using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dawn.Internal;
using DunGen;
using HarmonyLib;
using UnityEngine;

namespace MaterialAssetRestorerCore
{
    internal static class LLLLeverPatch
    {
        //runs before LLL does its check for if the lever should be locked/unlocked. Once I'm done, LLL can do its checks
        [HarmonyPatch(typeof(LethalLevelLoader.Patches), nameof(LethalLevelLoader.Patches.CheckLever)), HarmonyPrefix]
        public static bool LeverPatch(InteractTrigger trigger)
        {
            MaterialAssetRestorerCore.Logger.LogWarning("Lever enabled: " + NetworkBool.materialsInitialized.Value);
            if (!NetworkBool.materialsInitialized.Value)
            {
                trigger.disabledHoverTip = "[ M.A.R.C. still caching materials!]";
                trigger.interactable = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    internal static class DawnLibLeverPatch
    {
        [HarmonyTargetMethod]
        public static MethodBase FindUnlockLever()
        {
            try
            {
                var allNested = typeof(DawnMoonNetworker).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance);

                var stateMachineType = allNested.FirstOrDefault(t => t.Name.Contains("UnlockLever")); //get UnlockLever from DawnLib's DawnMoonNetworker
                if (stateMachineType == null)
                {
                    MaterialAssetRestorerCore.Logger.LogError("Could not find UnlockLever state machine type!");
                    return null;
                }

                MaterialAssetRestorerCore.Logger.LogInfo($"Found the UnlockLever");
                return stateMachineType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance); //get MoveNext from the UnlockLever state machine, which is where the WaitUntil is
            }
            catch (Exception ex)
            {
                MaterialAssetRestorerCore.Logger.LogError($"Error in DawnLibLeverPatch.FindUnlockLeverInstruction(): {ex}");
                return null;
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectWaitInstruction(IEnumerable<CodeInstruction> instructions)
        {
            var waitUntilConstructor = typeof(WaitUntil).GetConstructor(new[] { typeof(Func<bool>) }); //get the constructor for WaitUntil call
            bool foundWaitUntil = false;

            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Newobj && instruction.operand as ConstructorInfo == waitUntilConstructor) //if the current instruction is the constructor for WaitUntil, inject own check for materialsInitialized
                {
                    MaterialAssetRestorerCore.Logger.LogInfo("Found WaitUntil in original UnlockLever(), injecting wait for materials.");
                    foundWaitUntil = true;
                    yield return new CodeInstruction(OpCodes.Call, typeof(DawnLibLeverPatch).GetMethod(nameof(WaitForMaterials), BindingFlags.Static | BindingFlags.Public));
                }
                yield return instruction; //keeps original instructions, including the original WaitUntil
            }
            if(!foundWaitUntil){ MaterialAssetRestorerCore.Logger.LogWarning("Never found WaitUntil constructor in MoveNext!"); }
        }

        public static Func<bool> WaitForMaterials(Func<bool> original)
        {
            return () => original() && NetworkBool.materialsInitialized.Value;
        }
    }
}
