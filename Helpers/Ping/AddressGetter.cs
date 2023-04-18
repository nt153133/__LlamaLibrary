using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers.Ping
{
    /**
     * Almost every part of this class was taken from
     * https://github.com/karashiiro/PingPlugin
     * and karashiiro takes credit for getting the current ffxiv server ip
     */
    public class AddressGetter
    {
        private static readonly LLogger Log = new(nameof(AddressGetter), Colors.Chocolate);

        private const int AF_INET = 2;
        private const int TCP_TABLE_OWNER_PID_CONNECTIONS = 4;
        private const int MIB_TCP_STATE_LISTEN = 2;

        private const ushort XIV_MIN_PORT_1 = 54992;
        private const ushort XIV_MAX_PORT_1 = 54994;
        private const ushort XIV_MIN_PORT_2 = 55006;
        private const ushort XIV_MAX_PORT_2 = 55007;
        private const ushort XIV_MIN_PORT_3 = 55021;
        private const ushort XIV_MAX_PORT_3 = 55040;
        private const ushort XIV_MIN_PORT_4 = 55296;
        private const ushort XIV_MAX_PORT_4 = 55551;

        public IPAddress Address { get; set; }
        public int Port { get; set; }

        public IPAddress GetAddress(bool verbose = false)
        {
            IntPtr pTcpTable = IntPtr.Zero;
            var address = IPAddress.Loopback;
            try
            {
                var bufferLength = 0;
                _ = GetExtendedTcpTable(IntPtr.Zero, ref bufferLength, false, AF_INET, TCP_TABLE_OWNER_PID_CONNECTIONS);

                pTcpTable = Marshal.AllocHGlobal(bufferLength);

                var error = GetExtendedTcpTable(pTcpTable, ref bufferLength, false, AF_INET, TCP_TABLE_OWNER_PID_CONNECTIONS);
                if (error != (uint)WinError.NO_ERROR)
                {
                    return IPAddress.Loopback;
                }

                var table = new List<TcpRow>();
                var rowSize = Marshal.SizeOf<TcpRow>();
                var dwNumEntries = Marshal.ReadInt32(pTcpTable);
                var pRows = pTcpTable + 4;
                for (var i = 0; i < dwNumEntries && bufferLength - (4 + (i * rowSize)) >= rowSize; i++)
                {
                    var nextRow = Marshal.PtrToStructure<TcpRow>(pRows + (i * rowSize));
                    table.Add(nextRow);
                }

                var pid = Core.Memory.Process.Id;
                for (var i = 0; i < table.Count; i++)
                {
                    var state = table[i].dwState;
                    var tcpPid = table[i].dwOwningPid;
                    var tcpRemoteAddr = new IPAddress(table[i].dwRemoteAddr);
                    var tcpRemotePort = (ushort)table[i].dwRemotePort;
                    var trpBytes = BitConverter.GetBytes(tcpRemotePort).Reverse().ToArray();
                    tcpRemotePort = BitConverter.ToUInt16(trpBytes, 0);

                    if (state == MIB_TCP_STATE_LISTEN || Equals(tcpRemoteAddr, IPAddress.Loopback))
                    {
                        continue;
                    }

                    // ReSharper disable once InvertIf
                    if ((int)tcpPid == pid && InXIVPortRange(tcpRemotePort))
                    {
                        address = tcpRemoteAddr;
                        Port = tcpRemotePort;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            finally
            {
                if (pTcpTable != IntPtr.Zero)
                    Marshal.FreeHGlobal(pTcpTable);
            }

            if (verbose && !Equals(address, IPAddress.Loopback) && !Equals(address, Address))
            {
                Log.Information($"Detected newly-connected FFXIV server address {address}");
            }

            Address = address;
            return address;
        }

        private static bool InXIVPortRange1(ushort port)
        {
            return port is >= XIV_MIN_PORT_1 and <= XIV_MAX_PORT_1;
        }

        private static bool InXIVPortRange2(ushort port)
        {
            return port is >= XIV_MIN_PORT_2 and <= XIV_MAX_PORT_2;
        }

        private static bool InXIVPortRange3(ushort port)
        {
            return port is >= XIV_MIN_PORT_3 and <= XIV_MAX_PORT_3;
        }

        private static bool InXIVPortRange4(ushort port)
        {
            return port is >= XIV_MIN_PORT_4 and <= XIV_MAX_PORT_4;
        }

        private static bool InXIVPortRange(ushort port)
        {
            return InXIVPortRange1(port) || InXIVPortRange2(port) || InXIVPortRange3(port) || InXIVPortRange4(port);
        }

        [DllImport("Iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, int tblClass, uint reserved = 0);

        [StructLayout(LayoutKind.Sequential)]
        private struct TcpRow
        {
            public readonly uint dwState;
            public readonly uint dwLocalAddr;
            public readonly uint dwLocalPort;
            public readonly uint dwRemoteAddr;
            public readonly uint dwRemotePort;
            public readonly uint dwOwningPid;
        }
    }

    public enum WinError
    {
        UNKNOWN = -1,
        NO_ERROR = 0,
        ACCESS_DENIED = 5,
        NOT_ENOUGH_MEMORY = 8,
        OUTOFMEMORY = 14,
        NOT_SUPPORTED = 50,
        INVALID_PARAMETER = 87,
        ERROR_INVALID_NETNAME = 1214,
        WSAEINTR = 10004,
        WSAEACCES = 10013,
        WSAEFAULT = 10014,
        WSAEINVAL = 10022,
        WSAEWOULDBLOCK = 10035,
        WSAEINPROGRESS = 10036,
        WSAEALREADY = 10037,
        WSAENOTSOCK = 10038,
        WSAENETUNREACH = 10051,
        WSAENETRESET = 10052,
        WSAECONNABORTED = 10053,
        WSAECONNRESET = 10054,
        IP_REQ_TIMED_OUT = 11010,
    }
}