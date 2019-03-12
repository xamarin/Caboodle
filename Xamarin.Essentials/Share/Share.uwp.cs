﻿using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Xamarin.Essentials
{
    public static partial class Share
    {
        static Task PlatformRequestAsync(ShareTextRequest request)
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += ShareTextHandler;

            DataTransferManager.ShowShareUI();

            void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
            {
                var newRequest = e.Request;

                newRequest.Data.Properties.Title = request.Title ?? AppInfo.Name;

                if (!string.IsNullOrWhiteSpace(request.Text))
                {
                    newRequest.Data.SetText(request.Text);
                }

                if (!string.IsNullOrWhiteSpace(request.Uri))
                {
                    newRequest.Data.SetWebLink(new Uri(request.Uri));
                }

                dataTransferManager.DataRequested -= ShareTextHandler;
            }

            return Task.CompletedTask;
        }

        static async Task PlatformRequestAsync(ShareFileRequest request)
        {
            var storageFile = request.File.File ?? await StorageFile.GetFileFromPathAsync(request.File.FullPath);

            var dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += ShareTextHandler;

            DataTransferManager.ShowShareUI();

            void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
            {
                var newRequest = e.Request;

                newRequest.Data.SetStorageItems(new IStorageItem[] { storageFile });
                newRequest.Data.Properties.Title = request.Title ?? AppInfo.Name;

                dataTransferManager.DataRequested -= ShareTextHandler;
            }
        }
    }
}
