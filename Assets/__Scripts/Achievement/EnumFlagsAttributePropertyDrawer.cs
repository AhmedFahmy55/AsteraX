using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using System;



public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}

#if UNITY_EDITOR 
[CustomPropertyDrawer( typeof(EnumFlagsAttribute) )]
public class EnumFlagsAttributePropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        // Remove all and none from the list because Nothing and Everything already exist
        List<string> propsToShow = new List<string>(_property.enumNames);
        propsToShow.Remove("none"); // If "none" was in the List, it is removed.
        propsToShow.Remove("all");

        // Show the MaskField in the Inspector
        _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, propsToShow.ToArray());
    }
}
#endif