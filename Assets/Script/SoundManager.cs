using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip Pick, Drop, Heal, Walk, Wrong, Placed, Tile;
    static AudioSource audioSrc;

    void Awake()
    {
        Pick = Resources.Load<AudioClip>("pick");
        Drop = Resources.Load<AudioClip>("drop");
        Heal = Resources.Load<AudioClip>("heal");
        Walk = Resources.Load<AudioClip>("walk");
        Wrong = Resources.Load<AudioClip>("wrong");
        Placed = Resources.Load<AudioClip>("placed");
        Tile = Resources.Load<AudioClip>("tile");
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
            case "walk":
                audioSrc.pitch = 1f;
                audioSrc.PlayOneShot(Walk);
                break;
            case "wrong":
                audioSrc.pitch = 1f;
                audioSrc.PlayOneShot(Wrong);
                break;
            case "placed":
                audioSrc.pitch = 1f;
                audioSrc.PlayOneShot(Placed);
                break;
            case "tile":
                audioSrc.pitch = 1f;
                audioSrc.PlayOneShot(Tile);
                break;
        }
    }
}
