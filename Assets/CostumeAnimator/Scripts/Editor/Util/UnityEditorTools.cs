using UnityEditor;
using UnityEngine;
using System.Linq;

namespace CostumeAnimator
{
    public class UnityEditorTools
    {
        [MenuItem("CostumeAnimator/TransTest")]
        static void TransTest()
        {
            PlayableAnimatorUtil util = new PlayableAnimatorUtil();
            var allPaths = AssetDatabase.GetAllAssetPaths();
            var paths = allPaths.Where(path => path.EndsWith(".controller"));
            foreach(var path in paths)
            {
                util.TransAnimator2Asset(path);
            }
        }
    }
}
