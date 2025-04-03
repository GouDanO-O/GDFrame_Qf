#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using System.Reflection;
    using Utilities;
    using Sirenix.Utilities.Editor;

#endif

    // Example demonstrating how reflection can be used to enhance custom drawers.
    [TypeInfoBox(
        "This example demonstrates how reflection can be used to extend drawers from what otherwise would be possible.\n\n" +
        "In this case, a user can specify one of their own methods to receive a callback from the drawer chain.\n\n" +
        "Note that this is a manual approach; it is recommended to use ValueResolver<T> and ActionResolver instead.")]
    public class ReflectionExample : MonoBehaviour
    {
        [OnClickMethod("OnClick")] public int InstanceMethod;

        [OnClickMethod("StaticOnClick")] public int StaticMethod;

        [OnClickMethod("InvalidOnClick")] public int InvalidMethod;

        private void OnClick()
        {
            Debug.Log("Hello?");
        }

        private static void StaticOnClick()
        {
            Debug.Log("Static Hello?");
        }
    }

    // Attribute with name of call back method.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OnClickMethodAttribute : Attribute
    {
        public string MethodName { get; private set; }

        public OnClickMethodAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }

#if UNITY_EDITOR

    // Place the drawer script file in an Editor folder.
    // Remember to add the OdinDrawer to your custom drawer classes, or they will not be found by Odin.
    public class OnClickMethodAttributeDrawer : OdinAttributeDrawer<OnClickMethodAttribute>
    {
        // This field is used to display errors messages to the user, if something goes wrong.
        private string ErrorMessage;

        // Reference to the method specified by the user in the attribute.
        private MethodInfo Method;

        protected override void Initialize()
        {
            // Use reflection to find the specified method, and store the method info in the context object.
            Method = Property.ParentType.GetMethod(Attribute.MethodName, Flags.StaticInstanceAnyVisibility, null,
                Type.EmptyTypes, null);

            if (Method == null)
                ErrorMessage = "Could not find a parameterless method named '" + Attribute.MethodName +
                               "' in the type '" + Property.ParentType + "'.";
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Display any error that might have occured.
            if (ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(ErrorMessage);

                // Continue drawing the rest of the property as normal.
                CallNextDrawer(label);
            }
            else
            {
                // Get the mouse down event.
                var clicked = Event.current.rawType == EventType.MouseDown && Event.current.button == 0 &&
                              Property.LastDrawnValueRect.Contains(Event.current.mousePosition);

                if (clicked)
                {
                    // Invoke the method stored in the context object.
                    if (Method.IsStatic)
                        Method.Invoke(null, null);
                    else
                        Method.Invoke(Property.ParentValues[0], null);
                }

                // Draw the property.
                CallNextDrawer(label);

                if (clicked)
                    // If the event havn't been used yet, then use it here.
                    Event.current.Use();
            }
        }
    }

#endif
}
#endif