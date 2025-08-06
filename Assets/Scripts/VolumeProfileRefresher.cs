using UnityEngine;
using UnityEngine.Rendering;

public class VolumeProfileRefresher : MonoBehaviour
{
    public Volume volume; // Drag your Global Volume here in the Inspector

    void Start()
    {
        if (volume != null)
        {
            var tempProfile = volume.profile;
            volume.profile = null;
            volume.profile = tempProfile;
        }
    }
}
