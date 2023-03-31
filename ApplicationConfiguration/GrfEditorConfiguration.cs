// Decompiled with JetBrains decompiler
// Type: ActEditor.ApplicationConfiguration.GrfEditorConfiguration
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using ActEditor.Core.WPF.GenericControls;
using ColorPicker.Sliders;
using ErrorManager;
using GRF.Image;
using GRF.IO;
using GrfToWpfBridge;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TokeiLibrary;
using Utilities;

namespace ActEditor.ApplicationConfiguration
{
  public static class GrfEditorConfiguration
  {
    private static ConfigAsker _configAsker;
    private static bool? _showAnchors;
    private static bool? _useAliasing;
    private static BitmapScalingMode? _mode;

    public static ConfigAsker ConfigAsker
    {
      get
      {
        ConfigAsker configAsker = GrfEditorConfiguration._configAsker;
        if (configAsker != null)
          return configAsker;
        string[] strArray = new string[3]
        {
          Configuration.ApplicationDataPath,
          GrfEditorConfiguration.ProgramName,
          "config.txt"
        };
        return GrfEditorConfiguration._configAsker = new ConfigAsker(GrfPath.Combine(strArray));
      }
      set => GrfEditorConfiguration._configAsker = value;
    }

    public static int EncodingCodepage
    {
      get => int.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Encoding codepage]", "1252"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Encoding codepage]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static string ProgramDataPath => GrfPath.Combine(Configuration.ApplicationDataPath, GrfEditorConfiguration.ProgramName);

    public static ErrorLevel WarningLevel
    {
      get => (ErrorLevel) int.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Warning level]", "0"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Warning level]"] = ((int) value).ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static string ExtractingServiceLastPath
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - ExtractingService - Latest directory]", Configuration.ApplicationPath];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - ExtractingService - Latest directory]"] = value;
    }

    public static string SaveAdvancedLastPath
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Save advanced path]", GrfEditorConfiguration.ExtractingServiceLastPath];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Save advanced path]"] = value;
    }

    public static string AppLastPath
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Application latest file name]", Configuration.ApplicationPath];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Application latest file name]"] = value;
    }

    public static string AppLastGrfPath
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Application latest grf file name]", Configuration.ApplicationPath];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Application latest grf file name]"] = value;
    }

    public static bool KeepPreviewSelectionFromActionChange
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Keep action selection]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor -  Keep action selection]"] = value.ToString();
    }

    public static bool ActEditorGifUniform
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Gif - Uniform]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor -  Gif - Uniform]"] = value.ToString();
    }

    public static bool ActEditorGifHideDialog
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Gif - Hide saving dialog]", false.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Gif - Hide saving dialog]"] = value.ToString();
    }

    public static float ActEditorGifDelayFactor
    {
      get => float.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Gif - Delay factor]", "1"].Replace(",", "."), (IFormatProvider) CultureInfo.InvariantCulture);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Gif - Delay factor]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture).Replace(",", ".");
    }

    public static int ActEditorGifMargin
    {
      get => int.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Gif - Margin]", "3"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Gif - Margin]"] = (value < 2 ? 2 : value).ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static Color ActEditorGifBackgroundColor
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Background color]", GrfColor.ToHex(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]).ToColor();
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Background color]"] = GrfColor.ToHex(value.A, value.R, value.G, value.B);
    }

    public static Color ActEditorGifGuidelinesColor
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Guidelines color]", GrfColor.ToHex((byte) 0, (byte) 0, (byte) 0, (byte) 0)]).ToColor();
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Guidelines color]"] = GrfColor.ToHex(value.A, value.R, value.G, value.B);
    }

    public static Brush UIPanelPreviewBackground => (Brush) new SolidColorBrush(GrfEditorConfiguration.ActEditorBackgroundColor);

    public static List<string> Resources
    {
      get => Methods.StringToList(GrfEditorConfiguration.ConfigAsker["[ActEditor - Resources]", ""]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Resources]"] = Methods.ListToString(value);
    }

    public static bool PaletteEditorOpenWindowsEdits
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[Palette editor - Open palette edits in a window]", false.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[Palette editor - Open palette edits in a window]"] = value.ToString();
    }

    public static string ActEditorAccessoryId
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Client integration - Accessory Id]", ""];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Client integration - Accessory Id]"] = value;
    }

    public static string ActEditorAccname
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Client integration - Accessory name]", ""];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Client integration - Accessory name]"] = value;
    }

    public static string ActEditorItemInfo
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Client integration - Item Info]", ""];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Client integration - Item Info]"] = value;
    }

    public static string ActEditorViewId
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Client integration - View id]", ""];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Client integration - View id]"] = value;
    }

    public static string ActEditorScriptRunnerScript
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - Script Runner - Latest script]", "// Script example, for a complete list of available methods,__%LineBreak%// click on the 'Help' button__%LineBreak%foreach (var selectedLayerIndex in selectedLayerIndexes) {__%LineBreak%\tvar layer = act[selectedActionIndex, selectedFrameIndex, selectedLayerIndex];__%LineBreak%\tlayer.Translate(-10, 0);__%LineBreak%\tlayer.Rotate(15);__%LineBreak%}__%LineBreak%__%LineBreak%foreach (var action in act) {__%LineBreak%\tforeach (var frame in action) {__%LineBreak%\t\tforeach (var layer in frame) {__%LineBreak%\t\t\tlayer.OffsetX = 2 * layer.OffsetX;__%LineBreak%\t\t\tlayer.ScaleX *= 2f;__%LineBreak%\t\t\tlayer.Scale(1f, 2f);__%LineBreak%\t\t}__%LineBreak%\t}__%LineBreak%}__%LineBreak%"];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Script Runner - Latest script]"] = value;
    }

    public static bool ActEditorCopyFromCurrentFrame
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Copy from current frame]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Copy from current frame]"] = value.ToString();
    }

    public static bool ShowAnchors
    {
      get
      {
        if (!GrfEditorConfiguration._showAnchors.HasValue)
          GrfEditorConfiguration._showAnchors = new bool?(bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Show anchors]", false.ToString()]));
        return GrfEditorConfiguration._showAnchors.Value;
      }
      set
      {
        GrfEditorConfiguration.ConfigAsker["[ActEditor - Show anchors]"] = value.ToString();
        GrfEditorConfiguration._showAnchors = new bool?(value);
      }
    }

    public static bool UseAliasing
    {
      get
      {
        if (!GrfEditorConfiguration._useAliasing.HasValue)
          GrfEditorConfiguration._useAliasing = new bool?(bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Use aliasing]", false.ToString()]));
        return GrfEditorConfiguration._useAliasing.Value;
      }
      set
      {
        GrfEditorConfiguration.ConfigAsker["[ActEditor - Use aliasing]"] = value.ToString();
        GrfEditorConfiguration._useAliasing = new bool?(value);
      }
    }

    public static bool ReverseAnchor
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - ReverseAnchor]", false.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - ReverseAnchor]"] = value.ToString();
    }

    public static bool ActEditorPlaySound
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Play sounds]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Play sounds]"] = value.ToString();
    }

    public static bool ReopenLatestFile
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Open latest file on startup]", false.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Open latest file on startup]"] = value.ToString();
    }

    public static bool ActEditorRefreshLayerEditor
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Refresh layer editor]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Refresh layer editor]"] = value.ToString();
    }

    public static bool ShellAssociateAct
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[Application - Shell associate - Act]", false.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[Application - Shell associate - Act]"] = value.ToString();
    }

    public static bool ActEditorGridLineHVisible
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Grid line horizontal visible]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Grid line horizontal visible]"] = value.ToString();
    }

    public static bool ActEditorGridLineVVisible
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Grid line vertical visible]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Grid line vertical visible]"] = value.ToString();
    }

    public static Color ActEditorBackgroundColor
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Background preview color]", GrfColor.ToHex((byte) 150, (byte) 0, (byte) 0, (byte) 0)]).ToColor();
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Background preview color]"] = GrfColor.ToHex(value.A, value.R, value.G, value.B);
    }

    public static Color ActEditorSpriteBackgroundColor
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Background sprite preview color]", GrfColor.ToHex((byte) 128, byte.MaxValue, byte.MaxValue, byte.MaxValue)]).ToColor();
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Background sprite preview color]"] = GrfColor.ToHex(value.A, value.R, value.G, value.B);
    }

    public static GrfColor ActEditorGridLineHorizontal
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Grid line horizontal color]", GrfColor.ToHex(byte.MaxValue, (byte) 0, (byte) 0, (byte) 0)]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Grid line horizontal color]"] = value.ToHexString();
    }

    public static GrfColor ActEditorGridLineVertical
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Grid line vertical color]", GrfColor.ToHex(byte.MaxValue, (byte) 0, (byte) 0, (byte) 0)]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Grid line vertical color]"] = value.ToHexString();
    }

    public static GrfColor ActEditorSpriteSelectionBorder
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Selected sprite border color]", GrfColor.ToHex(byte.MaxValue, byte.MaxValue, (byte) 0, (byte) 0)]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Selected sprite border color]"] = value.ToHexString();
    }

    public static GrfColor ActEditorSpriteSelectionBorderOverlay
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Selected sprite overlay color]", GrfColor.ToHex((byte) 0, byte.MaxValue, byte.MaxValue, byte.MaxValue)]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Selected sprite overlay color]"] = value.ToHexString();
    }

    public static GrfColor ActEditorSelectionBorder
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Selection border color]", GrfColor.ToHex(byte.MaxValue, (byte) 0, (byte) 0, byte.MaxValue)]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Selection border color]"] = value.ToHexString();
    }

    public static GrfColor ActEditorSelectionBorderOverlay
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Selection overlay color]", GrfColor.ToHex((byte) 50, (byte) 128, (byte) 128, byte.MaxValue)]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Selection overlay color]"] = value.ToHexString();
    }

    public static GrfColor ActEditorAnchorColor
    {
      get => new GrfColor(GrfEditorConfiguration.ConfigAsker["[ActEditor - Anchor color]", GrfColor.ToHex((byte) 200, byte.MaxValue, byte.MaxValue, (byte) 0)]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Anchor color]"] = value.ToHexString();
    }

    public static float ActEditorZoomInMultiplier
    {
      get => float.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Zoom in multiplier]", "1"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Zoom in multiplier]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static BitmapScalingMode ActEditorScalingMode
    {
      get
      {
        if (GrfEditorConfiguration._mode.HasValue)
          return GrfEditorConfiguration._mode.Value;
        BitmapScalingMode editorScalingMode = (BitmapScalingMode) Enum.Parse(typeof (BitmapScalingMode), GrfEditorConfiguration.ConfigAsker["[ActEditor - Scale mode]", BitmapScalingMode.NearestNeighbor.ToString()], true);
        GrfEditorConfiguration._mode = new BitmapScalingMode?(editorScalingMode);
        return editorScalingMode;
      }
      set
      {
        GrfEditorConfiguration.ConfigAsker["[ActEditor - Scale mode]"] = value.ToString();
        GrfEditorConfiguration._mode = new BitmapScalingMode?(value);
      }
    }

    public static string GrfShellLatest
    {
      get => GrfEditorConfiguration.ConfigAsker["[ActEditor - GrfShell - Latest file]", ""];
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - GrfShell - Latest file]"] = value;
    }

    public static bool InterpolateOffsets
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Offsets]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Offsets]"] = value.ToString();
    }

    public static bool InterpolateScale
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Scale]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Scale]"] = value.ToString();
    }

    public static bool InterpolateAngle
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Angle]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Angle]"] = value.ToString();
    }

    public static bool InterpolateColor
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Color]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Color]"] = value.ToString();
    }

    public static bool InterpolateMirror
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Mirror]", true.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Mirror]"] = value.ToString();
    }

    public static int InterpolateEase
    {
      get => int.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Ease]", "0"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Ease]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static int BrushSize
    {
      get => int.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Brush size]", "5"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Brush size]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static double InterpolateTolerance
    {
      get => (double) FormatConverters.SingleConverter(GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Tolerance]", "0.9"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Tolerance]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static int InterpolateRange
    {
      get => int.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Range]", "20"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Interpolation - Range]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static bool UseDithering
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[Sprite Editor - Use dithering]", false.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[Sprite Editor - Use dithering]"] = value.ToString();
    }

    public static bool UseTgaImages
    {
      get => bool.Parse(GrfEditorConfiguration.ConfigAsker["[Sprite Editor - Use TGA images]", false.ToString()]);
      set => GrfEditorConfiguration.ConfigAsker["[Sprite Editor - Use TGA images]"] = value.ToString();
    }

    public static int TransparencyMode
    {
      get => int.Parse(GrfEditorConfiguration.ConfigAsker["[Sprite Editor - Transparency mode]", "1"]);
      set => GrfEditorConfiguration.ConfigAsker["[Sprite Editor - Transparency mode]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static int FormatConflictOption
    {
      get => int.Parse(GrfEditorConfiguration.ConfigAsker["[Sprite Editor - Last format conflict option]", "2"]);
      set => GrfEditorConfiguration.ConfigAsker["[Sprite Editor - Last format conflict option]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static string PublicVersion => "1.1.0";

    public static string Author => "Tokeiburu";

    public static string ProgramName => "Act Editor";

    public static string RealVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

    public static int PatchId
    {
      get => int.Parse(GrfEditorConfiguration.ConfigAsker["[ActEditor - Patch ID]", "0"]);
      set => GrfEditorConfiguration.ConfigAsker["[ActEditor - Patch ID]"] = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static void Bind(CheckBox checkBox, Func<bool> get, Action<bool> set)
    {
      checkBox.IsChecked = new bool?(get());
      checkBox.Checked += (RoutedEventHandler) ((e, a) => set(true));
      checkBox.Unchecked += (RoutedEventHandler) ((e, a) => set(false));
    }

    public static void Bind(CheckBox checkBox, Func<bool> get, Action<bool> set, Action extra)
    {
      checkBox.IsChecked = new bool?(get());
      checkBox.Checked += (RoutedEventHandler) ((e, a) =>
      {
        set(true);
        extra();
      });
      checkBox.Unchecked += (RoutedEventHandler) ((e, a) =>
      {
        set(false);
        extra();
      });
    }

    public static void Bind(MenuItem checkBox, Func<bool> get, Action<bool> set, Action extra)
    {
      checkBox.IsChecked = get();
      checkBox.Checked += (RoutedEventHandler) ((e, a) =>
      {
        set(true);
        extra();
      });
      checkBox.Unchecked += (RoutedEventHandler) ((e, a) =>
      {
        set(false);
        extra();
      });
    }

    public static void Bind(QuickColorSelector qcs, Func<Color> get, Action<Color> set)
    {
      qcs.Color = get();
      qcs.ColorChanged += (SliderGradient.GradientPickerColorEventHandler) ((sender, value) => set(value));
    }

    public static void Bind<T>(TextBox tb, Func<T> get, Action<T> set, Func<string, T> converter)
    {
      tb.Text = get().ToString();
      tb.TextChanged += (TextChangedEventHandler) delegate
      {
        try
        {
          set(converter(tb.Text));
        }
        catch
        {
        }
      };
    }

    public static void Bind<T>(
      TextBox tb,
      Func<T> get,
      Action<T> set,
      Func<string, T> converter,
      Action extra)
    {
      tb.Text = get().ToString();
      tb.TextChanged += (TextChangedEventHandler) delegate
      {
        try
        {
          set(converter(tb.Text));
          extra();
        }
        catch
        {
        }
      };
    }
  }
}
