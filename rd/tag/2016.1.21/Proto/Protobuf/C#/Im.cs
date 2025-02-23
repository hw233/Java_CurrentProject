//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: Protocol/Im.proto
namespace PB
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSImMsg")]
  public partial class HSImMsg : global::ProtoBuf.IExtensible
  {
    public HSImMsg() {}
    
    private int _type;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"type", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int type
    {
      get { return _type; }
      set { _type = value; }
    }
    private int _channel;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"channel", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int channel
    {
      get { return _channel; }
      set { _channel = value; }
    }
    private int _senderId;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"senderId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int senderId
    {
      get { return _senderId; }
      set { _senderId = value; }
    }
    private string _senderName = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"senderName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string senderName
    {
      get { return _senderName; }
      set { _senderName = value; }
    }
    private string _origText;
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"origText", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string origText
    {
      get { return _origText; }
      set { _origText = value; }
    }
    private string _transText = "";
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"transText", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string transText
    {
      get { return _transText; }
      set { _transText = value; }
    }
    private string _expansion = "";
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"expansion", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string expansion
    {
      get { return _expansion; }
      set { _expansion = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSImPlayer")]
  public partial class HSImPlayer : global::ProtoBuf.IExtensible
  {
    public HSImPlayer() {}
    
    private int _playerId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"playerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerId
    {
      get { return _playerId; }
      set { _playerId = value; }
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
    private int _guildId = default(int);
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"guildId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int guildId
    {
      get { return _guildId; }
      set { _guildId = value; }
    }
    private string _guildName = "";
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"guildName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string guildName
    {
      get { return _guildName; }
      set { _guildName = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSImChatSend")]
  public partial class HSImChatSend : global::ProtoBuf.IExtensible
  {
    public HSImChatSend() {}
    
    private int _channel;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"channel", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int channel
    {
      get { return _channel; }
      set { _channel = value; }
    }
    private string _text;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"text", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string text
    {
      get { return _text; }
      set { _text = value; }
    }
    private string _expansion = "";
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"expansion", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string expansion
    {
      get { return _expansion; }
      set { _expansion = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSImPush")]
  public partial class HSImPush : global::ProtoBuf.IExtensible
  {
    public HSImPush() {}
    
    private readonly global::System.Collections.Generic.List<HSImMsg> _imMsg = new global::System.Collections.Generic.List<HSImMsg>();
    [global::ProtoBuf.ProtoMember(1, Name=@"imMsg", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<HSImMsg> imMsg
    {
      get { return _imMsg; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSImPlayerGet")]
  public partial class HSImPlayerGet : global::ProtoBuf.IExtensible
  {
    public HSImPlayerGet() {}
    
    private int _playerId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"playerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerId
    {
      get { return _playerId; }
      set { _playerId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSImPlayerGetRet")]
  public partial class HSImPlayerGetRet : global::ProtoBuf.IExtensible
  {
    public HSImPlayerGetRet() {}
    
    private HSImPlayer _imPlayer;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"imPlayer", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public HSImPlayer imPlayer
    {
      get { return _imPlayer; }
      set { _imPlayer = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}