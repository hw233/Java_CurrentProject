--输出日志--
function log(str)
    Util.Log(str);
end

--打印字符串--
function print(str) 
	Util.Log(str);
end

--错误日志--
function error(str) 
	Util.LogError(str);
end

--警告日志--
function warn(str) 
	Util.LogWarning(str);
end

--查找对象--
function find(str)
	return GameObject.Find(str);
end

function destroy(obj)
	GameObject.Destroy(obj);
end

function newobject(prefab)
	return GameObject.Instantiate(prefab);
end

--创建面板--
function createPanel(name)
	ioo.panelManager:CreatePanel(name);
end

function child(str)
	return transform:FindChild(str);
end

function subGet(childNode, typeName)		
	return child(childNode):GetComponent(typeName);
end

function findPanel(str) 
	local obj = find(str);
	if obj == nil then
		error(str.." is null");
		return nil;
	end
	return obj:GetComponent("BaseLua");
end

function toInt(number)
    return math.floor(tonumber(number) or error("Could not cast '" .. tostring(number) .. "' to number.'"))
end

function clone(tSrc)
	local tDest
	for key,value in tSrc do 
		if type(value)=='table' and value["spuer"]==nil then 
			tDest[key] = {} 
			deepcopy(tDest[key],value) 
		else 
			tDest[key]=value
		end 
	end 
	return tDest
end