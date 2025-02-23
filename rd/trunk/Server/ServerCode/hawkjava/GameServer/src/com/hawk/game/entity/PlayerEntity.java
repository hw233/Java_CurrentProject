package com.hawk.game.entity;

import java.util.Calendar;
import java.util.LinkedList;
import java.util.List;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.Id;
import javax.persistence.Table;
import javax.persistence.Transient;

import org.hawk.db.HawkDBEntity;
import org.hawk.os.HawkTime;
import org.hawk.util.HawkJsonUtil;
import org.hibernate.annotations.GenericGenerator;

import com.google.gson.reflect.TypeToken;
import com.hawk.game.util.GsConst;

/**
 * 玩家基础数据
 * 
 * @author hawk
 * 
 */
@Entity
@Table(name = "player")
public class PlayerEntity extends HawkDBEntity {
	@Id
	@GenericGenerator(name = "AUTO_INCREMENT", strategy = "native")
	@GeneratedValue(generator = "AUTO_INCREMENT")
	@Column(name = "id", unique = true)
	private int id = 0;

	@Column(name = "puid", unique = true, nullable = false)
	protected String puid = "";

	@Column(name = "nickname", nullable = false)
	protected String nickname = GsConst.UNCOMPLETE_NICKNAME;

	@Column(name = "portrait", nullable = false)
	protected int portrait = 0;

	@Column(name = "recharge", nullable = false)
	protected int recharge = 0;

	@Column(name = "vipLevel", nullable = false)
	protected int vipLevel = 0;

	@Column(name = "level", nullable = false)
	protected short level = 1;

	@Column(name = "exp", nullable = false)
	protected int exp = 0;

	@Column(name = "honorPoint", nullable = false)
	protected int honorPoint = 0;

	@Column(name = "goldBuy", nullable = false)
	protected int goldBuy = 0;

	@Column(name = "goldFree", nullable = false)
	protected int goldFree = 0;

	@Column(name = "coin", nullable = false)
	protected long coin = 0;

	@Column(name = "coinTower", nullable = false)
	protected int coinTower = 0;

	@Column(name = "coinArena", nullable = false)
	protected int coinArena = 0;

	@Column(name = "blockPlayer", nullable = false)
	protected String blockPlayerJson = "";

	@Column(name = "language", nullable = false)
	protected String language = GsConst.DEFAULT_LANGUAGE;

	@Column(name = "device", nullable = false)
	protected String device = "";

	@Column(name = "platform", nullable = false)
	protected String platform = "";

	@Column(name = "phoneType", nullable = false)
	protected String phoneType = "";

	@Column(name = "osVersion", nullable = false)
	protected String osVersion = "";

	@Column(name = "osName", nullable = false)
	protected String osName = "";

	// 锁定时间
	@Column(name = "lockTime", nullable = false)
	protected int lockTime = 0;

	@Column(name = "loginTime")
	protected Calendar loginTime = null;

	@Column(name = "logoutTime")
	protected Calendar logoutTime = null;

	@Column(name = "resetTime")
	protected Calendar resetTime = null;

	@Column(name = "createTime", nullable = false)
	protected int createTime = 0;

	@Column(name = "updateTime")
	protected int updateTime;

	@Column(name = "invalid", nullable = false)
	protected boolean invalid;

	@Transient
	private List<Integer> blockPlayerList = new LinkedList<Integer>();

	public PlayerEntity() {
		this.loginTime = HawkTime.getCalendar();
	}

	public PlayerEntity(String puid){
		this.puid = puid;
		this.loginTime = HawkTime.getCalendar();
	}

	public PlayerEntity(String puid, String nickname){
		this.puid = puid;
		this.nickname = nickname;
		this.loginTime = HawkTime.getCalendar();
	}

	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public String getPuid() {
		return puid;
	}

	public void setPuid(String puid) {
		this.puid = puid;
	}

	public String getNickname() {
		return this.nickname;
	}

	public void setNickname(String nickname) {
		this.nickname = nickname;
	}

	public int getPortrait() {
		return portrait;
	}

	public void setPortrait(int portrait) {
		this.portrait = portrait;
	}

	public short getLevel() {
		return level;
	}

	public void setLevel(int level) {
		this.level = (short)level;
	}

	public int getExp() {
		return exp;
	}

	public void setExp(int exp) {
		this.exp = exp;
	}

	public int getHonorPoint() {
		return honorPoint;
	}

	public void addHonorPoint(int honorPoint) {
		this.honorPoint += honorPoint;
	}

	public long getCoin() {
		return coin;
	}

	public void setCoin(long coin) {
		this.coin = coin;
	}

	public int getBuyGold() {
		return goldBuy;
	}

	public int getFreeGold() {
		return goldFree;
	}

	public void setBuyGold(int gold) {
		this.goldBuy = gold;
	}

	public void setFreeGold(int gold) {
		this.goldFree = gold;
	}

	public int getTowerCoin() {
		return coinTower;
	}

	public void setTowerCoin(int towerCoin) {
		this.coinTower = towerCoin;
	}

	public int getArenaCoin() {
		return coinArena;
	}

	public void setArenaCoin(int arenaCoin) {
		this.coinArena = arenaCoin;
	}

	public List<Integer> getBlockPlayerList() {
		return blockPlayerList;
	}

	public void addBlockPlayer(int playerId) {
		if (false == blockPlayerList.contains(playerId)) {
			blockPlayerList.add(playerId);
		}
	}

	public void removeBlockPlayer(int playerId) {
		blockPlayerList.remove(Integer.valueOf(playerId));
	}

	public String getLanguage() {
		return language;
	}

	public void setLanguage(String language) {
		this.language = language;
	}

	public int getRecharge() {
		return recharge;
	}

	public void setRecharge(int recharge) {
		this.recharge = recharge;
	}

	public int getVipLevel() {
		return vipLevel;
	}

	public void setVipLevel(int vipLevel) {
		this.vipLevel = vipLevel;
	}

	public String getDevice() {
		return device;
	}

	public void setDevice(String device) {
		this.device = device;
	}

	public String getPlatform() {
		return platform;
	}

	public void setPlatform(String platform) {
		this.platform = platform;
	}

	public String getPhoneType() {
		return phoneType;
	}

	public void setPhoneType(String phoneType) {
		this.phoneType = phoneType;
	}

	public String getOsVersion() {
		return osVersion;
	}

	public void setOsVersion(String osVersion) {
		this.osVersion = osVersion;
	}

	public String getOsName() {
		return osName;
	}

	public void setOsName(String osName) {
		this.osName = osName;
	}

	public int getLockTime() {
		return lockTime;
	}

	public void setLockTime(int lockTime) {
		this.lockTime = lockTime;
	}

	public Calendar getLoginTime() {
		return loginTime;
	}

	public void setLoginTime(Calendar loginTime) {
		this.loginTime = loginTime;
	}

	public Calendar getLogoutTime() {
		return logoutTime;
	}

	public void setLogoutTime(Calendar logoutTime) {
		this.logoutTime = logoutTime;
	}

	public Calendar getResetTime() {
		return resetTime;
	}

	public void setResetTime(Calendar resetTime) {
		this.resetTime = resetTime;
	}

	@Override
	public boolean decode() {
		if (blockPlayerJson != null && false == "".equals(blockPlayerJson) && false == "null".equals(blockPlayerJson)) {
			blockPlayerList = HawkJsonUtil.getJsonInstance().fromJson(blockPlayerJson, new TypeToken<List<Integer>>() {}.getType());
		}

		return true;
	}

	@Override
	public boolean encode() {
		blockPlayerJson = HawkJsonUtil.getJsonInstance().toJson(blockPlayerList);
		return true;
	}

	@Override
	public int getCreateTime() {
		return createTime;
	}

	@Override
	public void setCreateTime(int createTime) {
		this.createTime = createTime;
	}

	@Override
	public int getUpdateTime() {
		return updateTime;
	}

	@Override
	public void setUpdateTime(int updateTime) {
		this.updateTime = updateTime;
	}

	@Override
	public boolean isInvalid() {
		return invalid;
	}

	@Override
	public void setInvalid(boolean invalid) {
		this.invalid = invalid;
	}
}
