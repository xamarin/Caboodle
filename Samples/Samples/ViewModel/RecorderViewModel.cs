using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class RecorderViewModel : BaseViewModel
    {
        public ICommand StartRecording { get; }

        public ICommand StopRecording { get; }

        string recordingPath;

        public string RecordingPath { get => recordingPath; set => SetProperty(ref recordingPath, value); }

        public RecorderViewModel()
        {
            StartRecording = new Command(async () => await Recorder.RecordAsync());
            StopRecording = new Command(async () => await OnRecordingStopped());
        }

        async Task OnRecordingStopped()
        {
            var record = await Recorder.StopAsync();
            var size = File.ReadAllBytes(record.FullPath).Length;
            await DisplayAlertAsync($"Recording saved in {record.FullPath}, size is {size}");
            await Share.RequestAsync(new ShareFileRequest("recording", record));
        }
    }
}
