using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MetadataExtractor;
using Xamarin.Essentials;
using Xamarin.Forms;
using Directory = MetadataExtractor.Directory;

namespace Samples.ViewModel
{
    public class MediaPickerViewModel : BaseViewModel
    {
        string photoPath;
        string videoPath;
        string metadata;
        bool showPhoto;
        bool showVideo;

        public MediaPickerViewModel()
        {
            PickPhotoCommand = new Command(DoPickPhoto);
            CapturePhotoCommand = new Command(DoCapturePhoto, () => MediaPicker.IsCaptureSupported);

            PickVideoCommand = new Command(DoPickVideo);
            CaptureVideoCommand = new Command(DoCaptureVideo, () => MediaPicker.IsCaptureSupported);
        }

        public ICommand PickPhotoCommand { get; }

        public ICommand CapturePhotoCommand { get; }

        public ICommand PickVideoCommand { get; }

        public ICommand CaptureVideoCommand { get; }

        public string Metadata
        {
            get => metadata;
            set => SetProperty(ref metadata, value);
        }

        public bool ShowPhoto
        {
            get => showPhoto;
            set => SetProperty(ref showPhoto, value);
        }

        public bool ShowVideo
        {
            get => showVideo;
            set => SetProperty(ref showVideo, value);
        }

        public string PhotoPath
        {
            get => photoPath;
            set => SetProperty(ref photoPath, value);
        }

        public string VideoPath
        {
            get => videoPath;
            set => SetProperty(ref videoPath, value);
        }

        async void DoPickPhoto()
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();

                await LoadPhotoAsync(photo);

                Console.WriteLine($"PickPhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PickPhotoAsync THREW: {ex.Message}");
            }
        }

        async void DoCapturePhoto()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                await LoadPhotoAsync(photo);

                Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }
        }

        async void DoPickVideo()
        {
            try
            {
                var video = await MediaPicker.PickVideoAsync();

                await LoadVideoAsync(video);

                Console.WriteLine($"PickVideoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PickVideoAsync THREW: {ex.Message}");
            }
        }

        async void DoCaptureVideo()
        {
            try
            {
                var photo = await MediaPicker.CaptureVideoAsync();

                await LoadVideoAsync(photo);

                Console.WriteLine($"CaptureVideoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CaptureVideoAsync THREW: {ex.Message}");
            }
        }

        async Task LoadPhotoAsync(FileResult photo)
        {
            Metadata = null;

            // canceled
            if (photo == null)
            {
                PhotoPath = null;
                return;
            }

            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            PhotoPath = newFile;
            ShowVideo = false;
            ShowPhoto = true;

            ShowMetadata(await photo.OpenReadAsync());
        }

        async Task LoadVideoAsync(FileResult video)
        {
            Metadata = null;

            // canceled
            if (video == null)
            {
                VideoPath = null;
                return;
            }

            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, video.FileName);
            using (var stream = await video.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            VideoPath = newFile;
            ShowVideo = true;
            ShowPhoto = false;

            ShowMetadata(await video.OpenReadAsync());
        }

        void ShowMetadata(Stream stream)
        {
            Metadata = null;
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(stream);
                var sb = new StringBuilder();
                foreach (var directory in directories)
                {
                    // Each directory stores values in tags
                    foreach (var tag in directory.Tags)
                    {
                        sb.AppendLine(tag.ToString());
                    }

                    // Each directory may also contain error messages
                    foreach (var error in directory.Errors)
                    {
                        sb.AppendLine("ERROR: " + error);
                    }
                }

                Metadata = sb.ToString();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public override void OnDisappearing()
        {
            PhotoPath = null;
            VideoPath = null;

            base.OnDisappearing();
        }
    }
}
