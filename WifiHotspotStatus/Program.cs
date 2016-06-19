using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WifiHotspotStatus
{
    class Program
    {
        static void Main(string[] args)
        {

            var state = NWork.GetHotSpotState();
            switch (state)
            {
                case NWork.HotSpotState.Idle:
                    Console.WriteLine("HOTSPOT set but not started");
                    break;
                case NWork.HotSpotState.Active:
                    Console.WriteLine("HOTSPOT set and running");
                    break;
                case NWork.HotSpotState.Unavailable:
                    Console.WriteLine("HOTSPOT not set");
                    break;
                default:
                    break;
            }
            Console.ReadKey();
        }
        
    }

    public static class NWork
    {
        public enum HotSpotState
        {
            Idle,
            Active,
            Unavailable

        }

        public static HotSpotState GetHotSpotState()
        {
            IntPtr pNetStatus = new IntPtr();
            IntPtr clientHandle;
            uint negotiatedVersion;
            HotSpotState returnValue = HotSpotState.Unavailable;

            var openHandleSuccess = WlanOpenHandle(2, IntPtr.Zero, out negotiatedVersion, out clientHandle);

            uint hostedNetworkQueryStatusSuccess = WlanHostedNetworkQueryStatus(clientHandle, out pNetStatus, IntPtr.Zero);
            if (openHandleSuccess == 0)
            {
                var netStat = (WLAN_HOSTED_NETWORK_STATUS)Marshal.PtrToStructure(pNetStatus, typeof(WLAN_HOSTED_NETWORK_STATUS));

                if (netStat.HostedNetworkState != WLAN_HOSTED_NETWORK_STATE.wlan_hosted_network_unavailable)
                {
                    
                    returnValue = netStat.HostedNetworkState == WLAN_HOSTED_NETWORK_STATE.wlan_hosted_network_active ? HotSpotState.Active : HotSpotState.Idle;
                }
                WlanCloseHandle(clientHandle, IntPtr.Zero);
                WlanFreeMemory(pNetStatus);

            }
            return returnValue;
        }

        [DllImport("Wlanapi", EntryPoint = "WlanFreeMemory")]
        public static extern void WlanFreeMemory([In] IntPtr pMemory);

        [DllImport("Wlanapi.dll")]
        private static extern int WlanOpenHandle(
         uint dwClientVersion,
         IntPtr pReserved, //not in MSDN but required
         [Out] out uint pdwNegotiatedVersion,
         out IntPtr ClientHandle);

        [DllImport("Wlanapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WlanCloseHandle(
          [In] IntPtr hClientHandle,
          IntPtr pReserved
        );
        [DllImport("Wlanapi.dll", SetLastError = true)]
        static extern UInt32 WlanHostedNetworkQueryStatus(
            [In] IntPtr hClientHandle,
            [Out] out IntPtr ppWlanHostedNetworkStatus,
            [In, Out] IntPtr pvReserved
        );

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WLAN_HOSTED_NETWORK_STATUS
        {
            public WLAN_HOSTED_NETWORK_STATE HostedNetworkState;
            public Guid IPDeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
            public string wlanHostedNetworkBSSID;
            public _DOT11_PHY_TYPE dot11PhyType;
            public UInt32 ulChannelFrequency;
            public UInt32 dwNumberOfPeers;
            public IntPtr PeerList;
        }
        public enum WLAN_HOSTED_NETWORK_STATE
        {
            wlan_hosted_network_unavailable,
            wlan_hosted_network_idle,
            wlan_hosted_network_active
        }
        public enum _DOT11_PHY_TYPE : uint
        {
            dot11_phy_type_unknown = 0,
            dot11_phy_type_any = 0,
            dot11_phy_type_fhss = 1,
            dot11_phy_type_dsss = 2,
            dot11_phy_type_irbaseband = 3,
            dot11_phy_type_ofdm = 4,
            dot11_phy_type_hrdsss = 5,
            dot11_phy_type_erp = 6,
            dot11_phy_type_ht = 7,
            dot11_phy_type_IHV_start = 0x80000000,
            dot11_phy_type_IHV_end = 0xffffffff
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct _WLAN_HOSTED_NETWORK_PEER_STATE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
            public string PeerMacAddress;
            _WLAN_HOSTED_NETWORK_PEER_AUTH_STATE PeerAuthState;
        }

        public enum _WLAN_HOSTED_NETWORK_PEER_AUTH_STATE
        {
            wlan_hosted_network_peer_state_invalid,
            wlan_hosted_network_peer_state_authenticated
        }
    }
}