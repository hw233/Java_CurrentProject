//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: Protocol/Snapshot.proto
namespace PB
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"PlayerSnapInfo")]
  public partial class PlayerSnapInfo : global::ProtoBuf.IExtensible
  {
    public PlayerSnapInfo() {}
    
    private int _id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int id
    {
      get { return _id; }
      set { _id = value; }
    }
    private string _nickname;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"nickname", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string nickname
    {
      get { return _nickname; }
      set { _nickname = value; }
    }
    private int _level;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"level", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int level
    {
      get { return _level; }
      set { _level = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"AllianceSnapInfo")]
  public partial class AllianceSnapInfo : global::ProtoBuf.IExtensible
  {
    public AllianceSnapInfo() {}
    
    private int _id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int id
    {
      get { return _id; }
      set { _id = value; }
    }
    private int _allianceId;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"allianceId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int allianceId
    {
      get { return _allianceId; }
      set { _allianceId = value; }
    }
    private int _playerId;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"playerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerId
    {
      get { return _playerId; }
      set { _playerId = value; }
    }
    private int _contribution;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"contribution", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int contribution
    {
      get { return _contribution; }
      set { _contribution = value; }
    }
    private int _postion;
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"postion", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int postion
    {
      get { return _postion; }
      set { _postion = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"SnapshotInfo")]
  public partial class SnapshotInfo : global::ProtoBuf.IExtensible
  {
    public SnapshotInfo() {}
    
    private int _playerId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"playerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerId
    {
      get { return _playerId; }
      set { _playerId = value; }
    }
    private PlayerSnapInfo _playerInfo;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"playerInfo", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public PlayerSnapInfo playerInfo
    {
      get { return _playerInfo; }
      set { _playerInfo = value; }
    }
    private AllianceSnapInfo _allianceInfo;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"allianceInfo", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public AllianceSnapInfo allianceInfo
    {
      get { return _allianceInfo; }
      set { _allianceInfo = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}