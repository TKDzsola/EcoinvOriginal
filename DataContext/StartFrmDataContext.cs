using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Ecoinv.Components;
using Ecoinv.Forms;

namespace Ecoinv.DataContext
{
  public class StartFrmDataContext : DataContextBase
  {
    public StartFrmDataContext()
    {
      
    }


    #region ... VersionStr property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __versionStr;

    public string VersionStr
    {
      get
      {
        try
        {
          __versionStr = "Verzió: " + System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version.ToString(); 
        }
        catch (Exception)
        {
          __versionStr = "Nincs kinyerhető verziószám!";
        }

        return __versionStr;
      }
      set => SetPropertyValue(nameof(VersionStr), ref __versionStr, value);
    }

    #endregion ... end of VersionStr property ...

    




    #region ... CommandMenuQuit property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandMenuQuit;

    public ICommand CommandMenuQuit => __commandMenuQuit ??= new DelegateCommand(ac => MenuQuitExecute(), fc => GetMenuQuitCanExecute());

    private static bool GetMenuQuitCanExecute() => true;

    private static void MenuQuitExecute()
    {
      for (var intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
      {
        var aktform = Application.Current.Windows[intCounter];
        aktform?.Close();
      }
    }

    #endregion ... end of CommandMenuQuit property ...

    #region ... CommandMenuCREATEINVOICEClick property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandMenuCREATEINVOICEClick;

    public ICommand CommandMenuCREATEINVOICEClick => __commandMenuCREATEINVOICEClick ??= new DelegateCommand(ac => MenuCREATEINVOICEExecute(), fc => GetMenuCREATEINVOICECanExecute());

    private static bool GetMenuCREATEINVOICECanExecute() => true;

    private static void MenuCREATEINVOICEExecute()
    {
      var vanform = false;
      for (var intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
      {
        var aktform = Application.Current.Windows[intCounter];
        if (aktform?.ToString() == "Ecoinv.Forms.CREATEINVOICEFrm")
        {
          vanform = true;
        }
      }

      if (!vanform)
      {
        var alkfrm = new CREATEINVOICEFrm();
        alkfrm.Show();
      }
    }

    #endregion ... end of CommandMenuCLIENTSClick property ...

    #region ... CommandMenuSEARCHINVOICEClick property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandMenuSEARCHINVOICEClick;

    public ICommand CommandMenuSEARCHINVOICEClick => __commandMenuSEARCHINVOICEClick ??= new DelegateCommand(ac => MenuSEARCHINVOICEExecute(), fc => GetMenuSEARCHINVOICECanExecute());

    private static bool GetMenuSEARCHINVOICECanExecute() => true;

    private static void MenuSEARCHINVOICEExecute()
    {
      var vanform = false;
      for (var intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
      {
        var aktform = Application.Current.Windows[intCounter];
        if (aktform?.ToString() == "Ecoinv.Forms.SEARCHINVOICEFrm")
        {
          vanform = true;
        }
      }

      if (!vanform)
      {
        var alkfrm = new SEARCHINVOICEFrm();
        alkfrm.Show();
      }
    }

    #endregion ... end of CommandMenuSEARCHINVOICEClick property ...


    #region ... CommandMenuEUSERSClick property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandMenuEUSERSClick;

    public ICommand CommandMenuEUSERSClick => __commandMenuEUSERSClick ??= new DelegateCommand(ac => MenuEUSERSExecute(), fc => GetMenuEUSERSCanExecute());

    private static bool GetMenuEUSERSCanExecute() => true;

    private static void MenuEUSERSExecute()
    {
      var vanform = false;
      for (var intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
      {
        var aktform = Application.Current.Windows[intCounter];
        if (aktform?.ToString() == "Ecoinv.Forms.EUSERSFrm")
        {
          vanform = true;
        }
      }

      if (!vanform)
      {
        var alkfrm = new EUSERSFrm();
        alkfrm.Show();
      }
    }

    #endregion ... end of CommandMenuEUSERSClick property ...

    #region ... CommandMenuVATRATESClick property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandMenuVATRATESClick;

    public ICommand CommandMenuVATRATESClick => __commandMenuVATRATESClick ??= new DelegateCommand(ac => MenuVATRATESExecute(), fc => GetMenuVATRATESCanExecute());

    private static bool GetMenuVATRATESCanExecute() => true;

    private static void MenuVATRATESExecute()
    {
      var vanform = false;
      for (var intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
      {
        var aktform = Application.Current.Windows[intCounter];
        if (aktform?.ToString() == "Ecoinv.Forms.VATRATESFrm")
        {
          vanform = true;
        }
      }

      if (!vanform)
      {
        var alkfrm = new VATRATESFrm();
        alkfrm.Show();
      }
    }

    #endregion ... end of CommandMenuVATRATESClick property ...

    #region ... CommandMenuCLIENTSClick property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandMenuCLIENTSClick;

    public ICommand CommandMenuCLIENTSClick => __commandMenuCLIENTSClick ??= new DelegateCommand(ac => MenuCLIENTSExecute(), fc => GetMenuCLIENTSCanExecute());

    private static bool GetMenuCLIENTSCanExecute() => true;

    private static void MenuCLIENTSExecute()
    {
      var vanform = false;
      for (var intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
      {
        var aktform = Application.Current.Windows[intCounter];
        if (aktform?.ToString() == "Ecoinv.Forms.CLIENTSFrm")
        {
          vanform = true;
        }
      }

      if (!vanform)
      {
        var alkfrm = new CLIENTSFrm();
        alkfrm.Show();
      }
    }

    #endregion ... end of CommandMenuCLIENTSClick property ...

    #region ... CommandMenuSERVICESClick property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandMenuSERVICESClick;

    public ICommand CommandMenuSERVICESClick => __commandMenuSERVICESClick ??= new DelegateCommand(ac => MenuSERVICESExecute(), fc => GetMenuSERVICESCanExecute());

    private static bool GetMenuSERVICESCanExecute() => true;

    private static void MenuSERVICESExecute()
    {
      var vanform = false;
      for (var intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
      {
        var aktform = Application.Current.Windows[intCounter];
        if (aktform?.ToString() == "Ecoinv.Forms.SERVICESFrm")
        {
          vanform = true;
        }
      }

      if (!vanform)
      {
        var alkfrm = new SERVICESFrm();
        alkfrm.Show();
      }
    }

    #endregion ... end of CommandMenuSERVICESClick property ...



  }

}
