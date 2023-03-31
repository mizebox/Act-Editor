// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.WPF.Dialogs.ActionInsertDialog
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.WPF.EditorControls;
using ActEditor.Core.WPF.GenericControls;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using TokeiLibrary;
using TokeiLibrary.WPF.Styles;

namespace ActEditor.Core.WPF.Dialogs
{
  public partial class ActionInsertDialog : TkWindow, IComponentConnector
  {
    private readonly Act _act;
    internal RadioButton _mode0;
    internal RadioButton _mode4;
    internal RadioButton _mode1;
    internal RadioButton _mode2;
    internal RadioButton _mode3;
    internal Label _labelStartIndex;
    internal Label _labelRange;
    internal Grid _gridEndIndex;
    internal FancyButton _lastIndex;
    internal CheckBox _cbCopyContent;
    internal Border _borderIndexStart;
    internal ClickSelectTextBox _tbIndexStart;
    internal ActionSelector _asIndexStart;
    internal ActionSelector _asIndexEnd;
    internal Border _borderRange;
    internal ClickSelectTextBox _tbIndexRange;
    internal Border _borderIndexEnd;
    internal ClickSelectTextBox _tbIndexEnd;
    internal Grid _gridActionPresenter;
    internal Button _buttonOk;
    internal Button _buttonCancel;
    private bool _contentLoaded;

    public ActionInsertDialog()
      : base("Action edit", "advanced.png")
    {
      this.InitializeComponent();
      WpfUtilities.AddFocus((FrameworkElement) this._tbIndexStart, (FrameworkElement) this._tbIndexEnd, (FrameworkElement) this._tbIndexRange);
      this._asIndexStart.ActionChanged += new ActIndexSelector.FrameIndexChangedDelegate(this._asIndexStart_ActionChanged);
      this._asIndexEnd.ActionChanged += new ActIndexSelector.FrameIndexChangedDelegate(this._asIndexEnd_ActionChanged);
      this._tbIndexEnd.TextChanged += (TextChangedEventHandler) delegate
      {
        int result;
        if (!int.TryParse(this._tbIndexEnd.Text, out result))
          return;
        this._asIndexEnd.SelectedAction = result;
      };
      this._tbIndexStart.TextChanged += (TextChangedEventHandler) delegate
      {
        int result;
        if (!int.TryParse(this._tbIndexStart.Text, out result))
          return;
        this._asIndexStart.SelectedAction = result;
      };
      GrfEditorConfiguration.Bind(this._cbCopyContent, (Func<bool>) (() => GrfEditorConfiguration.ActEditorCopyFromCurrentFrame), (Action<bool>) (v => GrfEditorConfiguration.ActEditorCopyFromCurrentFrame = v), (System.Action) (() =>
      {
        if (GrfEditorConfiguration.ActEditorCopyFromCurrentFrame)
        {
          this._labelStartIndex.Visibility = Visibility.Visible;
          this._borderIndexStart.Visibility = Visibility.Visible;
          this._asIndexStart.Visibility = Visibility.Visible;
        }
        else
        {
          this._labelStartIndex.Visibility = Visibility.Collapsed;
          this._borderIndexStart.Visibility = Visibility.Collapsed;
          this._asIndexStart.Visibility = Visibility.Collapsed;
        }
      }));
    }

    public ActionInsertDialog(Act act)
      : this()
    {
      this._asIndexStart.SetAct(act);
      this._asIndexEnd.SetAct(act);
      this._act = act;
    }

    public int StartIndex
    {
      get
      {
        int result;
        return int.TryParse(this._tbIndexStart.Text, out result) ? result : -1;
      }
      set => this._tbIndexStart.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public int EndIndex
    {
      get
      {
        int result;
        return int.TryParse(this._tbIndexEnd.Text, out result) ? result : -1;
      }
      set => this._tbIndexEnd.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public int Range
    {
      get
      {
        int result;
        return int.TryParse(this._tbIndexRange.Text, out result) ? result : -1;
      }
      set => this._tbIndexRange.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public ActionInsertDialog.EditMode Mode
    {
      get
      {
        bool? isChecked1 = this._mode0.IsChecked;
        if ((!isChecked1.GetValueOrDefault() ? 0 : (isChecked1.HasValue ? 1 : 0)) != 0)
          return ActionInsertDialog.EditMode.Delete;
        bool? isChecked2 = this._mode1.IsChecked;
        if ((!isChecked2.GetValueOrDefault() ? 0 : (isChecked2.HasValue ? 1 : 0)) != 0)
          return ActionInsertDialog.EditMode.Insert;
        bool? isChecked3 = this._mode2.IsChecked;
        if ((!isChecked3.GetValueOrDefault() ? 0 : (isChecked3.HasValue ? 1 : 0)) != 0)
          return ActionInsertDialog.EditMode.Move;
        bool? isChecked4 = this._mode3.IsChecked;
        if ((!isChecked4.GetValueOrDefault() ? 0 : (isChecked4.HasValue ? 1 : 0)) != 0)
          return ActionInsertDialog.EditMode.Switch;
        bool? isChecked5 = this._mode4.IsChecked;
        return (!isChecked5.GetValueOrDefault() ? 0 : (isChecked5.HasValue ? 1 : 0)) == 0 ? ActionInsertDialog.EditMode.None : ActionInsertDialog.EditMode.Replace;
      }
      set
      {
        switch (value)
        {
          case ActionInsertDialog.EditMode.Delete:
            this._mode0.IsChecked = new bool?(true);
            this._deleteMode();
            break;
          case ActionInsertDialog.EditMode.Insert:
            this._mode1.IsChecked = new bool?(true);
            this._insertMode();
            break;
          case ActionInsertDialog.EditMode.Move:
            this._mode2.IsChecked = new bool?(true);
            this._setAllVisible();
            this._cbCopyContent.Visibility = Visibility.Collapsed;
            break;
          case ActionInsertDialog.EditMode.Switch:
            this._mode3.IsChecked = new bool?(true);
            this._setAllVisible();
            this._cbCopyContent.Visibility = Visibility.Collapsed;
            break;
          case ActionInsertDialog.EditMode.Replace:
            this._mode4.IsChecked = new bool?(true);
            this._setAllVisible();
            this._cbCopyContent.Visibility = Visibility.Collapsed;
            break;
        }
      }
    }

    private void _insertMode()
    {
      this._setAllVisible();
      if (GrfEditorConfiguration.ActEditorCopyFromCurrentFrame)
        return;
      this._labelStartIndex.Visibility = Visibility.Collapsed;
      this._borderIndexStart.Visibility = Visibility.Collapsed;
      this._asIndexStart.Visibility = Visibility.Collapsed;
    }

    private void _deleteMode()
    {
      this._setAllVisible();
      this._cbCopyContent.Visibility = Visibility.Collapsed;
      this._gridEndIndex.Visibility = Visibility.Collapsed;
      this._borderIndexEnd.Visibility = Visibility.Collapsed;
      this._asIndexEnd.Visibility = Visibility.Collapsed;
    }

    private void _setAllVisible()
    {
      this._labelStartIndex.Visibility = Visibility.Visible;
      this._labelRange.Visibility = Visibility.Visible;
      this._gridEndIndex.Visibility = Visibility.Visible;
      this._cbCopyContent.Visibility = Visibility.Visible;
      this._borderIndexStart.Visibility = Visibility.Visible;
      this._borderRange.Visibility = Visibility.Visible;
      this._borderIndexEnd.Visibility = Visibility.Visible;
      this._asIndexStart.Visibility = Visibility.Visible;
      this._asIndexEnd.Visibility = Visibility.Visible;
    }

    private void _asIndexEnd_ActionChanged(object sender, int actionindex) => this._tbIndexEnd.Text = actionindex.ToString((IFormatProvider) CultureInfo.InvariantCulture);

    private void _asIndexStart_ActionChanged(object sender, int actionindex) => this._tbIndexStart.Text = actionindex.ToString((IFormatProvider) CultureInfo.InvariantCulture);

    protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
        this.DialogResult = new bool?(true);
      base.GRFEditorWindowKeyDown(sender, e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      bool? dialogResult = this.DialogResult;
      if ((!dialogResult.GetValueOrDefault() ? 0 : (dialogResult.HasValue ? 1 : 0)) == 0 || this.CanExecute())
        return;
      this.DialogResult = new bool?();
      e.Cancel = true;
    }

    public bool CanExecute()
    {
      if (this.Mode == ActionInsertDialog.EditMode.None)
        return false;
      try
      {
        this.Execute(this._act, false);
        return true;
      }
      catch (Exception ex)
      {
        ErrorHandler.HandleException(ex);
        return false;
      }
    }

    private void _buttonOk_Click(object sender, RoutedEventArgs e) => this.DialogResult = new bool?(true);

    private void _buttonCancel_Click(object sender, RoutedEventArgs e) => this.Close();

    public void Execute(Act act, bool executeCommands = true)
    {
      switch (this.Mode)
      {
        case ActionInsertDialog.EditMode.None:
          throw new Exception("No command selected.");
        case ActionInsertDialog.EditMode.Delete:
          if (this.StartIndex < 0)
            this.StartIndex = 0;
          if (this.StartIndex >= act.NumberOfActions)
            this.StartIndex = act.NumberOfActions - 1;
          if (this.StartIndex + this.Range > act.NumberOfActions)
            this.Range = act.NumberOfActions - this.StartIndex;
          if (this.Range < 1)
            this.Range = 1;
          if (act.NumberOfActions - this.Range <= 0)
            throw new ArgumentException("There must be at least one action left in the act.");
          if (!executeCommands)
            break;
          try
          {
            act.Commands.BeginNoDelay();
            for (int index = 0; index < this.Range; ++index)
              act.Commands.ActionDelete(this.StartIndex);
            break;
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex, ErrorLevel.Warning);
            break;
          }
          finally
          {
            act.Commands.End();
          }
        case ActionInsertDialog.EditMode.Insert:
          if (GrfEditorConfiguration.ActEditorCopyFromCurrentFrame)
          {
            if (this.StartIndex < 0)
              throw new ArgumentException("Start index must be greater or equal than 0.");
            if (this.StartIndex >= act.NumberOfActions)
              throw new ArgumentException("Start index must be less than the number of actions (" + (object) act.NumberOfActions + ").");
          }
          if (this.Range < 1)
            this.Range = 1;
          if (this.EndIndex > act.NumberOfActions)
            this.EndIndex = act.NumberOfActions;
          if (this.EndIndex < 0)
            this.EndIndex = 0;
          if (!executeCommands)
            break;
          try
          {
            act.Commands.BeginNoDelay();
            if (GrfEditorConfiguration.ActEditorCopyFromCurrentFrame)
            {
              for (int index = 0; index < this.Range; ++index)
              {
                if (this.EndIndex > this.StartIndex)
                  act.Commands.ActionCopyAt(this.StartIndex, this.EndIndex);
                else
                  act.Commands.ActionCopyAt(this.StartIndex + index, this.EndIndex);
              }
              break;
            }
            for (int index = 0; index < this.Range; ++index)
              act.Commands.ActionInsertAt(this.EndIndex);
            break;
          }
          catch (Exception ex)
          {
            act.Commands.CancelEdit();
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            act.Commands.End();
            act.InvalidateVisual();
          }
        case ActionInsertDialog.EditMode.Move:
          if (this.StartIndex >= act.NumberOfActions)
            this.StartIndex = act.NumberOfActions - 1;
          if (this.EndIndex >= act.NumberOfActions)
            this.EndIndex = act.NumberOfActions - 1;
          if (this.StartIndex < 0)
            this.StartIndex = 0;
          if (this.EndIndex < 0)
            this.EndIndex = 0;
          if (this.StartIndex == this.EndIndex)
            throw new Exception("The start and end indexes cannot be the same.");
          if (this.Range < 1)
            this.Range = 1;
          if (this.StartIndex + this.Range > act.NumberOfActions)
          {
            this.Range = act.NumberOfActions - this.StartIndex;
            throw new Exception("The range goes beyond the number of actions.");
          }
          if (this.EndIndex <= this.StartIndex + this.Range && this.EndIndex + 1 > this.StartIndex)
            throw new Exception("Indexes intersect with each other.");
          if (!executeCommands)
            break;
          try
          {
            act.Commands.ActionMoveRange(this.StartIndex, this.Range, this.EndIndex);
            break;
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            act.InvalidateVisual();
          }
        case ActionInsertDialog.EditMode.Switch:
          if (this.StartIndex >= act.NumberOfActions)
            this.StartIndex = act.NumberOfActions - 1;
          if (this.EndIndex >= act.NumberOfActions)
            this.EndIndex = act.NumberOfActions - 1;
          if (this.StartIndex < 0)
            this.StartIndex = 0;
          if (this.EndIndex < 0)
            this.EndIndex = 0;
          if (this.StartIndex == this.EndIndex)
            throw new Exception("The start and end indexes cannot be the same.");
          if (this.Range < 1)
            this.Range = 1;
          if (this.StartIndex + this.Range > act.NumberOfActions)
          {
            this.Range = act.NumberOfActions - this.StartIndex;
            throw new Exception("The range goes beyond the number of actions.");
          }
          if (this.EndIndex < this.StartIndex + this.Range && this.EndIndex + 1 > this.StartIndex)
            throw new Exception("Indexes intersect with each other (overlapping).");
          if (!executeCommands)
            break;
          try
          {
            act.Commands.ActionSwitchRange(this.StartIndex, this.Range, this.EndIndex);
            break;
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            act.InvalidateVisual();
          }
        case ActionInsertDialog.EditMode.Replace:
          if (this.StartIndex == this.EndIndex)
          {
            this.EndIndex = this.StartIndex + 1;
            --this.Range;
          }
          if (this.StartIndex >= act.NumberOfActions)
            this.StartIndex = act.NumberOfActions - 1;
          if (this.StartIndex < 0)
            this.StartIndex = 0;
          if (this.EndIndex < 0)
            this.EndIndex = 0;
          if (this.Range < 1)
            this.Range = 1;
          if (this.EndIndex < this.StartIndex + 1 && this.EndIndex + this.Range > this.StartIndex)
            throw new Exception("Indexes intersect with each other (overlapping).");
          if (!executeCommands)
            break;
          try
          {
            act.Commands.BeginNoDelay();
            while (this.EndIndex + this.Range > act.NumberOfActions)
              act.Commands.ActionInsertAt(act.NumberOfActions);
            for (int index = 0; index < this.Range; ++index)
              act.Commands.ActionReplaceTo(this.StartIndex, this.EndIndex + index);
            break;
          }
          catch (Exception ex)
          {
            ErrorHandler.HandleException(ex);
            break;
          }
          finally
          {
            act.Commands.End();
            act.InvalidateVisual();
          }
      }
    }

    private void _mode_Checked(object sender, RoutedEventArgs e)
    {
      ActionInsertDialog.EditMode editMode = ActionInsertDialog.EditMode.None;
      if (sender == this._mode0)
        editMode = ActionInsertDialog.EditMode.Delete;
      else if (sender == this._mode1)
        editMode = ActionInsertDialog.EditMode.Insert;
      else if (sender == this._mode2)
        editMode = ActionInsertDialog.EditMode.Move;
      else if (sender == this._mode3)
        editMode = ActionInsertDialog.EditMode.Switch;
      else if (sender == this._mode4)
        editMode = ActionInsertDialog.EditMode.Replace;
      this.Mode = editMode;
    }

    private void _lastIndex_Click(object sender, RoutedEventArgs e) => this.EndIndex = this._act.NumberOfActions;

    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/Act Editor;component/core/wpf/dialogs/actioninsertdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler) => Delegate.CreateDelegate(delegateType, (object) this, handler);

    [DebuggerNonUserCode]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this._mode0 = (RadioButton) target;
          this._mode0.Checked += new RoutedEventHandler(this._mode_Checked);
          break;
        case 2:
          this._mode4 = (RadioButton) target;
          this._mode4.Checked += new RoutedEventHandler(this._mode_Checked);
          break;
        case 3:
          this._mode1 = (RadioButton) target;
          this._mode1.Checked += new RoutedEventHandler(this._mode_Checked);
          break;
        case 4:
          this._mode2 = (RadioButton) target;
          this._mode2.Checked += new RoutedEventHandler(this._mode_Checked);
          break;
        case 5:
          this._mode3 = (RadioButton) target;
          this._mode3.Checked += new RoutedEventHandler(this._mode_Checked);
          break;
        case 6:
          this._labelStartIndex = (Label) target;
          break;
        case 7:
          this._labelRange = (Label) target;
          break;
        case 8:
          this._gridEndIndex = (Grid) target;
          break;
        case 9:
          this._lastIndex = (FancyButton) target;
          this._lastIndex.Click += new RoutedEventHandler(this._lastIndex_Click);
          break;
        case 10:
          this._cbCopyContent = (CheckBox) target;
          break;
        case 11:
          this._borderIndexStart = (Border) target;
          break;
        case 12:
          this._tbIndexStart = (ClickSelectTextBox) target;
          break;
        case 13:
          this._asIndexStart = (ActionSelector) target;
          break;
        case 14:
          this._asIndexEnd = (ActionSelector) target;
          break;
        case 15:
          this._borderRange = (Border) target;
          break;
        case 16:
          this._tbIndexRange = (ClickSelectTextBox) target;
          break;
        case 17:
          this._borderIndexEnd = (Border) target;
          break;
        case 18:
          this._tbIndexEnd = (ClickSelectTextBox) target;
          break;
        case 19:
          this._gridActionPresenter = (Grid) target;
          break;
        case 20:
          this._buttonOk = (Button) target;
          this._buttonOk.Click += new RoutedEventHandler(this._buttonOk_Click);
          break;
        case 21:
          this._buttonCancel = (Button) target;
          this._buttonCancel.Click += new RoutedEventHandler(this._buttonCancel_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public enum EditMode
    {
      None,
      Delete,
      Insert,
      Move,
      Switch,
      Replace,
    }
  }
}
