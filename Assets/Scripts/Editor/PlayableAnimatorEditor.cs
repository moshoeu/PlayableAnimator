using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CostumeAnimator
{
    [CustomEditor(typeof(AssetBlendTree))]
    class AssetBlendTree1DEditor : Editor
    {
        private AssetBlendTree m_Target;
        private SerializedProperty m_Motions;
        private SerializedProperty m_Param;
        private SerializedProperty m_Type;
        private SerializedProperty m_Name;

        private void OnEnable()
        {
            m_Target = target as AssetBlendTree;
            m_Motions = serializedObject.FindProperty("motions");
            m_Param = serializedObject.FindProperty("parameter");
            m_Type = serializedObject.FindProperty("blendTreeType");
            m_Name = serializedObject.FindProperty("stateName");

            m_Target.Sort();
            m_Target.SetBlendTreeType();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Type);
            EditorGUILayout.PropertyField(m_Name); 
            EditorGUILayout.PropertyField(m_Param);
            EditorGUILayout.PropertyField(m_Motions, true);

            serializedObject.ApplyModifiedProperties();
        }

    }

    [CustomPropertyDrawer(typeof(AssetStateController.Motion))]
    class BlendTreeDrawer : PropertyDrawer
    {
        static class Style
        {
            public static GUIContent clipContent = new GUIContent("Clip");
            public static GUIContent thresholdContent = new GUIContent("Threshold");
            public static GUIContent speedContent = new GUIContent("Speed");
        }

        //private SerializedProperty m_Clip;
        //private SerializedProperty m_Threshold;
        //private SerializedProperty m_Speed;
        private const float SUB_HEIGHT = 16f;
        private int m_SubContentCount = 4;

        

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var blendTreeTypeEnum = property.FindPropertyRelative("blendTreeType");
            
            if (blendTreeTypeEnum.enumValueIndex == (int)AssetBlendTree.BlendTreeType._1D)
            {
                m_SubContentCount = 4;
            }
            else if (blendTreeTypeEnum.enumValueIndex == (int)AssetBlendTree.BlendTreeType.None)
            {
                m_SubContentCount = 3;
            }

            return m_SubContentCount * SUB_HEIGHT;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.LabelField(position, label);

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel + 1;

            
            position.y += SUB_HEIGHT;

            Rect[] rects = new Rect[m_SubContentCount - 1];     //  第一个为标签
            for (int i = 0; i < m_SubContentCount - 1; i++)
            {
                rects[i] = new Rect(position.x, position.y + i * SUB_HEIGHT, position.width, SUB_HEIGHT);
            }

            //EditorGUILayout.BeginVertical();
            EditorGUI.PropertyField(rects[0], property.FindPropertyRelative("clip"));
            var blendTreeTypeEnum = property.FindPropertyRelative("blendTreeType");
            if (blendTreeTypeEnum.enumValueIndex == (int)AssetBlendTree.BlendTreeType.None)
            {
                EditorGUI.PropertyField(rects[1], property.FindPropertyRelative("stateName"));
            }
            else if (blendTreeTypeEnum.enumValueIndex == (int)AssetBlendTree.BlendTreeType._1D)
            {
                EditorGUI.PropertyField(rects[1], property.FindPropertyRelative("threshold"));
                EditorGUI.PropertyField(rects[2], property.FindPropertyRelative("speed"));
            }
            //EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }
    }
}


