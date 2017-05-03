using UnityEngine;

namespace Multiverse
{
    [RequireComponent(typeof(UNetworkManager))]
    public sealed class UNetworkManagerHUD : MonoBehaviour
    {
        private UNetworkManager manager;

        void Awake()
        {
            manager = GetComponent<UNetworkManager>();
        }

        void OnGUI()
        {
            int xpos = 10 + 0;
            int ypos = 40 + 0;
            const int spacing = 24;

            if (manager.isManagerActive)
            {
                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Stop manager"))
                {
                    manager.StopManager();
                }

                return;
            }

            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Host"))
            {
                manager.StartHost();
            }

            ypos += spacing;

            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Server"))
            {
                manager.StartServer();
            }

            ypos += spacing;

            if (GUI.Button(new Rect(xpos, ypos, 105, 20), "LAN Client"))
            {
                manager.StartClient();
            }

            manager.targetServerAddress = GUI.TextField(new Rect(xpos + 100, ypos, 95, 20), manager.targetServerAddress);
        }
    }
}
