using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using ColossalFramework.UI;


namespace PropPainter
{
    [HarmonyPatch(typeof(PropInstance), "RenderInstance", new Type[] {typeof(RenderManager.CameraInfo), typeof(ushort), typeof(int)})]
    public static class PropPainterPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            var GetColorOriginal = typeof(PropInfo).GetMethod("GetColor", new Type[] {typeof(ColossalFramework.Math.Randomizer).MakeByRefType()});
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

            if(GetColorOriginal == null){
                Db.e("Could not bind original GetColor. Aborting transpiler.");
                return codes.AsEnumerable();
            }

            if(GetColorFix == null){
                Db.e("Could not bind custom GetColorFix. Aborting transpiler.");
                return codes.AsEnumerable();
            }

            for (int i = 0; i < codes.Count; i++){

                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand == GetColorOriginal)
                    {
                        foundInstruction = true;

                        /** Original IL [Harmony]:
                         * L_00bc: ldloc.0
                         * L_00bd: ldloca.s 3 (ColossalFramework.Math.Randomizer)
                         * L_00bf: callvirt Color GetColor(Randomizer ByRef)   [We target this line, so the first line is 2 before.]
                         * L_00c4: stloc.s 5 (UnityEngine.Color)
                         */

                        int FIRST = i - 2;

                        codes.RemoveRange(FIRST, 4);

                        /** Modified IL [dnSpy]:
                         * IL_00A4: ldarg.0
                         * IL_00A5: ldloc.0
                         * IL_00A6: ldarg.2
                         * IL_00A7: ldloca.s randomizer [3]
                         * IL_00A9: call instance valuetype[UnityEngine]UnityEngine.Color global::GetColorFix(class PropInfo, uint16, valuetype[ColossalManaged] ColossalFramework.Math.Randomizer&)
                         * IL_00AE: stloc.s color [5]
                        */

                        codes.InsertRange(FIRST, fixedInstructions);
                        break;
                    }
                }
            }

            if (!foundInstruction) Debug.LogError("Did not find CodeInstruction, GetColor not patched.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    public static class PropPainterInstallationPatch{
        static MethodBase TargetMethod(){
            var t = Type.GetType("MoveIt.MoveItTool, MoveIt");
            Debug.Log(t);
            var x = t.GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);;
            Debug.Log(x);
            return x;
        }

        public static void Postfix(){
            //UIToolOptionPanel.instance

            var t = Type.GetType("MoveIt.UIToolOptionPanel, MoveIt");
            var UIToolOptionPanel = (t.GetField("instance").GetValue(null));
            Db.l(UIToolOptionPanel == null);
            if (UIToolOptionPanel == null || PropPainterManager.instance.colorField != null) return;

            //UIComponent UIToolOptionPanel = UIView.GetAView().FindUIComponent("MoveIt_ToolOptionPanel");
            PropPainterManager.instance.colorField = CreateColorField((UIComponent) UIToolOptionPanel, "PropPainterCF");
        }

        // The general idea for this mod is more or less stolen from TPB's Painter mod, even down to the name.
        // However, this bit of code is literally stolen from him. So. Yeah. Thanks for open-sourcing your code.
        private static UIColorField cfT;

        private static UIColorField CreateColorField(UIComponent parent, string name)
        {
            if (cfT == null)
            {
                UIComponent template = UITemplateManager.Get("LineTemplate");
                if (template == null) return null;

                cfT = template.Find<UIColorField>("LineColor");
                if (cfT == null) return null;
            }
            UIColorField cF = UnityEngine.Object.Instantiate(cfT.gameObject).GetComponent<UIColorField>();
            parent.AttachUIComponent(cF.gameObject);
            cF.name = name;
            cF.AlignTo(parent, UIAlignAnchor.TopRight);
            cF.relativePosition += new Vector3(-30f, 0f, 0f);
            cF.size = new Vector2(26f, 26f);
            cF.pickerPosition = UIColorField.ColorPickerPosition.LeftAbove;
            cF.eventSelectedColorChanged += ChangeSelectionColors;
            /*cF.eventColorPickerOpen += (a, b, ref c) => {
                
            };*/
            return cF;
        }

        private static void ChangeSelectionColors(UIComponent picker, Color color)
        {
            Debug.Log("Color changed to (" + color.ToString() + ")");
        }

        private static void AcquireColor()
        {

        }
    }
}
