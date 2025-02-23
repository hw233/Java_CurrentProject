package com.hawk.game.module.alliance;

import org.hawk.app.HawkAppObj;
import org.hawk.config.HawkConfigManager;
import org.hawk.msg.HawkMsg;
import org.hawk.msg.HawkMsgHandler;
import org.hawk.net.protocol.HawkProtocol;

import com.hawk.game.ServerData;
import com.hawk.game.BILog.BIGuildMemberFlowData;
import com.hawk.game.BILog.BIBehaviorAction.Action;
import com.hawk.game.config.ImSysCfg;
import com.hawk.game.entity.AllianceEntity;
import com.hawk.game.entity.PlayerAllianceEntity;
import com.hawk.game.log.BILogger;
import com.hawk.game.manager.AllianceManager;
import com.hawk.game.manager.ImManager;
import com.hawk.game.player.Player;
import com.hawk.game.protocol.Alliance.HSAllianceChangePos;
import com.hawk.game.protocol.Alliance.HSAllianceChangePosRet;
import com.hawk.game.protocol.HS;
import com.hawk.game.protocol.Status;
import com.hawk.game.util.AllianceUtil;
import com.hawk.game.util.GsConst;

public class AllianceChangePosHandler implements HawkMsgHandler{
	/**
	 * 消息处理器
	 * 
	 * @param appObj
	 * @param msg
	 * @return
	 */
	@Override
	public boolean onMessage(HawkAppObj appObj, HawkMsg msg)
	{
		Player player = (Player) msg.getParam(0);
		HawkProtocol protocol = (HawkProtocol)msg.getParam(1);
		HSAllianceChangePos request = protocol.parseProtocol(HSAllianceChangePos.getDefaultInstance());
		
		if (player.getAllianceId() == 0) {
			player.sendError(protocol.getType(), Status.allianceError.ALLIANCE_NOT_JOIN_VALUE);
			return true;
		}
		
		AllianceEntity allianceEntity = AllianceManager.getInstance().getAlliance(player.getAllianceId());
		if (allianceEntity == null) {
			player.sendError(protocol.getType(), Status.allianceError.ALLIANCE_NOT_EXIST_VALUE);
			return true;
		}
		
		PlayerAllianceEntity playerAllianceEntity = allianceEntity.getMember(player.getId());
		if (playerAllianceEntity == null) {
			player.sendError(protocol.getType(), Status.error.SERVER_ERROR_VALUE);
			return true;
		}
		
		if (playerAllianceEntity.getPostion() != GsConst.Alliance.ALLIANCE_POS_MAIN) {
			player.sendError(protocol.getType(), Status.allianceError.ALLIANCE_POSITION_ERROR_VALUE);
			return true;
		}
		
		PlayerAllianceEntity targetPlayerAllianceEntity = allianceEntity.getMember(request.getPlayerId());
		if (targetPlayerAllianceEntity == null) {
			player.sendError(protocol.getType(), Status.allianceError.ALLIANCE_TARGET_NOT_JOIN_VALUE);
			return true;
		}
		
		if (request.getPostion() != GsConst.Alliance.ALLIANCE_POS_COMMON && request.getPostion() != GsConst.Alliance.ALLIANCE_POS_COPYMAIN) {
			player.sendError(protocol.getType(), Status.allianceError.ALLIANCE_POSITION_ERROR_VALUE);
			return true;
		}
		
		if (targetPlayerAllianceEntity.getPostion() == request.getPostion()) {
			player.sendError(protocol.getType(), Status.allianceError.ALLIANCE_SAME_POSITION_VALUE);
			return true;
		}
		
		if (request.getPostion() == GsConst.Alliance.ALLIANCE_POS_COPYMAIN && AllianceUtil.getCopyMainCount(allianceEntity) >= GsConst.Alliance.MAX_COPYMAIN_COUNT) {
			player.sendError(protocol.getType(), Status.allianceError.ALLIANCE_MAX_COPY_MAIN_VALUE);
			return true;
		}

		if (request.getPostion() == GsConst.Alliance.ALLIANCE_POS_COPYMAIN) {
			ImSysCfg imCfg = HawkConfigManager.getInstance().getConfigByKey(ImSysCfg.class, GsConst.SysIm.ALLIANCE_ELDER_PROMOTE);
			if (imCfg != null) {
				ImManager.getInstance().postSys(imCfg, allianceEntity.getId(), targetPlayerAllianceEntity.getName());
			}
		}
		else if(targetPlayerAllianceEntity.getPostion() == GsConst.Alliance.ALLIANCE_POS_COPYMAIN){
			ImSysCfg imCfg = HawkConfigManager.getInstance().getConfigByKey(ImSysCfg.class, GsConst.SysIm.ALLIANCE_ELDER_DEMOTE);
			if (imCfg != null) {
				ImManager.getInstance().postSys(imCfg, allianceEntity.getId(), targetPlayerAllianceEntity.getName());
			}
		}

		targetPlayerAllianceEntity.setPostion(request.getPostion());
		targetPlayerAllianceEntity.notifyUpdate(true);
		
		player.sendProtocol(HawkProtocol.valueOf(HS.code.ALLIANCE_CHANGE_POS_S_VALUE, HSAllianceChangePosRet.newBuilder()));
		
		BILogger.getBIData(BIGuildMemberFlowData.class).logOperation(
				player, 
				request.getPostion() == GsConst.Alliance.ALLIANCE_POS_COPYMAIN ? Action.GUILD_COPY_MAIN_ADD : Action.GUILD_COPY_MAIN_REMOVE, 
				allianceEntity,
				playerAllianceEntity, 
				ServerData.getInstance().getPuidByPlayerId(targetPlayerAllianceEntity.getPlayerId())
				);
		
		// 广播
		//HSMemberPosChangeNotify.Builder builder = HSMemberPosChangeNotify.newBuilder();
		//builder.setPlayerId(request.getPlayerId());
		//builder.setNewPos(request.getPostion());
		//AllianceManager.getInstance().broadcastNotify(allianceEntity.getId(), HawkProtocol.valueOf(HS.code.ALLIANCE_CHANGE_POS_N_S_VALUE, builder), player.getId());

		return true;
	}
}
