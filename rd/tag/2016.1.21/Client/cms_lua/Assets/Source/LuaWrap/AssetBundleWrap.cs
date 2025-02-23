﻿using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class AssetBundleWrap
{
	public static LuaMethod[] regs = new LuaMethod[]
	{
		new LuaMethod("CreateFromMemory", CreateFromMemory),
		new LuaMethod("CreateFromMemoryImmediate", CreateFromMemoryImmediate),
		new LuaMethod("CreateFromFile", CreateFromFile),
		new LuaMethod("Contains", Contains),
		new LuaMethod("LoadAsset", LoadAsset),
		new LuaMethod("LoadAssetAsync", LoadAssetAsync),
		new LuaMethod("LoadAssetWithSubAssets", LoadAssetWithSubAssets),
		new LuaMethod("LoadAssetWithSubAssetsAsync", LoadAssetWithSubAssetsAsync),
		new LuaMethod("LoadAllAssets", LoadAllAssets),
		new LuaMethod("LoadAllAssetsAsync", LoadAllAssetsAsync),
		new LuaMethod("Unload", Unload),
		new LuaMethod("GetAllAssetNames", GetAllAssetNames),
		new LuaMethod("GetAllScenePaths", GetAllScenePaths),
		new LuaMethod("New", _CreateAssetBundle),
		new LuaMethod("GetClassType", GetClassType),
		new LuaMethod("__eq", Lua_Eq),
	};

	static LuaField[] fields = new LuaField[]
	{
		new LuaField("mainAsset", get_mainAsset, null),
	};

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateAssetBundle(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 0)
		{
			AssetBundle obj = new AssetBundle();
			LuaScriptMgr.Push(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: AssetBundle.New");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, typeof(AssetBundle));
		return 1;
	}

	public static void Register(IntPtr L)
	{
		LuaScriptMgr.RegisterLib(L, "UnityEngine.AssetBundle", typeof(AssetBundle), regs, fields, typeof(UnityEngine.Object));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_mainAsset(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		AssetBundle obj = (AssetBundle)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name mainAsset");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index mainAsset on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.mainAsset);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateFromMemory(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		byte[] objs0 = LuaScriptMgr.GetArrayNumber<byte>(L, 1);
		AssetBundleCreateRequest o = AssetBundle.CreateFromMemory(objs0);
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateFromMemoryImmediate(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		byte[] objs0 = LuaScriptMgr.GetArrayNumber<byte>(L, 1);
		AssetBundle o = AssetBundle.CreateFromMemoryImmediate(objs0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateFromFile(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 1);
		AssetBundle o = AssetBundle.CreateFromFile(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Contains(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		bool o = obj.Contains(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAsset(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		if (count == 2)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			Object o = obj.LoadAsset(arg0);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			Type arg1 = LuaScriptMgr.GetTypeObject(L, 3);
			Object o = obj.LoadAsset(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: AssetBundle.LoadAsset");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAssetAsync(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		if (count == 2)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			AssetBundleRequest o = obj.LoadAssetAsync(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 3)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			Type arg1 = LuaScriptMgr.GetTypeObject(L, 3);
			AssetBundleRequest o = obj.LoadAssetAsync(arg0,arg1);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: AssetBundle.LoadAssetAsync");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAssetWithSubAssets(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		if (count == 2)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			Object[] o = obj.LoadAssetWithSubAssets(arg0);
			LuaScriptMgr.PushArray(L, o);
			return 1;
		}
		else if (count == 3)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			Type arg1 = LuaScriptMgr.GetTypeObject(L, 3);
			Object[] o = obj.LoadAssetWithSubAssets(arg0,arg1);
			LuaScriptMgr.PushArray(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: AssetBundle.LoadAssetWithSubAssets");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAssetWithSubAssetsAsync(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		if (count == 2)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			AssetBundleRequest o = obj.LoadAssetWithSubAssetsAsync(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 3)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			Type arg1 = LuaScriptMgr.GetTypeObject(L, 3);
			AssetBundleRequest o = obj.LoadAssetWithSubAssetsAsync(arg0,arg1);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: AssetBundle.LoadAssetWithSubAssetsAsync");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAllAssets(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		if (count == 1)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			Object[] o = obj.LoadAllAssets();
			LuaScriptMgr.PushArray(L, o);
			return 1;
		}
		else if (count == 2)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			Type arg0 = LuaScriptMgr.GetTypeObject(L, 2);
			Object[] o = obj.LoadAllAssets(arg0);
			LuaScriptMgr.PushArray(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: AssetBundle.LoadAllAssets");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAllAssetsAsync(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		if (count == 1)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			AssetBundleRequest o = obj.LoadAllAssetsAsync();
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 2)
		{
			AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
			Type arg0 = LuaScriptMgr.GetTypeObject(L, 2);
			AssetBundleRequest o = obj.LoadAllAssetsAsync(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: AssetBundle.LoadAllAssetsAsync");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Unload(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		obj.Unload(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetAllAssetNames(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
		string[] o = obj.GetAllAssetNames();
		LuaScriptMgr.PushArray(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetAllScenePaths(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		AssetBundle obj = LuaScriptMgr.GetUnityObject<AssetBundle>(L, 1);
		string[] o = obj.GetAllScenePaths();
		LuaScriptMgr.PushArray(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Lua_Eq(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		Object arg0 = LuaScriptMgr.GetVarObject(L, 1) as Object;
		Object arg1 = LuaScriptMgr.GetVarObject(L, 2) as Object;
		bool o = arg0 == arg1;
		LuaScriptMgr.Push(L, o);
		return 1;
	}
}

