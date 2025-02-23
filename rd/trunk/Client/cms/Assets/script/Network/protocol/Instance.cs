//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: Protocol/Instance.proto
// Note: requires additional types generated from: Protocol/Const.proto
// Note: requires additional types generated from: Protocol/Reward.proto
// Note: requires additional types generated from: Protocol/Consume.proto
// Note: requires additional types generated from: Protocol/Monster.proto
namespace PB
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSAssist")]
  public partial class HSAssist : global::ProtoBuf.IExtensible
  {
    public HSAssist() {}
    
    private int _playerId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"playerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerId
    {
      get { return _playerId; }
      set { _playerId = value; }
    }
    private int _playerLevel;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"playerLevel", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerLevel
    {
      get { return _playerLevel; }
      set { _playerLevel = value; }
    }
    private int _playerPower;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"playerPower", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerPower
    {
      get { return _playerPower; }
      set { _playerPower = value; }
    }
    private string _playerName;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"playerName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string playerName
    {
      get { return _playerName; }
      set { _playerName = value; }
    }
    private bool _isFriend;
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"isFriend", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public bool isFriend
    {
      get { return _isFriend; }
      set { _isFriend = value; }
    }
    private HSMonster _monster;
    [global::ProtoBuf.ProtoMember(6, IsRequired = true, Name=@"monster", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public HSMonster monster
    {
      get { return _monster; }
      set { _monster = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSBattle")]
  public partial class HSBattle : global::ProtoBuf.IExtensible
  {
    public HSBattle() {}
    
    private string _battleCfgId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"battleCfgId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string battleCfgId
    {
      get { return _battleCfgId; }
      set { _battleCfgId = value; }
    }
    private readonly global::System.Collections.Generic.List<string> _monsterCfgId = new global::System.Collections.Generic.List<string>();
    [global::ProtoBuf.ProtoMember(2, Name=@"monsterCfgId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<string> monsterCfgId
    {
      get { return _monsterCfgId; }
    }
  
    private readonly global::System.Collections.Generic.List<HSRewardInfo> _monsterDrop = new global::System.Collections.Generic.List<HSRewardInfo>();
    [global::ProtoBuf.ProtoMember(3, Name=@"monsterDrop", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<HSRewardInfo> monsterDrop
    {
      get { return _monsterDrop; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceEnter")]
  public partial class HSInstanceEnter : global::ProtoBuf.IExtensible
  {
    public HSInstanceEnter() {}
    
    private string _instanceId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"instanceId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string instanceId
    {
      get { return _instanceId; }
      set { _instanceId = value; }
    }
    private readonly global::System.Collections.Generic.List<int> _battleMonsterId = new global::System.Collections.Generic.List<int>();
    [global::ProtoBuf.ProtoMember(2, Name=@"battleMonsterId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public global::System.Collections.Generic.List<int> battleMonsterId
    {
      get { return _battleMonsterId; }
    }
  
    private int _friendId = default(int);
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"friendId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int friendId
    {
      get { return _friendId; }
      set { _friendId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceEnterRet")]
  public partial class HSInstanceEnterRet : global::ProtoBuf.IExtensible
  {
    public HSInstanceEnterRet() {}
    
    private string _instanceId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"instanceId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string instanceId
    {
      get { return _instanceId; }
      set { _instanceId = value; }
    }
    private readonly global::System.Collections.Generic.List<HSBattle> _battle = new global::System.Collections.Generic.List<HSBattle>();
    [global::ProtoBuf.ProtoMember(2, Name=@"battle", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<HSBattle> battle
    {
      get { return _battle; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceSettle")]
  public partial class HSInstanceSettle : global::ProtoBuf.IExtensible
  {
    public HSInstanceSettle() {}
    
    private int _passBattleCount;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"passBattleCount", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int passBattleCount
    {
      get { return _passBattleCount; }
      set { _passBattleCount = value; }
    }
    private int _deadMonsterCount;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"deadMonsterCount", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int deadMonsterCount
    {
      get { return _deadMonsterCount; }
      set { _deadMonsterCount = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceSettleRet")]
  public partial class HSInstanceSettleRet : global::ProtoBuf.IExtensible
  {
    public HSInstanceSettleRet() {}
    
    private int _starCount = (int)0;
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"starCount", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((int)0)]
    public int starCount
    {
      get { return _starCount; }
      set { _starCount = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceSweep")]
  public partial class HSInstanceSweep : global::ProtoBuf.IExtensible
  {
    public HSInstanceSweep() {}
    
    private string _instanceId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"instanceId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string instanceId
    {
      get { return _instanceId; }
      set { _instanceId = value; }
    }
    private int _count;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"count", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int count
    {
      get { return _count; }
      set { _count = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceSweepRet")]
  public partial class HSInstanceSweepRet : global::ProtoBuf.IExtensible
  {
    public HSInstanceSweepRet() {}
    
    private readonly global::System.Collections.Generic.List<HSRewardInfo> _completeReward = new global::System.Collections.Generic.List<HSRewardInfo>();
    [global::ProtoBuf.ProtoMember(1, Name=@"completeReward", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<HSRewardInfo> completeReward
    {
      get { return _completeReward; }
    }
  
    private HSRewardInfo _sweepReward = null;
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"sweepReward", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public HSRewardInfo sweepReward
    {
      get { return _sweepReward; }
      set { _sweepReward = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceResetCount")]
  public partial class HSInstanceResetCount : global::ProtoBuf.IExtensible
  {
    public HSInstanceResetCount() {}
    
    private string _instanceId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"instanceId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string instanceId
    {
      get { return _instanceId; }
      set { _instanceId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceResetCountRet")]
  public partial class HSInstanceResetCountRet : global::ProtoBuf.IExtensible
  {
    public HSInstanceResetCountRet() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceRevive")]
  public partial class HSInstanceRevive : global::ProtoBuf.IExtensible
  {
    public HSInstanceRevive() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceReviveRet")]
  public partial class HSInstanceReviveRet : global::ProtoBuf.IExtensible
  {
    public HSInstanceReviveRet() {}
    
    private int _reviveCount;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"reviveCount", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int reviveCount
    {
      get { return _reviveCount; }
      set { _reviveCount = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSChapterBox")]
  public partial class HSChapterBox : global::ProtoBuf.IExtensible
  {
    public HSChapterBox() {}
    
    private int _chapterId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"chapterId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int chapterId
    {
      get { return _chapterId; }
      set { _chapterId = value; }
    }
    private int _difficulty;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"difficulty", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int difficulty
    {
      get { return _difficulty; }
      set { _difficulty = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSChapterBoxRet")]
  public partial class HSChapterBoxRet : global::ProtoBuf.IExtensible
  {
    public HSChapterBoxRet() {}
    
    private int _chapterId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"chapterId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int chapterId
    {
      get { return _chapterId; }
      set { _chapterId = value; }
    }
    private int _difficulty;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"difficulty", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int difficulty
    {
      get { return _difficulty; }
      set { _difficulty = value; }
    }
    private int _boxState;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"boxState", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int boxState
    {
      get { return _boxState; }
      set { _boxState = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSHoleEnter")]
  public partial class HSHoleEnter : global::ProtoBuf.IExtensible
  {
    public HSHoleEnter() {}
    
    private int _holeId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"holeId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int holeId
    {
      get { return _holeId; }
      set { _holeId = value; }
    }
    private string _instanceId;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"instanceId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string instanceId
    {
      get { return _instanceId; }
      set { _instanceId = value; }
    }
    private readonly global::System.Collections.Generic.List<int> _battleMonsterId = new global::System.Collections.Generic.List<int>();
    [global::ProtoBuf.ProtoMember(3, Name=@"battleMonsterId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public global::System.Collections.Generic.List<int> battleMonsterId
    {
      get { return _battleMonsterId; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSTowerEnter")]
  public partial class HSTowerEnter : global::ProtoBuf.IExtensible
  {
    public HSTowerEnter() {}
    
    private int _towerId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"towerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int towerId
    {
      get { return _towerId; }
      set { _towerId = value; }
    }
    private int _floor;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"floor", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int floor
    {
      get { return _floor; }
      set { _floor = value; }
    }
    private readonly global::System.Collections.Generic.List<int> _battleMonsterId = new global::System.Collections.Generic.List<int>();
    [global::ProtoBuf.ProtoMember(3, Name=@"battleMonsterId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public global::System.Collections.Generic.List<int> battleMonsterId
    {
      get { return _battleMonsterId; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSGuildInstanceEnter")]
  public partial class HSGuildInstanceEnter : global::ProtoBuf.IExtensible
  {
    public HSGuildInstanceEnter() {}
    
    private string _instanceId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"instanceId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string instanceId
    {
      get { return _instanceId; }
      set { _instanceId = value; }
    }
    private readonly global::System.Collections.Generic.List<int> _battleMonsterId = new global::System.Collections.Generic.List<int>();
    [global::ProtoBuf.ProtoMember(2, Name=@"battleMonsterId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public global::System.Collections.Generic.List<int> battleMonsterId
    {
      get { return _battleMonsterId; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceAssist")]
  public partial class HSInstanceAssist : global::ProtoBuf.IExtensible
  {
    public HSInstanceAssist() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceAssistRet")]
  public partial class HSInstanceAssistRet : global::ProtoBuf.IExtensible
  {
    public HSInstanceAssistRet() {}
    
    private readonly global::System.Collections.Generic.List<HSAssist> _assist = new global::System.Collections.Generic.List<HSAssist>();
    [global::ProtoBuf.ProtoMember(1, Name=@"assist", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<HSAssist> assist
    {
      get { return _assist; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceOpenCard")]
  public partial class HSInstanceOpenCard : global::ProtoBuf.IExtensible
  {
    public HSInstanceOpenCard() {}
    
    private int _openCount;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"openCount", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int openCount
    {
      get { return _openCount; }
      set { _openCount = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"HSInstanceOpenCardRet")]
  public partial class HSInstanceOpenCardRet : global::ProtoBuf.IExtensible
  {
    public HSInstanceOpenCardRet() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}