package com.hawk.game.manager;

import java.text.MessageFormat;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Queue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.atomic.AtomicInteger;

import org.hawk.app.HawkApp;
import org.hawk.app.HawkAppObj;
import org.hawk.cache.HawkCacheObj;
import org.hawk.net.HawkSession;
import org.hawk.net.protocol.HawkProtocol;
import org.hawk.os.HawkException;
import org.hawk.thread.HawkTask;
import org.hawk.util.services.FunPlusTranslateService;
import org.hawk.util.services.FunPlusTranslateService.Translation;
import org.hawk.xid.HawkXID;

import com.hawk.game.GsConfig;
import com.hawk.game.config.ImSysCfg;
import com.hawk.game.player.Player;
import com.hawk.game.protocol.Const;
import com.hawk.game.protocol.HS;
import com.hawk.game.protocol.Im.HSImMsg;
import com.hawk.game.protocol.Im.HSImPush;
import com.hawk.game.util.GsConst;

/**
 * 即时通讯管理器
 * 
 * @author walker
 */
public class ImManager extends HawkAppObj {

	// 结构----------------------------------------------------------------------------------------------------------

	public class ImMsg {
		// 必须的----------------------------
		public int type = 0;
		public int channel = 0;
		public int senderId = 0;

		// 可选的----------------------------
		public int guildId = 0;
		public int receiverId = 0;
		public ImPlayer receiver = null;
		public String expansion = null;

		// 系统消息----------------------------
		public ImSysCfg sysCfg = null;
		public Object[] sysArgs = null;

		// 非系统消息-------------------------
		public String senderName = "";
		public String origLang = GsConst.DEFAULT_LANGUAGE;
		public String origText = "";
		/** @key 语言IOS代码 */
		public Map<String, String> transText = null;

		// 定时消息-------------------
		public int times = 0;
		public int intervalSecond = 0;
		public int nextTime = 0;
	}

	/**
	 * 封装只读的Player，不需锁定
	 */
	public class ImPlayer {
		private Player playerObj;

		public ImPlayer(Player playerObj) {
			this.playerObj = playerObj;
		}
		public String getLanguage() {
			return playerObj.getLanguage();
		}
		public HawkSession getSession() {
			return playerObj.getSession();
		}
		public List<Integer> getBlockPlayerList() {
			return playerObj.getEntity().getBlockPlayerList();
		}
	}

	// 属性----------------------------------------------------------------------------------------------------------

	/**
	 * 世界频道会话列表
	 * @key playerId
	 */
	private ConcurrentHashMap<Integer, ImPlayer> worldPlayerMap;

	/**
	 * 公会频道会话列表
	 * @key guildId
	 */
	private ConcurrentHashMap<Integer, ConcurrentHashMap<Integer, ImPlayer>> guildPlayerMap;

	/**
	 * 世界频道语言计数
	 * @key 语言ISO代码
	 * @value 该语言player数量
	 */
	private ConcurrentHashMap<String, Integer> worldLangMap;

	/**
	 * 公会频道语言计数
	 * @key guildId
	 */
	private ConcurrentHashMap<Integer, ConcurrentHashMap<String, Integer>> guildLangMap;

	/**
	 * 待推送消息队列
	 */
	// TODO: 在真实环境中测试哪种结构最优
	private ConcurrentLinkedQueue<ImMsg> personMsgQueue;
	private ConcurrentLinkedQueue<ImMsg> worldMsgQueue;
	//private LinkedBlockingQueue<ImMsg> worldMsgQueue;
	//private List<ImMsg> worldMsgQueue;
	private ConcurrentHashMap<Integer, ConcurrentLinkedQueue<ImMsg>> guildMsgQueueMap;

	// ConcurrentLinkedQueue.size()效率低，单独记录
	private AtomicInteger worldMsgQueueSize = new AtomicInteger(0);

	/**
	 * 待翻译消息队列
	 */
	private ConcurrentLinkedQueue<ImMsg> transMsgQueue;

	/**
	 * 定时消息队列
	 */
	private List<ImMsg> clockMsgList;

	/**
	 * 帧计数
	 */
	private int pushTickCounter = 0;
	private int transTickCounter = 0;

	private static ImManager instance = null;
	public static ImManager getInstance() {
		return instance;
	}

	public ImManager(HawkXID xid) {
		super(xid);
		if (instance == null) {
			instance = this;
		}

		worldPlayerMap = new ConcurrentHashMap<Integer, ImPlayer>();
		worldLangMap = new ConcurrentHashMap<String, Integer>();
		guildPlayerMap = new ConcurrentHashMap<Integer, ConcurrentHashMap<Integer, ImPlayer>>();
		guildLangMap = new ConcurrentHashMap<Integer, ConcurrentHashMap<String, Integer>>();
		personMsgQueue = new ConcurrentLinkedQueue<ImMsg>();
		worldMsgQueue = new ConcurrentLinkedQueue<ImMsg>();
		//worldMsgQueue = new LinkedBlockingQueue<ImMsg>();
		//worldMsgQueue = new LinkedList<ImMsg>();
		guildMsgQueueMap = new ConcurrentHashMap<Integer, ConcurrentLinkedQueue<ImMsg>>();
		transMsgQueue = new ConcurrentLinkedQueue<ImMsg>();
		clockMsgList = new LinkedList<ImMsg>();
	}

	/**
	 * 加入世界频道
	 */
	public void joinWorld(Player playerObj) {
		if (playerObj != null) {
			worldPlayerMap.put(playerObj.getId(), new ImPlayer(playerObj));

			synchronized (worldLangMap) {
				Integer count = worldLangMap.get(playerObj.getLanguage());
				if (count == null) {
					count = 0;
				}
				worldLangMap.put(playerObj.getLanguage(), count + 1);
			}
		}
	}

	/**
	 * 加入公会频道
	 */
	public void joinGuild(int guildId, Player playerObj) {
		if (playerObj != null) {
			ConcurrentHashMap<Integer, ImPlayer> playerMap = null;
			synchronized (guildPlayerMap) {
				playerMap = guildPlayerMap.get(guildId);
				if (playerMap == null) {
					playerMap = new ConcurrentHashMap<Integer, ImPlayer>();
					guildPlayerMap.put(guildId, playerMap);
				}
			}
			// 直接覆盖，不需同步
			playerMap.put(playerObj.getId(), new ImPlayer(playerObj));

			synchronized (guildLangMap) {
				ConcurrentHashMap<String, Integer> langMap = guildLangMap.get(guildId);
				if (langMap == null) {
					langMap = new ConcurrentHashMap<String, Integer>();
					guildLangMap.put(guildId, langMap);
				}

				Integer count = langMap.get(playerObj.getLanguage());
				if (count == null) {
					count = 0;
				}
				langMap.put(playerObj.getLanguage(), count + 1);
			}
		}
	}

	/**
	 * 退出世界频道
	 */
	public void quitWorld(int playerId) {
		ImPlayer imPlayer = worldPlayerMap.remove(playerId);
		if (imPlayer != null) {
			synchronized (worldLangMap) {
				Integer count = worldLangMap.get(imPlayer.getLanguage());
				if (count != null && count != 0) {
					worldLangMap.put(imPlayer.getLanguage(), count - 1);
				}
			}
		}
	}

	/**
	 * 退出公会频道
	 */
	public void quitGuild(int guildId, int playerId) {
		Map<Integer, ImPlayer> playerMap = guildPlayerMap.get(guildId);
		if (playerMap != null) {
			ImPlayer imPlayer = playerMap.remove(playerId);
			if (imPlayer != null) {
				Map<String, Integer> langMap = guildLangMap.get(guildId);
				if (langMap != null) {
					synchronized(langMap) {
						Integer count = langMap.get(imPlayer.getLanguage());
						if (count != null && count != 0) {
							langMap.put(imPlayer.getLanguage(), count - 1);
						}
					}
				}
			}
		}
	}

	/**
	 * 删除公会
	 */
	public void removeGuild(int guildId) {
		guildPlayerMap.remove(guildId);
		guildLangMap.remove(guildId);
	}

	/**
	 * 切换语言
	 */
	public void changeLanguage(String oldLang, String newLang, int guildId) {
		synchronized (worldLangMap) {
			Integer count = worldLangMap.get(oldLang);
			if (count != null && count != 0) {
				worldLangMap.put(oldLang, count - 1);
			}
			count = worldLangMap.get(newLang);
			if (count == null) {
				count = 0;
			}
			worldLangMap.put(newLang, count + 1);
		}

		if (guildId != 0) {
			Map<String, Integer> langMap = guildLangMap.get(guildId);
			if (langMap != null) {
				synchronized(langMap) {
					Integer count = langMap.get(oldLang);
					if (count != null && count != 0) {
						langMap.put(oldLang, count - 1);
					}
					count = langMap.get(newLang);
					if (count == null) {
						count = 0;
					}
					langMap.put(newLang, count + 1);
				}
			}
		}
	}

	/**
	 * 投递聊天消息(由player自身调用)
	 */
	public void postChat(Player player, int channel, String chatText, String expansion) {
		ImMsg msgObj = new ImMsg();
		msgObj.type = Const.ImType.CHAT_VALUE;
		msgObj.channel = channel;
		msgObj.senderId = player.getId();
		msgObj.expansion = expansion;
		msgObj.senderName = player.getName();
		msgObj.origLang = player.getLanguage();
		msgObj.origText = chatText;
		msgObj.transText = null;

		if (channel == Const.ImChannel.GUILD_VALUE) {
			msgObj.guildId = player.getPlayerData().getPlayerAllianceEntity().getAllianceId();
		}

		post(msgObj);
	}

	/**
	 * 投递系统消息
	 */
	public void postSys(ImSysCfg imCfg, int guildId, Object... args) {
		ImMsg msgObj = new ImMsg();
		msgObj.type = imCfg.getType();
		msgObj.channel = imCfg.getChannel();
		msgObj.senderId = 0;
		msgObj.expansion = null;
		msgObj.sysCfg = imCfg;
		msgObj.sysArgs = args;

		if (msgObj.channel == Const.ImChannel.GUILD_VALUE) {
			msgObj.guildId = guildId;
		}

		post(msgObj);
	}

	/**
	 * 投递消息
	 */
	public void post(ImMsg msgObj) {
		// 接收者是否在线
		if (msgObj.channel == Const.ImChannel.PERSON_VALUE) {
			ImPlayer receiver = worldPlayerMap.get(msgObj.receiverId);
			if (receiver == null) {
				return;
			} else {
				msgObj.receiver = receiver;
			}
		}

		// 如果开启翻译服务，并且非系统消息
		if (true == GsConfig.getInstance().isImTranslate() && msgObj.sysCfg == null) {
			// 加入待翻译列表
			if (false == transMsgQueue.offer(msgObj)) {
				// 失败直接加入待推送列表
				queueMsg(msgObj);
			}
		} else {
			// 加入待推送列表
			queueMsg(msgObj);
		}
	}

	/**
	 * 删除某类型的待发送定时消息
	 */
	public void clearClockMsg(int type) {
		if (false == clockMsgList.isEmpty()) {
			synchronized (clockMsgList) {
				Iterator<ImMsg> iter = clockMsgList.iterator();
				while (iter.hasNext()) {
					if (iter.next().type == type) {
						iter.remove();
					}
				}
			}
		}
	}

	/**
	 * 线程主执行函数
	 */
	@Override
	public boolean onTick(long tickTime) {
		++pushTickCounter;
		++transTickCounter;
		ImMsg msgObj = null;

		// 定时消息----------------------------------------------------------------------------------
		if (false == clockMsgList.isEmpty()) {
			synchronized (clockMsgList) {
				int curSecond = (int) (tickTime / 1000);
				Iterator<ImMsg> iter = clockMsgList.iterator();
				while (iter.hasNext()) {
					msgObj = iter.next();
					if (curSecond >= msgObj.nextTime) {
						queueOneTimeMsg(msgObj);

						if (--msgObj.times == 0) {
							iter.remove();
						} else {
							msgObj.nextTime += msgObj.intervalSecond;
						}
					}
				}
			}
		}

		// 推送---------------------------------------------------------------------------------------
		if (pushTickCounter % GsConfig.getInstance().getImPushTaskTickCount() == 0) {
			pushTickCounter = 0;

			final int pushMaxCount = GsConfig.getInstance().getImPushTaskMsgMaxCount();
			Map<ImPlayer, List<ImMsg>> pushPersonMap = null;
			List<ImMsg> pushWorldList = null;
			Map<Integer, List<ImMsg>> pushGuildMap = null;

			if (false == personMsgQueue.isEmpty()) {
				pushPersonMap = new HashMap<>();
				for (int i = 0; i < pushMaxCount; ++i) {
					msgObj = personMsgQueue.poll();
					if (null != msgObj) {
						List<ImMsg> pushList = pushPersonMap.get(msgObj.receiver);
						if (pushList == null) {
							pushList = new ArrayList<ImMsg>();
							pushPersonMap.put(msgObj.receiver, pushList);
						}
						pushList.add(msgObj);
					} else {
						break;
					}
				}
			}

			if (false == worldMsgQueue.isEmpty()) {
				pushWorldList = new ArrayList<ImMsg>();
				for (int i = 0; i < pushMaxCount; ++i) {
					msgObj = worldMsgQueue.poll();
					if (null != msgObj) {
						worldMsgQueueSize.decrementAndGet();
						pushWorldList.add(msgObj);
					} else {
						break;
					}
				}
			}
//			if (false == worldMsgQueue.isEmpty()) {
//				pushWorldList = new ArrayList<ImMsg>();
//				synchronized(worldMsgQueue){
//					for (int i = 0; i < pushMaxCount; ++i) {
//						msgObj = worldMsgQueue.get(0);
//						if (null != msgObj) {
//							pushWorldList.add(msgObj);
//							worldMsgQueue.remove(0);
//						} else {
//							break;
//						}
//					}
//				}
//			}

			if (false == guildMsgQueueMap.isEmpty()) {
				pushGuildMap = new HashMap<>();
				for (Entry<Integer, ConcurrentLinkedQueue<ImMsg>> entry : guildMsgQueueMap.entrySet()) {
					// TODO 根据运行情况考虑是否移除空queue
					Queue<ImMsg> queue = entry.getValue();
					if (true == queue.isEmpty()) {
						continue;
					}

					List<ImMsg> pushGuildList = new ArrayList<ImMsg>();
					pushGuildMap.put(entry.getKey(), pushGuildList);

					for (int i = 0; i < pushMaxCount; ++i) {
						msgObj = queue.poll();
						if (null != msgObj) {
							pushGuildList.add(msgObj);
						} else {
							break;
						}
					}
				}
			}

			if ((null != pushPersonMap && false == pushPersonMap.isEmpty())
					|| (null != pushWorldList && false == pushWorldList.isEmpty())
					|| (null != pushGuildMap && false == pushGuildMap.isEmpty())) {
				int[] threadRange = GsConfig.getInstance().getImPushThreadRange();
				HawkApp.getInstance().postCommonTask(new ImPushTask(pushPersonMap, pushWorldList, pushGuildMap), threadRange[0], threadRange[1]);
			}
		}

		// 翻译------------------------------------------------------------------------------------------
		if (transTickCounter % GsConfig.getInstance().getImTransTaskTickCount() == 0) {
			transTickCounter = 0;

			final int transMaxCount = GsConfig.getInstance().getImTransTaskMsgMaxCount();
			if (false == transMsgQueue.isEmpty()) {
				List<ImMsg> transList = new ArrayList<ImMsg>();

				for (int i = 0; i < transMaxCount; ++i) {
					msgObj = transMsgQueue.poll();
					if (msgObj != null) {
						transList.add(msgObj);
					} else {
						break;
					}
				}

				if (false == transList.isEmpty()) {
					int[] threadRange = GsConfig.getInstance().getImTransThreadRange();
					HawkApp.getInstance().postCommonTask(new ImTransBatchTask(transList), threadRange[0], threadRange[1]);
				}
			}
		}

		return true;
	}

	// 内部----------------------------------------------------------------------------------------------------------

	/**
	 * 消息进队列
	 */
	private void queueMsg(ImMsg msgObj) {
		if (msgObj.times != 0) {
			queueClockMsg(msgObj);
		} else {
			queueOneTimeMsg(msgObj);
		}
	}

	/**
	 * 定时消息进队列
	 */
	private void queueClockMsg(ImMsg msgObj) {
		synchronized (clockMsgList) {
			clockMsgList.add(msgObj);
		}
	}

	/**
	 * 一次性消息进队列
	 */
	private void queueOneTimeMsg(ImMsg msgObj) {
		switch (msgObj.channel) {
			case Const.ImChannel.PERSON_VALUE: {
				personMsgQueue.offer(msgObj);
				break;
			}
			case Const.ImChannel.WORLD_VALUE: {
				worldMsgQueue.offer(msgObj);
//				synchronized (worldMsgQueue){
//					worldMsgQueue.add(msgObj);
//				}
				// 消息队列满，释放空间
				if (worldMsgQueueSize.incrementAndGet() > GsConfig.getInstance().getImWorldQueueFreeBeginSize()) {
					while (worldMsgQueueSize.get() > GsConfig.getInstance().getImWorldQueueFreeEndSize()) {
						msgObj = worldMsgQueue.poll();
						if (null != msgObj) {
							worldMsgQueueSize.decrementAndGet();
						} else {
							break;
						}
					}
				}
				break;
			}
			case Const.ImChannel.GUILD_VALUE: {
				ConcurrentLinkedQueue<ImMsg> guildMsgQueue = null;
				synchronized (guildMsgQueueMap) {
					guildMsgQueue = guildMsgQueueMap.get(msgObj.guildId);
					if (null == guildMsgQueue) {
						guildMsgQueue = new ConcurrentLinkedQueue<>();
						guildMsgQueueMap.put(msgObj.guildId, guildMsgQueue);
					}
				}
				guildMsgQueue.offer(msgObj);
				break;
			}
		}
	}

	/**
	 * IM批量翻译任务
	 */
	private class ImTransBatchTask extends HawkTask {

		private List<ImMsg> transList;

		protected ImTransBatchTask(List<ImMsg> transList) {
			this.transList = transList;
		}

		@Override
		protected HawkCacheObj clone() {
			return new ImTransBatchTask(this.transList);
		}

		@Override
		protected int run() {
			if (null == transList || true == transList.isEmpty()) {
				return 0;
			}

			// 生成批量请求
			Translation[] transArray = new Translation[transList.size()];
			for (int i = 0; i < transList.size(); ++i) {
				ImMsg msgObj = transList.get(i);
				msgObj.transText = new HashMap<String, String>();

				List<String> langList = new ArrayList<String>();
				switch (msgObj.channel) {
					case Const.ImChannel.PERSON_VALUE: {
						langList.add(msgObj.receiver.getLanguage());
						break;
					}
					case Const.ImChannel.WORLD_VALUE: {
						for (Entry<String, Integer> entry : worldLangMap.entrySet()) {
							if (entry.getValue() > 0) {
								langList.add(entry.getKey());
							}
						}
						break;
					}
					case Const.ImChannel.GUILD_VALUE: {
						for (Entry<String, Integer> entry : guildLangMap.get(msgObj.guildId).entrySet()) {
							if (entry.getValue() > 0) {
								langList.add(entry.getKey());
							}
						}
						break;
					}
				}

				transArray[i] = new Translation();
				transArray[i].sourceText = msgObj.origText;
				transArray[i].sourceLang = msgObj.origLang;
				transArray[i].targetLangArray = new String[langList.size()];
				for (int j = 0; j < langList.size(); ++j) {
					transArray[i].targetLangArray[j] = langList.get(j);
				}
				transArray[i].profanity = GsConst.Translation.CENSOR_PROFANITY;
				transArray[i].textType = GsConst.Translation.CHAT_TYPE;
			}

			// 异步翻译
			FunPlusTranslateService.getInstance().translateBatch(transArray);

			// 结果加入待推送队列
			for (int i = 0; i < transArray.length; ++i) {
				ImMsg msgObj = transList.get(i);
				Translation trans = transArray[i];
				if (trans.transTextArray != null) {
					for (int j = 0; j < trans.targetLangArray.length; ++j) {
						// 如果翻译失败，译文为null，设为原文
						if (trans.transTextArray[j] == null) {
							msgObj.transText.put(trans.targetLangArray[j], trans.sourceText);
						} else {
							msgObj.transText.put(trans.targetLangArray[j], trans.transTextArray[j]);
						}
					}
				}
				queueMsg(msgObj);
			}

			return 0;
		}
	}

	/**
	 * IM推送任务
	 */
	private class ImPushTask extends HawkTask {
		private Map<ImPlayer, List<ImMsg>> pushPersonMap;
		private List<ImMsg> pushWorldList;
		private Map<Integer, List<ImMsg>> pushGuildMap;

		protected ImPushTask(Map<ImPlayer, List<ImMsg>> pushPersonMap, List<ImMsg> pushWorldList, Map<Integer, List<ImMsg>> pushGuildMap) {
			this.pushPersonMap = pushPersonMap;
			this.pushWorldList = pushWorldList;
			this.pushGuildMap = pushGuildMap;
		}

		@Override
		protected HawkCacheObj clone() {
			return new ImPushTask(this.pushPersonMap, this.pushWorldList, this.pushGuildMap);
		}

		@Override
		protected int run() {
			// 推送私人频道
			if (null != pushPersonMap && false == pushPersonMap.isEmpty()) {
				for (Entry<ImPlayer, List<ImMsg>> entry : pushPersonMap.entrySet()) {
					push(entry.getValue(), entry.getKey());
				}
			}

			// 推送世界频道
			if (null != pushWorldList && false == pushWorldList.isEmpty()) {
				push(pushWorldList, worldPlayerMap);
			}

			// 推送公会频道
			if (null != pushGuildMap && false == pushGuildMap.isEmpty()) {
				for(Entry<Integer, List<ImMsg>> pushGuildEntry : pushGuildMap.entrySet()) {
					List<ImMsg> pushGuildList = pushGuildEntry.getValue();
					ConcurrentHashMap<Integer, ImPlayer> playerMap = guildPlayerMap.get(pushGuildEntry.getKey());
					if (playerMap != null) {
						push(pushGuildList, playerMap);
					}
				}
			}

			return 0;
		}
	}

	/**
	 * 广播推送消息
	 * @param imMsgList 消息队列
	 * @param playerMap 玩家表，需要同步
	 */
	private void push(List<ImMsg> imMsgList, ConcurrentHashMap<Integer, ImPlayer> playerMap) {
		Iterator<Entry<Integer, ImPlayer>> iterator = playerMap.entrySet().iterator();
		while (iterator.hasNext()) {
			Entry<Integer, ImPlayer> entry = iterator.next();
			push(imMsgList, entry.getValue());

			if (null == entry.getValue().getSession()) {
				iterator.remove();
			}
		}
	}

	/**
	 * 单人推送消息
	 * @param imMsgList 消息队列
	 * @param playerObj 玩家
	 */
	private void push(List<ImMsg> imMsgList,  ImPlayer playerObj) {
		int count = 0;
		int maxCount = GsConfig.getInstance().getImPushPackMsgMaxCount();
		HSImPush.Builder builder = HSImPush.newBuilder();

		try {
			for (ImMsg msgObj : imMsgList) {
				// 检查屏蔽
				if (false == playerObj.getBlockPlayerList().contains(msgObj.senderId)) {
					builder.addImMsg(genImMsgBuilder(msgObj, playerObj.getLanguage()));
					++count;
				}

				// 分批发送
				if (count >= maxCount) {
					count = 0;
					playerObj.getSession().sendProtocol(HawkProtocol.valueOf(HS.code.IM_PUSH_S, builder));
					builder.clear();
				}
			}

			if (count > 0) {
				playerObj.getSession().sendProtocol(HawkProtocol.valueOf(HS.code.IM_PUSH_S, builder));
			}
		} catch (NullPointerException e) {
			// playerObj可能已经下线，sesson == null，为了效率不用锁操作修正问题，而是忽略异常
			if (playerObj.getSession() != null) {
				HawkException.catchException(e);
			}
		}
	}

	/**
	 * 生成ImMsg
	 */
	private HSImMsg.Builder genImMsgBuilder(ImMsg imMsg, String language) {
		HSImMsg.Builder builder = HSImMsg.newBuilder();
		builder.setType(imMsg.type);
		builder.setChannel(imMsg.channel);
		builder.setSenderId(imMsg.senderId);
		if (imMsg.expansion != null) {
			builder.setExpansion(imMsg.expansion);
		}

		// 系统消息
		if (imMsg.sysCfg != null) {
			builder.setOrigText(MessageFormat.format(imMsg.sysCfg.getContent(language), imMsg.sysArgs));
			builder.setSenderName(imMsg.sysCfg.getSender(language));
		}
		// 非系统消息
		else {
			builder.setOrigText(imMsg.origText);
			if (imMsg.senderName != null) {
				builder.setSenderName(imMsg.senderName);
			}
			if (imMsg.transText != null) {
				String transMsg = imMsg.transText.get(language);
				if (transMsg != null) {
					builder.setTransText(transMsg);
				}
			}
		}

		return builder;
	}
}
