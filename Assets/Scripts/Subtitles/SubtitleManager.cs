using Assets.Scripts.Notifications;
using Framework.Maths;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Queries.Subtitles
{
    class SubtitleManager : MonoBehaviour
    {
        const float delayPerWord = 0.375f;
        const float minimumSubtitleDelay = 1.5f;
        const float nameForgetTime = 60.0f;

        [SerializeField] private float letterboxFadeTime;
        [SerializeField] private GameObject subtitlePrefab;
        [SerializeField] private Image letterbox;

        private CanvasGroup subtitleGroup;

        /// <summary>
        /// This dictionary contains character names and the last time they spoke.
        /// </summary>
        private readonly Dictionary<string, float> speakerHistory = new Dictionary<string, float>();

        /// <summary>
        /// Currently displaying subtitles.
        /// </summary>
        private readonly List<Subtitle> activeSubtitles = new List<Subtitle>();

        private void Awake()
        {
            subtitleGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            this.AddObserver(OnDialogue, Notify.Action<SubtitleArgs>());
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnDialogue, Notify.Action<SubtitleArgs>());
        }

        private void OnDialogue(object sender, object args)
        {
            DisplaySubtitle(args as SubtitleArgs);
        }

        private void Update()
        {
            var toRemove = new List<Subtitle>();

            foreach (var subtitle in activeSubtitles)
            {
                if (Utility.Elapsed(subtitle.StartTime, subtitle.Duration))
                {
                    toRemove.Add(subtitle);
                }
            }

            foreach (var r in toRemove)
            {
                Destroy(r.GameObject);
                activeSubtitles.Remove(r);
            }

            // Fade in/out the subtitle canvas
            var targetAlpha = (activeSubtitles.Count == 0) ? 0f : 1f;
            subtitleGroup.alpha = Mathf.MoveTowards(subtitleGroup.alpha, targetAlpha, (1f / letterboxFadeTime) * Time.deltaTime);
        }

        private void DisplaySubtitle(SubtitleArgs subtitle)
        {
            if (string.IsNullOrWhiteSpace(subtitle.Speaker))
            {
                return;
            }

            var words = subtitle.Text.CountWords();
            var color = ColorUtility.ToHtmlStringRGB(subtitle.Color);

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"<#{color}>");

            if (ShouldAppendSpeaker(subtitle.Speaker))
            {
                stringBuilder.Append($"{subtitle.Speaker}: ");
            }

            speakerHistory[subtitle.Speaker] = Time.time;

            stringBuilder.Append(subtitle.Text);
            stringBuilder.Append("</color>");
            var text = stringBuilder.ToString();

            var duration = Mathf.Clamp(
                words * delayPerWord, 
                minimumSubtitleDelay, 
                float.MaxValue);

            var s = Instantiate(subtitlePrefab, letterbox.transform);
            s.GetComponent<TMPro.TextMeshProUGUI>().text = text;

            activeSubtitles.Add(new Subtitle(s, duration, Time.time));
        }

        private bool ShouldAppendSpeaker(string speaker)
        {
            if (speakerHistory.TryGetValue(speaker, out var timeOfLastSpeaking))
            {
                return (Time.time - timeOfLastSpeaking) > nameForgetTime;
            }

            // New character we haven't heard from before/recently.
            return true;
        }

        private void ClearSpeakerHistory()
        {
            speakerHistory.Clear();
        }
    }

    class Subtitle
    {
        public Subtitle(GameObject gameObject, float duration, float startTime)
        {
            GameObject = gameObject;
            Duration = duration;
            StartTime = startTime;
        }

        public GameObject GameObject { get; }

        public float Duration { get; }

        public float StartTime { get; }
    }
}
