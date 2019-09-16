using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;


namespace PropPainter
{
    [HarmonyPatch(typeof(PropInstance), "RenderInstance", new Type[] {typeof(RenderManager.CameraInfo), typeof(ushort), typeof(int)})]
    public static class PropPainterPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            var GetColorOriginal = typeof(PropInfo).GetMethod("GetColor", new Type[] {typeof(ColossalFramework.Math.Randomizer).MakeByRefType()});
            Debug.Log(GetColorOriginal);
            var GetColorFix = typeof(PropPainterGetColorFix).GetMethod("GetColor");
            var foundInstruction = false;

            var fixedInstructions = new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloca_S, 3),
                new CodeInstruction(OpCodes.Callvirt, GetColorFix),
                new CodeInstruction(OpCodes.Stloc_S, 5)
            };

            for (int i = 0; i < codes.Count; i++){

                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand == GetColorOriginal)
                    {
                        Debug.Log("Found GetColor instruction.");
                        foundInstruction = true;

                        /** Original IL [Harmony]:
                         * L_00bc: ldloc.0
                         * L_00bd: ldloca.s 3 (ColossalFramework.Math.Randomizer)
                         * L_00bf: callvirt Color GetColor(Randomizer ByRef)   [We target this line, so the first line is 2 before.]
                         * L_00c4: stloc.s 5 (UnityEngine.Color)
                         */

                        int FIRST = i - 2;

                        codes.RemoveRange(FIRST, 4);

                        Debug.Log("Deleted instructions.");

                        /** Modified IL [dnSpy]:
                         * IL_00A4: ldarg.0
                         * IL_00A5: ldloc.0
                         * IL_00A6: ldarg.2
                         * IL_00A7: ldloca.s randomizer [3]
                         * IL_00A9: call instance valuetype[UnityEngine]UnityEngine.Color global::GetColorFix(class PropInfo, uint16, valuetype[ColossalManaged] ColossalFramework.Math.Randomizer&)
                         * IL_00AE: stloc.s color [5]
                        */

                        codes.InsertRange(FIRST, fixedInstructions);

                        Debug.Log("Added new instructions.");
                        break;
                    }
                }
            }

            if (!foundInstruction) Debug.LogError("Did not find CodeInstruction, GetColor not patched.");
            return codes.AsEnumerable();
        }
    }
}
