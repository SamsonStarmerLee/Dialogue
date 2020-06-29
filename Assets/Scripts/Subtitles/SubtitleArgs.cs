using UnityEngine;

namespace Assets.Scripts.Queries.Subtitles
{
    public class SubtitleArgs
    {
        /// <summary>
        /// Name of the speaker.
        /// </summary>
        public string Speaker { get; }

        /// <summary>
        /// Dialogue to display.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Intended subtitle color.
        /// </summary>
        public Color Color { get; }

        public SubtitleArgs(string speaker, string text, Color color)
        {
            Speaker = speaker;
            Text = text;
            Color = color;
        }
    }
}
