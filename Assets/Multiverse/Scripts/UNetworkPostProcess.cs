
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif
using UnityEngine;

namespace Multiverse
{
    public static class UNetworkPostProcess
    {
#if UNITY_EDITOR
        [PostProcessScene]
        public static void OnPostProcessScene()
        {
            ulong nextSceneId = 1;

            foreach (UNetworkIdentity uv in GameObject.FindObjectsOfType<UNetworkIdentity>())
            {
                if (uv.GetComponent<UNetworkManager>() != null)
                {
                    Debug.LogError("UNetworkManager instance on scene has UNetworkIdentity component attached, remove it");
                    continue;
                }

                if (uv.isClient || uv.isServer)
                    continue;

                uv.gameObject.SetActive(false);
                uv.sceneId = nextSceneId;

                nextSceneId++;
            }
            
        }
#endif
    }
}
