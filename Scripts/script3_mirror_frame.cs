using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GRF.FileFormats.ActFormat.Commands;
using GRF.Image;
using GrfToWpfBridge;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace Scripts {
    public class Script : IActScript {
		public object DisplayName {
			get { return "Flip frame or selection"; }
		}
		
		public string Group {
			get { return "Scripts"; }
		}
		
		public string InputGesture {
			get { return "Ctrl-Shift-G"; }
		}
		
		public string Image {
			get { return "flipFrame.png"; }
		}
		
		public void Execute(Act act, int selectedActionIndex, int selectedFrameIndex, int[] selectedLayerIndexes) {
			if (act == null) return;
			
			bool mirrorSelected = selectedLayerIndexes.Length > 0;
			
			try {
				act.Commands.Begin();
				act.Commands.Backup(_ => {
					List<Layer> layers = act[selectedActionIndex, selectedFrameIndex].Layers;
					
					if (mirrorSelected) {
						foreach (int index in selectedLayerIndexes) {
							Layer layer = layers[index];
							layer.OffsetX *= -1;
							int rotation = 360 - layer.Rotation;
							layer.Rotation = rotation < 0 ? rotation + 360 : rotation;
							layer.Mirror = !layer.Mirror;
						}
					}
					else {
						foreach (Layer layer in layers) {
							layer.OffsetX *= -1;
							int rotation = 360 - layer.Rotation;
							layer.Rotation = rotation < 0 ? rotation + 360 : rotation;
							layer.Mirror = !layer.Mirror;
						}
					}
					
				}, "Mirror script");
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
