using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ecoinv.Common
{
  public class SimpleModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public virtual bool SetPropertyValue<T>(string name, ref T field, T value)
    {
      if (EqualityComparer<T>.Default.Equals(field, value))
        return false;

      field = value;
      OnPropertyChanged(name);

      return true;
    }

    public virtual bool SetPropertyValue<T>(string name, Action<T> setValue, T value)
    {
      setValue(value);
      OnPropertyChanged(name);

      return true;
    }
  }
}