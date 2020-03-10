using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

namespace CostumeAnimator
{
    public class TransAnimatorWindow : EditorWindow
    {
        private AnimatorController animCtrl;

        [MenuItem("CostumeAnimator/TransAnimatorWindow")]
        static void CreateWindow()
        {
            Rect rect = new Rect(0, 0, 300, 300);
            TransAnimatorWindow window = GetWindowWithRect<TransAnimatorWindow>(rect, true, "TransAnimatorWindow");
            Selection.activeObject = null;
            window.Show();
        }

        private void OnGUI()
        {
            animCtrl = EditorGUILayout.ObjectField("AnimatorController", animCtrl, typeof(AnimatorController), true) as AnimatorController;
            
            if (Selection.activeObject == null || (Selection.activeObject != null && !(Selection.activeObject is DefaultAsset)))
            {
                EditorGUILayout.LabelField("请先选中生成asset的目标文件夹！");
            }
            else if (animCtrl == null)
            {
                EditorGUILayout.LabelField("请先选择要转化的AnimatorController！");
            }
            else
            {
                if (GUILayout.Button("生成asset", GUILayout.Width(200)))
                {
                    PlayableAnimatorUtil.GetInstance().TransAnimator2Asset(animCtrl);
                    this.Close();
                }
            }
        }

        private void OnInspectorUpdate()
        {
            this.Repaint();
        }

        private void OnFocus()
        {
            
        }

        private void OnLostFocus()
        {
            
        }
    }
}
