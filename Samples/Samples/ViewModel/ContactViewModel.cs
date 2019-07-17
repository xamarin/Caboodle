﻿using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Samples.ViewModel
{
    public class ContactViewModel : BaseViewModel
    {
        string name;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        string birthday;

        public string Birthday
        {
            get => birthday;
            set => SetProperty(ref birthday, value);
        }

        string phones;

        public string Phones
        {
            get => phones;
            set => SetProperty(ref phones, value);
        }

        string emails;

        public string Emails
        {
            get => emails;
            set => SetProperty(ref emails, value);
        }

        string contactName;

        public string ContactName
        {
            get => contactName;
            set => SetProperty(ref contactName, value);
        }

        string contactNumber;

        public string ContactNumber
        {
            get => contactNumber;
            set => SetProperty(ref contactNumber, value);
        }

        string contactEmail;

        public string ContactEmail
        {
            get => contactEmail;
            set => SetProperty(ref contactEmail, value);
        }

        public ICommand GetContactCommand { get; }

        public ICommand SaveContactCommand { get; }

        public ContactViewModel()
        {
            GetContactCommand = new Command(OnGetContact);
            SaveContactCommand = new Command(OnSaveContact);
        }

        async void OnSaveContact()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                // TODO verificar se vale a pena criar um SaveCOntact que leva o phoneContact
                await Contact.SaveContactAsync(ContactName, ContactNumber, ContactEmail);

                ContactName = string.Empty;
                ContactNumber = string.Empty;
                ContactEmail = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync($"Error:{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async void OnGetContact()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                Phones = string.Empty;
                Emails = string.Empty;
                Name = string.Empty;
                Birthday = string.Empty;

                var contact = await Contact.PickContactAsync();
                contact.Numbers.ForEach(item =>
                {
                    item.ForEach(v => Phones += v + Environment.NewLine);
                });
                contact.Emails.ForEach(item =>
                {
                    item.ForEach(v => Emails += v + Environment.NewLine);
                });
                Name = contact.Name;
                Birthday = contact.Birthday.ToString();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync($"Error:{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
