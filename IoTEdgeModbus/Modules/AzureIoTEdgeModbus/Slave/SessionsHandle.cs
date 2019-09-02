namespace AzureIoTEdgeModbus.Slave
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// This class contains the handle for this module. In this case, it is a list of active Modbus sessions.
    /// </summary>
    public class SessionsHandle : ISessionsHandle
    {
        public async Task<SessionsHandle> CreateHandleFromConfiguration(ModuleConfig config)
        {
            SessionsHandle sessionsHandle = null;
            foreach (var config_pair in config.SlaveConfigs)
            {
                ModbusSlaveConfig slaveConfig = config_pair.Value;

                switch (slaveConfig.GetConnectionType())
                {
                    case ConnectionType.ModbusTCP:
                        {
                            if (sessionsHandle == null)
                            {
                                sessionsHandle = new SessionsHandle();
                            }

                            ModbusSlaveSession slave = new ModbusTCPSlaveSession(slaveConfig);
                            await slave.InitSessionAsync();
                            sessionsHandle.ModbusSessionList.Add(slave);
                            break;
                        }
                    case ConnectionType.ModbusRTU:
                        {
                            if (sessionsHandle == null)
                            {
                                sessionsHandle = new SessionsHandle();
                            }

                            ModbusSlaveSession slave = new ModbusRTUSlaveSession(slaveConfig);
                            await slave.InitSessionAsync();
                            sessionsHandle.ModbusSessionList.Add(slave);
                            break;
                        }
                    case ConnectionType.ModbusASCII:
                        {
                            break;
                        }
                    case ConnectionType.Unknown:
                        {
                            break;
                        }
                }
            }
            return sessionsHandle;
        }

        public List<ModbusSlaveSession> ModbusSessionList = new List<ModbusSlaveSession>();

        public ModbusSlaveSession GetSlaveSession(string hwid)
        {
            return this.ModbusSessionList.Find(x => x.config.HwId.ToUpper() == hwid.ToUpper());
        }
        public void Release()
        {
            foreach (var session in this.ModbusSessionList)
            {
                session.ReleaseSessionAsync();
            }
            this.ModbusSessionList.Clear();
        }
        public async Task<List<object>> CollectAndResetOutMessageFromSessionsAsync()
        {
            List<object> obj_list = new List<object>();

            foreach (ModbusSlaveSession session in this.ModbusSessionList)
            {
                var obj = session.GetOutMessage();
                if (obj != null)
                {
                    obj_list.Add(obj);
                    await session.ClearOutMessageAsync();
                }
            }
            return obj_list;
        }
    }
}