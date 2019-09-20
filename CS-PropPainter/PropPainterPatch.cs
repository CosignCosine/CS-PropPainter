﻿using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using ColossalFramework.UI;
using MoveIt;


namespace PropPainter
{
    [HarmonyPatch(typeof(PropInstance), "RenderInstance", new Type[] { typeof(RenderManager.CameraInfo), typeof(ushort), typeof(int) })]
    public static class PropPainterPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            var GetColorOriginal = typeof(PropInfo).GetMethod("GetColor", new Type[] { typeof(ColossalFramework.Math.Randomizer).MakeByRefType() });
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

            if (GetColorOriginal == null)
            {
                Db.e("Could not bind original GetColor. Aborting transpiler.");
                return codes.AsEnumerable();
            }

            if (GetColorFix == null)
            {
                Db.e("Could not bind custom GetColorFix. Aborting transpiler.");
                return codes.AsEnumerable();
            }

            for (int i = 0; i < codes.Count; i++)
            {

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

            if (!foundInstruction) Db.e("Did not find CodeInstruction, GetColor not patched.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(UIToolOptionPanel), "Start")]
    public static class PropPainterInstallationPatch
    {
        public static void Postfix()
        {
            if (UIToolOptionPanel.instance == null || PropPainterManager.instance.colorField != null) return;

            CreateUI((UIComponent)UIToolOptionPanel.instance, "PropPainterCF");
        }

        // The general idea for this mod is more or less stolen from TPB's Painter mod, even down to the name.
        // However, this bit of code is literally stolen from him. So. Yeah. Thanks for open-sourcing your code.
        public static bool doNotUpdateColors = false;

        // Updated code thanks to TPB- @TODO implement! 
        private static void CreateUI(UIComponent parent, string name)
        {
            UIColorField field = UITemplateManager.Get<UIPanel>("LineTemplate").Find<UIColorField>("LineColor");
            field = UnityEngine.Object.Instantiate<UIColorField>(field);
            field.isVisible = false;
            field.name = "PropPickerColorField";
            UIColorPicker picker = UnityEngine.Object.Instantiate<UIColorPicker>(field.colorPicker);
            picker.eventColorUpdated += ChangeSelectionColors;
            picker.color = Color.white;
            picker.component.color = Color.white;
            picker.name = name;
            UIPanel pickerPanel = picker.component as UIPanel;
            pickerPanel.backgroundSprite = "";
            pickerPanel.isVisible = false;
            picker.component.size = new Vector2(254f, 217f); // ?/
            parent.AttachUIComponent(picker.gameObject);

            UIMultiStateButton propPickerButton = parent.AddUIComponent<UIMultiStateButton>();
            propPickerButton.name = name + "button";

            propPickerButton.backgroundSprites.AddState();

            propPickerButton.backgroundSprites[0].normal = "OptionBase";
            propPickerButton.backgroundSprites[0].focused = "OptionBase";
            propPickerButton.backgroundSprites[0].hovered = "OptionBaseHovered";
            propPickerButton.backgroundSprites[0].pressed = "OptionBasePressed";
            propPickerButton.backgroundSprites[0].disabled = "OptionBaseDisabled";

            propPickerButton.foregroundSprites[0].normal = "EyeDropper";

            propPickerButton.backgroundSprites.AddState();

            propPickerButton.backgroundSprites[1].normal = "OptionBaseFocused";
            propPickerButton.backgroundSprites[1].focused = "OptionBaseFocused";
            propPickerButton.backgroundSprites[1].hovered = "OptionBaseHovered";
            propPickerButton.backgroundSprites[1].pressed = "OptionBasePressed";
            propPickerButton.backgroundSprites[1].disabled = "OptionBaseDisabled";

            propPickerButton.foregroundSprites[1].normal = "EyeDropper";

            propPickerButton.size = new Vector2(36, 36);
            propPickerButton.activeStateIndex = 0;

            FieldInfo f = typeof(UIToolOptionPanel).GetField("m_alignTools", BindingFlags.Instance | BindingFlags.NonPublic);
            UIButton AlignTools = f.GetValue(UIToolOptionPanel.instance) as UIButton;
            propPickerButton.absolutePosition = AlignTools.absolutePosition + new Vector3(AlignTools.width, 0);

            var GetIconsAtlas = typeof(UIToolOptionPanel).GetMethod("GetIconsAtlas", BindingFlags.Instance | BindingFlags.NonPublic);
            propPickerButton.atlas = GetIconsAtlas.Invoke(UIToolOptionPanel.instance, new object[] { }) as UITextureAtlas;

            propPickerButton.eventClick += (component, eventParam) => {
                if(propPickerButton.activeStateIndex == 0){
                    propPickerButton.activeStateIndex = 1;
                }else{
                    propPickerButton.activeStateIndex = 0;
                }
                pickerPanel.isVisible = propPickerButton.activeStateIndex == 1;
            };


            PropPainterManager.instance.colorField = field;
            PropPainterManager.instance.colorPicker = picker;
            PropPainterManager.instance.propPainterButton = propPickerButton;
        }

        private static void ChangeSelectionColors(Color color)
        {
            if (doNotUpdateColors){
                doNotUpdateColors = false;
                return;
            }
            Db.l("Color of selected objects changed to (" + color.ToString() + ")");
            List<ushort> props = PropPainterManager.instance.ExtractPropsFromMoveItSelection();
            for (int i = 0; i < props.Count; i++){
                PropPainterManager.instance.SetColor(props[i], color);
            }
        }
    }

    [HarmonyPatch(typeof(SelectAction), "Add")]
    public static class PropPainterMoveItSelectionBinderPatch{

        private static void Postfix()
        {
            List<ushort> t = PropPainterManager.instance.ExtractPropsFromMoveItSelection();
            for (int i = 0; i < t.Count; i++){
                Db.w((i + 1) + ": " + t[i]);
            }
            Color? r = PropPainterManager.instance.ParseAggregateColor(t);
            Color trueColor = new Color32(255, 255, 255, 255);
            if (r != null) trueColor = (Color) r;

            PropPainterInstallationPatch.doNotUpdateColors = true;
            PropPainterManager.instance.colorField.selectedColor = trueColor;
            PropPainterManager.instance.propPainterButton.color = trueColor;
        }
    }
}
