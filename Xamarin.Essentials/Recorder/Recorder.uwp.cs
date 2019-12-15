using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace Xamarin.Essentials
{
    public static partial class Recorder
    {
        static Task<bool> PlatformCanRecordAudio => Task.FromResult(true);

        static MediaCapture mediaCapture;

        static string audioFilePath;

        static async Task PlatformRecordAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync(
                    new MediaCaptureInitializationSettings { StreamingCaptureMode = StreamingCaptureMode.Audio });
                mediaCapture.RecordLimitationExceeded += sender =>
                {
                    DeleteMediaCapture();
                    throw new Exception("Record Limitation Exceeded");
                };

                mediaCapture.Failed += (sender, errorEventArgs) =>
                {
                    DeleteMediaCapture();
                    throw new Exception($"Audio recording failed: {errorEventArgs.Code}. {errorEventArgs.Message}");
                };

                var fileOnDisk = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(Path.ChangeExtension(Path.GetRandomFileName(), ".wav"));
                audioFilePath = fileOnDisk.Path;
                await mediaCapture.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality.High), fileOnDisk);
            }
            catch
            {
                DeleteMediaCapture();
                throw;
            }
        }

        static async Task<RecordedAudio> PlatformStopAsync()
        {
            await mediaCapture.StopRecordAsync();
            var capture = new RecordedAudio(audioFilePath);
            mediaCapture.Dispose();
            mediaCapture = null;

            return capture;
        }

        static void DeleteMediaCapture()
        {
            mediaCapture?.Dispose();
            try
            {
                if (!string.IsNullOrWhiteSpace(audioFilePath) && File.Exists(audioFilePath))
                    File.Delete(audioFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting audio file: {ex}");
            }

            audioFilePath = string.Empty;
            mediaCapture = null;
        }
    }
}
