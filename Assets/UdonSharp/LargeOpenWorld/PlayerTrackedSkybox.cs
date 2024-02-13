
using UdonSharp;
using VRC.SDKBase;

namespace LargeOpenWorld
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerTrackedSkybox : UdonSharpBehaviour
    {
        public void FixedUpdate()
        {
            gameObject.transform.position = Networking.LocalPlayer.GetPosition();
        }
    }
}
