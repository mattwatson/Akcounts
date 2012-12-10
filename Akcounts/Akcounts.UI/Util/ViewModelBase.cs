using System;
using System.ComponentModel;

namespace Akcounts.UI.Util
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public virtual string DisplayName { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        public void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                var msg = "Invalid property name: " + propertyName;
                throw new Exception(msg);
            }
        }
        
        public void Dispose()
        {
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }
    }
}
