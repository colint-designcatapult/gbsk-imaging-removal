using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Heracles.Core.Models;
using Xcc.Core.Logging;

namespace Heracles.Application.Services
{
    public class WakeOnLanService : IWakeOnLanService
    {
        #region Constructors
        public WakeOnLanService(IHeraclesMainSettings heraclesMainSettings, ILogRepository logWriter)
        {
            _heraclesMainSettings = heraclesMainSettings;
            _logWriter = logWriter;
        }
        #endregion Constructors

        #region Private Properties
        private IHeraclesMainSettings _heraclesMainSettings;
        private ILogRepository _logWriter;
        private IList<int> _ports = new List<int>() {7}; // { 0, 7, 9 }; according to https://en.wikipedia.org/wiki/Wake-on-LAN
        #endregion Private Properties
        #region Private Methods
        private async Task sendMagicPacket(PhysicalAddress remoteMacAdress, string remoteAddress, int remotePort)
        {
            byte[] macAddrInBytes = remoteMacAdress.GetAddressBytes();
            byte[] packet = generateMagicPacket(macAddrInBytes);
            // Send the magic packet using UDP
            using (UdpClient client = new UdpClient())
            {
                client.Connect(remoteAddress, remotePort);
                await client.SendAsync(packet, packet.Length);
                await _logWriter.LogAsync($"WakeOnLanService.sendMagicPacket: Magic packet sent to mac={remoteMacAdress.ToString()} remoteAddress={remoteAddress} port={remotePort}", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);
            }
        }

        private static byte[] generateMagicPacket(byte[] macBytes)
        {
            // Create the magic packet
            byte[] packet = new byte[102];
            for (int i = 0; i < 6; i++)
            {
                packet[i] = 0xFF;
            }
            for (int i = 1; i <= 16; i++)
            {
                Array.Copy(macBytes, 0, packet, i * 6, 6);
            }

            return packet;
        }
        #endregion Private Methods
        #region Public Methods
        public async Task WakeUpAsync()
        {
            foreach (int port in _ports)
            {
                // Robot wake-up removed as part of robot control system removal
                //await sendMagicPacket(_heraclesMainSettings.RobotGrpcServerMac, IPAddress.Broadcast.ToString(), port);
                // TODO: get mac by RobotGrpcServerUri
                //await sendMagicPacket(_appSettings.RobotGrpcServerMac, _appSettings.RobotGrpcServerUri.Host, port);
            }
        }
        #endregion Public Methods
    }
}
