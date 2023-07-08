using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip Pick, Drop, Heal;
    static AudioSource audioSrc;

    void Awake()
    {
        Pick = Resources.Load<AudioClip>("pick");
        Drop = Resources.Load<AudioClip>("drop");
        Heal = Resources.Load<AudioClip>("heal");
        audioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string clip)
    {
        switch (clip)
        {
            case "pick":
                audioSrc.pitch = 1f;
                audioSrc.PlayOneShot(Pick);
                break;
            case "drop":
                audioSrc.pitch = 1f;
                audioSrc.PlayOneShot(Drop);
                break;
            case "heal":
                audioSrc.pitch = 1f;
                audioSrc.PlayOneShot(Heal);
                break;
        }
    }
}
