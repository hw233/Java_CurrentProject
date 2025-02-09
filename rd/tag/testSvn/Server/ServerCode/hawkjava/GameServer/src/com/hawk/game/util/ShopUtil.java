package com.hawk.game.util;

import java.util.List;

import org.hawk.config.HawkConfigManager;
import org.hawk.os.HawkTime;
import com.hawk.game.config.SellItemCfg;
import com.hawk.game.config.ShopCfg;
import com.hawk.game.entity.ShopEntity;
import com.hawk.game.item.ShopItemInfo;
import com.hawk.game.player.Player;
import com.hawk.game.protocol.Const;
import com.hawk.game.protocol.Shop.ShopData;
import com.hawk.game.protocol.Shop.ShopItem;

public class ShopUtil {
	/**
	 * 刷新物品列表
	 * 
	 * @param player
	 * @return
	 */
	public static List<ShopItemInfo> getPlayerShopItems(Player player, int shopType) {
		int level = player.getLevel();
		ShopCfg shopcfg = ShopCfg.getShopCfg(shopType, level);
		if (shopcfg == null) {
			throw new NullPointerException("shop cfg not found");
		}
		
		SellItemCfg sellItemCfg = HawkConfigManager.getInstance().getConfigByKey(SellItemCfg.class, shopcfg.getSellItemId());
		if (sellItemCfg == null) {
			throw new NullPointerException("sellItem cfg not found");
		}
		
		return sellItemCfg.getItemList();
	}
	
	public static ShopData.Builder generateShopData(Player player, int shopType) {
		ShopData.Builder response = ShopData.newBuilder();
		ShopEntity shopEntity = player.getPlayerData().getShopEntity();
		ShopCfg shopCfg = ShopCfg.getShopCfg(shopType, player.getLevel());
		if (shopCfg == null ) {
			return response;
		}
		response.setType(shopType);
		response.setShopId(shopEntity.getShopId(shopType));
		response.setRefreshTimesLeft(shopCfg.getRefreshMaxNumByHand() - shopEntity.getShopRefreshNum(shopType));
		for (ShopItemInfo element : shopEntity.getShopItemsList(shopType)) {
			ShopItem.Builder shopItem = ShopItem.newBuilder();
			shopItem.setType(element.getType());
			shopItem.setItemId(element.getItemId());
			shopItem.setCount(element.getCount());
			if (element.getType() == Const.itemType.EQUIP_VALUE) {
				shopItem.setStage(element.getStage());
				shopItem.setLevel(element.getLevel());
			}
			shopItem.setPrice(element.getPrice());
			shopItem.setPriceType(element.getPriceType());
			shopItem.setDiscount(element.getDiscount());
			shopItem.setSlot(element.getSlot());
			shopItem.setHasBuy(element.isHasBuy());
			response.addItemInfos(shopItem);
		}
		
		return response;
	}
	
	public static void refreshShopData(Player player){
		refreshShopData(Const.shopType.NORMALSHOP_VALUE, player);
		refreshShopData(Const.shopType.ALLIANCESHOP_VALUE, player);
		refreshShopData(Const.shopType.TOWERSHOP_VALUE, player);
	}
	
	public static void refreshShopData(int type, Player player){
		ShopEntity shopEntity = player.getPlayerData().getShopEntity();
		List<ShopItemInfo> shopItemList = ShopUtil.getPlayerShopItems(player, type);
		for (int i = 0; i < shopItemList.size(); i++) {
			shopItemList.get(i).setSlot(i);
			shopItemList.get(i).setHasBuy(false);
			if (type == Const.shopType.ALLIANCESHOP_VALUE) {
				shopItemList.get(i).setPriceType(Const.moneyType.ALLIANCE_COTRIBUTION_VALUE);
			}
			else if (type == Const.shopType.TOWERSHOP_VALUE){
				shopItemList.get(i).setPriceType(Const.moneyType.TOWER_COIN_VALUE);
			}
		}
		shopEntity.increaseShopId(type);
		shopEntity.setRefreshDate(type, HawkTime.getSeconds());
		shopEntity.setShopItemsList(type, shopItemList);
		shopEntity.notifyUpdate(true);
	}
}
