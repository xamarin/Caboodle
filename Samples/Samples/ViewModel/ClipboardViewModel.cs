﻿using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    class ClipboardViewModel : BaseViewModel
    {
        string fieldValue;
        string lastCopied;

        public ClipboardViewModel()
        {
            CopyCommand = new Command(OnCopy);
            PasteCommand = new Command(OnPaste);
        }

        public ICommand CopyCommand { get; }

        public ICommand PasteCommand { get; }

        public string FieldValue
        {
            get => fieldValue;
            set => SetProperty(ref fieldValue, value);
        }

        public string LastCopied
        {
            get => lastCopied;
            set => SetProperty(ref lastCopied, value);
        }

        public override void OnAppearing()
        {
            Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
        }

        public override void OnDisappearing()
        {
            Clipboard.ClipboardContentChanged -= OnClipboardContentChanged;
        }

        void OnClipboardContentChanged(object sender, EventArgs args)
        {
            LastCopied = $"Last copied text at {DateTime.UtcNow:T}";
        }

        async void OnCopy() => await Clipboard.SetTextAsync(FieldValue);

        async void OnPaste()
        {
            var text = await Clipboard.GetTextAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                FieldValue = text;
            }
        }
    }
}
