package org.hawk.app;

import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.hawk.delay.HawkDelayAction;
import org.hawk.delay.HawkDelayManager;
import org.hawk.listener.HawkListener;
import org.hawk.log.HawkLog;
import org.hawk.msg.HawkMsg;
import org.hawk.net.HawkSession;
import org.hawk.net.protocol.HawkProtocol;
import org.hawk.os.HawkException;
import org.hawk.xid.HawkXID;

/**
 * 应用程序对象
 * 
 * @author hawk
 */
public class HawkAppObj extends HawkListener {
	/**
	 * 应用对象唯一标识
	 */
	protected HawkXID objXid;
	/**
	 * 会话对象
	 */
	protected HawkSession session;
	/**
	 * 应用对象支持的模块对象
	 */
	protected Map<Integer, HawkObjModule> objModules;
	/**
	 * 对象延迟队列
	 */
	protected HawkDelayManager delayManager;
	
	/**
	 * 应用对象构造
	 * 
	 * @param xid
	 */
	public HawkAppObj(HawkXID xid) {
		setXid(xid);
		objModules = new LinkedHashMap<Integer, HawkObjModule>();
		delayManager = new HawkDelayManager();
	}

	/**
	 * 获取对象Id
	 * 
	 * @return
	 */
	public HawkXID getXid() {
		return objXid;
	}

	/**
	 * 设置对象Id
	 * 
	 * @param xid
	 */
	public void setXid(HawkXID xid) {
		this.objXid = xid;
	}

	/**
	 * 获取对象所对应的会话
	 * 
	 * @return
	 */
	public HawkSession getSession() {
		return session;
	}

	/**
	 * 设置对象所对应的会话
	 * 
	 * @param session
	 */
	public void setSession(HawkSession session) {
		this.session = session;
	}

	/**
	 * 判断对象是否在线
	 * @return
	 */
	public boolean isOnline() {
		return this.session != null && this.session.isActive();
	}
	
	/**
	 * 发送协议
	 * 
	 * @param protocol
	 * @return
	 */
	public boolean sendProtocol(HawkProtocol protocol) {
		if (protocol != null && session != null && session.isActive()) {
			// 将协议内容格式化成json
			if (HawkApp.getInstance().getAppCfg().isDebug()) {
			//	Message protoBuilder = protocol.getBuilder().build();
			//	String protoJson = JsonFormat.printToString(protoBuilder);
				HawkLog.logPrintln(String.format("send protocol: %d, size: %d, target: %s",
						protocol.getType(), protocol.getSize(), getXid().toString()));
			}
			return session.sendProtocol(protocol);
		}
		return false;
	}

	/**
	 * 获取指定Id的功能模块
	 * 
	 * @param moduleId
	 * @return
	 */
	public HawkObjModule getModule(int moduleId) {
		return objModules.get(moduleId);
	}

	/**
	 * 获取对象所有模块表
	 * 
	 * @return
	 */
	public Map<Integer, HawkObjModule> getObjModules() {
		return objModules;
	}
	
	/**
	 * 注册功能模块
	 * 
	 * @param moduleId
	 * @param objModule
	 */
	public void registerModule(int moduleId, HawkObjModule objModule) {
		objModules.put(moduleId, objModule);
	}

	/**
	 * 移除功能模块
	 * 
	 * @param moduleId
	 */
	public boolean removeModule(int moduleId) {
		if (objModules.containsKey(moduleId)) {
			objModules.remove(moduleId);
			return true;
		}
		return false;
	}
	
	/**
	 * 添加延时行为对象
	 * 
	 * @param delayTime
	 * @param action
	 */
	public void addDelayAction(long delayTime, HawkDelayAction action) {
		if (delayManager != null) {
			delayManager.addDelayAction(delayTime, action);
		}
	}
	
	/**
	 * 更新, 子类在处理自身逻辑后需要调用父类接口
	 * 
	 * @return
	 */
	public boolean onTick(long tickTime) {
		// 更新延时对象
		if (delayManager != null) {
			delayManager.updateAction();
		}
		
		for (Entry<Integer, HawkObjModule> entry : objModules.entrySet()) {
			try {
				entry.getValue().onTick(tickTime);
			} catch (Exception e) {
				HawkException.catchException(e);
			}
		}
		return true;
	}

	/**
	 * 刷新数据
	 */
	protected boolean onRefresh(long refreshTime) {
		return true;
	}

	/**
	 * 消息响应, 子类在处理自身逻辑后需要调用父类接口
	 * 
	 * @param msg
	 * @return true: 表示消息被处理; false: 消息未被处理, 错误显示
	 */
	public boolean onMessage(HawkMsg msg) {		
		boolean processed = super.invokeMessage(this, msg);
		for (Entry<Integer, HawkObjModule> entry : objModules.entrySet()) {
			try {
				if (entry.getValue().isListenMsg(msg.getMsg())) {
					entry.getValue().onMessage(msg);
					processed |= true;
				}
			} catch (Exception e) {
				HawkException.catchException(e);
			}
		}
		return processed;
	}

	/**
	 * 协议响应, 子类在处理自身逻辑后需要调用父类接口
	 * 
	 * @param protocol
	 * @return true: 表示协议被处理; false: 协议未被处理, 未监听错误
	 */
	public boolean onProtocol(HawkProtocol protocol) {
		boolean processed = super.invokeProtocol(this, protocol);
		for (Entry<Integer, HawkObjModule> entry : objModules.entrySet()) {
			try {
				if (entry.getValue().isListenProto(protocol.getType())) {
					entry.getValue().onProtocol(protocol);
					processed |= true;
				}
			} catch (Exception e) {
				HawkException.catchException(e);
			}
		}
		return processed;
	}
}
