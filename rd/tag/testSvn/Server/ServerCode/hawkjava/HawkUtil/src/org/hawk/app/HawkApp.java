package org.hawk.app;

import java.io.File;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.Collection;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;
import java.util.TreeMap;
import java.util.concurrent.ConcurrentHashMap;

import net.sf.json.JSONObject;

import org.apache.mina.core.session.IoSession;
import org.apache.mina.util.ConcurrentHashSet;
import org.hawk.app.task.HawkMsgTask;
import org.hawk.app.task.HawkProtoTask;
import org.hawk.app.task.HawkRefreshTask;
import org.hawk.app.task.HawkTickTask;
import org.hawk.config.HawkConfigManager;
import org.hawk.db.HawkDBManager;
import org.hawk.intercept.HawkInterceptHandler;
import org.hawk.log.HawkLog;
import org.hawk.msg.HawkMsg;
import org.hawk.nativeapi.HawkNativeApi;
import org.hawk.net.HawkIoHandler;
import org.hawk.net.HawkNetManager;
import org.hawk.net.HawkNetStatistics;
import org.hawk.net.HawkSession;
import org.hawk.net.protocol.HawkProtocol;
import org.hawk.obj.HawkObjBase;
import org.hawk.obj.HawkObjManager;
import org.hawk.os.HawkException;
import org.hawk.os.HawkOSOperator;
import org.hawk.os.HawkShutdownHook;
import org.hawk.os.HawkTime;
import org.hawk.script.HawkScriptManager;
import org.hawk.service.HawkServiceManager;
import org.hawk.shell.HawkShellExecutor;
import org.hawk.thread.HawkTask;
import org.hawk.thread.HawkThreadPool;
import org.hawk.timer.HawkTimerManager;
import org.hawk.util.HawkTickable;
import org.hawk.xid.HawkXID;
import org.hawk.zmq.HawkZmq;
import org.hawk.zmq.HawkZmqManager;

/**
 * 应用层封装
 * 
 * @author hawk
 */
public abstract class HawkApp extends HawkAppObj {
	/**
	 * 循环退出状态
	 */
	private static int LOOP_BREAK = 0x0001;
	/**
	 * 循环关闭状态
	 */
	private static int LOOP_CLOSED = 0x0002;
	/**
	 * 单例使用
	 */
	protected static HawkApp instance;
	/**
	 * 工作路径
	 */
	protected String workPath;
	/**
	 * 本机ip地址
	 */
	protected String myHostIp = "127.0.0.1";
	/**
	 * 当前时间(毫秒, 用于初略的逻辑时间计算)
	 */
	protected long currentTime;
	/**
	 * 帧计数
	 */
	private int tickCounter = 0;
	/**
	 * 更新对象列表
	 */
	protected Set<HawkTickable> tickables;
	/**
	 * 是否在运行中
	 */
	protected volatile boolean running;
	/**
	 * 是否退出循环
	 */
	protected volatile int loopState;
	/**
	 * 上次清理对象事件
	 */
	protected long lastRemoveObjTime;
	/**
	 * 打印当前服务器状态
	 */
	protected long lastShowStateTime;
	/**
	 * 是否允许执行shell
	 */
	protected boolean shellEnable;
	/**
	 * 应用配置对象
	 */
	protected HawkAppCfg appCfg;
	/**
	 * 消息逻辑线程池
	 */
	protected HawkThreadPool msgExecutor;
	/**
	 * 任务逻辑线程池
	 */
	protected HawkThreadPool taskExecutor;
	/**
	 * 对象管理器
	 */
	protected Map<Integer, HawkObjManager<HawkXID, HawkAppObj>> objMans;
	/**
	 * 当前活跃会话列表
	 */
	protected Set<HawkSession> activeSessions;
	/**
	 * 对象id列表
	 */
	protected Collection<HawkXID> objXidList;
	/**
	 * 对象列表
	 */
	protected Collection<HawkAppObj> appObjList;
	/**
	 * tick时使用xid线程分类表
	 */
	protected Map<Integer, List<HawkXID>> threadTickXids;
	/**
	 * refresh时使用xid线程分类表
	 */
	protected Map<Integer, List<HawkXID>> threadRefreshXids;
	/**
	 * 拦截器
	 */
	protected Map<String, HawkInterceptHandler> interceptMap;

	/**
	 * 获取全局管理器
	 * 
	 * @return
	 */
	public static HawkApp getInstance() {
		return instance;
	}

	/**
	 * 默认构造函数
	 */
	public HawkApp(HawkXID xid) {
		super(xid);

		if (instance != null) {
			throw new RuntimeException("app instance exist");
		}
		instance = this;
		Thread.currentThread().setName("AppMain");

		// 加载库目录
		HawkOSOperator.installLibPath();
				
		// 初始化工作目录
		workPath = System.getProperty("user.dir") + File.separator;
		loopState = LOOP_CLOSED;
		shellEnable = true;

		// 初始化系统对象
		appCfg = new HawkAppCfg();
		tickables = new ConcurrentHashSet<HawkTickable>();
		activeSessions = new ConcurrentHashSet<HawkSession>();
		interceptMap = new ConcurrentHashMap<String, HawkInterceptHandler>();
		objMans = new TreeMap<Integer, HawkObjManager<HawkXID, HawkAppObj>>();
		objXidList = new LinkedList<HawkXID>();
		appObjList = new LinkedList<HawkAppObj>();
		lastRemoveObjTime = HawkTime.getMillisecond();
		lastShowStateTime = HawkTime.getMillisecond();
	}

	/**
	 * 初始化框架
	 * 
	 * @param appCfg
	 * @return
	 * @throws UnknownHostException 
	 */
	public boolean init(HawkAppCfg appCfg) {
		this.appCfg = appCfg;
		if (this.appCfg == null) {
			return false;
		}

		// 设置系统时间偏移
		HawkTime.setMsOffset(appCfg.calendarOffset);
		// 初始化系统时间
		currentTime = HawkTime.getMillisecond();
		// 打印系统信息
		HawkOSOperator.printOsEnv();
		// 开启关闭钩子
		HawkShutdownHook.getInstance().install();
		// 定时器管理器初始化
		HawkTimerManager.getInstance().init(false);

		// 脚本初始化
		if (appCfg.scriptXml != null && appCfg.scriptXml.length() > 0) {
			if (!HawkScriptManager.getInstance().init(appCfg.scriptXml)) {
				HawkLog.logPrintln("script init fail");
				//return false;
			}
		}

		// 获取本机ip
		myHostIp = HawkOSOperator.getMyIp(2000);

		// TODO
		try {
			if (myHostIp.equals("") || myHostIp.equals("127.0.0.1") || myHostIp.equals("123.126.3.94")) {
				myHostIp = InetAddress.getLocalHost().getHostAddress();
			}
		} catch (Exception e) {
			HawkLog.logPrintln("ip 获取失败");
			return false;
		}
		
		// 初始化网络统计
		HawkNetStatistics.getInstance().init();
				
		// 初始化zmq管理器
		HawkZmqManager.getInstance().init(HawkZmq.HZMQ_CONTEXT_THREAD);
		
		// 初始化service管理对象
		if (appCfg.servicePath != null && appCfg.servicePath.length() > 0) {
			if (!HawkServiceManager.getInstance().init(appCfg.servicePath)) {
				return false;
			}
		}

		// 初始化配置
		if (appCfg.configPackages != null && appCfg.configPackages.length() > 0) {
			if (!HawkConfigManager.getInstance().init(appCfg.configPackages, getWorkPath())) {
				System.err.println("----------------------------------------------------------------------");
				System.err.println("-------------config crashed, take weapon to fuck designer-------------");
				System.err.println("----------------------------------------------------------------------");
				return false;
			}
		}

		// 开启消息线程池
		if (msgExecutor == null && appCfg.threadNum > 0) {
			msgExecutor = new HawkThreadPool("MsgExecutor");
			if (!msgExecutor.initPool(appCfg.threadNum) || !msgExecutor.start()) {
				HawkLog.errPrintln(String.format("init msgExecutor failed, threadNum: %d", appCfg.threadNum));
				return false;
			}
			HawkLog.logPrintln(String.format("start msgExecutor, threadNum: %d", appCfg.threadNum));
		}

		// 开启任务线程池
		int taskThreadNum = appCfg.taskThreads;
		if (taskThreadNum <= 0) {
			taskThreadNum = appCfg.threadNum;
		}
		if (taskExecutor == null && taskThreadNum > 0) {
			taskExecutor = new HawkThreadPool("TaskExecutor");
			if (!taskExecutor.initPool(taskThreadNum) || !taskExecutor.start()) {
				HawkLog.errPrintln(String.format("init taskExecutor failed, threadNum: %d", taskThreadNum));
				return false;
			}
			HawkLog.logPrintln(String.format("start taskExecutor, threadNum: %d", taskThreadNum));
		}

		// 初始化数据库连接
		if (appCfg.dbHbmXml != null && appCfg.dbConnUrl != null && appCfg.dbUserName != null && appCfg.dbPassWord != null) {
			if (!HawkDBManager.getInstance().init(appCfg.dbHbmXml, appCfg.dbConnUrl, appCfg.dbUserName, appCfg.dbPassWord, appCfg.entityPackages)) {
				return false;
			}

			// 开启数据库异步落地
			if (appCfg.dbAsyncPeriod > 0) {
				int dbThreadNum = appCfg.dbThreads;
				if (dbThreadNum <= 0) {
					dbThreadNum = appCfg.threadNum;
				}

				if (dbThreadNum > 0) {
					HawkDBManager.getInstance().startAsyncThread(appCfg.dbAsyncPeriod, dbThreadNum);
					HawkLog.logPrintln(String.format("start dbExecutor, threadNum: %d", dbThreadNum));
				}
			}
		}

		// 自动脚本运行
		HawkScriptManager.getInstance().autoRunScript();

		return true;
	}
	
	/**
	 * 开启网络
	 * 
	 * @return
	 */
	public boolean startNetwork() {
		if (appCfg.acceptorPort > 0) {
			if (!HawkNetManager.getInstance().initFromAppCfg(appCfg)) {
				HawkLog.errPrintln("init network failed, port: " + appCfg.acceptorPort);
				return false;
			}

			HawkLog.logPrintln("start network, port: " + appCfg.acceptorPort);
		}
		return true;
	}

	/**
	 * 更新ip白名单控制
	 */
	public void updateIpControl(boolean ipLimit, Map<String, Integer> iptables) {
		// 黑白名单功能
		try {
			HawkIoHandler ioHandler = HawkNetManager.getInstance().getIoHandler();
			if (ioHandler != null) {
				if (ipLimit) {
					ioHandler.setIpUsage(HawkIoHandler.IpUsage.WHITE_IPTABLES | HawkIoHandler.IpUsage.BLACK_IPTABLES);
				} else {
					ioHandler.setIpUsage(0);
				}
				HawkNetManager.getInstance().clearWhiteIp();
				HawkNetManager.getInstance().clearBlackIp();
				
				if (ipLimit && iptables != null) {
					for (Entry<String, Integer> entry : iptables.entrySet()) {
						if (entry.getValue() > 0) {
							HawkNetManager.getInstance().addWhiteIp(entry.getKey());
						} else {
							HawkNetManager.getInstance().addBlackIp(entry.getKey());
						}
					}
				}
			}
		} catch (Exception e) {
			HawkException.catchException(e);
		}
	}
	
	/**
	 * 获取hash线程索引
	 */
	public int getHashThread(HawkXID xid, int threadNum) {
		return xid.getId() % threadNum;
	}
	
	/**
	 * 获取工作目录
	 * 
	 * @return
	 */
	public String getWorkPath() {
		return workPath;
	}

	/**
	 * 获取当前系统时间
	 * 
	 * @return
	 */
	public long getCurrentTime() {
		return currentTime;
	}

	/**
	 * 设置本机ip地址
	 * 
	 * @param myip
	 */
	public void setMyHostIp(String myHostIp) {
		this.myHostIp = myHostIp;
	}
	
	/**
	 * 获取本机ip
	 * 
	 * @return
	 */
	public String getMyHostIp() {
		return myHostIp;
	}

	/**
	 * 获取应用配置对象
	 * 
	 * @return
	 */
	public HawkAppCfg getAppCfg() {
		return appCfg;
	}

	/**
	 * 设置应用配置对象
	 * 
	 * @return
	 */
	public void setAppCfg(HawkAppCfg appCfg) {
		this.appCfg = appCfg;
	}

	/**
	 * 是否运行状态
	 * 
	 * @return
	 */
	public boolean isRunning() {
		return running;
	}

	/**
	 * 通知退出循环
	 * 
	 * @return
	 */
	public boolean breakLoop() {
		loopState |= LOOP_BREAK;
		return true;
	}

	/**
	 * 开启或关闭shell命令执行权限
	 * 
	 * @param enable
	 */
	public void enableShell(boolean enable) {
		this.shellEnable = enable;
	}

	/**
	 * 线程池线程数目
	 * 
	 * @return
	 */
	public int getThreadNum() {
		if (msgExecutor != null) {
			return msgExecutor.getThreadNum();
		}

		if (taskExecutor != null) {
			return taskExecutor.getThreadNum();
		}

		return 0;
	}

	/**
	 * 获取线程id列表
	 * 
	 * @return
	 */
	public Collection<Long> getThreadIds() {
		List<Long> threadIds = new LinkedList<Long>();
		for (int i = 0; msgExecutor != null && i < msgExecutor.getThreadNum(); i++) {
			threadIds.add(msgExecutor.getThreadId(i));
		}
		return threadIds;
	}

	/**
	 * 获取活跃会话集合
	 * 
	 * @return
	 */
	public Set<HawkSession> getActiveSessions() {
		return activeSessions;
	}

	/**
	 * 获取tickable的集合
	 * 
	 * @return
	 */
	public Set<HawkTickable> getTickables() {
		return tickables;
	}

	/**
	 * 添加可tick对象
	 * 
	 * @param tickable
	 */
	public void addTickable(HawkTickable tickable) {
		tickables.add(tickable);
	}

	/**
	 * 移除tick对象
	 * 
	 * @param tickable
	 */
	public void removeTickable(HawkTickable tickable) {
		tickables.remove(tickable);
	}

	/**
	 * 移除tick对象
	 * 
	 * @param tickable
	 */
	public void removeTickable(String name) {
		Iterator<HawkTickable> iterator = tickables.iterator();
		while (iterator.hasNext()) {
			try {
				HawkTickable tickable = iterator.next();
				if (tickable != null && tickable.getName().equals(name)) {
					iterator.remove();
				}
			} catch (Exception e) {
				HawkException.catchException(e);
			}
		}
	}

	/**
	 * 清空tick对象
	 */
	public void clearTickable() {
		tickables.clear();
	}

	/**
	 * 添加拦截器
	 * 
	 * @param name
	 * @param handler
	 */
	public void addInterceptHandler(String name, HawkInterceptHandler handler) {
		interceptMap.put(name, handler);
	}

	/**
	 * 获取拦截器
	 * 
	 * @param name
	 * @param handler
	 */
	public HawkInterceptHandler getInterceptHandler(String name) {
		return interceptMap.get(name);
	}

	/**
	 * 移除拦截器
	 * 
	 * @param name
	 */
	public void removeInterceptHandler(String name) {
		interceptMap.remove(name);
	}

	/**
	 * 清除拦截器
	 */
	public void clearInterceptHandler() {
		interceptMap.clear();
	}

	/**
	 * 是否为debug模式
	 * 
	 * @return
	 */
	public boolean isDebug() {
		return appCfg.isDebug;
	}

	/**
	 * 设置上次对象移除时间
	 * 
	 * @param lastRemoveObjTime
	 */
	public void setLastRemoveObjTime(long lastRemoveObjTime) {
		this.lastRemoveObjTime = lastRemoveObjTime;
	}

	/**
	 * 是否为退出状态
	 * 
	 * @return
	 */
	public boolean isWaitingClose() {
		return !running || (loopState & LOOP_BREAK) > 0;
	}

	/**
	 * 启动服务
	 * 
	 * @return
	 */
	public boolean run() {
		if (!running) {
			// 构建db会话工厂
			HawkDBManager.getInstance().buildSessionFactory();
			
			// 检测网络是否开启
			if (appCfg.acceptorPort > 0 && !HawkNetManager.getInstance().isStart()) {
				if (!startNetwork()) {
					return false;
				}
			}

			// 设置状态
			running = true;
			loopState = 0;

			HawkLog.logPrintln("server running......");
			while (running && (loopState & LOOP_BREAK) == 0) {
				currentTime = HawkTime.getMillisecond();

				// 逻辑帧更新
				try {
					onTick(currentTime);
					++tickCounter;

					if (tickCounter % appCfg.getRefreshTickCount() == 0) {
						tickCounter = 0;
						onRefresh(currentTime);
					}
				} catch (Exception e) {
					HawkException.catchException(e);
				}

				HawkOSOperator.osSleep(appCfg.tickPeriod);
			}
			onClosed();
			running = false;
			HawkLog.logPrintln("hawk main loop exit");
			return true;
		}
		return false;
	}

	/**
	 * 帧更新
	 */
	@Override
	public boolean onTick(long tickTime) {
		// 更新检测
		if (!HawkNativeApi.tickHawk()) {
			return false;
		}

		// tick对象的更新
		for (HawkTickable tickable : tickables) {
			if (tickable.isTickable()) {
				tickable.onTick();
			}
		}

		// 对象管理器的更新(每小时一个周期)
		if (tickTime - lastRemoveObjTime >= 3600000) {
			lastRemoveObjTime = tickTime;
			int removeCount = 0;
			for (Entry<Integer, HawkObjManager<HawkXID, HawkAppObj>> entry : objMans.entrySet()) {
				HawkObjManager<HawkXID, HawkAppObj> objMan = entry.getValue();
				if (objMan != null && objMan.getObjTimeout() > 0) {
					// 清理超时对象
					List<HawkAppObj> removeAppObjs = objMan.removeTimeoutObj(tickTime);
					if (removeAppObjs != null) {
						for (HawkAppObj appObj : removeAppObjs) {
							onRemoveTimeoutObj(appObj);
						}
						removeCount = removeAppObjs.size();
					}
					HawkLog.logPrintln(String.format("app remove timeout obj, manager: %d, count: %d", entry.getKey(), removeCount));
				}
			}
		}

		//打印任务队列状态
		if (tickTime - lastShowStateTime >= 5000) {
			lastShowStateTime = tickTime;
			printState();
			
			// 检测内存不足
			Runtime run = Runtime.getRuntime();
			if ((run.maxMemory() - run.totalMemory() + run.freeMemory()) * 1.0 / run.maxMemory() < 0.2) {
				onMemoryOutWarning();
			}
		}
		
		// 对象更新
		for (Entry<Integer, HawkObjManager<HawkXID, HawkAppObj>> entry : objMans.entrySet()) {
			HawkObjManager<HawkXID, HawkAppObj> objMan = entry.getValue();
			if (objMan != null) {
				objXidList.clear();
				if (objMan.collectObjKey(objXidList, null) > 0) {
					postTick(objXidList, tickTime);
				}
			}
		}

		return super.onTick(tickTime);
	}

	/**
	 * 刷新数据
	 */
	@Override
	protected boolean onRefresh(long refreshTime) {
		// 对象刷新数据
		for (Entry<Integer, HawkObjManager<HawkXID, HawkAppObj>> entry : objMans.entrySet()) {
			HawkObjManager<HawkXID, HawkAppObj> objMan = entry.getValue();
			if (objMan != null) {
				objXidList.clear();
				if (objMan.collectObjKey(objXidList, null) > 0) {
					postRefresh(objXidList, refreshTime);
				}
			}
		}

		return super.onTick(refreshTime);
	}

	/**
	 * 移除超时应用对象
	 * 
	 * @param appObj
	 */
	protected void onRemoveTimeoutObj(HawkAppObj appObj) {
	}

	/**
	 * 内存不足警告
	 * 
	 * @param appObj
	 */
	protected void onMemoryOutWarning() {
		HawkLog.errPrintln("内存不足警告");
	}
	
	/**
	 * 处理shell命令, 不可手动调用, 由脚本管理器调用
	 * 
	 * @param params
	 */
	public String onShellCommand(String cmd, long timeout) {
		if (shellEnable && cmd != null && cmd.length() > 0) {
			String result = HawkShellExecutor.execute(cmd, timeout);
			HawkLog.logPrintln("shell command: " + cmd + "\r\n" + result);
			return result;
		}
		return null;
	}

	/**
	 * 程序被关闭时的回调
	 */
	public void onShutdown() {
		breakLoop();

		// 等待循环状态
		while ((loopState & LOOP_CLOSED) != LOOP_CLOSED) {
			HawkOSOperator.sleep();
		}
	}

	/**
	 * 应用程序退出时回调
	 */
	protected void onClosed() {
		try {
			try {
				// 关闭网络
				HawkNetManager.getInstance().close();
			} catch (Exception e) {
				HawkException.catchException(e);
			}

			try {
				// 停止定时器管理器
				HawkTimerManager.getInstance().stop();
			} catch (Exception e) {
				HawkException.catchException(e);
			}

			try {
				// 停止数据库
				HawkDBManager.getInstance().stop();
			} catch (Exception e) {
				HawkException.catchException(e);
			}

			try {
				// 等待消息线程结束
				if (msgExecutor != null) {
					msgExecutor.close(true);
				}
			} catch (Exception e) {
				HawkException.catchException(e);
			}

			try {
				// 等待任务线程池结束
				if (taskExecutor != null) {
					taskExecutor.close(true);
				}
			} catch (Exception e) {
				HawkException.catchException(e);
			}

			try {
				// 关闭脚本管理器
				HawkScriptManager.getInstance().close();
			} catch (Exception e) {
				HawkException.catchException(e);
			}

			try {
				// 关闭数据库
				HawkDBManager.getInstance().close();
			} catch (Exception e) {
				HawkException.catchException(e);
			}

		} finally {
			// 设置关闭状态
			loopState |= LOOP_CLOSED;
		}
	}

	/**
	 * 获取指定类型的管理器
	 * 
	 * @param type
	 * @return
	 */
	public HawkObjManager<HawkXID, HawkAppObj> getObjMan(int type) {
		return objMans.get(type);
	}

	/**
	 * 注册创建对象管理器
	 * 
	 * @param type
	 * @return
	 */
	protected HawkObjManager<HawkXID, HawkAppObj> createObjMan(int type) {
		HawkObjManager<HawkXID, HawkAppObj> objMan = getObjMan(type);
		if (objMan == null) {
			objMan = new HawkObjManager<HawkXID, HawkAppObj>(true);
			objMans.put(type, objMan);
		}
		return objMan;
	}

	/**
	 * 创建对象通用接口
	 * 
	 * @param xid
	 * @param sid
	 * @return
	 */
	public HawkObjBase<HawkXID, HawkAppObj> createObj(HawkXID xid) {
		if (xid.isValid()) {
			// 获取管理器
			HawkObjManager<HawkXID, HawkAppObj> objMan = getObjMan(xid.getType());
			if (objMan == null) {
				HawkLog.errPrintln("objMan type nonentity: " + xid.getType());
				return null;
			}

			// 创建应用层对象
			HawkAppObj appObj = onCreateObj(xid);
			if (appObj != null) {
				// 添加到管理器容器
				HawkObjBase<HawkXID, HawkAppObj> objBase = objMan.allocObject(xid, appObj);
				return objBase;
			}
		}
		HawkLog.errPrintln("create obj failed: " + xid);
		return null;
	}

	/**
	 * 应用层创建对象, 应用层必须重写此函数
	 * 
	 * @param xid
	 * @return
	 */
	protected HawkAppObj onCreateObj(HawkXID xid) {
		return null;
	}

	/**
	 * 查询指定id对象, 非线程安全
	 * 
	 * @param xid
	 * @return
	 */
	public HawkObjBase<HawkXID, HawkAppObj> queryObject(HawkXID xid) {
		if (xid != null && xid.isValid()) {
			HawkObjManager<HawkXID, HawkAppObj> objMan = objMans.get(xid.getType());
			if (objMan != null) {
				return objMan.queryObject(xid);
			}
		}
		return null;
	}

	/**
	 * 查询唯一id对象, 必须把返回对象进行unlockObj操作以避免不解锁
	 * 
	 * @param xid
	 * @return
	 */
	public HawkObjBase<HawkXID, HawkAppObj> lockObject(HawkXID xid) {
		if (xid != null && xid.isValid()) {
			HawkObjManager<HawkXID, HawkAppObj> objMan = objMans.get(xid.getType());
			if (objMan != null) {
				return objMan.lockObject(xid);
			}
		}
		return null;
	}

	/**
	 * 销毁对象
	 * 
	 * @param xid
	 * @return
	 */
	public boolean removeObj(HawkXID xid) {
		if (xid.isValid()) {
			// 获取管理器
			HawkObjManager<HawkXID, HawkAppObj> objMan = getObjMan(xid.getType());
			if (objMan != null) {
				objMan.freeObject(xid);
				return true;
			}
		}
		return false;
	}

	/**
	 * 投递通用型任务到线程池处理
	 * 
	 * @param task
	 * @return
	 */
	public boolean postCommonTask(HawkTask task) {
		if (running && task != null && taskExecutor != null) {
			return taskExecutor.addTask(task);
		}
		return false;
	}

	/**
	 * 投递通用型任务到线程池处理
	 * 
	 * @param task
	 * @param threadIdx 线程id
	 * @return
	 */
	public boolean postCommonTask(HawkTask task, int threadIdx) {
		if (running && task != null && taskExecutor != null) {
			return taskExecutor.addTask(task, Math.abs(threadIdx), false);
		}
		return false;
	}

	/**
	 * 投递通用型任务到线程池处理
	 * 
	 * @param task
	 * @param threadIdMin 线程区间下界
	 * @param threadIdMax 线程区间上界
	 * @return
	 */
	public boolean postCommonTask(HawkTask task, int threadIdMin, int threadIdMax) {
		if (running && task != null && taskExecutor != null) {
			return taskExecutor.addTask(task, threadIdMin, threadIdMax, false);
		}
		return false;
	}

	/**
	 * 投递任务到消息线程组
	 * 
	 * @param task
	 * @return
	 * @throws Exception
	 */
	public boolean postMsgTask(HawkMsgTask task) {
		if (running && task != null) {
			int threadIdx = getHashThread(task.getXid(), getThreadNum());
			return postMsgTask(task, threadIdx);
		}
		return false;
	}

	/**
	 * 投递任务到消息线程组
	 * 
	 * @param task
	 * @param threadIdx
	 * @return
	 */
	protected boolean postMsgTask(HawkTask task, int threadIdx) {
		if (running && task != null) {
			return msgExecutor.addTask(task, Math.abs(threadIdx), false);
		}
		return false;
	}

	/**
	 * 接收到协议后投递到应用
	 * 
	 * @param xid
	 * @param protocol
	 * @return
	 */
	public boolean postProtocol(HawkXID xid, HawkProtocol protocol) {
		if (running && xid != null && protocol != null) {
			int threadIdx = getHashThread(xid, getThreadNum());
			return postMsgTask(HawkProtoTask.valueOf(xid, protocol), threadIdx);
		}
		return false;
	}

	/**
	 * 向特定对象投递消息
	 * 
	 * @param xid
	 * @param msg
	 * @return
	 */
	public boolean postMsg(HawkXID xid, HawkMsg msg) {
		if (running && xid != null && msg != null) {
			msg.setTarget(xid);
			return postMsg(msg);
		}
		return false;
	}

	/**
	 * 直接投递消息
	 * 
	 * @param msg
	 * @return
	 */
	public boolean postMsg(HawkMsg msg) {
		if (running && msg != null) {
			int threadIdx = getHashThread(msg.getTarget(), getThreadNum());
			if (HawkApp.getInstance().getAppCfg().isDebug()) {
				HawkLog.logPrintln(String.format("post message: %d, target: %s, thread: %d", msg.getMsg(), msg.getTarget().toString(), threadIdx));
			}
			HawkMsgTask hawkMsgTask = HawkMsgTask.valueOf(msg.getTarget(), msg);
			hawkMsgTask.setMustRun(true);
			return postMsgTask(hawkMsgTask, threadIdx);
		}
		return false;
	}

	/**
	 * 群发消息
	 * 
	 * @param xidList
	 * @param msg
	 * @return
	 */
	public boolean postMsg(Collection<HawkXID> xidList, HawkMsg msg) {
		if (running && xidList != null && xidList.size() > 0 && msg != null) {
			Map<Integer, List<HawkXID>> threadXidMap = new HashMap<Integer, List<HawkXID>>();
			// 计算xid列表所属线程
			for (HawkXID xid : xidList) {
				int threadIdx = getHashThread(xid, getThreadNum());
				List<HawkXID> threadXidList = threadXidMap.get(threadIdx);
				if (threadXidList == null) {
					threadXidList = new LinkedList<HawkXID>();
					threadXidMap.put(threadIdx, threadXidList);
				}
				threadXidList.add(xid);
			}

			// 按线程投递消息
			for (Map.Entry<Integer, List<HawkXID>> entry : threadXidMap.entrySet()) {
				postMsgTask(HawkMsgTask.valueOf(entry.getValue(), msg), entry.getKey());
			}
			return true;
		}
		return false;
	}

	/**
	 * 投递更新, 只会在主线程调用
	 * 
	 * @param xidList
	 * @return
	 */
	public boolean postTick(Collection<HawkXID> xidList, long tickTime) {
		if (running && xidList != null && xidList.size() > 0) {
			// 先创建线程tick表
			if (threadTickXids == null) {
				threadTickXids = new HashMap<Integer, List<HawkXID>>();
				for (int i = 0; i < getThreadNum(); i++) {
					threadTickXids.put(i, new LinkedList<HawkXID>());
				}
			} else {
				for (int i = 0; i < getThreadNum(); i++) {
					threadTickXids.get(i).clear();
				}
			}

			if (xidList != null && xidList.size() > 0) {
				// 计算xid列表所属线程
				for (HawkXID xid : xidList) {
					int threadIdx = getHashThread(xid, getThreadNum());
					// app对象本身不参与线程tick更新计算, 本身的tick在主线程执行
					if (false == xid.equals(this.objXid)) {
						threadTickXids.get(threadIdx).add(xid);
					}
				}

				// 按线程投递消息
				for (Map.Entry<Integer, List<HawkXID>> entry : threadTickXids.entrySet()) {
					if (entry.getValue().size() > 0) {
						// 不存在即创建
						postMsgTask(HawkTickTask.valueOf(entry.getValue(), tickTime), entry.getKey());
					}
				}
			}
		}
		return true;
	}

	/**
	 * 投递刷新, 只会在主线程调用
	 */
	public boolean postRefresh(Collection<HawkXID> xidList, long refreshTime) {
		if (running && xidList != null && xidList.size() > 0) {
			// 先创建线程refresh表
			if (threadRefreshXids == null) {
				threadRefreshXids = new HashMap<Integer, List<HawkXID>>();
				for (int i = 0; i < getThreadNum(); i++) {
					threadRefreshXids.put(i, new LinkedList<HawkXID>());
				}
			} else {
				for (int i = 0; i < getThreadNum(); i++) {
					threadRefreshXids.get(i).clear();
				}
			}

			if (xidList != null && xidList.size() > 0) {
				// 计算xid列表所属线程
				for (HawkXID xid : xidList) {
					int threadIdx = getHashThread(xid, getThreadNum());
					// app对象本身不参与线程refresh更新计算, 本身的refresh在主线程执行
					if (false == xid.equals(this.objXid)) {
						threadRefreshXids.get(threadIdx).add(xid);
					}
				}

				// 按线程投递消息
				for (Map.Entry<Integer, List<HawkXID>> entry : threadRefreshXids.entrySet()) {
					if (entry.getValue().size() > 0) {
						// 不存在即创建
						postMsgTask(HawkRefreshTask.valueOf(entry.getValue(), refreshTime), entry.getKey());
					}
				}
			}
		}
		return true;
	}

	/**
	 * 广播消息
	 * 
	 * @param msg
	 * @param xidList
	 * @return
	 */
	public boolean broadcastMsg(HawkMsg msg, Collection<HawkXID> xidList) {
		if (running && msg != null && xidList != null) {
			return postMsg(xidList, msg);
		}
		return false;
	}

	/**
	 * 广播消息
	 * 
	 * @param msg
	 * @param objMan
	 * @return
	 */
	public boolean broadcastMsg(HawkMsg msg, HawkObjManager<HawkXID, HawkAppObj> objMan) {
		if (msg != null && objMan != null) {
			List<HawkXID> xidList = new LinkedList<HawkXID>();
			objMan.collectObjKey(xidList, null);
			return postMsg(xidList, msg);
		}
		return false;
	}

	/**
	 * 协议广播
	 * 
	 * @param protocol
	 * @return
	 */
	public boolean broadcastProtocol(HawkProtocol protocol) {
		for (HawkSession session : activeSessions) {
			try {
				session.sendProtocol(protocol);
			} catch (Exception e) {
				HawkException.catchException(e);
			}
		}
		return true;
	}

	/**
	 * 分发协议
	 * 
	 * @param xid
	 * @param protocol
	 * @return
	 */
	public boolean dispatchProto(HawkXID xid, HawkProtocol protocol) {
		if (xid != null && protocol != null) {
			if (xid.isValid()) {
				HawkObjBase<HawkXID, HawkAppObj> objBase = lockObject(xid);
				if (objBase != null) {
					long beginTimeMs = HawkTime.getMillisecond();
					try {
						if (objBase.isObjValid()) {
							objBase.setVisitTime(currentTime);
							
							try {
								HawkInterceptHandler interceptHandler = getInterceptHandler(objBase.getImpl().getClass().getName());
								if (interceptHandler != null && interceptHandler.onProtocol(objBase.getImpl(), protocol)) {
									return true;
								}
							} catch (Exception e) {
								HawkException.catchException(e);
							}
							
							return objBase.getImpl().onProtocol(protocol);
						}
					} catch (Exception e) {
						HawkException.catchException(e);
					} finally {
						objBase.unlockObj();
						long costTimeMs = HawkTime.getMillisecond() - beginTimeMs;
						if (costTimeMs > HawkApp.getInstance().getAppCfg().getProtoTimeout()) {
							HawkLog.logPrintln("protocol timeout, protocol: " + protocol.getType() + ", costtime: " + costTimeMs);
						}
					}
				}
			} else {
				return onProtocol(protocol);
			}
		}
		return false;
	}

	/**
	 * 分发消息
	 * 
	 * @param xid
	 * @param msg
	 * @return
	 * @throws Exception
	 */
	public boolean dispatchMsg(HawkXID xid, HawkMsg msg) {
		if (xid != null && msg != null) {
			if (xid.isValid()) {
				if (HawkApp.getInstance().getAppCfg().isDebug()) {
					HawkLog.logPrintln(String.format("dispatch message: %d, target: %s", msg.getMsg(), xid.toString()));
				}

				HawkObjBase<HawkXID, HawkAppObj> objBase = lockObject(xid);
				if (objBase != null) {
					long beginTimeMs = HawkTime.getMillisecond();
					try {
						if (objBase.isObjValid()) {
							
							try {
								HawkInterceptHandler interceptHandler = getInterceptHandler(objBase.getImpl().getClass().getName());
								if (interceptHandler != null && interceptHandler.onMessage(objBase.getImpl(), msg)) {
									return true;
								}
							} catch (Exception e) {
								HawkException.catchException(e);
							}
							
							return objBase.getImpl().onMessage(msg);
						}
					} catch (Exception e) {
						HawkException.catchException(e);
					} finally {
						objBase.unlockObj();
						long costTimeMs = HawkTime.getMillisecond() - beginTimeMs;
						if (costTimeMs > HawkApp.getInstance().getAppCfg().getProtoTimeout()) {
							HawkLog.logPrintln("message timeout, msg: " + msg.getMsg() + ", costtime: " + costTimeMs);
						}
					}
				}
			} else {
				return onMessage(msg);
			}
		}
		return false;
	}

	/**
	 * 分发更新事件
	 * @throws Exception
	 */
	public boolean dispatchTick(HawkXID xid, long tickTime) {
		if (xid != null) {
			if (xid.isValid()) {
				HawkObjBase<HawkXID, HawkAppObj> objBase = lockObject(xid);
				if (objBase != null) {
					try {
						if (objBase.isObjValid()) {
							
							try {
								HawkInterceptHandler interceptHandler = getInterceptHandler(objBase.getImpl().getClass().getName());
								if (interceptHandler != null && interceptHandler.onTick(objBase.getImpl())) {
									return true;
								}
							} catch (Exception e) {
								HawkException.catchException(e);
							}
							
							return objBase.getImpl().onTick(tickTime);
						}
					} catch (Exception e) {
						HawkException.catchException(e);
					} finally {
						objBase.unlockObj();
					}
				}
			} else {
				return onTick(tickTime);
			}
		}
		return false;
	}

	/**
	 * 分发刷新事件
	 * @throws Exception
	 */
	public boolean dispatchRefresh(HawkXID xid, long refreshTime) {
		if (xid != null) {
			if (xid.isValid()) {
				HawkObjBase<HawkXID, HawkAppObj> objBase = lockObject(xid);
				if (objBase != null) {
					try {
						if (objBase.isObjValid()) {

							try {
								HawkInterceptHandler interceptHandler = getInterceptHandler(objBase.getImpl().getClass().getName());
								if (interceptHandler != null && interceptHandler.onRefresh(objBase.getImpl())) {
									return true;
								}
							} catch (Exception e) {
								HawkException.catchException(e);
							}

							return objBase.getImpl().onRefresh(refreshTime);
						}
					} catch (Exception e) {
						HawkException.catchException(e);
					} finally {
						objBase.unlockObj();
					}
				}
			} else {
				return onRefresh(refreshTime);
			}
		}
		return false;
	}

	/**
	 * 会话开启回调
	 * 
	 * @param session
	 */
	public boolean onSessionOpened(HawkSession session) {
		activeSessions.add(session);
		return true;
	}

	/**
	 * 会话协议回调, 由IO线程直接调用, 非线程安全
	 * 
	 * @param session
	 * @param protocol
	 * @return
	 */
	public boolean onSessionProtocol(HawkSession session, HawkProtocol protocol) {
		if (running && session != null && protocol != null && session.getAppObject() != null) {
			return postProtocol(session.getAppObject().getXid(), protocol);
		}
		return false;
	}

	/**
	 * 会话关闭回调
	 * 
	 * @param session
	 */
	public void onSessionClosed(HawkSession session) {
		activeSessions.remove(session);
	}

	/**
	 * 报告异常信息(主要通过邮件)
	 * 
	 * @param e
	 */
	public void reportException(Exception e) {
		
	}

	/**
	 * 聊天控制器发过来消息
	 * 
	 * @param msg
	 */
	public void onChatMonitorNotify(String msg) {
	}

	/**
	 * 订单服务器通知(订单生成, 订单发货)
	 * 
	 * @param jsonInfo
	 *            (action字段区分通知数据类型, 具体参考HawkOrderService的action定义)
	 */
	public void onOrderNotify(JSONObject jsonInfo) {
	}
	
	/**
	 * ip服务拒绝连接
	 * 
	 * @param ip
	 * @param usage
	 */
	public boolean onRefuseByIptables(IoSession session, String ip, int usage) {
		return false;
	}
	
	/**
	 * 打印当前队列任务数量
	 * 
	 * @return
	 */
	public void printState() {
		int pushCount = 0;
		int popCount = 0;
		
		for (int i = 0; i < msgExecutor.getThreadNum(); i++) {
			pushCount += msgExecutor.getThread(i).getPushTaskCnt();
			popCount += msgExecutor.getThread(i).getPopTaskCnt();
		}
		
		//HawkLog.errPrintln(String.format("msg总任务数量： %d, 处理数量： %d", pushCount, popCount));

		pushCount = 0;
		popCount = 0;
		
		for (int i = 0; i < taskExecutor.getThreadNum(); i++) {
			pushCount += taskExecutor.getThread(i).getPushTaskCnt();
			popCount += taskExecutor.getThread(i).getPopTaskCnt();
		}
		
		//HawkLog.errPrintln(String.format("tsk总任务数量： %d, 处理数量： %d", pushCount, popCount));	
		
		Runtime run = Runtime.getRuntime(); 
		long max = run.maxMemory(); 
		long total = run.totalMemory(); 
		long free = run.freeMemory(); 
		long usable = max - total + free; 
		
		//HawkLog.errPrintln(String.format("最大内存 = %d, 已分配内存 = %d , 已分配内存中的剩余空间 = %d, 最大可用内存= %d", max, total, free, usable));	
	}
}
