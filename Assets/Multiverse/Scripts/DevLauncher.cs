﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Multiverse
{
    public sealed class DevLauncher : MonoBehaviour
    {
        private LoginServer loginServer;

        private void Awake()
        {
            loginServer = FindObjectOfType<LoginServer>();
        }

        public void StartLoginServer()
        {

        }

        public void StartLoginClient()
        {

        }

        public void StartZoneClient(string sceneName)
        {

        }

        public void StartZoneMaster()
        {
            
        }

        private void OnGUI()
        {
            DrawLoginServerControl();
        }

        private static readonly float WndInset = 5;
        private static readonly Vector2 LoginWindowSize = new Vector2(200, 200);
        private Rect rectLoginServer = Rect.MinMaxRect(0, 0, LoginWindowSize.x, LoginWindowSize.y);

        private void WndLoginServer(int id)
        {
            GUILayout.BeginArea(Rect.MinMaxRect(WndInset, 20, LoginWindowSize.x - WndInset, LoginWindowSize.y - WndInset));

            if (!loginServer.isRunning)
            {
                if (GUILayout.Button("Start Login Server"))
                {
                    loginServer.StartLoginServer();
                }
            }
            else
            {
                GUILayout.Label("Running: " + loginServer.isRunning);
                GUILayout.Label("Num connections: " + loginServer.numConnections);
                GUILayout.Label("Num logged: " + loginServer.numSessions);

                if(GUILayout.Button("Stop Login Server"))
                {
                    loginServer.StopLoginServer();
                }
            }

            GUILayout.EndArea();

            GUI.DragWindow(Rect.MinMaxRect(0,0,10000, 20));
        }

        private void DrawLoginServerControl()
        {
            rectLoginServer = GUI.Window(0, rectLoginServer, WndLoginServer, "Login Server");
        }
    }
}
