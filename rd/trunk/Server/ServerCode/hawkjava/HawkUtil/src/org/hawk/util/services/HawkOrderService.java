package org.hawk.util.services;

import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;

import net.sf.json.JSONObject;

import org.hawk.app.HawkApp;
import org.hawk.log.HawkLog;
import org.hawk.os.HawkException;
import org.hawk.os.HawkTime;
import org.hawk.util.HawkTickable;
import org.hawk.util.services.helper.HawkOrderClient;

import com.google.gson.JsonObject;

public class HawkOrderService extends HawkTickable {
	/**
	 * 请求类型
	 */
	public static int ACTION_ORDER_HEART_BEAT = 0;
	public static int ACTION_ORDER_DELIVER_REQUEST = 1;
	public static int ACTION_ORDER_DELIVER_RESPONSE = 2;
	/**
	 * 默认心跳时间周期
	 */
	public final static int HEART_PERIOD = 15000;
	
	/*
	 * 错误类型
	 */
	public static int ORDER_STATUS_OK = 0;
	public static int ORDER_PLAYER_NOT_EXIST = -1;
	public static int ORDER_PRODUCT_NOT_EXIST = -2;
	
	public static int ORDER_STATUS_ERROR = -10;
	/**
	 * 实例id
	 */
	private String suuid = "";
	/**
	 * 游戏名
	 */
	private String game = "";
	/**
	 * 平台名
	 */
	private String platform = "";
	/**
	 * 渠道名
	 */
	private String channel = "";
	/**
	 * 服务器id
	 */
	private int serverId = 0;
	/**
	 * 上次tick周期时间
	 */
	private long lastTickTime = 0;
	/**
	 * 订单客户端对象
	 */
	HawkOrderClient orderClient;
	/**
	 * 信息发送队列
	 */
	BlockingQueue<String> sendQueue;
	/**
	 * 信息接收队列
	 */
	BlockingQueue<String> recvQueue;

	/**
	 * 实例对象
	 */
	static HawkOrderService instance = null;

	/**
	 * 获取全局实例对象
	 * 
	 * @return
	 */
	public static HawkOrderService getInstance() {
		if (instance == null) {
			instance = new HawkOrderService();
		}
		return instance;
	}

	/**
	 * 构造函数
	 */
	private HawkOrderService() {
		sendQueue = new LinkedBlockingQueue<String>();
		recvQueue = new LinkedBlockingQueue<String>();
	}

	public String getSuuid() {
		return suuid;
	}

	/**
	 * 初始化订单服务
	 * 
	 * @param addr
	 * @return
	 */
	public boolean init(String suuid, String addr, String game, String platform, String channel, int serverId) {
		this.game = game;
		this.platform = platform;
		this.serverId = serverId;
		this.channel = channel;
		this.suuid = suuid;
		if (this.suuid == null || this.suuid.length() <= 0) {
			this.suuid = String.format("%s.%s.%s.%d", game, platform, channel, serverId);
		}

		this.suuid.toLowerCase();
		
		// 初始化网络服务对象
		orderClient = new HawkOrderClient();
		if (!orderClient.init(this.suuid, addr)) {
			HawkLog.logPrintln("order client init failed, addr: " + addr);
			return false;
		}
		
		lastTickTime = HawkTime.getMillisecond();
		HawkLog.logPrintln("order client init success, addr: " + addr);
		HawkApp.getInstance().addTickable(this);
		return true;
	}

	/**
	 * 是否连接成功
	 * 
	 * @return
	 */
	public boolean isConnectOK() {
		return orderClient.isConnectOK();
	}

	/**
	 * 心跳检测
	 */
	private void heartbeatTick() {
		long curTime = HawkTime.getMillisecond();
		if (curTime - lastTickTime >= HEART_PERIOD) {
			lastTickTime = curTime;
			JsonObject jsonObject = new JsonObject();
			jsonObject.addProperty("suuid", this.suuid);
			jsonObject.addProperty("game", this.game);
			jsonObject.addProperty("platform", this.platform);
			jsonObject.addProperty("channel", this.channel);
			jsonObject.addProperty("serverId", this.serverId);
			jsonObject.addProperty("ip", HawkApp.getInstance().getMyHostIp());
			jsonObject.addProperty("action", ACTION_ORDER_HEART_BEAT);
			sendQueue.add(jsonObject.toString());
		}
	}
	
	/**
	 * 订单服务器通知
	 * 
	 * @param fromObject
	 */
	private void onNotify(String data) {
		try {
			// 订单服务器通知(订单生成, 订单发货)
			JSONObject jsonInfo = JSONObject.fromObject(data);
			HawkApp.getInstance().onOrderNotify(jsonInfo);
		} catch (Exception e) {
			HawkException.catchException(e);
		}
	}
	
	/**
	 * 响应充值发货请求
	 * 
	 * @param order
	 *            (订单号, 自生产的订单号)
	 * @param status
	 *            (0: 成功, 负数: 错误码)
	 * @param addGold
	 *            (充值钻石, 成功时才有)
	 * @param giftGold
	 *            (赠送钻石, 成功时才有)
	 * @return
	 */
	public boolean responseDeliver(String order, int status, int addGold, int giftGold) {
		try {
			JsonObject jsonObject = new JsonObject();
			// 发送生成订单请求
			jsonObject.addProperty("action", ACTION_ORDER_DELIVER_RESPONSE);
			jsonObject.addProperty("order", order);
			jsonObject.addProperty("addGold", addGold);
			jsonObject.addProperty("giftGold", giftGold);
			jsonObject.addProperty("status", status);
			sendQueue.add(jsonObject.toString());
			return true;
		} catch (Exception e) {
			HawkException.catchException(e);
		}
		return false;
	}

	/**
	 * 帧更新
	 */
	@Override
	public void onTick() {
		// 检测连接
		if (orderClient.checkConnection()) {
			// 添加心跳行为
			heartbeatTick();

			// 接收数据并通知app层处理
			recvQueue.clear();
			if (orderClient.updateRecvQueue(recvQueue)) {
				while (recvQueue.size() > 0) {
					// 订单服务器通知(订单生成, 订单发货)
					String info = recvQueue.poll();
					onNotify(info);
				}
			}

			// 写出发送队列所有数据
			orderClient.flushSendQueue(sendQueue);
		}
	}
}
