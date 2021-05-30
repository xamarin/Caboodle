using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;
using Photos;
using PhotosUI;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static UIImagePickerController picker;

        static bool PlatformIsCaptureSupported
            => UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);

        static Task<FileResult> PlatformPickPhotoAsync(MediaPickerOptions options)
            => PhotoAsync(options, true, true);

        static Task<IEnumerable<FileResult>> PlatformPickPhotosAsync(MediaPickerOptions options, MultiPickerOptions pickerOptions)
            => PhotosAsync(options, pickerOptions);

        static Task<FileResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
            => PhotoAsync(options, true, false);

        static Task<FileResult> PlatformPickVideoAsync(MediaPickerOptions options)
            => PhotoAsync(options, false, true);

        static Task<FileResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
            => PhotoAsync(options, false, false);

        static async Task<FileResult> PhotoAsync(MediaPickerOptions options, bool photo, bool pickExisting, bool multi = false)
        {
            var sourceType = pickExisting ? UIImagePickerControllerSourceType.PhotoLibrary : UIImagePickerControllerSourceType.Camera;
            var mediaType = photo ? UTType.Image : UTType.Movie;

            if (!UIImagePickerController.IsSourceTypeAvailable(sourceType))
                throw new FeatureNotSupportedException();
            if (!UIImagePickerController.AvailableMediaTypes(sourceType).Contains(mediaType))
                throw new FeatureNotSupportedException();

            if (!photo)
                await Permissions.EnsureGrantedAsync<Permissions.Microphone>();

            // Check if picking existing or not and ensure permission accordingly as they can be set independently from each other
            if (pickExisting && !Platform.HasOSVersion(11, 0))
                await Permissions.EnsureGrantedAsync<Permissions.Photos>();

            if (!pickExisting)
                await Permissions.EnsureGrantedAsync<Permissions.Camera>();

            var vc = Platform.GetCurrentViewController(true);

            picker = new UIImagePickerController();
            picker.SourceType = sourceType;
            picker.MediaTypes = new string[] { mediaType };
            picker.AllowsEditing = false;

            if (!photo && !pickExisting)
                picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Video;

            if (!string.IsNullOrWhiteSpace(options?.Title))
                picker.Title = options.Title;

            if (DeviceInfo.Idiom == DeviceIdiom.Tablet && picker.PopoverPresentationController != null && vc.View != null)
                picker.PopoverPresentationController.SourceRect = vc.View.Bounds;

            var tcs = new TaskCompletionSource<FileResult>(picker);
            picker.Delegate = new PhotoPickerDelegate
            {
                CompletedHandler = info => GetFileResult(info, tcs)
            };

            if (picker.PresentationController != null)
            {
                picker.PresentationController.Delegate = new PhotoPickerPresentationControllerDelegate
                {
                    CompletedHandler = info => GetFileResult(info, tcs)
                };
            }

            await vc.PresentViewControllerAsync(picker, true);

            var result = await tcs.Task;

            await vc.DismissViewControllerAsync(true);

            picker?.Dispose();
            picker = null;

            return result;
        }

        static void GetFileResult(NSDictionary info, TaskCompletionSource<FileResult> tcs)
        {
            try
            {
                tcs.TrySetResult(DictionaryToFileResult(info));
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        static FileResult DictionaryToFileResult(NSDictionary info)
        {
            if (info == null)
                return null;

            PHAsset phAsset = null;
            NSUrl assetUrl = null;

            if (Platform.HasOSVersion(11, 0))
            {
                assetUrl = info[UIImagePickerController.ImageUrl] as NSUrl;

                // Try the MediaURL sometimes used for videos
                if (assetUrl == null)
                    assetUrl = info[UIImagePickerController.MediaURL] as NSUrl;

                if (assetUrl != null)
                {
                    if (!assetUrl.Scheme.Equals("assets-library", StringComparison.InvariantCultureIgnoreCase))
                        return new UIDocumentFileResult(assetUrl);

                    phAsset = info.ValueForKey(UIImagePickerController.PHAsset) as PHAsset;
                }
            }

            if (phAsset == null)
            {
                assetUrl = info[UIImagePickerController.ReferenceUrl] as NSUrl;

                if (assetUrl != null)
                    phAsset = PHAsset.FetchAssets(new NSUrl[] { assetUrl }, null)?.LastObject as PHAsset;
            }

            if (phAsset == null || assetUrl == null)
            {
                var img = info.ValueForKey(UIImagePickerController.OriginalImage) as UIImage;

                if (img != null)
                    return new UIImageFileResult(img);
            }

            if (phAsset == null || assetUrl == null)
                return null;

            string originalFilename;

            if (Platform.HasOSVersion(9, 0))
                originalFilename = PHAssetResource.GetAssetResources(phAsset).FirstOrDefault()?.OriginalFilename;
            else
                originalFilename = phAsset.ValueForKey(new NSString("filename")) as NSString;

            return new PHAssetFileResult(assetUrl, phAsset, originalFilename);
        }

        class PhotoPickerDelegate : UIImagePickerControllerDelegate
        {
            public Action<NSDictionary> CompletedHandler { get; set; }

            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info) =>
                CompletedHandler?.Invoke(info);

            public override void Canceled(UIImagePickerController picker) =>
                CompletedHandler?.Invoke(null);
        }

        class PhotoPickerPresentationControllerDelegate : UIAdaptivePresentationControllerDelegate
        {
            public Action<NSDictionary> CompletedHandler { get; set; }

            public override void DidDismiss(UIPresentationController presentationController) =>
                CompletedHandler?.Invoke(null);
        }

        static async Task<IEnumerable<FileResult>> PhotosAsync(MediaPickerOptions options, MultiPickerOptions pickerOptions)
        {
            var config = new PHPickerConfiguration
            {
                SelectionLimit = 3,
                Filter = PHPickerFilter.ImagesFilter
            };

            var picker = new PHPickerViewController(config);

            if (!string.IsNullOrWhiteSpace(options?.Title))
                picker.Title = options.Title;

            var tcs = new TaskCompletionSource<IEnumerable<FileResult>>(picker);

            picker.Delegate = new PhotosPickerDelegate()
            {
                CompletedHandler = results => GetFileResults(results, tcs)
            };

            var vc = Platform.GetCurrentViewController(true);

            await vc.PresentViewControllerAsync(picker, true);

            var result = await tcs.Task;

            await vc.DismissViewControllerAsync(true);

            picker?.Dispose();
            picker = null;

            return result;
        }

        static void GetFileResults(PHPickerResult[] results, TaskCompletionSource<IEnumerable<FileResult>> tcs)
        {
            try
            {
                var fileResults = new List<FileResult>();
                foreach (var result in results)
                {
                    foreach (var registeredItemType in result.ItemProvider.RegisteredTypeIdentifiers)
                    {
                        result.ItemProvider.LoadFileRepresentation(registeredItemType, (url, error) =>
                        {
                            if (error != null || url == null)
                                return;

                            FileResult fileResult = new NSUrlFileResult(url);
                            var taskCompletionSource = new TaskCompletionSource<FileResult>();
                            taskCompletionSource.SetResult(fileResult);
                            fileResults.Add(taskCompletionSource.Task.Result);
                        });
                    }
                }
                tcs.TrySetResult(fileResults);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        class PhotosPickerDelegate : PHPickerViewControllerDelegate
        {
            public Action<PHPickerResult[]> CompletedHandler { get; set; }

            public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results) => CompletedHandler?.Invoke(results);
        }
    }
}
