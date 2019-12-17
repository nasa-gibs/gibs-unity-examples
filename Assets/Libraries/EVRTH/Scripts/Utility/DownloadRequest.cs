using System;
using UnityEngine.Networking;

namespace EVRTH.Scripts.Utility
{
    public struct DownloadRequest
    {
        public string url;
        public Action<UnityWebRequest> callbackAction;
    }

}
