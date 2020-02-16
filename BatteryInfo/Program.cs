using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript {
	partial class Program : MyGridProgram {
		// Start Script

		//readonly string cockpitName = "Cockpit";

		readonly bool error, bHydrogen, bGrinder;
		readonly List<IMyBatteryBlock> lBatteryes;
		//readonly List<IMyProgrammableBlock> lProgram;
		readonly List<IMyGasTank> lHydrogen;
		readonly List<IMyShipDrill> lDrill;
		//readonly IMyTextSurface ProgDisp0, ProgDisp1;
		//readonly IMyTextSurface[] lTextSurface = new IMyTextSurface[2];
		readonly List<IMyTextSurface> lTextSurface;// = new List<IMyTextSurface>(2);
		readonly List<IMyCockpit> lCockpit;


		public Program() {
			lBatteryes = new List<IMyBatteryBlock>();
			GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(lBatteryes, block => block.IsSameConstructAs(Me));
			if (lBatteryes.Count == 0) {
				Echo("Нет батарей!");
				error = true;
			}

			lHydrogen = new List<IMyGasTank>();
			GridTerminalSystem.GetBlocksOfType<IMyGasTank>(lHydrogen, block => block.IsSameConstructAs(Me));
			if (lHydrogen.Count > 0) {
				Echo("GasTanks: " + lHydrogen.Count);
				bHydrogen = true;
			}

			lDrill = new List<IMyShipDrill>();
			GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(lDrill, block => block.IsSameConstructAs(Me));
			if (lDrill.Count > 0) {
				bGrinder = true;
			}

			lCockpit = new List<IMyCockpit>();
			lTextSurface = new List<IMyTextSurface>();
			GridTerminalSystem.GetBlocksOfType(lCockpit, block => block.IsSameConstructAs(Me));
			if (lCockpit.Count > 0) {
				foreach (IMyCockpit cockpit in lCockpit) {
					if (cockpit.IsMainCockpit) {
						if (cockpit.SurfaceCount >= 4) {
							//Array.Resize(ref lTextSurface, 2);
							lTextSurface.Add(cockpit.GetSurface(1));
							lTextSurface.Add(cockpit.GetSurface(2));

							lTextSurface[0].FontSize = 1.8f;
							lTextSurface[0].BackgroundColor = Color.Black;
							lTextSurface[1].FontSize = 1.8f;
							lTextSurface[1].BackgroundColor = Color.Black;
						}
					} else {
						Echo("Нет главного кокпита! Обозначьте главный кокпит");
						error = true;
					}
				}

				//lProgram = new List<IMyProgrammableBlock>();
				//GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(lProgram, block => block.IsSameConstructAs(Me));
				//if (lProgram.Count > 0) {
				//    foreach (IMyProgrammableBlock pb in lProgram) {
				//        if (pb.DisplayNameText.Contains("BatteryInfo")) {
				//            ProgDisp0 = pb.GetSurface(0);
				//            ProgDisp0.FontSize = 1.8f;
				//            ProgDisp0.BackgroundColor = Color.Black;
				//            ProgDisp1 = pb.GetSurface(1);
				//            ProgDisp1.FontSize = 5f;
				//            ProgDisp1.BackgroundColor = Color.Black;
				//        }
				//    }
				//    if (ProgDisp0.DisplayName == "") {
				//        Echo("Нет программного блока! Добавьте в название 'BatteryInfo'");
				//        error = true;
				//    }
				//}

				if (error) {
					Runtime.UpdateFrequency = UpdateFrequency.None;
				} else {
					Runtime.UpdateFrequency = UpdateFrequency.Update10;
				}
			}
		}

		public void Save() {

		}

		public void Main(string argument, UpdateType updateSource) {
			string sName, sDisp;
			float fSPower, fCPowerP, fGas;
			float fCurStorPower = 0.0f;
			float fMaxStorPower = 0.0f;
			float fCurInputPower = 0.0f;
			float fCurOutputPower = 0.0f;
			//float fFilledHydrogen = 0.0f;
			//float fInventoryDrill = 0.0f;

			foreach (IMyBatteryBlock battery in lBatteryes) {
				sName = battery.DisplayNameText;
				if (battery.IsFunctional) {
					fSPower = battery.CurrentStoredPower;
					fCurStorPower += fSPower;
					fMaxStorPower += battery.MaxStoredPower;
					fCurInputPower += battery.CurrentInput;
					fCurOutputPower += battery.CurrentOutput;

					Echo(sName + ": " + fSPower);
				} else {
					Echo(sName + ": !no functional!");
				}
			}
			fCPowerP = (fCurStorPower / fMaxStorPower * 100);

			Echo("Макс заряд: " + fMaxStorPower + "\n");
			Echo("Заряд: " + fCPowerP.ToString("0.00") + "%");
			Echo("Потр: " + fCurInputPower.ToString("0.00"));
			Echo("Отдача: " + fCurOutputPower.ToString("0.00"));

			sDisp = "Батарея: " + fCPowerP.ToString("0") + "%\n";
			sDisp += "<- " + fCurInputPower.ToString("0.00")
				+ " | " + fCurOutputPower.ToString("0.00") + " ->";

			lTextSurface[1].WriteText(sDisp);

			if (fCPowerP >= 50) {
				lTextSurface[1].FontColor = Color.Lime;
			} else if (fCPowerP >= 25) {
				lTextSurface[1].FontColor = Color.Yellow;
			} else {
				lTextSurface[1].FontColor = Color.Red;
			}

			sDisp = "\n\n";
			if (bHydrogen) {
				foreach (IMyGasTank gas in lHydrogen) {
					//Echo(gas.GetType().ToString());
					//Echo(gas.DisplayNameText+": " + gas.Capacity);
					fGas = ((float)gas.FilledRatio * 100);
					sDisp += gas.DisplayNameText + ": " + fGas.ToString("0") + "%\n";
				}
			}

			if (bGrinder) {
				foreach (IMyShipDrill drill in lDrill) {
					sDisp += drill.DisplayNameText + ": " + drill.GetInventory().MaxVolume + "\n";
				}
			}

			Echo(sDisp);
			lTextSurface[0].WriteText(sDisp);
		}

		// End Script
	}
}
