package org.hawk.thread;

import java.util.List;
import java.util.ArrayList;
import java.util.concurrent.atomic.AtomicLong;

import org.hawk.nativeapi.HawkNativeApi;
import org.hawk.os.HawkException;

/**
 * 线程池封装
 * 
 * @author hawk
 * 
 */
public class HawkThreadPool {
	/**
	 * 线程数目
	 */
	protected int threadNum;
	/**
	 * 池名称
	 */
	protected String poolName;
	/**
	 * 线程队列
	 */
	protected List<HawkThread> threadList;
	/**
	 * 运行中
	 */
	protected volatile boolean running;
	/**
	 * 等待退出循环
	 */
	protected volatile boolean waitBreak;
	/**
	 * 当前轮换索引
	 */
	protected AtomicLong turnIndex;

	/**
	 * 线程池构造
	 */
	public HawkThreadPool(String poolName) {
		this.running = false;
		this.waitBreak = false;		
		this.turnIndex = new AtomicLong(0);
		this.threadList = new ArrayList<HawkThread>();
		this.poolName = poolName;
	}

	/**
	 * 初始化(poolSize表示线程数)
	 * 
	 * @param poolSize
	 * @return
	 */
	public boolean initPool(int poolSize) {
		// 检测
		if (!HawkNativeApi.checkHawk()) {
			return false;
		}
		
		threadNum = poolSize;
		for (int i = 0; i < threadNum; i++) {
			HawkThread thread = new HawkThread();
			thread.setName(poolName + "-" + thread.getId());
			threadList.add(thread);
		}
		return true;
	}

	/**
	 * 初始化(poolSize表示线程数)
	 * 
	 * @param poolSize
	 * @return
	 */
	public boolean initPool(int poolSize, boolean start) {
		// 检测
		if (!initPool(poolSize)) {
			return false;
		}
		// 开始
		if (start && !start()) {
			return false;
		}
		return true;
	}
	
	/**
	 * 添加执行任务(threadIdx指定线程执行)
	 * 
	 * @param task
	 * @return
	 */
	public boolean addTask(HawkTask task) {
		return addTask(task, -1, false);
	}

	/**
	 * 添加执行任务(threadIdx指定线程执行)
	 * 
	 * @param task
	 * @param threadIdx
	 * @param first
	 * @return
	 */
	public boolean addTask(HawkTask task, int threadIdx, boolean first) {
		if (running && !waitBreak) {
			if (threadIdx < 0) {
				threadIdx = (int) (turnIndex.incrementAndGet() % threadNum);
			} else {
				threadIdx = threadIdx % threadNum;
			}

			if (threadIdx >= 0 && threadIdx < threadNum) {
				if (first) {
					return threadList.get(threadIdx).insertTask(task);
				} else {
					return threadList.get(threadIdx).addTask(task);
				}
			}
		}
		return false;
	}

	/**
	 * 添加执行任务(threadIdMin~threadIdMax指定线程区间执行)
	 * 
	 * @param task
	 * @param threadIdMin, threadIdMax
	 * @param first
	 * @return
	 */
	public boolean addTask(HawkTask task, int threadIdMin, int threadIdMax, boolean first) {
		if (running && !waitBreak) {
			if (threadIdMin < 0) {
				threadIdMin = 0;
			} else if (threadIdMin >= threadNum) {
				threadIdMin = threadIdMin % threadNum;
			}

			if (threadIdMax < 0) {
				threadIdMax = threadNum - 1;
			} else if (threadIdMax >= threadNum) {
				threadIdMax = threadIdMax % threadNum;
			}

			if (threadIdMin > threadIdMax) {
				// 交换
				threadIdMin = threadIdMin ^ threadIdMax;
				threadIdMax = threadIdMin ^ threadIdMax;
				threadIdMin = threadIdMin ^ threadIdMax;
			}

			int intervalThreadNum = threadIdMax - threadIdMin + 1;
			int threadIdx = (int) (turnIndex.incrementAndGet() % intervalThreadNum) + threadIdMin;

			if (threadIdx >= 0 && threadIdx < threadNum) {
				if (first) {
					return threadList.get(threadIdx).insertTask(task);
				} else {
					return threadList.get(threadIdx).addTask(task);
				}
			}
		}
		return false;
	}

	/**
	 * 开始执行
	 * 
	 * @return
	 */
	public boolean start() {
		if (!running) {
			running = true;
			
			// 重构线程池列表
			if (threadList.size() == 0 && threadNum > 0) {
				for (int i = 0; i < threadNum; i++) {
					HawkThread thread = new HawkThread();
					thread.setName(poolName + "-" + thread.getId());
					threadList.add(thread);
				}
			}
			
			// 开启所有线程
			for (int i = 0; i < threadNum; i++) {
				threadList.get(i).start();
			}
		}
		return true;
	}

	/**
	 * 获得所有线程数
	 * 
	 * @return
	 */
	public int getThreadNum() {
		return threadNum;
	}

	/**
	 * 获取线程ID
	 * 
	 * @param threadIdx
	 * @return
	 */
	public long getThreadId(int threadIdx) {
		if (threadIdx >= 0 && threadIdx < threadNum) {
			return threadList.get(threadIdx).getId();
		}
		return 0;
	}

	/**
	 * 获取线程
	 * 
	 * @param threadIdx
	 * @return
	 */
	public HawkThread getThread(int threadIdx) {
		if (threadIdx >= 0 && threadIdx < threadNum) {
			return threadList.get(threadIdx);
		}
		return null;
	}
	
	/**
	 * 获取堆积的任务数量
	 */
	public long getUnprocessTaskCount() {
		long count = 0;
		for (int i = 0; i< threadNum; i++) {
			HawkThread thread = threadList.get(i);
			if (thread != null) {
				count += (thread.getPushTaskCnt() - thread.getPopTaskCnt());
			}
		}
		return count;
	}
	
	/**
	 * 结束所有线程
	 */
	public void close(boolean waitBreak) {
		if (this.running && !this.waitBreak) {
			// 设置等待退出或者直接退出模式
			this.waitBreak = waitBreak;
			if (!waitBreak) {
				this.running = false;
			}

			// 各个线程退出
			for (int i = 0; i < threadNum; i++) {
				try {
					threadList.get(i).close(waitBreak);
				} catch (Exception e) {
					HawkException.catchException(e);
				}
			}

			// 设置标记
			this.running = false;
			this.waitBreak = false;			
			threadList.clear();
		}
	}

	/**
	 * 查询是否开始运作(调度中)
	 * 
	 * @return
	 */
	public boolean isRunning() {
		return running;
	}

	/**
	 * 查询是否等待退出
	 * 
	 * @return
	 */
	public boolean isWaitBreak() {
		return waitBreak;
	}

}
