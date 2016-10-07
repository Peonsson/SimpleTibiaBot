using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SimpleTibiaBot
{
    class Program
    {
        private static IntPtr Handle;

        [Flags]
        public enum ProcessAccessType
        {
            PROCESS_TERMINATE = (0x0001),
            PROCESS_CREATE_THREAD = (0x0002),
            PROCESS_SET_SESSIONID = (0x0004),
            PROCESS_VM_OPERATION = (0x0008),
            PROCESS_VM_READ = (0x0010),
            PROCESS_VM_WRITE = (0x0020),
            PROCESS_DUP_HANDLE = (0x0040),
            PROCESS_CREATE_PROCESS = (0x0080),
            PROCESS_SET_QUOTA = (0x0100),
            PROCESS_SET_INFORMATION = (0x0200),
            PROCESS_QUERY_INFORMATION = (0x0400),
            PROCESS_QUERY_LIMITED_INFORMATION = (0x1000)
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);
        [DllImport("kernel32")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);

        public void OpenProcess()
        {
            ProcessAccessType access = ProcessAccessType.PROCESS_QUERY_INFORMATION |
                ProcessAccessType.PROCESS_VM_READ |
                ProcessAccessType.PROCESS_VM_WRITE |
                ProcessAccessType.PROCESS_VM_OPERATION;
        }

        public static byte[] ReadBytes(IntPtr Handle, Int64 Address, uint BytesToRead)
        {
            IntPtr bytesRead;
            byte[] buffer = new byte[BytesToRead];
            ReadProcessMemory(Handle, new IntPtr(Address), buffer, BytesToRead, out bytesRead);
            return buffer;
        }

        public static int ReadInt32(Int64 Address, IntPtr Handle)
        {
            return BitConverter.ToInt32(ReadBytes(Handle, Address, 4), 0);
        }

        public static string ReadString(long Address, IntPtr Handle, uint length = 32)
        {
            return ASCIIEncoding.Default.GetString(ReadBytes(Handle, Address, length)).Split('\0')[0];
        }

        public static void WriteMemory(IntPtr Address, byte[] buffer, out int bytesWritten)
        {
            IntPtr pBytesWritten = IntPtr.Zero;
            WriteProcessMemory(Handle, Address, buffer, (uint)buffer.Length, out pBytesWritten);
            bytesWritten = pBytesWritten.ToInt32();
        }

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetWindowSize(60, 60);

            Process Tibia;
            try
            {
                Tibia = Process.GetProcessesByName("classictibia")[0];
            } catch
            {
                return;
            }
            
            Handle = Tibia.Handle;
            UInt32 Base = (UInt32) Tibia.MainModule.BaseAddress.ToInt32();
            int bytesOut;

            //Skills, hp, mana etc. (generally the skills window)
            UInt32 Adr_Exp = 0x1C6840;
            UInt32 Adr_Lvl = 0x1C683C;
            UInt32 Adr_Hp = 0x1C6848;
            UInt32 Adr_Mana = 0x1C682C;
            UInt32 Adr_Cap = 0x1C6820;
            UInt32 Adr_MlvlLvl = 0x1C6838;
            UInt32 Adr_MlvlPerc = 0x1C6830;
            UInt32 Adr_FistLvl = 0x1C67F8;
            UInt32 Adr_FistPerc = 0x1C67DC;
            UInt32 Adr_ClubLvl = 0x1C67FC;
            UInt32 Adr_ClubPerc = 0x1C67E0;
            UInt32 Adr_SwordLvl = 0x1C6800;
            UInt32 Adr_SwordPerc = 0x1C67E4;
            UInt32 Adr_AxeLvl = 0x1C6804;
            UInt32 Adr_AxePerc = 0x1C67E8;
            UInt32 Adr_DistLvl = 0x1C6808;
            UInt32 Adr_DistPerc = 0x1C67EC;
            UInt32 Adr_ShieldLvl = 0x1C680C;
            UInt32 Adr_ShieldPerc = 0x1C67F0;
            UInt32 Adr_FishLvl = 0x1C6810;
            UInt32 Adr_FishPerc = 0x1C67F4;

            // X, Y, Z maps, char etc.
            //UInt32 Adr_PlayerX = 0x1C68D4;
            //UInt32 Adr_PlayerY = 0x1C68D8;
            //UInt32 Adr_PlayerZ = 0x1C68DC;
            //UInt32 Adr_PlayerR = 0x1C68DC;

            // Others
            //UInt32 Adr_MouseUse = $71C5E8;
            UInt32 Adr_BListBegin = 0x1C68B4; //begining of battle list
            //UInt32 Adr_BListEnd = 0x1CC248; //end of battle list
            //UInt32 Adr_SbText = $71DBE0; //Adress of the status bar 
            //UInt32 Adr_SbTimer = $71DBDC;
            //UInt32 Adr_FirstCont = 0x1CEDD8; //Adress of first open bp(not sure)
            UInt32 Adr_PlayerID = 0x1C684C;
            //UInt32 Adr_BPArray = 0x1CEE14;//Backpack array
            //UInt32 Adr_PlayerName = $194690;
            UInt32 Adr_FollowID = 0x1C6818;
            UInt32 Adr_AttackID = 0x1C681C; // ID of the person with the red square
            //UInt32 Adr_Online = $71C588; //tells if the char is online 0 = offline 8 = online
            //UInt32 Adr_LastItemClick1 = $71C630;
            //UInt32 Adr_LastItemClick2 = $71C63C;
            //UInt32 Adr_TeaKey = $719D78;//key to crypt/decrypt the packets of tibiaclient
            
            //Battle Window
            UInt32 Adr_BL_Light_Percent_Offset = 0x70;
            UInt32 Adr_BL_Health_Percent_Offset = 0x80;
            UInt32 Adr_BL_X_Offset = 0x20;
            UInt32 Adr_BL_Y_Offset = 0x24;
            UInt32 Adr_BL_Z_Offset = 0x28;
            UInt32 Adr_BL_Direction_Offset = 0x4C;

            UInt32 myBase = 0;
            int counter = 0;
            while (true)
            {
                if (ReadString(Base + Adr_BListBegin + (0x9C * counter), Handle).Equals("Peon"))
                {
                    myBase = Base + Adr_BListBegin + (0x9C * (uint)counter);
                    break;
                }
                counter++;
            }

            while (true)
            {
                Console.WriteLine("[Skills window]");
                Console.WriteLine("Adr_Exp: " + Convert.ToString(ReadInt32(Base + Adr_Exp, Handle)));
                Console.WriteLine("Adr_Lvl: " + Convert.ToString(ReadInt32(Base + Adr_Lvl, Handle)));
                Console.WriteLine("Adr_Hp: " + Convert.ToString(ReadInt32(Base + Adr_Hp, Handle)));
                Console.WriteLine("Adr_Mana: " + Convert.ToString(ReadInt32(Base + Adr_Mana, Handle)));
                Console.WriteLine("Adr_Cap: " + Convert.ToString(ReadInt32(Base + Adr_Cap, Handle)));
                Console.WriteLine("Adr_MlvlLvl: " + Convert.ToString(ReadInt32(Base + Adr_MlvlLvl, Handle)));
                Console.WriteLine("Adr_MlvlPerc: " + Convert.ToString(ReadInt32(Base + Adr_MlvlPerc, Handle)));
                Console.WriteLine("Adr_FistLvl: " + Convert.ToString(ReadInt32(Base + Adr_FistLvl, Handle)));
                Console.WriteLine("Adr_FistPerc: " + Convert.ToString(ReadInt32(Base + Adr_FistPerc, Handle)));
                Console.WriteLine("Adr_ClubLvl: " + Convert.ToString(ReadInt32(Base + Adr_ClubLvl, Handle)));
                Console.WriteLine("Adr_ClubPerc: " + Convert.ToString(ReadInt32(Base + Adr_ClubPerc, Handle)));
                Console.WriteLine("Adr_SwordLvl: " + Convert.ToString(ReadInt32(Base + Adr_SwordLvl, Handle)));
                Console.WriteLine("Adr_SwordPerc: " + Convert.ToString(ReadInt32(Base + Adr_SwordPerc, Handle)));
                Console.WriteLine("Adr_AxeLvl: " + Convert.ToString(ReadInt32(Base + Adr_AxeLvl, Handle)));
                Console.WriteLine("Adr_AxePerc: " + Convert.ToString(ReadInt32(Base + Adr_AxePerc, Handle)));
                Console.WriteLine("Adr_DistLvl: " + Convert.ToString(ReadInt32(Base + Adr_DistLvl, Handle)));
                Console.WriteLine("Adr_DistPerc: " + Convert.ToString(ReadInt32(Base + Adr_DistPerc, Handle)));
                Console.WriteLine("Adr_ShieldLvl: " + Convert.ToString(ReadInt32(Base + Adr_ShieldLvl, Handle)));
                Console.WriteLine("Adr_ShieldPerc: " + Convert.ToString(ReadInt32(Base + Adr_ShieldPerc, Handle)));
                Console.WriteLine("Adr_FishLvl: " + Convert.ToString(ReadInt32(Base + Adr_FishLvl, Handle)));
                Console.WriteLine("Adr_FishPerc: " + Convert.ToString(ReadInt32(Base + Adr_FishPerc, Handle)));
                Console.WriteLine("Adr_FishPerc: " + Convert.ToString(ReadInt32(Base + Adr_FishPerc, Handle)));
                Console.WriteLine("Adr_FishPerc: " + Convert.ToString(ReadInt32(Base + Adr_FishPerc, Handle)));
                Console.WriteLine("Adr_FishPerc: " + Convert.ToString(ReadInt32(Base + Adr_FishPerc, Handle)));
                Console.WriteLine("");

                Console.WriteLine("[X, Y, Z]");
                Console.WriteLine("Adr_PlayerX: " + Convert.ToString(ReadInt32(myBase + Adr_BL_X_Offset, Handle)));
                Console.WriteLine("Adr_PlayerY: " + Convert.ToString(ReadInt32(myBase + Adr_BL_Y_Offset, Handle)));
                Console.WriteLine("Adr_PlayerZ: " + Convert.ToString(ReadInt32(myBase + Adr_BL_Z_Offset, Handle)));
                Console.WriteLine("");

                Console.WriteLine("[Others]");
                Console.WriteLine("Adr_PlayerID: " + Convert.ToString(ReadInt32(Base + Adr_PlayerID, Handle)));
                Console.WriteLine("Adr_FollowID: " + Convert.ToString(ReadInt32(Base + Adr_FollowID, Handle)));
                Console.WriteLine("Adr_AttackID: " + Convert.ToString(ReadInt32(Base + Adr_AttackID, Handle)));
                Console.WriteLine("");

                if (ReadInt32(myBase + Adr_BL_Light_Percent_Offset, Handle) < 12)
                {
                    WriteMemory((IntPtr)(myBase + Adr_BL_Light_Percent_Offset), BitConverter.GetBytes(12), out bytesOut);
                }

                counter = 0;
                while (true)
                {
                    counter++;
                    if (ReadInt32(Base + Adr_BListBegin - 4 + (0x9C * counter), Handle) == ReadInt32(Base + Adr_AttackID, Handle))
                    {
                        Console.WriteLine("[Target]");
                        Console.WriteLine("Target name: " + ReadString(Base + Adr_BListBegin + (0x9C * counter), Handle, 32));
                        Console.WriteLine("Target health%: " + ReadInt32(Base + Adr_BListBegin + Adr_BL_Health_Percent_Offset + (0x9C * counter), Handle));
                        Console.WriteLine("Target X: " + ReadInt32(Base + Adr_BListBegin + Adr_BL_X_Offset + (0x9C * counter), Handle));
                        Console.WriteLine("Target Y: " + ReadInt32(Base + Adr_BListBegin + Adr_BL_Y_Offset + (0x9C * counter), Handle));
                        Console.WriteLine("Target Z: " + ReadInt32(Base + Adr_BListBegin + Adr_BL_Z_Offset + (0x9C * counter), Handle));

                        int direction = ReadInt32(Base + Adr_BListBegin + Adr_BL_Direction_Offset + (0x9C * counter),Handle);
                        string temp;
                        if (direction == 0)
                            temp = "NORTH";
                        else if (direction == 1)
                            temp = "EAST";
                        else if (direction == 2)
                            temp = "SOUTH";
                        else if (direction == 3)
                            temp = "EAST";
                        else
                            temp = "FATAL ERROR";
                        Console.WriteLine("Target Facing: " + temp);
                        break;
                    }
                }
                Thread.Sleep(250);
                Console.Clear();
                Console.SetCursorPosition(0, 0);
            }
        }
    }
}