using UnityEngine;

public class PlatformEndTrigger : MonoBehaviour
{
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PlatformEndTrigger")
        {
            Debug.Log("collded with" + other.name);
            // Ensure the object has a Platform component on the parent
            Platform platform = other.gameObject.transform.parent.GetComponent<Platform>();

            if (platform != null)
            {
                PlatformController.NotifyPlatformEnd(platform);
            }
            else
            {
                Debug.Log("platform is null");
            }
        }
       
    }
}