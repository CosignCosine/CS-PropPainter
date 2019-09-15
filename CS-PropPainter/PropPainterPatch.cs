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
            var GetColorOriginal = typeof(PropInfo).GetMethod("GetColor", new Type[] {typeof(ColossalFramework.Math.Randomizer)});
            var GetColorFix = typeof(PropPainterGetColorFix).GetMethod("GetColor");

            for (int i = 0; i < codes.Count; i++ ){
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand == GetColorOriginal)
                    {
                        Debug.Log("Found the correct CodeInstruction");
                        int FIRST = i - 2;

                        /** Original IL [Harmony]:
                         * L_00bc: ldloc.0
                         * L_00bd: ldloca.s 3 (ColossalFramework.Math.Randomizer)
                         * L_00bf: callvirt Color GetColor(Randomizer ByRef)
                         * L_00c4: stloc.s 5 (UnityEngine.Color)
                         */

                        /** Modified IL [dnSpy]:
                         * IL_00A4: ldarg.0
                         * IL_00A5: ldloc.0
                         * IL_00A6: ldarg.2
                         * IL_00A7: ldloca.s randomizer [3]
                         * IL_00A9: call instance valuetype[UnityEngine]UnityEngine.Color global::GetColorFix(class PropInfo, uint16, valuetype[ColossalManaged] ColossalFramework.Math.Randomizer&)
                         * IL_00AE: stloc.s color [5]
                        */

                        // IL_00A9: call instance valuetype [etc...] *
                        codes[i] = new CodeInstruction(codes[i])
                        {
                            opcode = OpCodes.Call,
                            operand = GetColorFix
                        };

                        // IL_00A4: ldarg.0 [FIRST] *
                        // IL_00A5: ldloc.0 (moved to second position)
                        // [...]
                        // IL_00A9: call instance valuetype [etc...]
                        codes.Insert(FIRST, new CodeInstruction(codes[FIRST])
                        {
                            opcode = OpCodes.Ldarg_0
                        });

                        // IL_00A4: ldarg.0 [FIRST]
                        // IL_00A5: ldloc.0
                        // IL_00A6: ldarg.2 *
                        // [...]
                        // IL_00A9: call instance valuetype [etc...]
                        codes.Insert(FIRST + 2, new CodeInstruction(codes[FIRST])
                        {
                            opcode = OpCodes.Ldarg_2
                        });

                        // IL_00A4: ldarg.0 [FIRST]
                        // IL_00A5: ldloc.0
                        // IL_00A6: ldarg.2
                        // IL_00A7: ldloca.s  randomizer [3] *
                        // IL_00A9: call instance valuetype [etc...]
                        codes.Insert(FIRST + 3, new CodeInstruction(codes[FIRST])
                        {
                            opcode = OpCodes.Ldloca_S,
                            operand = 3
                        });
                    }
                }
            }
            /*
            CodeInstruction toChange = codes.FirstOrDefault(instruction => (instruction.operand as String).Contains("GetColor"));

            if(toChange != null){
                int index = codes.IndexOf(toChange);
                Debug.Log(codes[index].operand);
            }*/
            
            return codes.AsEnumerable();
        }
    }
}
