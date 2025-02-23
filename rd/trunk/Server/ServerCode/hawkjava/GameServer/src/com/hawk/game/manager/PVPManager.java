package com.hawk.game.manager;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import org.hawk.app.HawkApp;
import org.hawk.app.HawkAppObj;
import org.hawk.config.HawkConfigManager;
import org.hawk.db.HawkDBManager;
import org.hawk.msg.HawkMsg;
import org.hawk.net.protocol.HawkProtocol;
import org.hawk.os.HawkException;
import org.hawk.os.HawkRand;
import org.hawk.os.HawkTime;
import org.hawk.xid.HawkXID;

import com.hawk.game.GsApp;
import com.hawk.game.ServerData;
import com.hawk.game.BILog.BIBehaviorAction.Action;
import com.hawk.game.config.MailSysCfg;
import com.hawk.game.config.PVPCfg;
import com.hawk.game.config.PVPRankRewardCfg;
import com.hawk.game.config.PVPStageRewardCfg;
import com.hawk.game.config.SysBasicCfg;
import com.hawk.game.entity.PVPDefenceEntity;
import com.hawk.game.entity.PVPDefenceRecordEntity;
import com.hawk.game.entity.PVPRankEntity;
import com.hawk.game.item.AwardItems;
import com.hawk.game.item.ConsumeItems;
import com.hawk.game.player.Player;
import com.hawk.game.protocol.Const;
import com.hawk.game.protocol.HS;
import com.hawk.game.protocol.Monster.HSMonster;
import com.hawk.game.protocol.Monster.HSMonsterDefence;
import com.hawk.game.protocol.PVP.HSPVPEnterRet;
import com.hawk.game.protocol.PVP.HSPVPInfoRet;
import com.hawk.game.protocol.PVP.HSPVPMatchTarget;
import com.hawk.game.protocol.PVP.HSPVPMatchTargetRet;
import com.hawk.game.protocol.PVP.HSPVPRankDefence;
import com.hawk.game.protocol.PVP.HSPVPRankDefenceRet;
import com.hawk.game.protocol.PVP.HSPVPRankRet;
import com.hawk.game.protocol.PVP.HSPVPSettle;
import com.hawk.game.protocol.PVP.HSPVPSettleRet;
import com.hawk.game.protocol.PVP.PVPRankData;
import com.hawk.game.protocol.Status;
import com.hawk.game.util.GsConst;
import com.hawk.game.util.MailUtil;

public class PVPManager extends HawkAppObj {
	
	public static class PVPRoom{
		int targetId;
		PVPDefenceEntity pvpDefenceEntity;
		int startTime;
		int changeTime;
		boolean isEnter;
		
		public PVPRoom(int targetId, int changeTime, PVPDefenceEntity pvpDefenceEntity) {
			super();
			this.targetId = targetId;
			this.changeTime = changeTime;
			this.pvpDefenceEntity = pvpDefenceEntity;
			this.startTime = HawkTime.getSeconds();
			isEnter = false;
		}
	}
	
	/**
	 * 全局对象, 便于访问
	 */
	private static PVPManager instance = null;
	
	/**
	 * 排行榜
	 */
	private List<PVPRankEntity> pvpRankList = new ArrayList<>(5000); 	
	
	/**
	 * 玩家和排行榜对应关系
	 */
	private Map<Integer, PVPRankEntity> playerRankMap = new HashMap<>();
	
	/**
	 * 匹配池
	 */
	private Map<Integer, HashMap<Integer, PVPDefenceEntity>> pvpPoolList = new HashMap<Integer, HashMap<Integer, PVPDefenceEntity>>();
	
	/**
	 * pvp房间
	 */
	private Map<Integer, PVPRoom> pvpRoomList = new HashMap<>();
	
	public static List<HawkMsg> test = new LinkedList<>();
	
	/**
	 * 排行榜刷新时间
	 */
	private long refreshTime = 0;
	
	/**
	 * 排行榜前一百名对应的防守阵容
	 */
	private Map<Integer, PVPDefenceEntity> pvpRankDefenceList = new HashMap<>(100);
	
	/**
	 * 前一百名的数据
	 */
	private HSPVPRankRet.Builder pvpRankBuilder = null;
	
	/**
	 * gm 匹配玩家
	 */
	static public int GMTargetID = 0;
	
	/**
	 * 获取全局实例对象
	 */
	public static PVPManager getInstance() {
		return instance;
	}
	
	/**
	 * 构造函数
	 */
	public PVPManager(HawkXID xid) {
		super(xid);
		if (instance == null) {
			instance = this;
		}
	}
	
	/**
	 * 数据加载
	 */
	public boolean init() {	
		List<PVPRankEntity> PVPRankEntities = HawkDBManager.getInstance().query("from PVPRankEntity where invalid = 0 and inRank = 1 order by point desc");
		if (PVPRankEntities.isEmpty() == false) {
			int rank = 0;
			for (PVPRankEntity pvpRankEntity : PVPRankEntities) {
				pvpRankEntity.setRank(rank++);
				pvpRankList.add(pvpRankEntity);
				playerRankMap.put(pvpRankEntity.getPlayerId(), pvpRankEntity);
			}
		}
		
		for (PVPCfg pvpCfg : HawkConfigManager.getInstance().getConfigList(PVPCfg.class)) {
			pvpPoolList.put(pvpCfg.getStage(), new HashMap<Integer, PVPDefenceEntity>());
		}
		
		generatePVPPool();
		
		return true;
	}
	
	@Override
	public boolean onMessage(HawkMsg msg) {
		if(msg.getMsg() == GsConst.MsgType.PVP_MATCH_TARGET){
			onPVPMatch(msg);
			return true;
		}
		else if (msg.getMsg() == GsConst.MsgType.PVP_ENTER_ROOM) {
			onPVPEnterRoom(msg);
			return true;
		}
		else if (msg.getMsg() == GsConst.MsgType.PVP_SETTLE) {
			onPVPSettle(msg);
			return true;
		}
		else if (msg.getMsg() == GsConst.MsgType.PVP_RANK_LIST) {
			onPVPRankList(msg);
			return true;
		}
		else if (msg.getMsg() == GsConst.MsgType.PVP_WEEK_REWARD) {
			onPVPWeekReward(msg);
			return true;
		}
		else if (msg.getMsg() == GsConst.MsgType.PLAYER_LEVEL_UP) {
			onPlayerLevelUp(msg);
			return true;
		}
		else if (msg.getMsg() == GsConst.MsgType.PVP_RANK_DEFENCE) {
			onPVPRankListDecence(msg);
			return true;
		}
		else if (msg.getMsg() == GsConst.MsgType.PVP_MY_INFO) {
			onPVPMyInfo(msg);
			return true;
		}
		else if (msg.getMsg() == GsConst.MsgType.PVP_LOGOUT) {
			onPVPLogout(msg);
			return true;
		}
		
		return super.onMessage(msg);
	}
	
	/**
	 * 重置匹配池
	 */
	public boolean generatePVPPool(){
		int endIndex = pvpRankList.size() - 1;
		int beginIndex = 0;
		HashSet<Integer> randomPlayers = new HashSet<Integer>();
		for (PVPCfg pvpCfg : HawkConfigManager.getInstance().getConfigList(PVPCfg.class)) {
			pvpPoolList.get(pvpCfg.getStage()).clear();
			randomPlayers.clear();
			beginIndex = endIndex;
			for (int i = endIndex; i >= 0; i--) {
				if (pvpCfg.getPoint() != 0 && pvpRankList.get(i).getPoint() > pvpCfg.getPoint()) {
					break;
				}

				beginIndex = i - 1;
			}
			
			// 选取种子用户
			if (endIndex - beginIndex > 2 * SysBasicCfg.getInstance().getPvpStageInitSize()) {
				while (randomPlayers.size() < SysBasicCfg.getInstance().getPvpStageInitSize()) {
					try {
						randomPlayers.add(HawkRand.randInt(beginIndex + 1, endIndex));
					} catch (Exception e) {
						
					}
				}
			}
			else if (endIndex - beginIndex > SysBasicCfg.getInstance().getPvpStageInitSize()) {
				for (int i = beginIndex + 1; i < beginIndex + SysBasicCfg.getInstance().getPvpStageInitSize(); i++) {
					randomPlayers.add(i);
				}
			}
			else if (endIndex - beginIndex > 0){
				for (int i = beginIndex + 1; i <= endIndex; i++) {
					randomPlayers.add(i);
				}
			}
			
			Map<Integer, PVPDefenceEntity> stageList = pvpPoolList.get(pvpCfg.getStage());
			for (int index : randomPlayers) {
				PVPDefenceEntity pvpDefenceEntity = getPVPDefenceEntity(pvpRankList.get(index).getPlayerId(), pvpRankList.get(index).getName(), pvpRankList.get(index).getLevel(), true);
				stageList.put(pvpRankList.get(index).getPlayerId(), pvpDefenceEntity);
			}
			
			endIndex = beginIndex;
		}
			
		return true;
	}
	
	/**
	 * 重置前一百名排行榜防守数据
	 */
	public void generatePVPRankDefenceList(){
		Map<Integer, PVPDefenceEntity> newDefenceList = new HashMap<>(100);
		for (PVPRankData rankData : pvpRankBuilder.getPvpRankListList()) {
			PVPDefenceEntity defenceEntity = pvpRankDefenceList.get(rankData.getPlayerId());
			if (defenceEntity != null) {
				newDefenceList.put(rankData.getPlayerId(), defenceEntity);
				continue;
			}
			
			defenceEntity = getPVPDefenceEntityFromPool(rankData.getPlayerId());
			if (defenceEntity != null) {
				newDefenceList.put(rankData.getPlayerId(), defenceEntity);
			}
		}
		
		pvpRankDefenceList = newDefenceList;
	}
	
	/**
	 * 生成排行榜缓存builder
	 */
	public void generatePVPRankBuilder(){
		pvpRankBuilder = HSPVPRankRet.newBuilder();
		
		int lastRank = 1;
		int lastPoint = 0;
		for (int i = 0; i < pvpRankList.size(); i++) {
			if (i == 0) {
				lastPoint = pvpRankList.get(0).getPoint();
			}

			PVPRankEntity pvpRankEntity = pvpRankList.get(i);
			PVPRankData.Builder pvpRankData = PVPRankData.newBuilder();
			pvpRankData.setPlayerId(pvpRankEntity.getPlayerId());
			pvpRankData.setLevel(pvpRankEntity.getLevel());
			pvpRankData.setPoint(pvpRankEntity.getPoint());
			pvpRankData.setName(pvpRankEntity.getName());
			
			// 设置排名 有并列情况
			if (pvpRankList.get(i).getPoint() == lastPoint) {
				pvpRankData.setRank(lastRank);
			}
			else{
				if (pvpRankBuilder.getPvpRankListCount() >= GsConst.PVP.PVP_RANK_SIZE) {
					break;
				}
				
				pvpRankData.setRank(i + 1);
				lastPoint = pvpRankList.get(i).getPoint();
				lastRank = i + 1;
			}
			
			pvpRankBuilder.addPvpRankList(pvpRankData);
		}
	
		generatePVPRankDefenceList();
	}
	
	/**
	 * pvp匹配
	 * @param msg
	 */
	public void onPVPMatch(HawkMsg msg){
		Player player = msg.getParam(0);	
		HawkProtocol cmd = msg.getParam(1);
		HSPVPMatchTarget protocol = cmd.parseProtocol(HSPVPMatchTarget.getDefaultInstance());
		
		if (!player.getPlayerData().getPVPDefenceEntity().hasSetDefenceMonsters()) {
			player.sendError(HS.code.PVP_MATCH_TARGET_C_VALUE, Status.pvpError.PVP_NOT_SET_DEFENCE_VALUE);
			return ;
		}
		
		player.regainPVPTime();
		if (player.getPlayerData().getStatisticsEntity().getPVPTime() < 1) {
			//player.sendError(HS.code.PVP_MATCH_TARGET_C_VALUE, Status.pvpError.PVP_TIMES_NOT_ENOUGH_VALUE);
			//return ;
		}
		
		PVPCfg pvpCfg = null;
		PVPRankEntity rankEntity = playerRankMap.get(player.getId());
		if (rankEntity == null) {
			rankEntity = player.getPlayerData().loadPVPRankData();
			if (rankEntity.getInRank() == false) {
				rankEntity.setInRank(true);
				rankEntity.notifyUpdate(true);
			}
			
			// 进入匹配池
			pvpCfg = PVPCfg.getPVPStageCfg(rankEntity.getPoint());
			if (pvpCfg == null) {
				player.sendError(HS.code.PVP_MATCH_TARGET_C_VALUE, Status.error.CONFIG_NOT_FOUND_VALUE);
				return;
			}
			
			rankEntity.setRank(pvpRankList.size());
			// 进入排行榜
			pvpRankList.add(rankEntity);
			reRank(rankEntity);
			playerRankMap.put(player.getId(), rankEntity);
		}
		
		// 读取配置
		if (pvpCfg == null) {
			pvpCfg = PVPCfg.getPVPStageCfg(rankEntity.getPoint());
			if (pvpCfg == null) {
				player.sendError(HS.code.PVP_MATCH_TARGET_C_VALUE, Status.error.CONFIG_NOT_FOUND_VALUE);
				return;
			}
		}
			
		Map<Integer, PVPDefenceEntity> stageList = pvpPoolList.get(pvpCfg.getStage());
		
		if (!stageList.containsKey(player.getId())) {
			// TODO 检查是否会影响player的释放
			addToPool(stageList, player.getPlayerData().getPVPDefenceEntity(), rankEntity);
		}
		
		// 匹配玩家
		PVPDefenceEntity target = null;
		while (true) {	
			if (GMTargetID != 0) {
				target = getPVPDefenceEntityFromPool(GMTargetID);
				GMTargetID = 0;
				if (target != null) {
					break;
				}
			}
			
			int i = pvpCfg.getStage();
			for (; i > 0; i--) {
				stageList = pvpPoolList.get(i);
				if (stageList != null && (stageList.containsKey(player.getId()) && stageList.size() > 1) || (!stageList.containsKey(player.getId()) && stageList.size() > 0)) {
					break;
				}
			}
			
			if (i == 0 || stageList == null || stageList.size() <= 0) {
				player.sendError(HS.code.PVP_MATCH_TARGET_C_VALUE, Status.pvpError.PVP_NOT_MATCH_TARGET_VALUE);
				return;
			}			
			
			target = stageList.get(randomPlayer(stageList));
			
			if(target.getPlayerId() != player.getId()){
				// 如果没有设置防守阵容 ，从匹配池中移除
				if (target.hasSetDefenceMonsters() == false) {
					stageList.remove(target.getPlayerId());
				}
				else {
					break;
				}
			}
			
			if (((stageList.containsKey(player.getId()) && stageList.size() == 1) || stageList.isEmpty()) && i == 1) {
				player.sendError(HS.code.PVP_MATCH_TARGET_C_VALUE, Status.pvpError.PVP_NOT_MATCH_TARGET_VALUE);
				return;
			}
		}
		
		rankEntity.setPvpCount(rankEntity.getPvpCount() + 1);
		rankEntity.notifyUpdate(true);
		
		if (protocol.getChangeTarget()) {
			PVPRoom pvpRoom = pvpRoomList.get(player.getId());
			if (pvpRoom == null) {
				player.sendError(HS.code.PVP_MATCH_TARGET_C_VALUE, Status.pvpError.PVP_NOT_MATCH_BEFORE_VALUE);
				return;
			}
			
			if (pvpRoom.isEnter == true) {
				player.sendError(HS.code.PVP_MATCH_TARGET_C_VALUE, Status.pvpError.PVP_ENTER_BEFORE_VALUE);
				return;
			}
			
			ConsumeItems consume = new ConsumeItems();
			float coin = GsConst.PVP.PVP_CHANGE_TARGET_K * (pvpRoom.changeTime + 1) * GsConst.PVP.PVP_CHANGE_TARGET_K * (pvpRoom.changeTime + 1) + GsConst.PVP.PVP_CHANGE_TARGET_B;
			consume.addCoin((int) coin);
			consume.consumeTakeAffectAndPush(player, Action.PVP_MATCH, cmd.getType());
			// 加入到房间
			pvpRoomList.put(player.getId(), new PVPRoom(target.getPlayerId(), pvpRoom.changeTime + 1, target));
		}
		else{
			pvpRoomList.put(player.getId(), new PVPRoom(target.getPlayerId(), 0, target));
			//player.consumePVPTime(1, Action.PVP_MATCH);
		}
		
		HSPVPMatchTargetRet.Builder response = HSPVPMatchTargetRet.newBuilder();
		response.setPlayerId(target.getPlayerId());
		response.setName(target.getName());
		response.setLevel(target.getLevel());
		response.setRank(0);
		response.setDefenceData(target.getMonsterDefenceBuilder());
		response.setPvpTime(player.getPlayerData().getStatisticsEntity().getPVPTime());
		response.setPvpTimeBeginTime(player.getPlayerData().getStatisticsEntity().getPVPTimeBeginTime());
		response.setChangeTime(pvpRoomList.get(player.getId()).changeTime);
		player.sendProtocol(HawkProtocol.valueOf(HS.code.PVP_MATCH_TARGET_S_VALUE, response));
	}
	
	/**
	 * pvp 进入房间
	 * @param msg
	 */
	public void onPVPEnterRoom(HawkMsg msg){
		Player player = msg.getParam(0);
		PVPRoom pvpRoom = pvpRoomList.get(player.getId());
		if (pvpRoom == null) {
			player.sendError(HS.code.PVP_SETTLE_C_VALUE, Status.pvpError.PVP_NOT_MATCH_BEFORE_VALUE);
			return;
		}
		
		if (pvpRoom.isEnter == true) {
			player.sendError(HS.code.PVP_SETTLE_C_VALUE, Status.pvpError.PVP_ENTER_BEFORE_VALUE);
			return;
		}
		
		pvpRoom.isEnter = true;
		
		HSPVPEnterRet.Builder response = HSPVPEnterRet.newBuilder();
		player.sendProtocol(HawkProtocol.valueOf(HS.code.PVP_ENTER_ROOM_S_VALUE, response));
	}
	
	/**
	 * pvp结算
	 * @param msg
	 */
	public void onPVPSettle(HawkMsg msg){
		Player player = msg.getParam(0);
		HawkProtocol cmd = msg.getParam(1);
		HSPVPSettle protocol = cmd.parseProtocol(HSPVPSettle.getDefaultInstance());
		
		PVPRoom pvpRoom = pvpRoomList.get(player.getId());
		if (pvpRoom == null) {
			player.sendError(HS.code.PVP_SETTLE_C_VALUE, Status.pvpError.PVP_NOT_MATCH_BEFORE_VALUE);
			return;
		}
		
		if (pvpRoom.isEnter == false) {
			player.sendError(HS.code.PVP_SETTLE_C_VALUE, Status.pvpError.PVP_NOT_ENTER_VALUE);
			return;
		}
		
		pvpRoomList.remove(player.getId());
		
		PVPRankEntity pvpRankEntity = playerRankMap.get(player.getId());
		if (pvpRankEntity == null) {
			player.sendError(HS.code.PVP_SETTLE_C_VALUE, Status.error.SERVER_ERROR_VALUE);
			return;
		}

		// 只涉及到PVP荣誉点  不需要加锁操作
		AwardItems reward = new AwardItems();
		int changePoint = PVPSettle(pvpRankEntity, pvpRoom.targetId, protocol.getResult(), reward);
		
		if (reward.hasAwardItem()) {
			reward.rewardTakeAffectAndPush(player, Action.PVP_SETTLE, cmd.getType());
		}
		
		HSPVPSettleRet.Builder response = HSPVPSettleRet.newBuilder();
		response.setPoint(pvpRankEntity.getPoint());
		response.setRank(pvpRankEntity.getRank() + 1);
		response.setRewardPoint(changePoint);
		player.sendProtocol(HawkProtocol.valueOf(HS.code.PVP_SETTLE_S_VALUE, response));
	}
	
	/**
	 * 内部结算函数
	 * @param pvpRankEntity
	 * @param targetId
	 * @param result
	 * @param reward
	 * @return
	 */
	public int PVPSettle(PVPRankEntity pvpRankEntity, int targetId, int result, AwardItems reward){
		PVPRankEntity targetRankEntity = playerRankMap.get(targetId);
		int selfPoint = pvpRankEntity != null ? pvpRankEntity.getPoint() : GsConst.PVP.PVP_DEFAULT_POINT;
		int targetPoint = targetRankEntity != null ? targetRankEntity.getPoint() : GsConst.PVP.PVP_DEFAULT_POINT;
		
		PVPCfg pvpCfg = PVPCfg.getPVPStageCfg(selfPoint);
		if (pvpCfg == null) {
			return 0;
		}
		
		int changePoint = calculatePVPPoint(selfPoint, targetPoint, pvpCfg, result);
		changePoolStage(pvpRankEntity, changePoint);
		pvpRankEntity.setPoint(pvpRankEntity.getPoint() + changePoint > 0 ? pvpRankEntity.getPoint() + changePoint : 0);
		pvpRankEntity.notifyUpdate(true);
		reRank(pvpRankEntity);
		
		if (result== Const.PvpResult.WIN_VALUE) {
			reward.addHonorPoint(pvpCfg.getWin());
		}
		else if (result == Const.PvpResult.DRAW_VALUE) {
			reward.addHonorPoint(pvpCfg.getDraw());
		}	
		
		PVPDefenceRecordEntity pvpDefenceRecordEntity = new PVPDefenceRecordEntity();
		pvpDefenceRecordEntity.setPlayerId(targetId);
		pvpDefenceRecordEntity.setChangePoint(-changePoint);
		pvpDefenceRecordEntity.setResult(Const.PvpResult.NUM_VALUE - result);
		pvpDefenceRecordEntity.setAttackerPoint(pvpRankEntity.getPoint());
		pvpDefenceRecordEntity.setAttackerName(pvpRankEntity.getName());
		pvpDefenceRecordEntity.setAttackerLevel(pvpRankEntity.getLevel());
		pvpDefenceRecordEntity.notifyCreate();
		
		
		if (GsApp.getInstance().queryPlayer(targetId) != null) {
			HawkMsg recordMsg = HawkMsg.valueOf(GsConst.MsgType.PVP_RECORD, HawkXID.valueOf( GsConst.ObjType.PLAYER, targetId));
	 		recordMsg.pushParam(pvpDefenceRecordEntity);
			HawkApp.getInstance().postMsg(recordMsg);
		}
		
		if (targetRankEntity != null) {
			changePoolStage(targetRankEntity, -changePoint);
			targetRankEntity.setPoint(targetRankEntity.getPoint() - changePoint > 0 ? targetRankEntity.getPoint() - changePoint : 0);
			targetRankEntity.notifyUpdate(true);
			reRank(targetRankEntity);
		}
		
		return changePoint;
	}
	
	/**
	 * pvp排行榜
	 * @param msg
	 */
	public void onPVPRankList(HawkMsg msg){
		Player player = msg.getParam(0);
		player.sendProtocol(HawkProtocol.valueOf(HS.code.PVP_RANK_LIST_S_VALUE, pvpRankBuilder));	
	}
	
	/**
	 * 排行榜中防守阵容
	 * @param msg
	 */
	public void onPVPRankListDecence(HawkMsg msg){
		Player player = msg.getParam(0);
		HawkProtocol cmd = msg.getParam(1);
		HSPVPRankDefence pvpRankDefence = cmd.parseProtocol(HSPVPRankDefence.getDefaultInstance());
		
		PVPDefenceEntity defenceEntity = pvpRankDefenceList.get(pvpRankDefence.getPlayerId());
		if (defenceEntity == null) {
			defenceEntity = getPVPDefenceEntity(pvpRankDefence.getPlayerId(), null, 0, false);
		}
		
		if (defenceEntity == null) {
			player.sendError(cmd.getType(), Status.error.SERVER_ERROR_VALUE);
			return;
		}
		
		pvpRankDefenceList.put(pvpRankDefence.getPlayerId(), defenceEntity);
		
		HSPVPRankDefenceRet.Builder response = HSPVPRankDefenceRet.newBuilder();
		response.setMonsterDefence(defenceEntity.getMonsterDefenceBuilder());
		for (HSMonster.Builder builder : response.getMonsterDefenceBuilder().getMonsterInfoBuilderList()) {
			builder.clearSkill();
			builder.clearEquipInfos();
		}
		
		response.setBp(defenceEntity.getBP());
		player.sendProtocol(HawkProtocol.valueOf(HS.code.PVP_GET_RANK_DEFENCE_S_VALUE, response));
	}
	
	/**
	 * 排行榜之内的玩家需要计算并列情况
	 * @param msg
	 */
	public void onPVPMyInfo(HawkMsg msg){
		Player player = msg.getParam(0);
		int leftTime = msg.getParam(1);
		HSPVPInfoRet.Builder response = HSPVPInfoRet.newBuilder();
		response.setMonthRewardTimeLeft(leftTime);
		PVPRankEntity rankEntity = playerRankMap.get(player.getId());
		if (rankEntity != null) {	
			// 排行榜之内的需要考虑并列情况
			response.setPvpPoint(rankEntity.getPoint());
			int rank = rankEntity.getRank();
			while (rank > 0) {
				if (pvpRankList.get(rank).getPoint() == pvpRankList.get(rank - 1).getPoint()) {
					rank--;
				}
				else {
					break;
				}
			}
			response.setPvpRank(rank + 1);		
		}
		else{
			response.setPvpPoint(GsConst.PVP.PVP_DEFAULT_POINT);
			response.setPvpRank(0);
		}
		
		player.sendProtocol(HawkProtocol.valueOf(HS.code.PVP_GET_MY_INFO_S_VALUE, response));
	}
	
	/**
	 * 退出结算
	 * @param msg
	 */
	public void onPVPLogout(HawkMsg msg){
		Player player = msg.getParam(0);
		PVPRoom pvpRoom = pvpRoomList.get(player.getId());
		if (pvpRoom != null) {
			pvpRoomList.remove(player.getId());
			
			PVPRankEntity pvpRankEntity = playerRankMap.get(player.getId());
			if (pvpRankEntity == null) {
				return;
			}
			
			if (pvpRoom.isEnter == true) {
				AwardItems reward = new AwardItems();
				PVPSettle(pvpRankEntity, pvpRoom.targetId, Const.PvpResult.LOSE_VALUE, reward);
				
				if (reward.hasAwardItem()) {
					reward.rewardTakeAffect(player, Action.PVP_SETTLE);
				}
			}	
		}
	}
	
	/**
	 * pvp 周结算
	 * @param msg
	 */
	public void onPVPWeekReward(HawkMsg msg){
		for (PVPRankEntity rankEntity : pvpRankList) {
			PVPCfg pvpCfg = PVPCfg.getPVPStageCfg(rankEntity.getPoint());
			if (pvpCfg == null) {
				continue;
			}
			
			PVPStageRewardCfg pvpStageRewardCfg = HawkConfigManager.getInstance().getConfigByKey(PVPStageRewardCfg.class, pvpCfg.getStage());
			if (pvpStageRewardCfg == null) {
				continue;
			}
			
			MailSysCfg mailCfg = HawkConfigManager.getInstance().getConfigByKey(MailSysCfg.class, GsConst.SysMail.PVP_WEEK_GRADE_REWARD);
			if (mailCfg != null) {
				MailUtil.SendSysMailWithReward(mailCfg, rankEntity.getPlayerId(), pvpStageRewardCfg.getReward1());
			}
		}
		
		//月结算
		onPVPMonthReward();
	}
	
	/**
	 * pvp 月结算
	 */
	public void onPVPMonthReward(){
		if (ServerData.getInstance().getPVPWeekRewardCount() == 3) {
			ServerData.getInstance().setPVPWeekRewardCoutn(0);
			
			for (PVPRankEntity rankEntity: pvpRankList) {
				PVPRankRewardCfg pvpRankRewardCfg = PVPRankRewardCfg.getPVPRankCfg(rankEntity.getLevel(), rankEntity.getRank() + 1);
				if (pvpRankRewardCfg != null) {
					MailSysCfg mailCfg = HawkConfigManager.getInstance().getConfigByKey(MailSysCfg.class, GsConst.SysMail.PVP_MONTH_RANK_REWARD);
					if (mailCfg != null) {
						MailUtil.SendSysMailWithReward(mailCfg, rankEntity.getPlayerId(), pvpRankRewardCfg.getReward1());
					}
				}
			}
			
			// 新的排行榜  
			Map<Integer, PVPRankEntity> playerRankMapTemp = new HashMap<>();
			List<PVPRankEntity> pvpRankListTemp = new ArrayList<>(5000);
			
			for (PVPRankEntity rankEntity: pvpRankList) {
				if (rankEntity.getPvpCount() == 0) {
					rankEntity.setPoint(GsConst.PVP.PVP_DEFAULT_POINT);
					rankEntity.setInRank(false);
				}
				else {
					rankEntity.setPoint((int) ((rankEntity.getPoint() - GsConst.PVP.PVP_DEFAULT_POINT) / GsConst.PVP.PVP_RERANK_K_VALUE + GsConst.PVP.PVP_DEFAULT_POINT));			
					rankEntity.setInRank(true);
					rankEntity.setRank(pvpRankListTemp.size());
					rankEntity.setPvpCount(0);
					pvpRankListTemp.add(rankEntity);
					playerRankMapTemp.put(rankEntity.getPlayerId(), rankEntity);
				}
				
				rankEntity.notifyUpdate(true);
			}
			
			pvpRankList = pvpRankListTemp;
			playerRankMap = playerRankMapTemp;
			
			generatePVPPool();
		}
		else {
			ServerData.getInstance().setPVPWeekRewardCoutn(ServerData.getInstance().getPVPWeekRewardCount() + 1);
		}
	}
	
	/**
	 * 玩家升级
	 * @param msg
	 */
	public void onPlayerLevelUp(HawkMsg msg){
		Player player = msg.getParam(0);
		
		PVPRankEntity rankEntity = playerRankMap.get(player.getId());
		if (rankEntity != null) {
			rankEntity.setLevel(player.getLevel());
			rankEntity.notifyUpdate(true);
		}
		
		if (player.getPlayerData().getPVPDefenceEntity() != null) {
			player.getPlayerData().getPVPDefenceEntity().setLevel(player.getLevel());
			player.getPlayerData().getPVPDefenceEntity().notifyUpdate(true);
		}
	}
	
	/**
	 * 添加到匹配池
	 * @param stageList
	 * @param defenceEntity
	 * @param rankEntity
	 */
	public void addToPool(Map<Integer, PVPDefenceEntity> stageList, PVPDefenceEntity defenceEntity, PVPRankEntity rankEntity){
		if (stageList.size() >= SysBasicCfg.getInstance().getPvpStageMaxSize()) {
			stageList.remove(randomPlayer(stageList));
			stageList.put(defenceEntity.getPlayerId(), defenceEntity);
		}
		else {
			stageList.put(rankEntity.getPlayerId(), defenceEntity);
		}
	}
	
	/**
	 * 随机出一个玩家
	 * @param stageList
	 * @return
	 */
	public int randomPlayer(Map<Integer, PVPDefenceEntity> stageList){
		if (stageList.size() == 0) {
			return 0;
		}
		
		Object[] playerArray = stageList.keySet().toArray();
		int randomIndex = 0;
		try {
			randomIndex = HawkRand.randInt(0, playerArray.length - 1);
		} catch (Exception e) {
			randomIndex = 0;
		}
		
		return (int)(playerArray[randomIndex]);
	}
	
	/**
	 * 更新匹配池中的位置
	 * @param rankEntity
	 */
	public void changePoolStage(PVPRankEntity rankEntity, int changePoint){	
		PVPCfg currentPvpCfg = PVPCfg.getPVPStageCfg(rankEntity.getPoint());
		if (currentPvpCfg == null) {
			return;
		}
		
		PVPCfg nextPvpCfg = PVPCfg.getPVPStageCfg(rankEntity.getPoint() + changePoint);
		if (currentPvpCfg == nextPvpCfg) {
			return;
		}
		else if (pvpPoolList.get(currentPvpCfg.getStage()).containsKey(rankEntity.getPlayerId())) {
			PVPDefenceEntity defenceEntity = pvpPoolList.get(currentPvpCfg.getStage()).remove(rankEntity.getPlayerId());
			Map<Integer, PVPDefenceEntity> stageList = pvpPoolList.get(nextPvpCfg.getStage());
			if (stageList.size() >= SysBasicCfg.getInstance().getPvpStageMaxSize()) {
				stageList.remove(randomPlayer(stageList));
			}
			stageList.put(defenceEntity.getPlayerId(), defenceEntity);
		}
	}
	
	/**
	 * 重新排名
	 * @param rankEntity
	 */
	public void reRank(PVPRankEntity rankEntity){
		int rank = rankEntity.getRank();
		if (rank > 0 && rankEntity.getPoint() > pvpRankList.get(rank - 1).getPoint()) {
			for (int i = rank - 1; i >= 0; i--) {
				if (pvpRankList.get(i).getPoint() < rankEntity.getPoint()) {
					pvpRankList.get(i).addRank();
					pvpRankList.set(i + 1, pvpRankList.get(i));
				}
				else {
					pvpRankList.set(i + 1, rankEntity);
					rankEntity.setRank(i + 1);
					break;
				}
				
				if (i == 0) {
					pvpRankList.set(0, rankEntity);
					rankEntity.setRank(0);
				}
			}
		}
		else if (rank < pvpRankList.size() - 1 && rankEntity.getPoint() < pvpRankList.get(rank + 1).getPoint()){
			for (int i = rank + 1; i < pvpRankList.size(); i++) {
				if (pvpRankList.get(i).getPoint() > rankEntity.getPoint()) {
					pvpRankList.get(i).removeRank();
					pvpRankList.set(i - 1, pvpRankList.get(i));
				}
				else {
					pvpRankList.set(i - 1, rankEntity);
					rankEntity.setRank(i - 1);
					break;
				}
				
				if (i == pvpRankList.size() - 1) {
					pvpRankList.set(pvpRankList.size() - 1, rankEntity);
					rankEntity.setRank(pvpRankList.size() - 1);
				}
			}
		}
	}
	
	/**
	 * ELO 算法计算得分
	 * @param point
	 * @param targetPoint
	 * @param pvpCfg
	 * @param result
	 * @return
	 */
	public int calculatePVPPoint(int point, int targetPoint, PVPCfg pvpCfg, int result){
		double finalResult = 0;
		if (result == Const.PvpResult.WIN_VALUE) {
			finalResult = pvpCfg.getK() * (1 - (1.0 / (1 + Math.pow(10, (targetPoint - point) / 400.0))));
		}
		else if (result == Const.PvpResult.LOSE_VALUE) {
			finalResult = pvpCfg.getK() * (0.0f - (1.0 / (1 + Math.pow(10, (targetPoint - point) / 400.0))));
		}
		else if (result == Const.PvpResult.DRAW_VALUE) {
			finalResult = pvpCfg.getK() * (0.5 - (1.0 / (1 + Math.pow(10, (targetPoint - point) / 400.0))));
		}
		
		return (int)finalResult;
	}
	
	/**
	 * 从匹配池中获取玩家防守数据
	 * @param playerId
	 * @return
	 */
	public PVPDefenceEntity getPVPDefenceEntityFromPool(int playerId){
		PVPDefenceEntity pvpDefenceEntity = null;
		PVPRankEntity rankEntity = playerRankMap.get(playerId);
		if (rankEntity != null) {
			PVPCfg pvpCfg = PVPCfg.getPVPStageCfg(rankEntity.getPoint());
			pvpDefenceEntity = pvpPoolList.get(pvpCfg.getStage()).get(playerId);
		}
		
		return pvpDefenceEntity;
	}
	
	/**
	 * 获取pvp防守数据
	 */
	public synchronized PVPDefenceEntity getPVPDefenceEntity(int playerId, String nickName, int level, boolean generateNew){
		PVPDefenceEntity pvpDefenceEntity = getPVPDefenceEntityFromPool(playerId);
		if (pvpDefenceEntity != null) {
			return pvpDefenceEntity;
		}
		
		Player player = GsApp.getInstance().queryPlayer(playerId);
		if (player != null) {
			pvpDefenceEntity = player.getPlayerData().getPVPDefenceEntity();
		}
		
		if (pvpDefenceEntity == null) {
			List<PVPDefenceEntity> resultList = HawkDBManager.getInstance().query("from PVPDefenceEntity where playerId = ? and invalid = 0", playerId);
			if (resultList != null && resultList.size() > 0) {
				pvpDefenceEntity = resultList.get(0);
				pvpDefenceEntity.decode();
			}
			
			if (generateNew && pvpDefenceEntity == null) {
				pvpDefenceEntity = new PVPDefenceEntity();
				pvpDefenceEntity.setPlayerId(playerId);
				pvpDefenceEntity.setName(nickName);
				pvpDefenceEntity.setLevel(level);
				pvpDefenceEntity.setMonsterDefenceBuilder(HSMonsterDefence.newBuilder());
				pvpDefenceEntity.notifyCreate();
			}
		}
		
		return pvpDefenceEntity;
	}
	
	/**
	 * 获取pvp排名数据
	 * @param playerId
	 * @return
	 */
	public PVPRankEntity getPVPRankEntity(int playerId){
		return playerRankMap.get(playerId); 
	}
	
	@Override
	public boolean onTick(long tickTime) {
		if (refreshTime + SysBasicCfg.getInstance().getPvpRankBuilderRefreshTime() * 1000 < tickTime) {
			refreshTime = tickTime;
			//HawkLog.logPrintln("排行榜:");
			//for (PVPRankEntity rankEntity : pvpRankList) {
			//	HawkLog.logPrintln(rankEntity.getName() + " " + rankEntity.getPlayerId() + " " + rankEntity.getRank() + " " + rankEntity.getPoint());
			//}
			
			//HawkLog.logPrintln("匹配池:");
			//for (Map.Entry<Integer, HashMap<Integer, PVPDefenceEntity>> stageList : pvpPoolList.entrySet()) {
			//	HawkLog.logPrintln("stage: " + stageList.getKey());
			//	for (PVPDefenceEntity pvpDefenceEntity : stageList.getValue().values()) {
			//		HawkLog.logPrintln(pvpDefenceEntity.getName() + " " + pvpDefenceEntity.getPlayerId() + " ");
			//	}
			//}
			generatePVPRankBuilder();
		}
	
		return true;
	}
}
