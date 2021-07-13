using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Foundation;
using Security;

namespace Xamarin.Essentials
{
    public static partial class SecureStorage
    {
        public static SecAccessible DefaultAccessible { get; set; } =
            SecAccessible.AfterFirstUnlock;

        public static Task SetAsync(string key, string value, SecAccessible accessible)
             => SetAsync(key, value, accessible, null);

        public static Task SetAsync(string key, string value, SecAccessible accessible, string accessGroup)
        {
            var alias = GetAlias(accessGroup);
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var kc = new KeyChain(accessible, accessGroup);
            kc.SetValueForKey(value, key, alias);

            return Task.CompletedTask;
        }

        static Task<string> PlatformGetAsync(string key, string accessGroup)
        {
            var alias = GetAlias(accessGroup);
            var kc = new KeyChain(DefaultAccessible, accessGroup);
            var value = kc.ValueForKey(key, alias);

            return Task.FromResult(value);
        }

        static Task PlatformSetAsync(string key, string data, string accessGroup) =>
            SetAsync(key, data, DefaultAccessible, accessGroup);

        static bool PlatformRemove(string key, string accessGroup)
        {
            var alias = GetAlias(accessGroup);
            var kc = new KeyChain(DefaultAccessible, accessGroup);

            return kc.Remove(key, alias);
        }

        static void PlatformRemoveAll(string accessGroup)
        {
            var alias = GetAlias(accessGroup);
            var kc = new KeyChain(DefaultAccessible, accessGroup);

            kc.RemoveAll(alias);
        }
    }

    class KeyChain
    {
        readonly SecAccessible accessible;
        readonly string accessGroup;

        internal KeyChain(SecAccessible accessible, string accessGroup)
        {
            this.accessible = accessible;
            this.accessGroup = accessGroup;
        }

        SecRecord ExistingRecordForKey(string key, string service)
        {
            var rec = new SecRecord(SecKind.GenericPassword)
            {
                Account = key,
                Service = service
            };

            if (!string.IsNullOrWhiteSpace(accessGroup))
                rec.AccessGroup = accessGroup;

            return rec;
        }

        internal string ValueForKey(string key, string service)
        {
            using var record = ExistingRecordForKey(key, service);
            using var match = SecKeyChain.QueryAsRecord(record, out var resultCode);
            if (resultCode == SecStatusCode.Success)
                return NSString.FromData(match.ValueData, NSStringEncoding.UTF8);
            else
                return null;
        }

        internal void SetValueForKey(string value, string key, string service)
        {
            using (var record = ExistingRecordForKey(key, service))
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!string.IsNullOrEmpty(ValueForKey(key, service)))
                        RemoveRecord(record);

                    return;
                }

                // if the key already exists, remove it
                if (!string.IsNullOrEmpty(ValueForKey(key, service)))
                    RemoveRecord(record);
            }

            using var newRecord = CreateRecordForNewKeyValue(key, value, service);
            var result = SecKeyChain.Add(newRecord);

            switch (result)
            {
                case SecStatusCode.DuplicateItem:
                    {
                        Debug.WriteLine("Duplicate item found. Attempting to remove and add again.");

                        // try to remove and add again
                        if (Remove(key, service))
                        {
                            result = SecKeyChain.Add(newRecord);
                            if (result != SecStatusCode.Success)
                                throw new Exception($"Error adding record: {result}");
                        }
                        else
                        {
                            Debug.WriteLine("Unable to remove key.");
                        }
                    }
                    break;
                case SecStatusCode.Success:
                    return;
                default:
                    throw new Exception($"Error adding record: {result}");
            }
        }

        internal bool Remove(string key, string service)
        {
            using (var record = ExistingRecordForKey(key, service))
            using (var match = SecKeyChain.QueryAsRecord(record, out var resultCode))
            {
                if (resultCode == SecStatusCode.Success)
                {
                    RemoveRecord(record);
                    return true;
                }
            }
            return false;
        }

        internal void RemoveAll(string service)
        {
            using var query = new SecRecord(SecKind.GenericPassword) { Service = service };
            if (!string.IsNullOrWhiteSpace(accessGroup))
                query.AccessGroup = accessGroup;

            SecKeyChain.Remove(query);
        }

        SecRecord CreateRecordForNewKeyValue(string key, string value, string service)
        {
            var rec = new SecRecord(SecKind.GenericPassword)
            {
                Account = key,
                Service = service,
                Label = key,
                Accessible = accessible,
                ValueData = NSData.FromString(value, NSStringEncoding.UTF8),
            };

            if (!string.IsNullOrWhiteSpace(accessGroup))
                rec.AccessGroup = accessGroup;

            return rec;
        }

        bool RemoveRecord(SecRecord record)
        {
            var result = SecKeyChain.Remove(record);
            if (result != SecStatusCode.Success && result != SecStatusCode.ItemNotFound)
                throw new Exception($"Error removing record: {result}");

            return true;
        }
    }
}
