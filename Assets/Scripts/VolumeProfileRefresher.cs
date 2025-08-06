using UnityEngine;
using UnityEngine.Rendering;

public class VolumeProfileRefresher : MonoBehaviour
{
    public Volume volume; 

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
