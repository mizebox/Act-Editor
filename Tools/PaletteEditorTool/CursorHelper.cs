// Decompiled with JetBrains decompiler
// Type: ActEditor.Tools.PaletteEditorTool.CursorHelper
// Assembly: Act Editor, Version=1.0.10.574, Culture=neutral, PublicKeyToken=null
// MVID: F959EA4D-9DCB-468F-99FC-E6CA4E863E84
// Assembly location: C:\Program Files (x86)\Act Editor\Act Editor.exe

using Microsoft.Win32.SafeHandles;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ActEditor.Tools.PaletteEditorTool
{
  public class CursorHelper
  {
    private static Cursor InternalCreateCursor(Bitmap bmp, System.Windows.Point hotspot)
    {
      CursorHelper.NativeMethods.IconInfo iconInfo = new CursorHelper.NativeMethods.IconInfo();
      CursorHelper.NativeMethods.GetIconInfo(bmp.GetHicon(), ref iconInfo);
      iconInfo.xHotspot = (int) hotspot.X;
      iconInfo.yHotspot = (int) hotspot.Y;
      iconInfo.fIcon = false;
      return CursorInteropHelper.Create((SafeHandle) CursorHelper.NativeMethods.CreateIconIndirect(ref iconInfo));
    }

    public static Cursor CreateCursor(UIElement element, System.Windows.Point hotspot)
    {
      element.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
      element.Arrange(new Rect(new System.Windows.Point(), element.DesiredSize));
      RenderTargetBitmap source = new RenderTargetBitmap((int) element.DesiredSize.Width, (int) element.DesiredSize.Height, 96.0, 96.0, PixelFormats.Pbgra32);
      source.Render((Visual) element);
      PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
      pngBitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource) source));
      using (MemoryStream memoryStream = new MemoryStream())
      {
        pngBitmapEncoder.Save((Stream) memoryStream);
        using (Bitmap bmp = new Bitmap((Stream) memoryStream))
          return CursorHelper.InternalCreateCursor(bmp, hotspot);
      }
    }

    private static class NativeMethods
    {
      [DllImport("user32.dll")]
      public static extern CursorHelper.SafeIconHandle CreateIconIndirect(
        ref CursorHelper.NativeMethods.IconInfo icon);

      [DllImport("user32.dll")]
      public static extern bool DestroyIcon(IntPtr hIcon);

      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool GetIconInfo(
        IntPtr hIcon,
        ref CursorHelper.NativeMethods.IconInfo pIconInfo);

      public struct IconInfo
      {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
      }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
      public SafeIconHandle()
        : base(true)
      {
      }

      protected override bool ReleaseHandle() => CursorHelper.NativeMethods.DestroyIcon(this.handle);
    }
  }
}
