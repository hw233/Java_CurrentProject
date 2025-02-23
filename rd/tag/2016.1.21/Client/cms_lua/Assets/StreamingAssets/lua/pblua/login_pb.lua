-- Generated By protoc-gen-lua Do not Edit
local protobuf = require "protobuf"
local Role_pb = require("Role_pb")
module('Login_pb')


HSLOGIN = protobuf.Descriptor();
local HSLOGIN_PUID_FIELD = protobuf.FieldDescriptor();
local HSLOGIN_DEVICEID_FIELD = protobuf.FieldDescriptor();
local HSLOGIN_PLATFORM_FIELD = protobuf.FieldDescriptor();
local HSLOGIN_VERSION_FIELD = protobuf.FieldDescriptor();
local HSLOGIN_TOKEN_FIELD = protobuf.FieldDescriptor();
HSLOGINRET = protobuf.Descriptor();
local HSLOGINRET_STATUS_FIELD = protobuf.FieldDescriptor();
local HSLOGINRET_PLAYERID_FIELD = protobuf.FieldDescriptor();
local HSLOGINRET_TIMESTAMP_FIELD = protobuf.FieldDescriptor();
local HSLOGINRET_ROLELIST_FIELD = protobuf.FieldDescriptor();
HSUSERKICKOUT = protobuf.Descriptor();
local HSUSERKICKOUT_PLAYERID_FIELD = protobuf.FieldDescriptor();
local HSUSERKICKOUT_REASON_FIELD = protobuf.FieldDescriptor();

HSLOGIN_PUID_FIELD.name = "puid"
HSLOGIN_PUID_FIELD.full_name = ".HSLogin.puid"
HSLOGIN_PUID_FIELD.number = 1
HSLOGIN_PUID_FIELD.index = 0
HSLOGIN_PUID_FIELD.label = 2
HSLOGIN_PUID_FIELD.has_default_value = false
HSLOGIN_PUID_FIELD.default_value = ""
HSLOGIN_PUID_FIELD.type = 9
HSLOGIN_PUID_FIELD.cpp_type = 9

HSLOGIN_DEVICEID_FIELD.name = "deviceId"
HSLOGIN_DEVICEID_FIELD.full_name = ".HSLogin.deviceId"
HSLOGIN_DEVICEID_FIELD.number = 2
HSLOGIN_DEVICEID_FIELD.index = 1
HSLOGIN_DEVICEID_FIELD.label = 1
HSLOGIN_DEVICEID_FIELD.has_default_value = true
HSLOGIN_DEVICEID_FIELD.default_value = "0"
HSLOGIN_DEVICEID_FIELD.type = 9
HSLOGIN_DEVICEID_FIELD.cpp_type = 9

HSLOGIN_PLATFORM_FIELD.name = "platform"
HSLOGIN_PLATFORM_FIELD.full_name = ".HSLogin.platform"
HSLOGIN_PLATFORM_FIELD.number = 3
HSLOGIN_PLATFORM_FIELD.index = 2
HSLOGIN_PLATFORM_FIELD.label = 1
HSLOGIN_PLATFORM_FIELD.has_default_value = true
HSLOGIN_PLATFORM_FIELD.default_value = "0"
HSLOGIN_PLATFORM_FIELD.type = 9
HSLOGIN_PLATFORM_FIELD.cpp_type = 9

HSLOGIN_VERSION_FIELD.name = "version"
HSLOGIN_VERSION_FIELD.full_name = ".HSLogin.version"
HSLOGIN_VERSION_FIELD.number = 4
HSLOGIN_VERSION_FIELD.index = 3
HSLOGIN_VERSION_FIELD.label = 1
HSLOGIN_VERSION_FIELD.has_default_value = true
HSLOGIN_VERSION_FIELD.default_value = "0.0.1"
HSLOGIN_VERSION_FIELD.type = 9
HSLOGIN_VERSION_FIELD.cpp_type = 9

HSLOGIN_TOKEN_FIELD.name = "token"
HSLOGIN_TOKEN_FIELD.full_name = ".HSLogin.token"
HSLOGIN_TOKEN_FIELD.number = 5
HSLOGIN_TOKEN_FIELD.index = 4
HSLOGIN_TOKEN_FIELD.label = 1
HSLOGIN_TOKEN_FIELD.has_default_value = false
HSLOGIN_TOKEN_FIELD.default_value = ""
HSLOGIN_TOKEN_FIELD.type = 9
HSLOGIN_TOKEN_FIELD.cpp_type = 9

HSLOGIN.name = "HSLogin"
HSLOGIN.full_name = ".HSLogin"
HSLOGIN.nested_types = {}
HSLOGIN.enum_types = {}
HSLOGIN.fields = {HSLOGIN_PUID_FIELD, HSLOGIN_DEVICEID_FIELD, HSLOGIN_PLATFORM_FIELD, HSLOGIN_VERSION_FIELD, HSLOGIN_TOKEN_FIELD}
HSLOGIN.is_extendable = false
HSLOGIN.extensions = {}
HSLOGINRET_STATUS_FIELD.name = "status"
HSLOGINRET_STATUS_FIELD.full_name = ".HSLoginRet.status"
HSLOGINRET_STATUS_FIELD.number = 1
HSLOGINRET_STATUS_FIELD.index = 0
HSLOGINRET_STATUS_FIELD.label = 2
HSLOGINRET_STATUS_FIELD.has_default_value = false
HSLOGINRET_STATUS_FIELD.default_value = 0
HSLOGINRET_STATUS_FIELD.type = 5
HSLOGINRET_STATUS_FIELD.cpp_type = 1

HSLOGINRET_PLAYERID_FIELD.name = "playerId"
HSLOGINRET_PLAYERID_FIELD.full_name = ".HSLoginRet.playerId"
HSLOGINRET_PLAYERID_FIELD.number = 2
HSLOGINRET_PLAYERID_FIELD.index = 1
HSLOGINRET_PLAYERID_FIELD.label = 1
HSLOGINRET_PLAYERID_FIELD.has_default_value = false
HSLOGINRET_PLAYERID_FIELD.default_value = 0
HSLOGINRET_PLAYERID_FIELD.type = 5
HSLOGINRET_PLAYERID_FIELD.cpp_type = 1

HSLOGINRET_TIMESTAMP_FIELD.name = "timeStamp"
HSLOGINRET_TIMESTAMP_FIELD.full_name = ".HSLoginRet.timeStamp"
HSLOGINRET_TIMESTAMP_FIELD.number = 3
HSLOGINRET_TIMESTAMP_FIELD.index = 2
HSLOGINRET_TIMESTAMP_FIELD.label = 1
HSLOGINRET_TIMESTAMP_FIELD.has_default_value = false
HSLOGINRET_TIMESTAMP_FIELD.default_value = 0
HSLOGINRET_TIMESTAMP_FIELD.type = 5
HSLOGINRET_TIMESTAMP_FIELD.cpp_type = 1

HSLOGINRET_ROLELIST_FIELD.name = "roleList"
HSLOGINRET_ROLELIST_FIELD.full_name = ".HSLoginRet.roleList"
HSLOGINRET_ROLELIST_FIELD.number = 4
HSLOGINRET_ROLELIST_FIELD.index = 3
HSLOGINRET_ROLELIST_FIELD.label = 3
HSLOGINRET_ROLELIST_FIELD.has_default_value = false
HSLOGINRET_ROLELIST_FIELD.default_value = {}
HSLOGINRET_ROLELIST_FIELD.message_type = Role_pb.HSROLEBRIEF
HSLOGINRET_ROLELIST_FIELD.type = 11
HSLOGINRET_ROLELIST_FIELD.cpp_type = 10

HSLOGINRET.name = "HSLoginRet"
HSLOGINRET.full_name = ".HSLoginRet"
HSLOGINRET.nested_types = {}
HSLOGINRET.enum_types = {}
HSLOGINRET.fields = {HSLOGINRET_STATUS_FIELD, HSLOGINRET_PLAYERID_FIELD, HSLOGINRET_TIMESTAMP_FIELD, HSLOGINRET_ROLELIST_FIELD}
HSLOGINRET.is_extendable = false
HSLOGINRET.extensions = {}
HSUSERKICKOUT_PLAYERID_FIELD.name = "playerId"
HSUSERKICKOUT_PLAYERID_FIELD.full_name = ".HSUserKickout.playerId"
HSUSERKICKOUT_PLAYERID_FIELD.number = 1
HSUSERKICKOUT_PLAYERID_FIELD.index = 0
HSUSERKICKOUT_PLAYERID_FIELD.label = 2
HSUSERKICKOUT_PLAYERID_FIELD.has_default_value = false
HSUSERKICKOUT_PLAYERID_FIELD.default_value = 0
HSUSERKICKOUT_PLAYERID_FIELD.type = 5
HSUSERKICKOUT_PLAYERID_FIELD.cpp_type = 1

HSUSERKICKOUT_REASON_FIELD.name = "reason"
HSUSERKICKOUT_REASON_FIELD.full_name = ".HSUserKickout.reason"
HSUSERKICKOUT_REASON_FIELD.number = 2
HSUSERKICKOUT_REASON_FIELD.index = 1
HSUSERKICKOUT_REASON_FIELD.label = 1
HSUSERKICKOUT_REASON_FIELD.has_default_value = false
HSUSERKICKOUT_REASON_FIELD.default_value = 0
HSUSERKICKOUT_REASON_FIELD.type = 5
HSUSERKICKOUT_REASON_FIELD.cpp_type = 1

HSUSERKICKOUT.name = "HSUserKickout"
HSUSERKICKOUT.full_name = ".HSUserKickout"
HSUSERKICKOUT.nested_types = {}
HSUSERKICKOUT.enum_types = {}
HSUSERKICKOUT.fields = {HSUSERKICKOUT_PLAYERID_FIELD, HSUSERKICKOUT_REASON_FIELD}
HSUSERKICKOUT.is_extendable = false
HSUSERKICKOUT.extensions = {}

HSLogin = protobuf.Message(HSLOGIN)
HSLoginRet = protobuf.Message(HSLOGINRET)
HSUserKickout = protobuf.Message(HSUSERKICKOUT)

