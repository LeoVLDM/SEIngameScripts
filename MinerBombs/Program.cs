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

		// Скорость сварки боеголовки
		int speed = 30;
		// Скорость поршня
		int speedPiston = 3;
		

		//Projector		//Welder		//Merge Block		//Warhead
		IMyProjector myProjector;
		List<IMyShipWelder> LmyWelders;
		List<IMyShipMergeBlock> LmyMergeBlocks;
		List<IMyWarhead> LmyWarhead;
		//List<IMyPistonBase> LmyPistonBases;
		

		bool on;
		int time;

		public Program() {
			myProjector = GridTerminalSystem.GetBlockWithName("Projector") as IMyProjector;

			LmyWelders = new List<IMyShipWelder>();
			LmyMergeBlocks = new List<IMyShipMergeBlock>();
			LmyWarhead = new List<IMyWarhead>();
			//LmyPistonBases = new List<IMyPistonBase>();

			Runtime.UpdateFrequency = UpdateFrequency.Update10;

			on = false;
			time = 0;

			//Main("Start", UpdateType.None);	// Автопуск
		}

		public void Save() {

		}

		public void Main(string argument, UpdateType updateSource) {
			if (updateSource == UpdateType.Update10) {

				Echo("Проектор включен? - " + myProjector.IsProjecting.ToString());
				Echo("Скорость пуска: " + speed.ToString());
				if (!myProjector.IsProjecting) {
					on = false;
				}

				if (on == true) {
					Echo("Блоков осталось: " + myProjector.RemainingBlocks.ToString());
					Echo("Тик: " + time.ToString());
					//if (time==10 && projector.RemainingBlocks==0) {
					if (time==5) {
						GridTerminalSystem.GetBlocksOfType<IMyWarhead>(LmyWarhead, block => block.IsSameConstructAs(Me));
						Echo("Боеголовка ... ("+ LmyWarhead.Count + ")");
						if (LmyWarhead.Count > 0) {
							foreach (IMyWarhead warhead in LmyWarhead) {
								if (warhead.IsFunctional && warhead.IsArmed == false) {
									warhead.IsArmed = true;
									Echo("\t взведена!");
								}
							}

							GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(LmyMergeBlocks, block => block.IsSameConstructAs(Me));
							foreach (IMyShipMergeBlock mergeBlock in LmyMergeBlocks) {
								Echo("Блок соединён? - " + mergeBlock.IsConnected);
								if (mergeBlock.IsConnected) {
									mergeBlock.Enabled = false;
									Echo("Пуск боеголовки!");
								}
							}

						} else {
							Echo("\t не готова!");
							Main("Stop", UpdateType.None);
							Main("Start", UpdateType.None);
						}

					}
					//if (time==30 && projector.RemainingBlocks != 0) {
					if (time==speed) {
						GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(LmyMergeBlocks, block => block.IsSameConstructAs(Me));
						foreach (IMyShipMergeBlock mergeBlock in LmyMergeBlocks) {
							if (mergeBlock.IsFunctional) {
								mergeBlock.Enabled = true;
								Echo("Соединитель включён!");
							}
						}
					}
					if (time > speed) time = 0;
					time += 1;
				}
			} else {
				switch (argument) {
					case "Start":
						if (myProjector.IsProjecting) {
							Echo("Запускаем сварку");
							GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(LmyWelders, block => block.IsSameConstructAs(Me));
							foreach (IMyShipWelder welder in LmyWelders) {
								if (welder.IsFunctional) {
									welder.Enabled = true;
								}
							}
							Echo("Включаем соединитель");
							GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(LmyMergeBlocks, block => block.IsSameConstructAs(Me));
							if (LmyMergeBlocks.Count > 0) {
								foreach (IMyShipMergeBlock mergeBlock in LmyMergeBlocks) {
									mergeBlock.Enabled = true;
								}
							}
							on = true;
						} else {
							Echo("Проектор выключен!");
						}
						break;

					case "Stop":
						Echo("Останавливаем сварку");
						GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(LmyWelders, block => block.IsSameConstructAs(Me));
						foreach (IMyShipWelder welder in LmyWelders) {
							if (welder.IsFunctional) {
								welder.Enabled = false;
							}
						}
						Echo("Отключаем соединитель");
						GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(LmyMergeBlocks, block => block.IsSameConstructAs(Me));
						if (LmyMergeBlocks.Count > 0) {
							foreach (IMyShipMergeBlock mergeBlock in LmyMergeBlocks) {
								mergeBlock.Enabled = false;
							}
						}
						on = false;
						break;

					case "Speed+":
						speed -= 1;
						if (speed < 10) speed = 10;
						break;

					case "Speed-":
						speed += 1;
						if (speed > 50) speed = 50;
						break;

					default:
						break;
				}

			}

		}
	}
}
