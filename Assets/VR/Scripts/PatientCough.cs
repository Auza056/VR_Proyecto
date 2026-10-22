using System.Collections;
using UnityEngine;

namespace HospitalVR
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PatientCough : MonoBehaviour
    {
        public AudioClip clip;
        [Min(1)] public float minimumInterval = 18;
        [Min(1)] public float maximumInterval = 40;
        AudioSource source;

        void Awake() => source = GetComponent<AudioSource>();

        IEnumerator Start()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(Random.Range(minimumInterval, Mathf.Max(minimumInterval, maximumInterval)));
                if (clip != null && source != null && !source.isPlaying) source.PlayOneShot(clip);
            }
        }
    }
}
