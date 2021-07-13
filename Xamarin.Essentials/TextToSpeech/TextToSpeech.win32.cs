using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class TextToSpeech
    {
        internal static Task<IEnumerable<Locale>> PlatformGetLocalesAsync() =>
            Task.FromResult(new SpeechSynthesizer().GetInstalledVoices().Select(v => new Locale(v.VoiceInfo.Culture.TwoLetterISOLanguageName, null, v.VoiceInfo.Name, v.VoiceInfo.Id)));

        internal static async Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken = default)
        {
            var tcsUtterance = new TaskCompletionSource<bool>();

            try
            {
                var speechSynthesizer = new SpeechSynthesizer();

                if (!string.IsNullOrWhiteSpace(options?.Locale?.Id))
                {
                    var voiceInfo = speechSynthesizer.GetInstalledVoices().FirstOrDefault(v => v.VoiceInfo.Id == options.Locale.Id);
                    if (voiceInfo is object)
                    {
                        speechSynthesizer.SelectVoice(voiceInfo.VoiceInfo.Name);
                    }
                }

                var ssml = GetSpeakParametersSSMLProsody(speechSynthesizer, text, options);

                speechSynthesizer.SpeakCompleted += SpeakCompleted;
                speechSynthesizer.SpeakAsync(new Prompt(ssml, SynthesisTextFormat.Ssml));

                void OnCancel()
                {
                    speechSynthesizer.SpeakAsyncCancelAll();
                }

                using (cancelToken.Register(OnCancel))
                {
                    await tcsUtterance.Task;
                }

                void SpeakCompleted(object sender, SpeakCompletedEventArgs e)
                {
                    tcsUtterance.TrySetResult(true);
                    speechSynthesizer.SpeakCompleted -= SpeakCompleted;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to speak: " + ex);
                tcsUtterance.TrySetException(ex);
            }
        }

        static string GetSpeakParametersSSMLProsody(SpeechSynthesizer synthesizer, string text, SpeechOptions options)
        {
            var volume = "default";
            var pitch = "default";
            var rate = "default";

            // Look for the specified language, otherwise the default voice
            var locale = options?.Locale?.Language ?? synthesizer.Voice.Culture.Name;

            if (options?.Volume.HasValue ?? false)
                volume = (options.Volume.Value * 100f).ToString(CultureInfo.InvariantCulture);

            if (options?.Pitch.HasValue ?? false)
                pitch = ProsodyPitch(options.Pitch);

            // SSML generation
            var ssml = new StringBuilder();
            ssml.AppendLine($"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{locale}'>");
            ssml.AppendLine($"<prosody pitch='{pitch}' rate='{rate}' volume='{volume}'>{text}</prosody> ");
            ssml.AppendLine($"</speak>");

            return ssml.ToString();
        }

        static string ProsodyPitch(float? pitch)
        {
            if (!pitch.HasValue)
                return "default";

            if (pitch.Value <= 0.25f)
                return "x-low";
            else if (pitch.Value > 0.25f && pitch.Value <= 0.75f)
                return "low";
            else if (pitch.Value > 0.75f && pitch.Value <= 1.25f)
                return "medium";
            else if (pitch.Value > 1.25f && pitch.Value <= 1.75f)
                return "high";
            else if (pitch.Value > 1.75f)
                return "x-high";

            return "default";
        }
    }
}
