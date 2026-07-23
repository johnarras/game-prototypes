using OxDb.SharedGame.UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace OxDb.Client.UI.Abstractions
{
    public class GScrollRect : ScrollRect, IScrollRect
    {
        protected override void OnDestroy()
        {
            if (Application.isPlaying)
            {
                RectTransform content = GetComponent<RectTransform>();
                if (content != null && content.gameObject != null)
                {
                    Transform parentTransform = content.gameObject.transform;

                    // Iterate backwards from the last child to the first (index 0)
                    // This is safe because destroying a child removes it from the list,
                    // shifting the subsequent elements' indices up.
                    for (int i = parentTransform.childCount - 1; i >= 0; i--)
                    {
                        // Get the child Transform
                        Transform child = parentTransform.GetChild(i);

                        // Call Destroy() on the child's GameObject.
                        // DO NOT use DestroyImmediate() at runtime.
                        Destroy(child.gameObject);
                    }
                }
            }
        }
    }
}


