using Assets.Scripts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Queries.Subtitles
{
    class SubtitleManager : MonoBehaviour
    {
        const float delayPerWord = 0.375f;
        const float minimumSubtitleDelay = 1.5f;
        const float nameForgetTime = 60.0f;

        [SerializeField] private float fadeInTime;
        [SerializeField] private float fadeOutTime;
        [SerializeField] private GameObject subtitlePrefab;
        [SerializeField] private Transform letterbox;

        /// <summary>
        /// This dictionary contains character names and the last time they spoke.
        /// </summary>
        private readonly Dictionary<string, float> speakerHistory = new Dictionary<string, float>();

        private readonly List<ActiveSubtitle> activeSubtitles = new List<ActiveSubtitle>();

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
            var toRemove = new List<ActiveSubtitle>();

            foreach (var subtitle in activeSubtitles)
            {
                subtitle.Elapsed += Time.deltaTime;

                var elapsed = subtitle.Elapsed;
                var duration = subtitle.Duration;

                // Fade In
                if (elapsed < fadeInTime)
                {
                    var t = elapsed / fadeInTime;
                    subtitle.Canvas.alpha = t;
                }

                // Fade Out
                else if (elapsed > duration - fadeOutTime)
                {
                    var r = elapsed - duration - fadeOutTime;
                    var t = r / fadeOutTime;
                    subtitle.Canvas.alpha = Mathf.Lerp(1f, 0f, t);
                }
                else
                {
                    subtitle.Canvas.alpha = 1f;
                }

                if (elapsed > duration)
                {
                    toRemove.Add(subtitle);
                }
            }

            foreach (var r in toRemove)
            {
                Destroy(r.GameObject);
                activeSubtitles.Remove(r);
            }
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
                (words * delayPerWord) + fadeInTime + fadeOutTime, 
                minimumSubtitleDelay, 
                float.MaxValue);

            var s = Instantiate(subtitlePrefab, letterbox);
            var c = s.GetComponent<CanvasGroup>();
            activeSubtitles.Add(new ActiveSubtitle(s, c, duration));

            s.GetComponent<TMPro.TextMeshProUGUI>().text = text;
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

    class ActiveSubtitle
    {
        public readonly GameObject GameObject;
        public readonly CanvasGroup Canvas;
        public readonly float Duration;

        public float Elapsed;

        public ActiveSubtitle(
            GameObject subtitleObject,
            CanvasGroup canvas,
            float duration)
        {
            GameObject = subtitleObject;
            Canvas = canvas;
            Duration = duration;
        }
    }
}
