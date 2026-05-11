using UnityEngine;

public class ExitDoorSoundOnly : MonoBehaviour
{
    public string exitDoorLayerName = "ExitDoor";

    public void PlayOnlyExitDoorSounds()
    {
        AudioSource[] allSources =
            Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        int exitDoorLayer = LayerMask.NameToLayer(exitDoorLayerName);

        foreach (AudioSource source in allSources)
        {
            if (source.gameObject.layer == exitDoorLayer)
                source.mute = false;
            else
                source.mute = true;
        }
    }
}