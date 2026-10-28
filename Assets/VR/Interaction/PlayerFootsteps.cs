using UnityEngine;

namespace HospitalVR.Interaction
{
    /// <summary>Follows actual grounded rig translation, not stick input or hand movement.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerFootsteps : MonoBehaviour
    {
        public AudioSource source;
        [Range(0, 1)] public float volume = .35f;
        [Min(.01f)] public float minimumSpeed = .12f;
        [Min(.01f)] public float fadeTime = .12f;
        public bool IsWalking { get; private set; }
        public int PlaybackStarts { get; private set; }
        CharacterController controller;
        Vector3 previous;

        void OnEnable()
        {
            controller = GetComponent<CharacterController>();
            previous = transform.position;
        }

        void LateUpdate()
        {
            var delta = transform.position - previous;
            previous = transform.position;
            float horizontal = new Vector2(delta.x, delta.z).magnitude;
            float speed = horizontal / Mathf.Max(Time.deltaTime, .0001f);
            // Reject teleports and vertical motion; rotation alone makes no footsteps.
            IsWalking = controller.enabled && controller.isGrounded && horizontal < .5f
                && speed >= minimumSpeed && speed < 6f && Mathf.Abs(delta.y) < .12f;
            if (source == null || source.clip == null) return;
            if (IsWalking && !source.isPlaying)
            {
                source.UnPause();
                if (!source.isPlaying) source.Play();
                PlaybackStarts++;
            }
            source.volume = Mathf.MoveTowards(source.volume, IsWalking ? volume : 0f,
                Time.deltaTime * volume / Mathf.Max(.01f, fadeTime));
            if (!IsWalking && source.volume <= 0f && source.isPlaying) source.Pause();
        }

        void OnDisable()
        {
            IsWalking = false;
            if (source != null) { source.Stop(); source.volume = 0f; }
        }
    }
}
