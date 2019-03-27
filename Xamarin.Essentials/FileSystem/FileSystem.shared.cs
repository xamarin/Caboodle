﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        public static string CacheDirectory
            => PlatformCacheDirectory;

        public static string AppDataDirectory
            => PlatformAppDataDirectory;

        public static Task<Stream> OpenAppPackageFileAsync(string filename)
            => PlatformOpenAppPackageFileAsync(filename);
    }

    public abstract partial class FileBase
    {
        internal const string DefaultContentType = "application/octet-stream";

        string contentType;

        internal FileBase(string fullPath)
        {
            if (fullPath == null)
                throw new ArgumentNullException(nameof(fullPath));
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("The attachment file path cannot be an empty string.", nameof(fullPath));
            if (string.IsNullOrWhiteSpace(Path.GetFileName(fullPath)))
                throw new ArgumentException("The attachment file path must be a file path.", nameof(fullPath));

            FullPath = fullPath;
        }

        public FileBase(FileBase file)
        {
            FullPath = file.FullPath;
            ContentType = file.ContentType;
            PlatformInit(file);
        }

        internal FileBase(string fullPath, string contentType)
            : this(fullPath)
        {
            FullPath = fullPath;
            ContentType = contentType;
        }

        public string FullPath { get; }

        public string ContentType
        {
            get => GetContentType();
            set => contentType = value;
        }

        internal string GetContentType()
        {
            // try the provided type
            if (!string.IsNullOrWhiteSpace(contentType))
                return contentType;

            // try get from the file extension
            var ext = Path.GetExtension(FullPath);
            if (!string.IsNullOrWhiteSpace(ext))
            {
                var content = PlatformGetContentType(ext);
                if (!string.IsNullOrWhiteSpace(content))
                    return content;
            }

            // we haven't been able to determine this
            // leave it up to the sender
            return null;
        }
    }
}
