using System.Windows;
using System.Windows.Controls;

namespace Ecoinv.Components
{
  /// <summary>
  /// Interaction logic for PasswordBox2.xaml
  /// </summary>
  public partial class PasswordBox2 : UserControl
  {
    public PasswordBox2()
    {
      InitializeComponent();
    }


    #region ... PasswordPorperty DependencyProperty ...

    private bool _isPasswordChanging;

    public string Password
    {
      get { return (string)GetValue(PasswordPorperty); }
      set { SetValue(PasswordPorperty, value); }
    }

    public int MaxLength
    {
      get { return (int)GetValue(MaxLengthPorperty); }
      set { SetValue(MaxLengthPorperty, value); }
    }

    public static readonly DependencyProperty PasswordPorperty =
      DependencyProperty.Register("Password", typeof(string), typeof(PasswordBox2), new PropertyMetadata(string.Empty, PasswordPropertyChanged));

    private static void PasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is PasswordBox2 passwordBox)
        passwordBox.UpdatePassword();
    }

    private void UpdatePassword()
    {
      if (!_isPasswordChanging)
        passwordBox.Password = Password;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      _isPasswordChanging = true;
      Password = passwordBox.Password;
      _isPasswordChanging = false;
    }

    public static readonly DependencyProperty MaxLengthPorperty =
      DependencyProperty.Register("MaxLength", typeof(int), typeof(PasswordBox2), new PropertyMetadata(32));

    #endregion


  }
}
