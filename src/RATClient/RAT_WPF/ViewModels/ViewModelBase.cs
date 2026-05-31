using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_WPF.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        // Used MVVM Tutorials by: https://www.youtube.com/@SingletonSean

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
