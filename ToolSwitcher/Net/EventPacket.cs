using ProtoBuf;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.ObjectBuilders;

namespace avaness.ToolSwitcher.Net
{
    [ProtoContract]
    public class EventPacket
    {
        public const ushort PacketId = 5465;

        [ProtoMember(1)]
        private byte mode;

        [ProtoMember(2)]
        private SerializableDefinitionId id;

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        public EventPacket() { }

        public EventPacket(Mode mode, MyDefinitionId id)
        {
            this.mode = (byte)mode;
            this.id = id;
        }

        public static void Received(byte[] data)
        {
            EventPacket packet = MyAPIGateway.Utilities.SerializeFromBinary<EventPacket>(data);
            if (packet != null)
                packet.Received();
        }

        public void SendTo(long id)
        {
            if(MyAPIGateway.Session.Player != null && (id == 0 || id == MyAPIGateway.Session.Player.IdentityId))
            {
                Received();
            }
            else
            {
                ulong uid = MyAPIGateway.Players.TryGetSteamId(id);
                if(uid != 0)
                {
                    byte[] data = MyAPIGateway.Utilities.SerializeToBinary(this);
                    MyAPIGateway.Multiplayer.SendMessageTo(PacketId, data, uid);
                }
            }
        }

        public void Received()
        {
            switch ((Mode)mode)
            {
                case Mode.PickUp:
                    ToolSwitcherSession.Instance.CheckItem(id, true);
                    break;
                case Mode.Drop:
                    ToolSwitcherSession.Instance.CheckItem(id, false);
                    break;
                case Mode.Spawn:
                    ToolSwitcherSession.Instance.EquipAll();
                    break;
            }

        }

        public enum Mode : byte
        {
            PickUp, Drop, Spawn
        }
    }
}
