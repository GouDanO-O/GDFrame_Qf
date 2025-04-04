﻿// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Attributes;
using KINEMATION.KAnimationCore.Runtime.Rig;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.KAnimationCore.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(ElementChainSelectorAttribute))]
    public class ElementChainSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var chainSelectorAttribute = attribute as ElementChainSelectorAttribute;

            if (chainSelectorAttribute == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var rig = (property.serializedObject.targetObject as IRigUser)?.GetRigAsset();
            var assetProp = property.serializedObject.FindProperty(chainSelectorAttribute.assetName);

            if (rig == null || assetProp != null)
            {
                if (assetProp == null) return;
                rig = assetProp.objectReferenceValue as KRig;
            }

            if (rig == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var options = new List<string> { "None" };
            var chainNames = rig.rigElementChains.Select(chain => chain.chainName).ToArray();
            options.AddRange(chainNames);

            var currentIndex = options.IndexOf(property.stringValue);
            currentIndex = EditorGUI.Popup(position, label.text, currentIndex, options.ToArray());
            var selection = currentIndex >= 0 ? options[currentIndex] : "None";

            property.stringValue = selection;
        }
    }
}