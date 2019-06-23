﻿using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidEnvironment = Android.OS.Environment;

namespace Xamarin.Essentials
{
    [ContentProvider(
        new[] { "${applicationId}.fileProvider" },
        Name = "xamarin.essentials.fileProvider",
        Exported = false,
        GrantUriPermissions = true)]
    [MetaData(
        "android.support.FILE_PROVIDER_PATHS",
        Resource = "@xml/xamarin_essentials_fileprovider_file_paths")]
    public class FileProvider : global::Android.Support.V4.Content.FileProvider
    {
        internal static bool AlwaysFailExternalMediaAccess { get; set; } = false;

        // This allows us to override the default temporary file location of Preferring external but falling back to internal
        // We can choose external only, or internal only as alternative options
        public static FileProviderLocation TemporaryLocation { get; set; } = FileProviderLocation.PreferExternal;

        internal static Java.IO.File GetTemporaryDirectory()
        {
            var root = GetTemporaryRootDirectory();
            var dir = new Java.IO.File(root, "2203693cc04e0be7f4f024d5f9499e13");
            dir.Mkdirs();
            dir.DeleteOnExit();
            return dir;
        }

        internal static Java.IO.File GetTemporaryRootDirectory()
        {
            // If we specifically want the internal storage, no extra checks are needed, we have permission
            if (TemporaryLocation == FileProviderLocation.Internal)
                return Platform.AppContext.CacheDir;

            // If we explicitly want only external locations we need to do some permissions checking
            var externalOnly = TemporaryLocation == FileProviderLocation.External;

            // Check to see if we are >= API Level 19 (KitKat) since we don't need to declare the permission on these API levels to save to the external cache/storage
            // If we're not on 19 or higher we do need to check for permissions, but if we aren't limiting to external only, don't throw an exception if the
            // permission wasn't declared because we can always fall back to internal cache
            var hasPermission = Platform.HasApiLevel(BuildVersionCodes.Kitkat) || Permissions.EnsureDeclared(PermissionType.WriteExternalStorage, externalOnly);

            // make sure the external storage is available
            var hasExternalMedia = Platform.HasApiLevel(BuildVersionCodes.Lollipop)
                ? AndroidEnvironment.GetExternalStorageState(Platform.AppContext.ExternalCacheDir) == AndroidEnvironment.MediaMounted
#pragma warning disable CS0618 // Type or member is obsolete
                : AndroidEnvironment.GetStorageState(Platform.AppContext.ExternalCacheDir) == AndroidEnvironment.MediaMounted;
#pragma warning restore CS0618 // Type or member is obsolete

            // undo all the work if we have requested a fail (mainly for testing)
            if (AlwaysFailExternalMediaAccess)
                hasExternalMedia = false;

            // fail if we need the external storage, but there is none
            if (externalOnly && !hasExternalMedia)
                throw new InvalidOperationException("Unable to access the external storage, the media is not mounted.");

            // based on permssions, return the correct directory
            // if permission were required, then it would have already thrown
            return hasPermission && hasExternalMedia
                ? Platform.AppContext.ExternalCacheDir
                : Platform.AppContext.CacheDir;
        }

        internal static bool IsFileInPublicLocation(string filename)
        {
            // get the Android path, we use "CanonicalPath" instead of "AbsolutePath"
            // because we want to resolve any ".." and links/redirects
            var file = new Java.IO.File(filename);
            filename = file.CanonicalPath;

            // the shared paths from the "xamarin_essentials_fileprovider_file_paths.xml" resource
            var publicLocations = new List<string>
            {
                AndroidEnvironment.ExternalStorageDirectory.CanonicalPath,
                Platform.AppContext.ExternalCacheDir.CanonicalPath
            };

            // the internal cache path is available only by file provider in N+
            if (Platform.HasApiLevelN)
                publicLocations.Add(Platform.AppContext.CacheDir.CanonicalPath);

            foreach (var location in publicLocations)
            {
                // make sure we have a trailing slash
                var suffixedPath = filename.EndsWith(Java.IO.File.Separator)
                    ? filename
                    : filename + Java.IO.File.Separator;

                // check if the requested file is in a folder
                if (suffixedPath.StartsWith(location, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }

    public enum FileProviderLocation
    {
        PreferExternal,
        Internal,
        External,
    }
}
