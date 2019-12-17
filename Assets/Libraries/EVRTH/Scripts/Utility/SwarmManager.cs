using System.Linq;
using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    /// <summary>
    /// sets up a swarm of download operations and maintains them until all downloads are complete
    /// </summary>
    public class SwarmManager : MonoBehaviour
    {
        public static SwarmManager Instance
        {
            get
            {
                if (!instance)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<SwarmManager>();
                    return instance;
                }
                return instance;
            }
        }

        public int RunningJobs
        {
            get { return operators.Sum(ops => ops.jobs); }
        }

        private static SwarmManager instance;
        private SwarmOperator[] operators;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            //more than 8 seems to make unity choke
            operators = new SwarmOperator[8];
            for (int i = 0; i < operators.Length; i++)
            {
                operators[i] = new SwarmOperator();
            }
        }

        public void EnqueueRequest(DownloadRequest request)
        {
            operators.OrderBy(so => so.jobs).First().AddJob(request);
        }

        private void OnApplicationQuit()
        {
            for (int i = 0; i < operators.Length; i++)
            {
                operators[i].KillThread();
            }
        }
    }
}