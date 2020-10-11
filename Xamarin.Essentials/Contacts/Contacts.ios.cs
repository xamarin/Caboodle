﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contacts;
using ContactsUI;

namespace Xamarin.Essentials
{
    public static partial class Contacts
    {
        static Task<Contact> PlatformPickContactAsync()
        {
            var uiView = Platform.GetCurrentViewController();
            if (uiView == null)
                throw new ArgumentNullException($"The View Controller can't be null.");

            var source = new TaskCompletionSource<Contact>();

            using var picker = new CNContactPickerViewController
            {
                Delegate = new ContactPickerDelegate(phoneContact =>
                    source?.TrySetResult(Contacts.ConvertContact(phoneContact)))
            };

            uiView.PresentViewController(picker, true, null);

            return source.Task;
        }

        // static IEnumerable<Contact> PlatformGetAllAsync()
        // {
        //    // await Task.CompletedTask;
        //    // return GetAll();
        //    return null;
        // }

        static IEnumerable<Contact> PlatformGetAllAsync()
        {
            var keys = new[]
            {
                CNContactKey.NamePrefix,
                CNContactKey.GivenName,
                CNContactKey.MiddleName,
                CNContactKey.FamilyName,
                CNContactKey.NameSuffix,
                CNContactKey.EmailAddresses,
                CNContactKey.PhoneNumbers,
                CNContactKey.Type
            };

            // var results = new List<Contact>();
            using (var store = new CNContactStore())
            {
                var containers = store.GetContainers(null, out var error);

                foreach (var container in containers)
                {
                        using var pred = CNContact.GetPredicateForContactsInContainer(container.Identifier);

                        // results.AddRange(store.GetUnifiedContacts(pred, keys, out error)?.Select(a => ConvertContact(a)));
                        var contacts = store.GetUnifiedContacts(pred, keys, out error);
                        foreach (var item in contacts)
                            yield return ConvertContact(item);
                }
            }

            // return results;
        }

        internal static Contact ConvertContact(CNContact contact)
        {
            if (contact == null)
                return default;

            try
            {
                var contactType = ToPhoneContact(contact.ContactType);
                var phones = new List<ContactPhone>();

                foreach (var item in contact.PhoneNumbers)
                    phones.Add(new ContactPhone(item?.Value?.StringValue, contactType));

                var emails = new List<ContactEmail>();

                foreach (var item in contact.EmailAddresses)
                    emails.Add(new ContactEmail(item?.Value?.ToString(), contactType));

                var name = string.Empty;

                // $"{item.NamePrefix} {item.GivenName} {item.MiddleName} {item.FamilyName} {item.NameSuffix}"
                if (string.IsNullOrEmpty(contact.MiddleName))
                    name = $"{contact.GivenName} {contact.FamilyName}";
                else
                    name = $"{contact.GivenName} {contact.MiddleName} {contact.FamilyName}";

                return new Contact(name, phones, emails, contactType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                contact.Dispose();
            }
        }

        static ContactType ToPhoneContact(CNContactType type) => type switch
        {
            CNContactType.Person => ContactType.Personal,
            CNContactType.Organization => ContactType.Work,
            _ => ContactType.Unknown,
        };

        class ContactPickerDelegate : CNContactPickerDelegate
        {
            public ContactPickerDelegate(Action<CNContact> didSelectContactHandler) =>
                DidSelectContactHandler = didSelectContactHandler;

            public ContactPickerDelegate(IntPtr handle)
                : base(handle)
            {
            }

            public Action<CNContact> DidSelectContactHandler { get; }

            public override void ContactPickerDidCancel(CNContactPickerViewController picker)
            {
                DidSelectContactHandler?.Invoke(default);
                picker.DismissModalViewController(true);
            }

            public override void DidSelectContact(CNContactPickerViewController picker, CNContact contact)
            {
                DidSelectContactHandler?.Invoke(contact);
                picker.DismissModalViewController(true);
            }

            public override void DidSelectContactProperty(CNContactPickerViewController picker, CNContactProperty contactProperty) =>
                picker.DismissModalViewController(true);
        }
    }
}
