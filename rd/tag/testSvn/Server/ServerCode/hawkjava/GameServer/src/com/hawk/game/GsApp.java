package com.hawk.game;


import java.util.Arrays;
import java.util.Calendar;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.concurrent.ConcurrentHashMap;

import net.sf.json.JSONObject;

import org.hawk.app.HawkApp;
import org.hawk.app.HawkAppObj;
import org.hawk.config.HawkConfigManager;
import org.hawk.config.HawkConfigStorage;
import org.hawk.db.HawkDBManager;
import org.hawk.log.HawkLog;
import org.hawk.msg.HawkMsg;
import org.hawk.net.HawkSession;
import org.hawk.net.protocol.HawkProtocol;
import org.hawk.obj.HawkObjBase;
import org.hawk.obj.HawkObjManager;
import org.hawk.os.HawkException;
import org.hawk.os.HawkShutdownHook;
import org.hawk.os.HawkTime;
import org.hawk.service.HawkServiceManager;
import org.hawk.util.services.FunPlusPushService;
import org.hawk.util.services.FunPlusTranslateService;
import org.hawk.util.services.HawkAccountService;
import org.hawk.util.services.HawkEmailService;
import org.hawk.util.services.HawkOrderService;
import org.hawk.xid.HawkXID;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.hawk.game.callback.ShutdownCallback;
import com.hawk.game.config.GrayPuidCfg;
import com.hawk.game.config.HoleCfg;
import com.hawk.game.config.SysBasicCfg;
import com.hawk.game.entity.PlayerEntity;
import com.hawk.game.entity.RechargeEntity;
import com.hawk.game.manager.AllianceManager;
import com.hawk.game.manager.ImManager;
import com.hawk.game.manager.ShopManager;
import com.hawk.game.manager.SnapShotManager;
import com.hawk.game.module.activity.http.ActivityHttpServer;
import com.hawk.game.player.Player;
import com.hawk.game.protocol.Const;
import com.hawk.game.protocol.HS;
import com.hawk.game.protocol.Login.HSLogin;
import com.hawk.game.protocol.Login.HSLoginRet;
import com.hawk.game.protocol.Login.HSReconnect;
import com.hawk.game.protocol.Login.HSReconnectRet;
import com.hawk.game.protocol.Player.HSPlayerCreate;
import com.hawk.game.protocol.Player.HSPlayerCreateRet;
import com.hawk.game.protocol.Status;
import com.hawk.game.protocol.SysProtocol.HSErrorCode;
import com.hawk.game.protocol.SysProtocol.HSHeartBeat;
import com.hawk.game.service.GmService_Dev;
import com.hawk.game.util.GsConst;
import com.hawk.game.util.ProtoUtil;
import com.hawk.game.util.TimeUtil;

/**
 * 游戏应用
 * 
 * @author hawk
 * 
 */
public class GsApp extends HawkApp {
	/**
	 * 日志记录器
	 */
	private static final Logger logger = LoggerFactory.getLogger("Server");

	/**
	 * puid登陆时间
	 */
	private Map<String, Long> puidLoginTime;

	/**
	 * 全局静态对象
	 */
	private static GsApp instance = null;

	/**
	 * 获取全局静态对象
	 * 
	 * @return
	 */
	public static GsApp getInstance() {
		return instance;
	}

	/**
	 * 构造函数
	 */
	public GsApp() {
		super(HawkXID.valueOf(GsConst.ObjType.MANAGER, GsConst.ObjId.APP));

		if (instance == null) {
			instance = this;
		}

		puidLoginTime = new ConcurrentHashMap<String, Long>();
	}

	/**
	 * 从配置文件初始化
	 * 
	 * @param cfg
	 * @return
	 */
	public boolean init(String cfg) {
		
		GsConfig appCfg = null;
		try {
			HawkConfigStorage cfgStorgae = new HawkConfigStorage(GsConfig.class, getWorkPath());
			appCfg = (GsConfig) cfgStorgae.getConfigList().get(0);
		} catch (Exception e) {
			HawkException.catchException(e);
			return false;
		}

		// 父类初始化
		if (!super.init(appCfg)) {
			return false;
		}
		// 初始化对象管理区
		if (!initAppObjMan()) {
			return false;
		}

		// 设置关服回调
		HawkShutdownHook.getInstance().setCallback(new ShutdownCallback());

		// 初始化全局实例对象
		HawkLog.logPrintln("init server data......");
		ServerData.getInstance().init();

		// 账号服务器初始化
		if (GsConfig.getInstance().getAccountHost().length() > 0) {
			HawkLog.logPrintln("install account service......");
			if (HawkAccountService.getInstance().install(
								GsConfig.getInstance().getGameId(),
								GsConfig.getInstance().getPlatform(),
								GsConfig.getInstance().getChannel(),
								GsConfig.getInstance().getServerId(),
								GsConfig.getInstance().getAccountTimeout(),
								GsConfig.getInstance().getAccountHost()))
			{
				HawkLog.logPrintln("register game server");
				try {
					// TODO
					String ip = getMyHostIp();


					HawkAccountService.getInstance().report(new HawkAccountService.RegitsterGameServer(ip, GsConfig.getInstance().getAcceptorPort()));
				}
				catch (Exception e) {
					HawkLog.logPrintln("get ip fail");
					return false;
				}
			}
		}

		//初始化订单服务器
		if (HawkOrderService.getInstance().init(
							null,
							GsConfig.getInstance().getOrderAddr(),
							GsConfig.getInstance().getGameId(),
							GsConfig.getInstance().getPlatform(),
							GsConfig.getInstance().getChannel(),
							GsConfig.getInstance().getServerId())) 
		{
			HawkLog.logPrintln("install order service success");
		}
		else {
			HawkLog.logPrintln("install order service fail");
			return false;
		}

		// 初始化活动服务器
		if (ActivityHttpServer.getInstance().setup(
							  GsConfig.getInstance().getActivityServerAddr(),
							  GsConfig.getInstance().getActivityServerport(), 
							  1)) 
		{
			HawkLog.logPrintln("install activity server success");
		}
		else {
			HawkLog.logPrintln("install activity server fail");
			return false;
		}
		
		// 初始化邮件服务
		if (GsConfig.getInstance().getEmailUser() != null && GsConfig.getInstance().getEmailUser().length() > 0) {
			HawkLog.logPrintln("install email service......");
			HawkEmailService.getInstance().install("smtp.163.com", 25,
					GsConfig.getInstance().getEmailUser(),
					GsConfig.getInstance().getEmailPwd());
		}

		// 初始化Funplus翻译服务
		if (true == GsConfig.getInstance().isTranslate()) {
			HawkLog.logPrintln("install translate service......");
			FunPlusTranslateService.getInstance().install(GsConst.FUNPLUS_APP_ID, GsConst.FUNPLUS_KEY);
		}

		// 初始化Funplus推送服务
		HawkLog.logPrintln("install push service......");
		FunPlusPushService.getInstance().install(GsConst.FUNPLUS_APP_ID, GsConst.FUNPLUS_KEY);

		// 公会初始化
		HawkLog.logPrintln("init alliance manager......");
		AllianceManager.getInstance().init();

		// 快照缓存对象初始
		HawkLog.logPrintln("init snapshot manager......");
		SnapShotManager.getInstance().init();

		// 初始化开发版GmService
		HawkServiceManager.getInstance().registerService("GM_Dev", new GmService_Dev());

		//开机刷新一次
		onRefresh(HawkTime.getMillisecond());

		return true;
	}

	/**
	 * 初始化应用对象管理器
	 * 
	 * @return
	 */
	private boolean initAppObjMan() {
		HawkObjManager<HawkXID, HawkAppObj> objMan = null;

		// 创建玩家管理区
		objMan = createObjMan(GsConst.ObjType.PLAYER);
		long timeout = SysBasicCfg.getInstance().getPlayerCacheTime();
		objMan.setObjTimeout(timeout);

		// 创建全局管理区, 并注册应用对象
		objMan = createObjMan(GsConst.ObjType.MANAGER);
		// 应用管理器
		objMan.allocObject(getXid(), this);
		// 创建玩家快照管理器
		createObj(HawkXID.valueOf(GsConst.ObjType.MANAGER, GsConst.ObjId.SNAPSHOT));
		// IM管理器
		createObj(HawkXID.valueOf(GsConst.ObjType.MANAGER, GsConst.ObjId.IM));
		// 商店管理器
		createObj(HawkXID.valueOf(GsConst.ObjType.MANAGER, GsConst.ObjId.SHOP));
		// 公会管理器
		createObj(HawkXID.valueOf(GsConst.ObjType.MANAGER, GsConst.ObjId.ALLIANCE));
		return true;
	}

	/**
	 * 查询Player
	 * @param playerId
	 * @return
	 */
	public Player queryPlayer(int playerId) {
		HawkObjBase<HawkXID, HawkAppObj> objBase =queryObject(HawkXID.valueOf(GsConst.ObjType.PLAYER, playerId));
		if(objBase != null) {
			return (Player)objBase.getImpl();
		}
		return null;
	}

	/**
	 * 获取hash线程索引
	 */
	@Override
	public int getHashThread(HawkXID xid, int threadNum) {
		//return super.getHashThread(xid, threadNum);
		if (threadNum <= 1) {
			return 0;
		}
		else if (xid.getType() == GsConst.ObjType.MANAGER) {
			return threadNum - 1;
		}
		else {
			return  xid.getId() % (threadNum - 1);
		}
	}

	/**
	 * 帧更新
	 */
	@Override
	public boolean onTick(long tickTime) {
		if (super.onTick(tickTime)) {
			// 显示服务器信息
			ServerData.getInstance().showServerInfo();
			return true;
		}
		return false;
	}

	/**
	 * 刷新数据
	 */
	@Override
	protected boolean onRefresh(long refreshTime) {
		// 第1步，刷新全局数据
		Calendar curTime = HawkTime.getCalendar(refreshTime);

		// 洞最大刷新时间
		class HoleRefreshMaxTime {
			long time = 0;
			boolean isOpen = false;
		}
		Map<Integer, HoleRefreshMaxTime> HoleRefreshMaxTimeMap = null;
		Map<Object, HoleCfg> holeCfgMap = null;
		boolean refreshHole = false;

		for (int index = 0; index < GsConst.SysRefreshTime.length; ++index) {
			int timeCfgId = GsConst.SysRefreshTime[index];

			Calendar lastRefreshTime = ServerData.getInstance().getLastRefreshTime(timeCfgId);
			Calendar expectedRefreshTime = TimeUtil.getExpectedRefreshTime(timeCfgId, curTime, lastRefreshTime);
			if (expectedRefreshTime != null) {
				// 刷新时间点
				ServerData.getInstance().setRefreshTime(timeCfgId, expectedRefreshTime);

				// 刷新数据
				if (0 != (GsConst.SysRefreshMask[index] & GsConst.RefreshMask.HOLE)) {
					if (false == refreshHole) {
						refreshHole = true;
						// 初始化洞刷新所需临时数据
						HoleRefreshMaxTimeMap = new HashMap<Integer, HoleRefreshMaxTime>();
						holeCfgMap = HawkConfigManager.getInstance().getConfigMap(HoleCfg.class);
						for (HoleCfg hole : holeCfgMap.values()) {
							HoleRefreshMaxTimeMap.put(hole.getId(), new HoleRefreshMaxTime());
						}
					}
					// 在所有洞刷新时间（都小于当前时间）中选择最接近当前时间的
					for (HoleCfg hole : holeCfgMap.values()) {
						Boolean isOpen = null;
						// 洞开启时间区间左闭右开，因此先判断关闭，再判断开启
						if (true == hole.isCloseTime(timeCfgId)) {
							isOpen = false;
						}
						if (true == hole.isOpenTime(timeCfgId)) {
							isOpen = true;
						}

						if (isOpen != null) {
							HoleRefreshMaxTime max = HoleRefreshMaxTimeMap.get(hole.getId());
							if (max.time < expectedRefreshTime.getTimeInMillis()) {
								max.time = expectedRefreshTime.getTimeInMillis();
								max.isOpen = isOpen;
							}
						}
					}
				}
			}
		}

		// 刷新洞
		if (true == refreshHole) {
			for (Entry<Integer, HoleRefreshMaxTime> entry : HoleRefreshMaxTimeMap.entrySet()) {
				ServerData.getInstance().setHoleOpen(entry.getKey(), entry.getValue().isOpen);
			}
		}

		// 第2步，调用父类刷新其它对象
		super.onRefresh(refreshTime);
		return true;
	}

	/**
	 * 清空玩家缓存
	 */
	@Override
	protected void onRemoveTimeoutObj(HawkAppObj appObj) {
		if (appObj != null && appObj instanceof Player) {
			Player player = (Player) appObj;
			if (player.isOnline()) {
				player.getSession().close(false);
			}

			if (player.getPlayerData() != null && player.getPlayerData().getPlayerEntity() != null) {
				 logger.info("remove player: {}, puid: {}, gold: {}, coin: {}, level: {}, vip: {}",
				 player.getId(), player.getPuid(), player.getGold(),
				 player.getCoin(), player.getLevel(), player.getVipLevel());
			}
		}
	}

	/**
	 * 打印状态
	 */
	@Override
	public void printState() {
		super.printState();
//		HawkLog.errPrintln(String.format("player 数量: %d ", objMans.get(GsConst.ObjType.PLAYER).getObjBaseMap().size()));
	}

	/**
	 * 内存不足警告
	 */
	@Override
	protected void onMemoryOutWarning() {
		super.onMemoryOutWarning();
		HawkObjManager<HawkXID, HawkAppObj> objMan = objMans.get(GsConst.ObjType.PLAYER);
		Iterator<Map.Entry<HawkXID, HawkObjBase<HawkXID, HawkAppObj>>> iterator = objMan.getObjBaseMap().entrySet().iterator();
		int removeCount = 0;
		while (iterator.hasNext()) {
			 HawkObjBase<HawkXID, HawkAppObj> objBase = iterator.next().getValue();
			 Player player = (Player)objBase.getImpl();
			 if (player.isOnline() == false && objBase.getVisitTime() + 600000 < currentTime) {
				iterator.remove();
				removeCount++;
			}
		}

		HawkLog.debugPrintln(String.format("移除对象个数 %d", removeCount));

		System.gc();
	}

	/**
	 * 创建应用对象
	 */
	@Override
	protected HawkAppObj onCreateObj(HawkXID xid) {
		HawkAppObj appObj = null;
		// 创建管理器
		if (xid.getType() == GsConst.ObjType.MANAGER) {
			if (xid.getId() == GsConst.ObjId.IM) {
				appObj = new ImManager(xid);
			}
			else if (xid.getId() == GsConst.ObjId.SHOP) {
				appObj = new ShopManager(xid);
			}
			else if (xid.getId() == GsConst.ObjId.ALLIANCE) {
				appObj = new AllianceManager(xid);
			}
			else if (xid.getId() == GsConst.ObjId.SNAPSHOT) {
				appObj = new SnapShotManager(xid);
			}
		}
		else if (xid.getType() == GsConst.ObjType.PLAYER) {
			appObj = new Player(xid);
		}

		if (appObj == null) {
			HawkLog.errPrintln("create obj failed: " + xid);
		}
		return appObj;
	}

	/**
	 * 分发消息
	 */
	@Override
	public boolean dispatchMsg(HawkXID xid, HawkMsg msg) {
		if (xid.equals(getXid())) {
			return onMessage(msg);
		}
		return super.dispatchMsg(xid, msg);
	}

	/**
	 * 报告异常
	 */
	@Override
	public void reportException(Exception e) {
		HawkEmailService emailService = HawkEmailService.getInstance();
		if (true == emailService.checkValid()) {
			String emailTitle = String.format("exception(%s_%s_%s)", GsConfig
					.getInstance().getGameId(), GsConfig.getInstance()
					.getPlatform(), GsConfig.getInstance().getServerId());
			emailService.sendEmail(emailTitle, HawkException.formatStackMsg(e),
					Arrays.asList("gamebugs@163.com"));
		}
	}

	/**
	 * 会话协议回调, 由IO线程直接调用, 非线程安全
	 */
	@Override
	public boolean onSessionProtocol(HawkSession session, HawkProtocol protocol) {
		// 协议解密
		protocol = ProtoUtil.decryptionProtocol(session, protocol);
		if (protocol == null) {
			return false;
		}

		long protoTime = HawkTime.getMillisecond();
		try {
			// 心跳协议直接处理
			if (protocol.checkType(HS.sys.HEART_BEAT)) {
				HSHeartBeat.Builder builder = HSHeartBeat.newBuilder();
				builder.setTimeStamp(HawkTime.getSeconds());
				protocol.getSession().sendProtocol(HawkProtocol.valueOf(HS.sys.HEART_BEAT, builder));
				return true;
			}

			try {
				if (session.getAppObject() == null) {
					// 登陆协议
					if (protocol.checkType(HS.code.LOGIN_C)) {

						String puid = protocol.parseProtocol(HSLogin.getDefaultInstance()).getPuid().trim().toLowerCase();
						if (!checkPuidValid(puid, session)) {
							return true;
						}

						// 登陆协议时间间隔控制
						synchronized (puidLoginTime) {
							if (puidLoginTime.containsKey(puid) && HawkTime.getMillisecond() <= puidLoginTime.get(puid) + 5000) {
								return true;
							}
							puidLoginTime.put(puid, HawkTime.getMillisecond());
						}

						if (!checkPlayerExist(puid, session)) {
							return true;
						}

						if (!preparePuidSession(HS.code.RECCONECT_C_VALUE, puid, session)) {
							return true;
						}

						// 登录成功协议
						int playerId = ServerData.getInstance().getPlayerIdByPuid(puid);
						HSLoginRet.Builder response = HSLoginRet.newBuilder();
						response.setStatus(Status.error.NONE_ERROR_VALUE);
						response.setPlayerId(playerId);
						session.sendProtocol(HawkProtocol.valueOf(HS.code.LOGIN_S, response));

						return true;
					} else if (protocol.checkType(HS.code.PLAYER_CREATE_C_VALUE)) {
						String puid = protocol.parseProtocol(HSPlayerCreate.getDefaultInstance()).getPuid().trim().toLowerCase();
						if (!CreateNewPlayer(puid, session, protocol)) {
							return true;
						}

						if (!preparePuidSession(HS.code.RECCONECT_C_VALUE, puid, session)) {
							return true;
						}

						return true;
					} else if (protocol.checkType(HS.code.RECCONECT_C_VALUE)) {
						HSReconnect cmd = protocol.parseProtocol(HSReconnect.getDefaultInstance());
						String puid = cmd.getPuid().trim().toLowerCase();
						if (!reconnectPuidSession(cmd, puid, session)) {
							return true;
						}

						HSReconnectRet.Builder response = HSReconnectRet.newBuilder();
						response.setStatus(Status.error.NONE_ERROR_VALUE);
						session.sendProtocol(HawkProtocol.valueOf(HS.code.RECCONECT_S_VALUE, response));

						// 通知玩家其他模块玩家登陆成功
						HawkXID xid = HawkXID.valueOf(GsConst.ObjType.PLAYER, ServerData.getInstance().getPlayerIdByPuid(puid));
						HawkMsg msg = HawkMsg.valueOf(GsConst.MsgType.PLAYER_RECONNECT, xid);
						msg.pushParam(cmd);
						if (!HawkApp.getInstance().postMsg(msg))
						{
							HawkLog.errPrintln("post player reconnect message failed: " + puid);
						}

						return true;
					} else {
						HawkLog.errPrintln("session appobj null cannot process unlogin protocol: " + protocol.getType());
						return false;
					}
				}
			} catch (Exception e) {
				HawkException.catchException(e);
				return false;
			}
			return super.onSessionProtocol(session, protocol);
		} finally {
			protoTime = HawkTime.getMillisecond() - protoTime;
			if (protoTime >= 20) {
				logger.info(
						"protocol cost time exception, protocolId: {}, costTime: {}ms",
						protocol.getType(), protoTime);
			}
		}
	}

	/**
	 * 会话关闭回调
	 */
	@Override
	public void onSessionClosed(HawkSession session) {
		if (session != null && session.getAppObject() != null) {
			HawkXID xid = session.getAppObject().getXid();
			if (xid != null && xid.isValid()) {
				postMsg(HawkMsg.valueOf(GsConst.MsgType.SESSION_CLOSED, xid));
			}
		}
		super.onSessionClosed(session);
	}

	/**
	 * 检测灰度账号
	 * 
	 * @param puid
	 * @return
	 */
	private boolean checkPuidValid(String puid, HawkSession session) {
		int grayState = GsConfig.getInstance().getGrayState();
		// 灰度状态下, 限制灰度账号
		if (grayState > 0) {
			GrayPuidCfg grayPuid = HawkConfigManager.getInstance().getConfigByKey(GrayPuidCfg.class, puid);
			if (grayPuid == null) {
				session.sendProtocol(ProtoUtil.genErrorProtocol(HS.code.LOGIN_C_VALUE, Status.error.SERVER_GRAY_STATE_VALUE, 1));
				return false;
			}
		}

		int playerId = ServerData.getInstance().getPlayerIdByPuid(puid);
		if (playerId == 0) {
			// 注册人数达到上限
			int registerMaxSize = GsConfig.getInstance().getRegisterMaxSize();
			if (registerMaxSize > 0 && ServerData.getInstance().getRegisterPlayer() >= registerMaxSize) {
				session.sendProtocol(ProtoUtil.genErrorProtocol(HS.code.LOGIN_C_VALUE,Status.error.REGISTER_MAX_LIMIT_VALUE, 1));
				return false;
			}
		}
		return true;
	}
	/**
	 * 
	 */
	@Override
	public void onShutdown() {
		HawkAccountService.getInstance().report(new HawkAccountService.UnRegitsterGameServer());
		HawkAccountService.getInstance().stop();
		ActivityHttpServer.getInstance().stop();
		super.onShutdown();
	}
	/**
	 * 检查puid对应的角色
	 * 
	 * @param puid
	 * @return
	 */
	private boolean checkPlayerExist(String puid, HawkSession session) {
		int playerId = ServerData.getInstance().getPlayerIdByPuid(puid);
		if (playerId == 0) {
			HSLoginRet.Builder response = HSLoginRet.newBuilder();
			response.setStatus(Status.error.NONE_ERROR_VALUE);
			response.setPlayerId(0);
			session.sendProtocol(HawkProtocol.valueOf(HS.code.LOGIN_S_VALUE, response));
			return false;
		}

		return true;
	}

	/**
	 * 创建puid对应的角色
	 * 
	 * @param puid
	 * @return
	 */
	private boolean CreateNewPlayer(String puid, HawkSession session, HawkProtocol cmd) {
		int playerId = ServerData.getInstance().getPlayerIdByPuid(puid);
		if (playerId == 0) {
			HSPlayerCreate protocol = cmd.parseProtocol(HSPlayerCreate.getDefaultInstance());
			if (ServerData.getInstance().isExistName(protocol.getNickname())) {
				HSErrorCode.Builder error = HSErrorCode.newBuilder();
				error.setErrCode(Status.PlayerError.PLAYER_NICKNAME_EXIST_VALUE);
				error.setHsCode(HS.code.PLAYER_CREATE_C_VALUE);
				session.sendProtocol(HawkProtocol.valueOf(HS.sys.ERROR_CODE, error));
				return false;
			}

			PlayerEntity playerEntity = new PlayerEntity(puid, protocol.getNickname(), (byte) (protocol.getCareer()), protocol.getGender(), protocol.getEye(), protocol.getHair(), protocol.getHairColor());
			if (false == playerEntity.notifyCreate()) {
				return false;
			}

			playerId = playerEntity.getId();
			ServerData.getInstance().addNameAndPlayerId(protocol.getNickname(), playerId);
			ServerData.getInstance().addPuidAndPlayerId(puid, playerId);
			logger.info("create player entity: {}, puid: {}", playerId, puid);

			HSPlayerCreateRet.Builder response = HSPlayerCreateRet.newBuilder();
			response.setStatus(Status.error.NONE_ERROR_VALUE);
			response.setPalyerID(playerId);
			session.sendProtocol(HawkProtocol.valueOf(HS.code.PLAYER_CREATE_S_VALUE, response));

			HawkAccountService.getInstance().report(new HawkAccountService.CreateRoleData(puid, playerId, protocol.getNickname()));
			return true;
		}
		else {
			session.sendProtocol(ProtoUtil.genErrorProtocol(HS.code.PLAYER_CREATE_C_VALUE, Status.PlayerError.PUID_EXIST_VALUE, 1));
		}
		return false;
	}

	/**
	 * 准备puid对应的会话
	 * 
	 * @param puid
	 * @return
	 */
	private boolean preparePuidSession(int hsCode, String puid, HawkSession session) {
		int playerId = ServerData.getInstance().getPlayerIdByPuid(puid);
		if (playerId == 0) {
			return false;
		}

		HawkXID xid = HawkXID.valueOf(GsConst.ObjType.PLAYER, playerId);
		HawkObjBase<HawkXID, HawkAppObj> objBase = lockObject(xid);
		try {
			// 对象不存在即创建
			if (objBase == null || !objBase.isObjValid()) {
				objBase = createObj(xid);
				if (objBase != null) {
					objBase.lockObj();
				}

			}

			// 会话绑定应用对象
			if (objBase != null) {
				// 已存在会话的情况下, 踢出玩家
				Player player = (Player) objBase.getImpl();
				if (player != null && player.getSession() != null && player.getSession() != session) {	

					player.kickout(Const.kickReason.DUPLICATE_LOGIN_VALUE);
					player.getSession().setAppObject(null);
				}

				// 设置玩家puid
				player.getPlayerData().setPuid(puid);
				// 绑定会话对象
				session.setAppObject(objBase.getImpl());
				player.setSession(session);
			}
		} finally {
			if (objBase != null) {
				objBase.unlockObj();
			}
		}
		return true;
	}

	/**
	 * 重新组装session
	 * 
	 * @param puid
	 * @return
	 */
	private boolean reconnectPuidSession(HSReconnect cmd, String puid, HawkSession session) {
		int playerId = ServerData.getInstance().getPlayerIdByPuid(puid);
		if (playerId == 0) {
			return false;
		}

		HawkXID xid = HawkXID.valueOf(GsConst.ObjType.PLAYER, playerId);
		HawkObjBase<HawkXID, HawkAppObj> objBase = lockObject(xid);
		try {
			// 对象不存在即创建
			if (objBase == null || !objBase.isObjValid()) {
				return false;
			}

			// 会话绑定应用对象
			Player player = (Player) objBase.getImpl();
			// 设置玩家puid
			player.getPlayerData().setPuid(puid);
			// 绑定会话对象
			session.setAppObject(objBase.getImpl());
			player.setSession(session);
		} finally {
			if (objBase != null) {
				objBase.unlockObj();
			}
		}

		return true;
	}

	// 主线程运行
	@Override
	public void onOrderNotify(JSONObject jsonInfo) {
		if (jsonInfo.getInt("action") == (HawkOrderService.ACTION_ORDER_DELIVER_REQUEST)) {

			logger.info("order notity:" + jsonInfo.toString());

			String puid = jsonInfo.getString("uid");
			String orderSerial = jsonInfo.getString("tid");
			String platform = jsonInfo.getString("type");
			String productId = jsonInfo.getString("product_id");

			int playerId = ServerData.getInstance().getPlayerIdByPuid(puid);
			if (playerId == 0) {
				HawkOrderService.getInstance().responseDeliver(orderSerial, HawkOrderService.ORDER_PLAYER_NOT_EXIST, 0, 0);
				return;
			}

			if (ServerData.getInstance().isExistOrder(orderSerial)) {
				List<RechargeEntity> resultList = HawkDBManager.getInstance().query("from RechargeEntity where orderSerial = ?", orderSerial);
				if (resultList != null && resultList.size() > 0) {
					RechargeEntity rechargeEntity = resultList.get(0);
					HawkOrderService.getInstance().responseDeliver(orderSerial, HawkOrderService.ORDER_STATUS_OK, rechargeEntity.getAddGold(), rechargeEntity.getGiftGold());
					return;
				}
			}

			HawkXID xid = HawkXID.valueOf(GsConst.ObjType.PLAYER, playerId);
			boolean playerOffline = false;
			HawkObjBase<HawkXID, HawkAppObj> objBase = lockObject(xid);
			try {
				// 离线对象创建临时数据
				if (objBase == null || !objBase.isObjValid()) {
					objBase = createObj(xid);
					if (objBase != null) {
						objBase.lockObj();
					}

					playerOffline = true;
				}

				if (objBase != null) {
					Player player = (Player) objBase.getImpl();
					if (playerOffline == true) {
						player.getPlayerData().setPuid(jsonInfo.getString("uid"));
						player.getPlayerData().loadPlayer();
						player.getPlayerData().loadStatistics();
					}

					// 处理订单
					ShopManager.getInstance().OnOrderNotify(player, puid, orderSerial, platform, productId);
				}
			} finally {
				if (objBase != null) {
					objBase.unlockObj();
				}
			}
		}
	}
}
