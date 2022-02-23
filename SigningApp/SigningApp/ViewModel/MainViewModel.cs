using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Xamarin.Forms;
using System.Runtime.CompilerServices;

namespace XamarinLicentaApp.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            MenuList = GetMenus();
        }

        public Menu selectedMenuOption;

        public Menu SelectedMenuOption
        {
            get { return selectedMenuOption; }
            set { selectedMenuOption = value; }
        }


        private ObservableCollection<Menu> menuList;
        public ObservableCollection<Menu> MenuList
        {
            get { return menuList; }
            set { menuList = value; }
        }

        private ObservableCollection<Menu> GetMenus()
        {
            return new ObservableCollection<Menu>
            {
                new Menu { Icon = "pdf.png", Name = "Sign PDF"},
                new Menu { Icon = "xml.png", Name = "Sign XML"},
                new Menu { Icon = "pdf.png", Name = "Keys Info"},
                new Menu { Icon = "logout.png", Name = "Logout"}
            };
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

    }

    public class Menu
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
