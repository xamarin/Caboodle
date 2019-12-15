using System;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Recorder
    {
        static Task<bool> PlatformCanRecordAudio => Task.FromResult(AVAudioSession.SharedInstance().InputAvailable);

        static AVAudioRecorder recorder;

        static void InitAudioSession()
        {
            var audioSession = AVAudioSession.SharedInstance();

            var err = audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
            if (err != null)
                ThrowNSError(err);

            err = audioSession.SetActive(true);
            if (err != null)
                ThrowNSError(err);
        }

        static void InitAudioRecorder()
        {
            var url = NSUrl.FromFilename(GetTempFileName());
            recorder = AVAudioRecorder.Create(url, new AudioSettings(settings), out var error);
            if (error != null)
                ThrowNSError(error);
        }

        static string GetTempFileName()
        {
            var tempFileName = Path.Combine(FileSystem.CacheDirectory, Path.GetTempFileName());

            return tempFileName;
        }

        static Task PlatformRecordAsync()
        {
            InitAudioSession();
            InitAudioRecorder();
            recorder.Record();
            return Task.CompletedTask;
        }

        static Task<RecordedAudio> PlatformStopAsync()
        {
            recorder.Stop();

            var recording = new RecordedAudio(recorder.Url.Path);
            recorder.Dispose();
            recorder = null;
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetActive(false);

            return Task.FromResult(recording);
        }

        static void ThrowNSError(NSError error)
            => throw new Exception(error.ToString());

        static readonly NSDictionary<NSString, NSObject> settings = new NSDictionary<NSString, NSObject>(
        new[]
        {
            AVAudioSettings.AVSampleRateKey,
            AVAudioSettings.AVFormatIDKey,
            AVAudioSettings.AVNumberOfChannelsKey,
            AVAudioSettings.AVLinearPCMBitDepthKey,
            AVAudioSettings.AVLinearPCMIsBigEndianKey,
            AVAudioSettings.AVLinearPCMIsFloatKey
#pragma warning disable SA1118 // Parameter should not span multiple lines
        }, new NSObject[]
        {
            NSNumber.FromFloat(16000),
            NSNumber.FromInt32((int)AudioToolbox.AudioFormatType.LinearPCM),
            NSNumber.FromInt32(1),
            NSNumber.FromInt32(16),
            NSNumber.FromBoolean(false),
            NSNumber.FromBoolean(false)
        });
#pragma warning restore SA1118 // Parameter should not span multiple lines
    }
}
