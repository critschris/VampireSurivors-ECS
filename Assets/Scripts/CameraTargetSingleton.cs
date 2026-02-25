using UnityEngine;

namespace TMG.Survivors
{
    public class CameraTargetSingleton : MonoBehaviour
    {
        public static CameraTargetSingleton instance;

        public void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("Warning multiple instances of CameraTargetSingleton. Destroying new", instance);
                Destroy(gameObject);
                return;
            }

            instance = this;
        }
    }
}