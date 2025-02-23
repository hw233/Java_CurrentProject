package org.hawk.util.services;

import java.util.Map;

import org.apache.http.HttpResponse;
import org.apache.http.HttpStatus;
import org.apache.http.client.HttpClient;
import org.apache.http.client.config.RequestConfig;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.utils.URIBuilder;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;
import org.hawk.log.HawkLog;
import org.hawk.os.HawkException;
import org.hawk.os.HawkOSOperator;

public class HawkCdkService {
	/**
	 * 前端访问状态码
	 */
	public static final int CDK_STATUS_OK = 0; // 可使用
	public static final int CDK_STATUS_NONEXIST = 1; // 不存在
	public static final int CDK_STATUS_FUTURE = 2; // 时间未到
	public static final int CDK_STATUS_PAST = 3; // 已过期
	public static final int CDK_STATUS_USED = 4; // 已使用
	public static final int CDK_STATUS_PLAT = 5; // 平台不正确
	public static final int CDK_STATUS_TYPE_MULTI = 6; // 已使用过相关类型
	public static final int CDK_STATUS_NOSERVICE = 7; // 服务关闭

	/**
	 * 后台操作错误码
	 */
	public static final int CDK_TYPE_EXIST = 10; // CDK类型已存在
	public static final int CDK_PARAM_ERROR = 11; // 参数错误
	public static final int CDK_TYPE_NONEXIST = 12; // 类型不存在

	/**
	 * 常量定义
	 */
	public static final int CDK_TOTAL_LENGTH = 16; // CDK长度
	public static final int CDK_DIGIT_MIN_COUNT = 3; // 数字最少个数
	public static final int CDK_CHAR_MIN_COUNT = 3; // 字符最少个数
	public static final int CDK_HEADER_SIZE = 2; // cdk标记字节
	public static final int CDK_NAMT_TYPE_LEN = 8; // 名字和类型占用的长度

	/**
	 * 服务器信息
	 */
	private String gameName = "";
	private String platform = "";
	private String channel = "";
	private int serverId = 0;
	private String token = "";

	/**
	 * http对象
	 */
	private HttpClient httpClient = null;
	private HttpGet httpGet = null;
	private URIBuilder uriBuilder = null;

	/**
	 * cdk服务参数串格式
	 */
	private static final String usePath = "/use_cdk";
	private static final String useQuery = "game=%s&platform=%s&server=%s&playerid=%d&puid=%s&playername=%s&cdk=%s";

	/**
	 * 服务是否可用
	 */
	private boolean serviceEnable = true;

	/**
	 * 实例对象
	 */
	private static HawkCdkService instance = null;

	/**
	 * 获取全局实例对象
	 * 
	 * @return
	 */
	public static HawkCdkService getInstance() {
		if (instance == null) {
			instance = new HawkCdkService();
		}
		return instance;
	}

	/**
	 * 构造函数
	 */
	private HawkCdkService() {
		httpClient = null;
		httpGet = null;
		uriBuilder = null;
	}

	/**
	 * 初始化cdk服务
	 * 
	 * @return
	 */
	public boolean install(String gameName, String platform, String channel, int serverId, String host, int timeout) {
		try {
			this.gameName = gameName;
			this.platform = platform;
			this.channel = channel;
			this.serverId = serverId;
			
			if (httpClient == null) {
				RequestConfig requestConfig = RequestConfig.custom()
						.setConnectTimeout(timeout)
						.setSocketTimeout(timeout)
						.build();
				httpClient = HttpClients.custom()
						.setDefaultRequestConfig(requestConfig)
						.build();
			}

			if (httpGet == null) {
				httpGet = new HttpGet();
			}

			if (uriBuilder == null) {
				uriBuilder = new URIBuilder(host);
			}
		} catch (Exception e) {
			HawkException.catchException(e);
		}

		if (httpClient == null || httpGet == null || uriBuilder == null) {
			HawkLog.errPrintln("install cdk service failed.");
			return false;
		}

		return true;
	}

	/**
	 * 设置校验令牌
	 * 
	 * @param token
	 */
	public void setToken(String token) {
		this.token = token;
	}

	/**
	 * 从cdk读取游戏名
	 * 
	 * @param cdk
	 * @return
	 */
	public static String getGameNameFromCdk(String cdk) {
		if (cdk.length() == CDK_TOTAL_LENGTH) {
			char[] cdkGameName = { cdk.charAt(1), cdk.charAt(3) };
			return String.valueOf(cdkGameName);
		}
		return "";
	}

	/**
	 * 从cdk读取游戏名
	 * 
	 * @param cdk
	 * @return
	 */
	public static String getTypeNameFromCdk(String cdk) {
		if (cdk.length() == CDK_TOTAL_LENGTH) {
			char[] cdkTypeName = { cdk.charAt(5), cdk.charAt(7) };
			return String.valueOf(cdkTypeName);
		}
		return "";
	}

	/**
	 * 开启或关闭服务
	 * 
	 * @param enable
	 */
	public void enableService(boolean enable) {
		this.serviceEnable = enable;
	}

	/**
	 * 使用CDK, 外部需要先判断是否已使用过同类型的CDK
	 * 
	 * @param cdk
	 * @return status {0: ok}
	 */
	@SuppressWarnings({ "unchecked", "unused" })
	public synchronized int useCdk(String puid, int playerid, String playername, String cdk, StringBuilder rewardRef) {
		// 服务示范可用
		if (!serviceEnable) {
			return CDK_STATUS_NOSERVICE;
		}

		// 长度校验不正确
		if (cdk.length() != CDK_TOTAL_LENGTH) {
			return CDK_STATUS_NONEXIST;
		}

		// 名字校验不正确
		String cdkGameName = getGameNameFromCdk(cdk);
		String cdkTypeName = getTypeNameFromCdk(cdk);
		if (!gameName.startsWith(cdkGameName)) {
			return CDK_STATUS_NONEXIST;
		}

		if (httpClient != null && httpGet != null && uriBuilder != null && cdk != null && cdk.length() == CDK_TOTAL_LENGTH) {
			String queryParam = String.format(useQuery, gameName, platform, serverId, playerid, puid, playername, cdk);

			// 添加令牌校验
			if (token != null && token.length() > 0) {
				queryParam += "&token" + token;
			}

			try {
				uriBuilder.setPath(usePath);
				uriBuilder.setCustomQuery(queryParam);
				httpGet.setURI(uriBuilder.build());
				HttpResponse httpResponse = httpClient.execute(httpGet);
				int statusCode =  httpResponse.getStatusLine().getStatusCode();
				if (statusCode == HttpStatus.SC_OK) {
					String response = EntityUtils.toString(httpResponse.getEntity());
					
					Map<String, String> respMap = HawkOSOperator.jsonToMap(response);
					int status = CDK_STATUS_NONEXIST;
					if (respMap.containsKey("status")) {
						status = Integer.valueOf(respMap.get("status"));
					}

					if (status == CDK_STATUS_OK && respMap.containsKey("reward")) {
						rewardRef.delete(0, rewardRef.length()).append(respMap.get("reward"));
					}
					return status;
				}
			} catch (Exception e) {
				HawkException.catchException(e);
			}
		}
		return CDK_STATUS_NONEXIST;
	}
}
