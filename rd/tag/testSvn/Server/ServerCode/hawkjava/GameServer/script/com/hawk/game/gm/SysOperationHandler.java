package com.hawk.game.gm;

import org.hawk.os.HawkException;
import org.hawk.script.HawkScript;
import org.hawk.script.HawkScriptManager;

import com.sun.net.httpserver.HttpExchange;

/**
 * 配置重新加载
 * 
 * @author hawk
 */
public class SysOperationHandler extends HawkScript {
	@Override
	public void action(String params, HttpExchange httpExchange) {
		try {
			HawkScriptManager.sendResponse(httpExchange, "{\"status\":1}");
		} catch (Exception e) {
			HawkException.catchException(e);
		}
	}
}