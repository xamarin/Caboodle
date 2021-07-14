using System;
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
        static UIViewController pickerRef;

        static bool PlatformIsCaptureSupported
            => UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);

        static Task<FileResult> PlatformPickPhotoAsync(MediaPickerOptions options)
            => PhotoAsync(options, true, true);

        static Task<FileResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
            => PhotoAsync(options, true, false);

        static Task<FileResult> PlatformPickVideoAsync(MediaPickerOptions options)
            => PhotoAsync(options, false, true);

        static Task<FileResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
            => PhotoAsync(options, false, false);

        static async Task<FileResult> PhotoAsync(MediaPickerOptions options, bool photo, bool pickExisting)
        {
           // microphone only needed if video will be captured
            if (!photo && !pickExisting)
                await Permissions.EnsureGrantedAsync<Permissions.Microphone>();

            // Check if picking existing or not and ensure permission accordingly as they can be set independently from each other
            if (pickExisting && !Platform.HasOSVersion(11, 0))
                await Permissions.EnsureGrantedAsync<Permissions.Photos>();

            if (!pickExisting)
                await Permissions.EnsureGrantedAsync<Permissions.Camera>();

            var vc = Platform.GetCurrentViewController(true);
            var tcs = new TaskCompletionSource<FileResult>();

            if (pickExisting && Platform.HasOSVersion(14, 0))
            {
                var config = new PHPickerConfiguration();
                config.Filter = photo
                    ? PHPickerFilter.ImagesFilter
                    : PHPickerFilter.VideosFilter;

                var picker = new PHPickerViewController(config);
                picker.Delegate = new PPD
                {
                    CompletedHandler = res =>
                        tcs.TrySetResult(PickerResultsToMediaFile(res))
                };

                pickerRef = picker;
            }
            else
            {
                var sourceType = pickExisting ? UIImagePickerControllerSourceType.PhotoLibrary : UIImagePickerControllerSourceType.Camera;
                var mediaType = photo ? UTType.Image : UTType.Movie;

                if (!UIImagePickerController.IsSourceTypeAvailable(sourceType))
                    throw new FeatureNotSupportedException();
                if (!UIImagePickerController.AvailableMediaTypes(sourceType).Contains(mediaType))
                    throw new FeatureNotSupportedException();

                var picker = new UIImagePickerController();
                picker.SourceType = sourceType;
                picker.MediaTypes = new string[] { mediaType };
                picker.AllowsEditing = false;
                if (!photo && !pickExisting)
                    picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Video;

                pickerRef = picker;

                picker.Delegate = new PhotoPickerDelegate
                {
                    CompletedHandler = async info =>
                    {
                        GetFileResult(info, tcs);
                        await vc.DismissViewControllerAsync(true);
                    }
                };
            }

            if (!string.IsNullOrWhiteSpace(options?.Title))
                pickerRef.Title = options.Title;

            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                pickerRef.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;

            if (pickerRef.PresentationController != null)
            {
                pickerRef.PresentationController.Delegate = new PhotoPickerPresentationControllerDelegate
                {
                    Handler = () => tcs.TrySetResult(null)
                };
            }

            await vc.PresentViewControllerAsync(pickerRef, true);

            var result = await tcs.Task;

            pickerRef?.Dispose();
            pickerRef = null;

            return result;
        }

        static void GetFileResult(NSDictionary info, TaskCompletionSource<FileResult> tcs)
        {
            try
            {
                tcs.TrySetResult(DictionaryToMediaFile(info));
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        static FileResult DictionaryToMediaFile(NSDictionary info)
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

        static FileResult PickerResultsToMediaFile(PHPickerResult[] results)
        {
            var file = results?.FirstOrDefault();

            return file == null
                ? null
                : new PHPickerFileResult(file.ItemProvider);
        }

        class PhotoPickerDelegate : UIImagePickerControllerDelegate
        {
            public Action<NSDictionary> CompletedHandler { get; set; }

            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
                picker.DismissViewController(true, null);
                CompletedHandler?.Invoke(info);
            }

            public override void Canceled(UIImagePickerController picker)
            {
                picker.DismissViewController(true, null);
                CompletedHandler?.Invoke(null);
            }
        }

        class PhotoPickerPresentationControllerDelegate : UIAdaptivePresentationControllerDelegate
        {
            public Action Handler { get; set; }

            public override void DidDismiss(UIPresentationController presentationController) =>
                Handler?.Invoke();

            protected override void Dispose(bool disposing)
            {
                Handler?.Invoke();
                base.Dispose(disposing);
            }
        }

        class PPD : PHPickerViewControllerDelegate
        {
            public Action<PHPickerResult[]> CompletedHandler { get; set; }

            public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
            {
                picker.DismissViewController(true, null);
                CompletedHandler?.Invoke(results?.Length > 0 ? results : null);
            }
        }
    }
}
