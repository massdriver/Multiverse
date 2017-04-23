using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiverse
{
    public class LoginClient : MonoBehaviour
    {
        public virtual void OnConnected()
        {

        }

        public virtual void OnAuthorized(bool success)
        {

        }

        public virtual void OnDisconnected()
        {

        }
    }
}