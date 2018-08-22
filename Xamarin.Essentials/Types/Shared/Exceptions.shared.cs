﻿using System;

namespace Xamarin.Essentials
{
    public class NotImplementedInReferenceAssemblyException : NotImplementedException
    {
        public NotImplementedInReferenceAssemblyException()
            : base("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.")
        {
        }
    }

    public class PermissionException : UnauthorizedAccessException
    {
        public PermissionException(string message)
            : base(message)
        {
        }
    }

    public class FeatureNotSupportedException : NotSupportedException
    {
        public FeatureNotSupportedException()
        {
        }

        public FeatureNotSupportedException(string message)
            : base(message)
        {
        }

        public FeatureNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class FeatureNotEnabledException : InvalidOperationException
    {
        public FeatureNotEnabledException()
        {
        }

        public FeatureNotEnabledException(string message)
            : base(message)
        {
        }

        public FeatureNotEnabledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
