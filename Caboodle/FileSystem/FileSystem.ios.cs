﻿using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;

namespace Microsoft.Caboodle
{
	public static partial class FileSystem
	{
		private static string cache;
		private static string appData;
		private static string userData;

		public static string CacheDirectory
			=> cache ?? (cache = GetNSDirectory(NSSearchPathDirectory.CachesDirectory));

		public static string AppDataDirectory
			=> appData ?? (appData = GetNSDirectory(NSSearchPathDirectory.ApplicationSupportDirectory));

		public static string UserDataDirectory
			=> userData ?? (userData = GetNSDirectory(NSSearchPathDirectory.DocumentDirectory));

		public static Task<Stream> OpenAppBundleFileAsync(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			var dir = Path.GetDirectoryName(filename);
			var file = Path.GetFileNameWithoutExtension(filename);
			var ext = Path.GetExtension(filename);
			if (string.IsNullOrEmpty(ext))
				ext = null;
			else
				ext = ext.Substring(1);

			var bundle = NSBundle.MainBundle.PathForResource(Path.Combine(dir, file), ext);
			return Task.FromResult((Stream)File.OpenRead(bundle));
		}

		static string GetNSDirectory(NSSearchPathDirectory directory)
		{
			var urls = NSFileManager.DefaultManager.GetUrls(directory, NSSearchPathDomain.User);
			if (urls == null || urls.Length == 0)
			{
				throw new ArgumentException(nameof(directory));
			}

			return urls[0].Path;
		}
	}
}
