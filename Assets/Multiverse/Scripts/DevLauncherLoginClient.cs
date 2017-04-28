using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    public sealed class DevLauncherLoginClient : MonoBehaviour
    {
        public LoginClient loginClient;

        private void OnGUI()
        {
            DrawLoginClientControl();
        }

        private static readonly float WndInset = 5;
        private static readonly Vector2 LoginWindowSize = new Vector2(200, 400);
        private Rect rectLoginClient = new Rect(new Vector2(200, 200), LoginWindowSize);//Rect.MinMaxRect(0, 250, LoginWindowSize.x, LoginWindowSize.y + 250);

        private string login = "";
        private string password = "";
        private string email = "";
        private string promoCode = "";

        private void WndLoginClient(int id)
        {
            GUILayout.BeginArea(Rect.MinMaxRect(WndInset, 20, LoginWindowSize.x - WndInset, LoginWindowSize.y - WndInset));

            if (!loginClient.isConnected)
            {
                if (GUILayout.Button("Start Login Client"))
                {
                    loginClient.StartLoginClient();
                }
            }
            else
            {
                GUILayout.Label("Connected: " + loginClient.isConnected);
                GUILayout.Label("Authorized: " + loginClient.isAuthorized);
                GUILayout.Label("Session ID: " + loginClient.sessionId);

                GUILayout.Height(15);

                if (showCreateAccount && !loginClient.isWatingForCreateAccountReply && !loginClient.isWaitingForAuthorizeReply)
                {
                    GUILayout.Label("Create new account");
                    GUILayout.Label("Login");
                    login = GUILayout.TextField(login);
                    GUILayout.Label("Password");
                    password = GUILayout.PasswordField(password, '*');
                    GUILayout.Label("Email");
                    email = GUILayout.TextField(email);
                    GUILayout.Label("Promocode");
                    promoCode = GUILayout.TextField(email);

                    GUILayout.Height(15);

                    if (GUILayout.Button("Register"))
                    {
                        loginClient.CreateAccount(login, HashUtil.HashPassword(password), email, promoCode);
                        showCreateAccount = false;

                        login = "";
                        password = "";
                        email = "";
                        promoCode = "";
                    }

                    if (GUILayout.Button("Cancel"))
                    {
                        showCreateAccount = false;
                    }
                }
                else
                if (loginClient.isWaitingForAuthorizeReply)
                {
                    GUILayout.Label("Loggin in ... ");
                    GUILayout.Button("Abort");
                }
                else
                if (!loginClient.isAuthorized)
                {
                    GUILayout.Label("Login");
                    login = GUILayout.TextField(login);
                    GUILayout.Label("Password");
                    password = GUILayout.PasswordField(password, '*');

                    if (GUILayout.Button("Login"))
                    {
                        loginClient.Login(login, password);
                        password = "";
                    }

                    GUILayout.Height(15);

                    if (GUILayout.Button("Register account"))
                    {
                        showCreateAccount = true;
                    }

                }

                if (GUILayout.Button("Stop Login Client"))
                {
                    loginClient.StopLoginClient();
                }
            }

            GUILayout.EndArea();

            GUI.DragWindow(Rect.MinMaxRect(0, 0, 10000, 20));
        }

        private bool showCreateAccount;

        private void DrawLoginClientControl()
        {
            rectLoginClient = GUI.Window(1, rectLoginClient, WndLoginClient, "Login Client");
        }
    }
}
