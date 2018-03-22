﻿using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Caboodle.Tests
{
    public class SecureStorage_Tests
    {
        [Fact]
        public async Task SecureStorage_LoadAsync_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => SecureStorage.GetAsync("filename.txt"));
        }

        [Fact]
        public async Task SecureStorage_SaveAsync_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => SecureStorage.SetAsync("filename.txt", "data"));
        }
    }
}
