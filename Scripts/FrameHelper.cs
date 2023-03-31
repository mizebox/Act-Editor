// Decompiled with JetBrains decompiler
// Type: ActEditor.Core.Scripts.FrameHelper
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.ApplicationConfiguration;
using ActEditor.Core.WPF.GenericControls;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace ActEditor.Core.Scripts
{
  public static class FrameHelper
  {
    public static Grid GenerateContentGrid() => new Grid()
    {
      RowDefinitions = {
        new RowDefinition()
        {
          Height = new GridLength(-1.0, GridUnitType.Auto)
        },
        new RowDefinition()
      }
    };

    public static OkCancelEmptyDialog GenerateOkCancelDialog(string title, string image)
    {
      OkCancelEmptyDialog dialog = new OkCancelEmptyDialog(title, image);
      dialog.Owner = WpfUtilities.TopWindow;
      dialog.PreviewKeyDown += (KeyEventHandler) delegate
      {
        if (!Keyboard.IsKeyDown(Key.Return))
          return;
        dialog.DialogResult = new bool?(true);
        dialog.Close();
      };
      return dialog;
    }

    public static TextBlock GenerateTextBlock(string input)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Margin = new Thickness(3.0);
      textBlock.TextWrapping = TextWrapping.Wrap;
      textBlock.Text = input;
      return textBlock;
    }

    public static ClickSelectTextBox GenerateClickSelectTb(int selectedFrameIndex)
    {
      ClickSelectTextBox clickSelectTb = new ClickSelectTextBox();
      clickSelectTb.SetValue(Grid.RowProperty, (object) 1);
      clickSelectTb.Text = selectedFrameIndex.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      clickSelectTb.Margin = new Thickness(3.0);
      clickSelectTb.SelectAll();
      clickSelectTb.Focus();
      return clickSelectTb;
    }

    public static UIElement GenerateCheckBox(string display = "Copy from the currently selected frame")
    {
      CheckBox checkBox1 = new CheckBox();
      checkBox1.Content = (object) "Copy from the currently selected frame";
      CheckBox checkBox2 = checkBox1;
      GrfEditorConfiguration.Bind(checkBox2, (Func<bool>) (() => GrfEditorConfiguration.ActEditorCopyFromCurrentFrame), (Action<bool>) (v => GrfEditorConfiguration.ActEditorCopyFromCurrentFrame = v));
      checkBox2.HorizontalAlignment = HorizontalAlignment.Left;
      checkBox2.VerticalAlignment = VerticalAlignment.Center;
      checkBox2.Margin = new Thickness(3.0);
      return (UIElement) checkBox2;
    }
  }
}
