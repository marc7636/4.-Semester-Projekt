﻿using AssemblyLineManager.CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyLineManager.Core
{
    internal class AssemblyLineThreadManager
    {
        Dictionary<string, ICommunicationController> instances;
        Thread thread;
        ManualResetEvent manualResetEvent = new ManualResetEvent(true);

        public AssemblyLineThreadManager(Dictionary<string, ICommunicationController> instances)
        {
            this.instances = instances;

            thread = new Thread(AssemblyLineThread);
        }

        public void StartThread()
        {
            thread.Start();
        }

        public void ResumeThread()
        {
            manualResetEvent.Set();
        }

        public void PauseThread()
        {
            manualResetEvent.Reset();
        }

        public void StopThread()
        {
            manualResetEvent.Reset();
            thread.Interrupt();
            thread.Join();
        }

        public void AssemblyLineThread()
        {
            Func<bool>[] orderOfOperations = new Func<bool>[] {
                //() => instances["Warehouse"].SendCommand("Put Item", new string[] { "1", "Assemblen't Piece" }),
                () => instances["Warehouse"].SendCommand("Pick Item", new string[] { "1" }),
                () => instances["AGV"].SendCommand("MoveToStorageOperation"),
                () => instances["AGV"].SendCommand("PickWarehouseOperation"),
                () => instances["AGV"].SendCommand("MoveToAssemblyOperation"),
                () => instances["AGV"].SendCommand("PutAssemblyOperation"),
                () => instances["AssemblyStation"].SendCommand("emulator/operation"),
                () => instances["AGV"].SendCommand("PickAssemblyOperation"),
                () => instances["AGV"].SendCommand("MoveToStorageOperation"),
                () => instances["AGV"].SendCommand("PutWarehouseOperation"),
                () => instances["Warehouse"].SendCommand("Insert Item", new string[] { "1", "Assembled Piece" })
            };

            foreach (var operation in orderOfOperations)
            {
                manualResetEvent.WaitOne();
                if (operation())
                {
                    Console.WriteLine(operation.Method.ToString() + " completed successfully!");
                }
                else
                {
                    Console.WriteLine(operation.Method.ToString() + " did not complete successfully!");
                }
            }
        }
    }
}
