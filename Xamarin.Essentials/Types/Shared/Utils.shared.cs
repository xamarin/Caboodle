﻿using System;
#if !NETSTANDARD1_0
using System.Security.Cryptography;
#endif
using System.Text;
using System.Threading;

namespace Xamarin.Essentials
{
    internal class Utils
    {
        public static Version ParseVersion(string version)
        {
            if (Version.TryParse(version, out var number))
                return number;

            return new Version(0, 0);
        }

        public static string Md5Hash(string input)
        {
#if NETSTANDARD1_0
            throw new NotImplementedInReferenceAssemblyException();
#else
            var hash = new StringBuilder();
            var md5provider = new MD5CryptoServiceProvider();
            var bytes = md5provider.ComputeHash(Encoding.UTF8.GetBytes(input));

            for (var i = 0; i < bytes.Length; i++)
                hash.Append(bytes[i].ToString("x2"));

            return hash.ToString();
#endif
        }

        public static CancellationToken TimeoutToken(CancellationToken cancellationToken, TimeSpan timeout)
        {
            // create a new linked cancellation token source
            var cancelTokenSrc = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // if a timeout was given, make the token source cancel after it expires
            if (timeout > TimeSpan.Zero)
                cancelTokenSrc.CancelAfter(timeout);

            // our Cancel method will handle the actual cancellation logic
            return cancelTokenSrc.Token;
        }
    }
}
