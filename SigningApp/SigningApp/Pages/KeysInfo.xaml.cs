using LicentaApp.JsonClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinLicentaApp.ViewModel;
using static XamarinLicentaApp.ViewModel.KeysInfoViewModel;

namespace XamarinLicentaApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KeysInfo : ContentPage
    {


        public KeysInfo()
        {
            InitializeComponent();
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.ShowDetails();
        }
    }
}