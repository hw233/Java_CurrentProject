package com.hawk.game.util;

import org.hawk.config.HawkConfigManager;
import org.hawk.log.HawkLog;

import com.hawk.game.config.ItemCfg;
import com.hawk.game.protocol.Const;

/**
 * 配置检查
 * 
 * 
 */
public class ConfigUtil {

	/**
	 * 检测itemType的itemId 
	 * 
	 * @param itemType
	 * @param itemId
	 * @return
	 */
	public static boolean check(int itemType, String itemId) {
		if (itemType == Const.itemType.PLAYER_ATTR_VALUE) {
			return true;
		}
		else if (itemType == Const.itemType.ITEM_VALUE || itemType == Const.itemType.EQUIP_VALUE) {
			if (HawkConfigManager.getInstance().getConfigByKey(ItemCfg.class, itemId) == null) {
				HawkLog.errPrintln("item config not found, itemId: " + itemId);
				return false;
			}
			return true;
		}
		return false;
	}
}
