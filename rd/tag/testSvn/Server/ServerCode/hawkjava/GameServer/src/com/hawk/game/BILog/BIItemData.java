package com.hawk.game.BILog;

import com.hawk.game.BILog.BIBehaviorAction.Action;
import com.hawk.game.config.ItemCfg;
import com.hawk.game.player.Player;
import com.hawk.game.protocol.Const;

public class BIItemData extends BIData{
	/**
	 * 
	 * @param player
	 * @param action
	 * @param itemId
	 * @param stage
	 * @param count
	 * @param itemCfg
	 */
	public void log(Player player, Action action, ItemCfg itemCfg, int stage, int itemIn, int itemOut) {	
		logBegin(player, "item_transaction");
		jsonPropertyData.put("item_id", itemCfg.getId());
		jsonPropertyData.put("item_name", itemCfg.getName());
		if (itemCfg.getType() == Const.toolType.EQUIPTOOL_VALUE) {
			jsonPropertyData.put("item_stage", stage);
			jsonPropertyData.put("item_type", "equip");
		}
		else if (itemCfg.getType() == Const.toolType.GEMTOOL_VALUE) {
			jsonPropertyData.put("item_type", "chip");
		}
		else if (itemCfg.getType() == Const.toolType.BOXTOOL_VALUE) {
			jsonPropertyData.put("item_type", "box");
		}
		else {
			jsonPropertyData.put("item_type", "item");
		}
		
		jsonPropertyData.put("action", action.getBICode());
		jsonPropertyData.put("item_out", itemOut);
		jsonPropertyData.put("item_in", itemIn);
		logEnd();
	}
}
