﻿// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Editor.Misc;
using KINEMATION.KAnimationCore.Runtime.Rig;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.KAnimationCore.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(KRigElementChain))]
    public class ElementChainDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var rig = RigDrawerUtility.TryGetRigAsset(fieldInfo, property);

            var elementChain = property.FindPropertyRelative("elementChain");
            var chainName = property.FindPropertyRelative("chainName");
            var isStandalone = property.FindPropertyRelative("isStandalone");

            if (rig != null)
            {
                var rigHierarchy = rig.rigHierarchy;

                var labelWidth = EditorGUIUtility.labelWidth;
                float indentLevel = EditorGUI.indentLevel;

                var totalWidth = position.width - indentLevel - labelWidth;

                var propertyFieldRect = new Rect(position.x + indentLevel, position.y,
                    labelWidth, EditorGUIUtility.singleLineHeight);

                var buttonRect = new Rect(position.x + indentLevel + labelWidth, position.y,
                    totalWidth, EditorGUIUtility.singleLineHeight);

                if (isStandalone.boolValue)
                    buttonRect = position;
                else
                    chainName.stringValue = EditorGUI.TextField(propertyFieldRect, chainName.stringValue);

                if (GUI.Button(buttonRect, $"Edit {chainName.stringValue}"))
                {
                    var selectedIds = new List<int>();

                    // Get the active element indexes.
                    var arraySize = elementChain.arraySize;
                    for (var i = 0; i < arraySize; i++)
                    {
                        var indexProp
                            = elementChain.GetArrayElementAtIndex(i).FindPropertyRelative("index");
                        selectedIds.Add(indexProp.intValue + 1);
                    }

                    var elementNames = rigHierarchy.Select(element => element.name).ToList();
                    KSelectorWindow.ShowWindow(ref elementNames, ref rig.rigDepths,
                        (selectedName, selectedIndex) => { },
                        items =>
                        {
                            elementChain.ClearArray();

                            foreach (var selection in items)
                            {
                                elementChain.arraySize++;
                                var lastIndex = elementChain.arraySize - 1;

                                var element = elementChain.GetArrayElementAtIndex(lastIndex);
                                var name = element.FindPropertyRelative("name");
                                var index = element.FindPropertyRelative("index");

                                name.stringValue = selection.Item1;
                                index.intValue = selection.Item2;
                            }

                            property.serializedObject.ApplyModifiedProperties();
                        },
                        true, selectedIds, "Element Chain Selection"
                    );
                }
            }
            else
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
            }

            EditorGUI.EndProperty();
        }
    }
}