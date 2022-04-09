using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinLicentaApp;

namespace SigningApp.ViewModel
{
    public class MultipleXMLSignPageViewModel : ContentView, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private List<string> docsPath;

        public List<string> DocsPath
        {
            get { return docsPath; }
            set { docsPath = value; }
        }

        private List<string> GetDocsPath(IEnumerable<FileResult> collection)
        {
            List<string> newList = new List<string>();

            foreach (FileResult elem in collection)
            {
                newList.Add(elem.FullPath);
            }

            return newList;
        }

        private ObservableCollection<string> keys;
        public ObservableCollection<string> Keys
        {
            get { return keys; }
            set { keys = value; }
        }

        public string SelectedKey { get; set; }

        public string SelectedType { get; set; }

        private ObservableCollection<string> GetKeys()
        {
            LoginPage.user.credentialsList();

            ObservableCollection<string> newList = new ObservableCollection<string>();

            foreach (string elem in LoginPage.user.credentialsIDs)
            {
                newList.Add(elem);
            }

            return newList;
        }

        public MultipleXMLSignPageViewModel()
        {
            Keys = GetKeys();
        }

        private Command signDocs;

        public ICommand SignDocs
        {
            get
            {
                if (signDocs == null)
                {
                    signDocs = new Command(SignXMLsButtonClicked);
                }

                return signDocs;
            }
        }

        private Command pickDocs;

        public ICommand PickDocs
        {
            get
            {
                if (pickDocs == null)
                {
                    pickDocs = new Command(PickFiles);
                }
                return pickDocs;
            }
        }

        private async void PickFiles()
        {
            try
            {
                // pickMultipleAsync pentru mai tarziu
                var file = await FilePicker.PickMultipleAsync(
                    new PickOptions
                    {
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.UWP, new [] {".xml"} },
                            { DevicePlatform.Android, new [] { ".xml" } },
                            { DevicePlatform.macOS, new [] { ".xml" } },
                            { DevicePlatform.iOS, new [] { ".xml" } }
                        })
                    });

                if (file == null)
                {
                    return;
                }

                DocsPath = GetDocsPath(file);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void SignXMLsButtonClicked()
        {


        }

    }
}