﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Share
    {
        public static Task RequestAsync(string text) =>
            RequestAsync(new ShareTextRequest(text));

        public static Task RequestAsync(string text, string title) =>
            RequestAsync(new ShareTextRequest(text, title));

        public static Task RequestAsync(ShareTextRequest request) =>
            PlatformRequestAsync(request);

        public static Task RequestAsync(ShareFileRequest request)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);

            return PlatformRequestAsync(request);
        }
    }

    public class ShareTextRequest
    {
        public ShareTextRequest()
        {
        }

        public ShareTextRequest(string text) => Text = text;

        public ShareTextRequest(string text, string title)
            : this(text) => Title = title;

        public string Title { get; set; }

        public string Subject { get; set; }

        public string Text { get; set; }

        public string Uri { get; set; }
    }

    public class ShareFileRequest
    {
        public ShareFileRequest()
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);
        }

        public ShareFileRequest(string title, ShareFile file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);
            Title = title;
            File = file;
        }

        public ShareFileRequest(string title, FileBase file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);
            Title = title;
            File = new ShareFile(file);
        }

        public ShareFileRequest(ShareFile file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);
            File = file;
        }

        public ShareFileRequest(FileBase file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);
            File = new ShareFile(file);
        }

        public string Title { get; set; }

        public ShareFile File { get; set; }
    }

    public class ShareFile : FileBase
    {
        public ShareFile(string fullPath)
            : base(fullPath)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);
        }

        public ShareFile(string fullPath, string contentType)
            : base(fullPath, contentType)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);
        }

        public ShareFile(FileBase file)
            : base(file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);
        }

        string attachmentName;

        public string AttachmentName
        {
            get => GetAttachmentName();
            set => attachmentName = value;
        }

        internal string GetAttachmentName()
        {
            // try the provided file name
            if (!string.IsNullOrWhiteSpace(attachmentName))
                return attachmentName;

            // try get from the path
            if (!string.IsNullOrWhiteSpace(FullPath))
                return Path.GetFileName(FullPath);

            // this should never happen as the path is validated in the constructor
            throw new InvalidOperationException($"Unable to determine the attachment file name from '{FullPath}'.");
        }
    }
}
