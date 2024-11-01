﻿using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using KKAPI.Utilities;
using Sirenix.Serialization.Utilities;
using UnityEngine;
using VRGIN.Core;

namespace KKS_VR.Fixes
{
    /// <summary>
    /// Avoid triggering resource unload when loading UI-only scenes.
    /// todo move into illusionfixes?
    /// </summary>
    [HarmonyPatch]
    public class ReduceAssetUnloads
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return CoroutineUtils.GetMoveNext(AccessTools.Method(typeof(Manager.Scene), nameof(Manager.Scene.LoadStart)));
        }

        private static AsyncOperation MaybeUnloadUnusedAssets()
        {
            var shouldUnload = Manager.Scene.IsFadeNow;
            if (shouldUnload)
            {
                return Resources.UnloadUnusedAssets();
            }
            else
            {
                VRLog.Info("Skipping unload");
                return null;
            }
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, MethodBase __originalMethod)
        {
            foreach (var inst in insts)
                if (inst.opcode == OpCodes.Call &&
                    inst.operand is MethodInfo method &&
                    method.Name == "UnloadUnusedAssets")
                {
                    yield return CodeInstruction.Call(() => MaybeUnloadUnusedAssets());
                    VRPlugin.Logger.LogDebug("Patched UnloadUnusedAssets in " + __originalMethod.GetFullName());
                }
                else
                {
                    yield return inst;
                }
        }
    }
}
