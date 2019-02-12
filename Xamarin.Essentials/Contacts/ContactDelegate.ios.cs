﻿using System;
using Contacts;
using ContactsUI;

namespace Xamarin.Essentials
{
    sealed class ContactSaveDelegate : CNContactViewControllerDelegate
    {
        public ContactSaveDelegate()
        {
        }

        public override void DidComplete(CNContactViewController viewController, CNContact contact)
        {
            var parent = Platform.GetCurrentViewController(true);
            parent.DismissViewController(true, null);
        }
    }

    sealed class ContactPickerDelegate : CNContactPickerDelegate
    {
        public ContactPickerDelegate()
        {
        }

        public ContactPickerDelegate(IntPtr handle)
            : base(handle)
        {
        }

        public override void ContactPickerDidCancel(CNContactPickerViewController picker)
        {
            Console.WriteLine("User canceled picker");
            Contact.CallBack(Contact.GetContact(null));
        }

        public override void DidSelectContact(CNContactPickerViewController picker, CNContact contact)
        {
            Console.WriteLine("Selected: {0}", contact);
            Contact.CallBack(Contact.GetContact(contact));
        }
    }
}
