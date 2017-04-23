using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Multiverse
{
    public class ZoneNetworkManager : UnityEngine.Networking.NetworkManager
    {
        public string zoneServerScene = "scene_zone_001";

        void OnGUI()
        {
            if (isNetworkActive)
                return;

            GUILayout.BeginArea(new Rect(new Vector2(300, 300), new Vector3(200, 400)));

            zoneServerScene = GUILayout.TextArea(zoneServerScene);


            if (GUILayout.Button("Start Zone Server"))
            {
                StartZoneServer();
            }

            GUILayout.EndArea();
        }

        private void StartZoneServer()
        {
            StartServer();

            if (zoneServerScene != null && zoneServerScene.Length > 0)
                ServerChangeScene(zoneServerScene);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            Debug.Log("OnClientConnect: " + networkSceneName);
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            Debug.Log(networkSceneName);

            base.OnClientSceneChanged(conn);
        }
    }
}