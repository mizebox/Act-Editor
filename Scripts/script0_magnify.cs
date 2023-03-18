using System;
using System.Globalization;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace Scripts {
    public class Script : IActScript {
		public const int Magnify = 2;
		
		public object DisplayName {
			get { return "Magnify"; }
		}
		
		public string Group {
			get { return "Scripts"; }
		}
		
		public string InputGesture {
			get { return "Ctrl-Shift-M"; }
		}
		
		public string Image {
			get { return "scale.png"; }
		}
		
		public void Execute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {
			if (act == null) return;
			
			var dialog = new InputDialog("Enter the magnifier scale.", "Magnify script", Configuration.ConfigAsker["[ActEditor - Magnify value]", "2"]);
			dialog.Owner = WpfUtilities.TopWindow;

			if (dialog.ShowDialog() != true) {
				return;
			}

			float mag;

			if (!float.TryParse(dialog.Input.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out mag)) {
				ErrorHandler.HandleException("The magnifier value is not valid. Only float values are allowed.", ErrorLevel.Warning);
				return;
			}
			
			Configuration.ConfigAsker["[ActEditor - Magnify value]"] = dialog.Input;
			
			try {
				act.Commands.Begin();
				act.Commands.Backup(_ =>  {
					foreach (Layer layer in act.GetAllLayers()) {
						layer.OffsetX = (int) (layer.OffsetX * mag);
						layer.OffsetY = (int) (layer.OffsetY * mag);
						layer.ScaleX *= mag;
						layer.ScaleY *= mag;
					}
					
					//foreach (Frame frame in act.GetAllFrames()) {
					//	foreach (Anchor pass in frame.Anchors) {
					//		pass.OffsetX = (int) (pass.OffsetX * mag);
					//		pass.OffsetY = (int) (pass.OffsetY * mag);
					//	}
					//}
				});
			}
			catch (Exception err) {
				act.Commands.CancelEdit();
				ErrorHandler.HandleException(err, ErrorLevel.Warning);
			}
			finally {
				act.Commands.End();
			}
		}
		
		public bool CanExecute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {
			return act != null;
		}
	}
}
