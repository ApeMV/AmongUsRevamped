using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;

namespace AmongUsRevamped;

public class BatchedMessage
{
    private readonly MessageWriter writer;
    private readonly int targetClientId;

    public BatchedMessage(int targetClientId = -1)
    {
        writer = MessageWriter.Get(SendOption.Reliable);
        this.targetClientId = targetClientId;

        if (targetClientId == -1)
        {
            writer.StartMessage(Tags.GameData);
            writer.Write(AmongUsClient.Instance.GameId);
        }
        else
        {
            writer.StartMessage(Tags.GameDataTo);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(targetClientId);
        }
    }

    private bool AmTarget => targetClientId == -1 || targetClientId == AmongUsClient.Instance.ClientId;

    public void QueueSetColor(PlayerControl source, byte color)
    {
        if (source == null || source.Data == null) return;

        if (AmTarget) source.SetColor(color);

        writer.StartMessage((byte)GameDataTypes.RpcFlag);
        writer.WritePacked(source.NetId);
        writer.Write((byte)RpcCalls.SetColor);
        writer.Write(source.Data.NetId);
        writer.Write(color);
        writer.EndMessage();
    }

    public void QueueSetRole(PlayerControl source, RoleTypes role, bool canOverride = false)
    {
        if (source == null) return;

        if (AmTarget) source.StartCoroutine(source.CoSetRole(role, canOverride));

        writer.StartMessage((byte)GameDataTypes.RpcFlag);
        writer.WritePacked(source.NetId);
        writer.Write((byte)RpcCalls.SetRole);
        writer.Write((ushort)role);
        writer.Write(canOverride);
        writer.EndMessage();
    }

    public void QueueMurderPlayer(PlayerControl source, PlayerControl target, MurderResultFlags result)
    {
        if (source == null || target == null) return;

        if (AmTarget) source.MurderPlayer(target, result);

        writer.StartMessage((byte)GameDataTypes.RpcFlag);
        writer.WritePacked(source.NetId);
        writer.Write((byte)RpcCalls.MurderPlayer);
        writer.WritePacked(target.NetId);
        writer.Write((int)result);
        writer.EndMessage();
    }

    public void QueueSetPetStr(PlayerControl source, string pet, byte seqId)
    {
        if (source == null || source.Data == null) return;

        if (AmTarget) source.SetPet(pet, source.Data.DefaultOutfit.ColorId);

        writer.StartMessage((byte)GameDataTypes.RpcFlag);
        writer.WritePacked(source.NetId);
        writer.Write((byte)RpcCalls.SetPetStr);
        writer.Write(pet);
        writer.Write(seqId);
        writer.EndMessage();
    }

    public void FinishBatch()
    {
        writer.EndMessage();
        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }
}
