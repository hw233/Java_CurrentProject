//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: Protocol/Chat.proto
namespace PB
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSSendChat")]
  public partial class HSSendChat : global::ProtoBuf.IExtensible
  {
    public HSSendChat() {}
    
    private string _chatMsg;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"chatMsg", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string chatMsg
    {
      get { return _chatMsg; }
      set { _chatMsg = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSPushChat")]
  public partial class HSPushChat : global::ProtoBuf.IExtensible
  {
    public HSPushChat() {}
    
    private int _type;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"type", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int type
    {
      get { return _type; }
      set { _type = value; }
    }
    private int _playerId;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"playerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerId
    {
      get { return _playerId; }
      set { _playerId = value; }
    }
    private string _name;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"name", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string name
    {
      get { return _name; }
      set { _name = value; }
    }
    private string _chatMsg;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"chatMsg", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string chatMsg
    {
      get { return _chatMsg; }
      set { _chatMsg = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}