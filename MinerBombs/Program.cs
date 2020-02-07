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
		// MinerBombs v1.0_b1 (pistons)

		// Скорость сварки боеголовки
		int speed = 10;
		// Скорость поршня
		float speedPiston = 5.0f;


		//Projector		//Welder		//Merge Block		//Warhead
		IMyProjector myProjector;
		List<IMyShipWelder> LmyWelders;
		List<IMyShipMergeBlock> LmyMergeBlocks;
		List<IMyWarhead> LmyWarhead;
		List<IMyPistonBase> LmyPistonBase;


		//// Переменные
		bool on = false;            // включен
		int ticks = 0;          // тики
		string debug = "";
		// PistonStatus pistonStatus;
		bool pistonsExtended = false;
		bool pistonsRetracted = false;
		bool pistonsStoped = false;
		// Projector
		int remainingBlocks = 0;
		int totalBlocks = 0;
		// MergeBlocks
		bool enabledMergeBlock = false;
		int countMergeBlocks;

		bool started = false;
		int gluck = 0;
		int regluck = 0;


		public Program() {
			myProjector = GridTerminalSystem.GetBlockWithName("Projector") as IMyProjector;

			LmyWelders = new List<IMyShipWelder>();
			LmyMergeBlocks = new List<IMyShipMergeBlock>();
			LmyWarhead = new List<IMyWarhead>();
			LmyPistonBase = new List<IMyPistonBase>();

			Runtime.UpdateFrequency = UpdateFrequency.Update10;

			//on = false;
			//ticks = 0;
			//regluck = 0;
			Main("Stop", UpdateType.Script);    // Автопуск
		}

		public void Save() {

		}

		public void Main(string argument, UpdateType updateSource) {

			// Выполнение скрипта по таймеру
			if (updateSource == UpdateType.Update10) {

				// Останов, если отключена проекция
				if (!myProjector.IsProjecting) {
					on = false;
				}

				// Выполнение
				if (on == true) {
					//debug = "";
					remainingBlocks = myProjector.RemainingBlocks;
					totalBlocks = myProjector.TotalBlocks;
					countMergeBlocks = LmyMergeBlocks.Count;

					//debug += "Всего соединителей: " + countMergeBlocks.ToString() + "\n";


					GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(LmyPistonBase, block => block.IsSameConstructAs(Me));
					GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(LmyMergeBlocks, block => block.IsSameConstructAs(Me));
					GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(LmyWelders, block => block.IsSameConstructAs(Me));
					GridTerminalSystem.GetBlocksOfType<IMyWarhead>(LmyWarhead, block => block.IsSameConstructAs(Me));


					// IMyPistonBase
					foreach (IMyPistonBase myPiston in LmyPistonBase) {
						switch (myPiston.Status) {
							case PistonStatus.Extended:
								pistonsExtended = true;
								pistonsRetracted = false;
								pistonsStoped = false;
								break;
							case PistonStatus.Retracted:
								pistonsExtended = false;
								pistonsRetracted = true;
								pistonsStoped = false;
								break;
							case PistonStatus.Stopped:
								pistonsExtended = false;
								pistonsRetracted = false;
								pistonsStoped = true;
								break;
							case PistonStatus.Extending:
								pistonsExtended = false;
								pistonsRetracted = false;
								pistonsStoped = false;
								break;
							case PistonStatus.Retracting:
								pistonsExtended = false;
								pistonsRetracted = false;
								pistonsStoped = false;
								break;
						}
					}

					// Отсоединение
					if (remainingBlocks == 0) {
						if (ticks >= speed) {
							debug += "Соединитель ...\n";
							foreach (IMyShipMergeBlock mergeBlock in LmyMergeBlocks) {
								if (mergeBlock.IsFunctional) {
									mergeBlock.Enabled = false;
								}
								enabledMergeBlock = false;
								debug += "  отключили\n";
							}
							debug += "Боеголовка ...\n";
							foreach (IMyWarhead warhead in LmyWarhead) {
								if (warhead.IsFunctional && warhead.IsArmed == false) {
									warhead.IsArmed = true;
									debug += "  взведена!\n";
								}
							}
							started = true;
							if (speed < ticks) speed = ticks;
							ticks = 0;
							debug = "";
						}
					} else {

						// Антиглюк код
						if (ticks > 60) {
							gluck += 1;
							Main("Stop", UpdateType.None);
							return;
						}

						// Выдвигание/втягивание поршней
						//if ((enabledMergeBlock && (pistonsRetracted && !pistonsStoped)) || started) {
						if (started) {
							started = false;
							debug += "Останавливаем сварку\n";
							foreach (IMyShipWelder welder in LmyWelders) {
								if (welder.IsFunctional) {
									welder.Enabled = false;
								}
							}
							debug += "Выдвигание поршней\n";
							foreach (IMyPistonBase myPiston in LmyPistonBase) {
								if (speedPiston > myPiston.MaxVelocity) speedPiston = myPiston.MaxVelocity;
								myPiston.Velocity = speedPiston;
								myPiston.Extend();
							}
						} else if (enabledMergeBlock && (pistonsExtended && !pistonsStoped)) {
							debug += "Запускаем сварку\n";
							foreach (IMyShipWelder welder in LmyWelders) {
								if (welder.IsFunctional) {
									welder.Enabled = true;
								}
							}
							debug += "Втягивание поршней\n";
							foreach (IMyPistonBase myPiston in LmyPistonBase) {
								if (speedPiston > myPiston.MaxVelocity) speedPiston = myPiston.MaxVelocity;
								myPiston.Velocity = speedPiston;
								myPiston.Retract();
							}
						}

						// Включение соединителя
						if ((!pistonsExtended || !pistonsRetracted) && !enabledMergeBlock) {
							foreach (IMyShipMergeBlock mergeBlock in LmyMergeBlocks) {
								if (mergeBlock.IsFunctional) {
									mergeBlock.Enabled = true;
								}
								enabledMergeBlock = true;
								debug += "включили соединитель/и\n";
							}
						}

					}



					// Вывод сообщений в программном блоке
					Echo("Глюков: " + regluck.ToString());
					Echo("Тик: " + ticks.ToString());
					Echo("Скорость пуска: " + speed.ToString());
					Echo("Скорость поршня: " + speedPiston.ToString());
					Echo("Проектор включен? - " + myProjector.IsProjecting.ToString());
					Echo("  блоков осталось: " + remainingBlocks.ToString());
					Echo("Всего соединителей: " + countMergeBlocks.ToString());
					Echo("  соединитель включён? " + enabledMergeBlock.ToString());
					if (pistonsExtended) {
						Echo("Поршень выдвинут");
					} else if (pistonsRetracted) {
						Echo("Поршень втянут");
					} else if (pistonsStoped) {
						Echo("Поршень остановлен");
					} else {
						Echo("Поршень в движении");
					}
					Echo("Запущен? " + started.ToString());

					if (debug.Length > 1000) debug = debug.Substring(0, 999);
					Echo("DEBUG:\n" + debug);

					ticks += 1;

				} else {


				}







			} else {

				// Обработка запуска скрипта с параметром
				switch (argument) {
					case "Start":
						if (gluck > 0 && (ticks > 80)) {
							Echo("Повторный запуск после глюка");
						} else {

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
									enabledMergeBlock = true;
								}
								on = true;
								debug = "";
								started = true;
								ticks = 0;
							} else {
								Echo("Проектор выключен!");
							}
						}
						break;

					case "Stop":
						on = false;
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
						Echo("Втягиваем поршни");
						GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(LmyPistonBase, block => block.IsSameConstructAs(Me));
						foreach (IMyPistonBase myPiston in LmyPistonBase) {
							myPiston.Velocity = myPiston.MaxVelocity;
							myPiston.Retract();
						}
						if (gluck > 0) {
							ticks = 0;
							Main("Start", UpdateType.Script);
						}
						break;

					case "Speed+":
						speed -= 1;
						if (speed < 10) speed = 10;
						break;

					case "Speed-":
						speed += 1;
						if (speed > 300) speed = 300;
						break;

					case "Restart":


						break;

					default:
						break;
				}

			}

		}
	}
}
