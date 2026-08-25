<template>
  <div class="stock-consolidation-page">
    <Header numb="库存整理" />

    <a-card class="status-card" :bordered="false">
      <div class="status-title">
        <span>线程状态</span>
        <div>
          <a-tag :color="status.isEnabled ? 'blue' : 'default'">
            {{ status.isEnabled ? '配置已启用' : '配置未启用' }}
          </a-tag>
          <a-tag :color="statusColor">{{ statusText }}</a-tag>
        </div>
      </div>

      <a-descriptions :column="1" size="small" bordered>
        <a-descriptions-item label="当前库位">
          {{ status.currentCellCode || '-' }}
        </a-descriptions-item>
        <a-descriptions-item label="当前物料">
          {{ status.currentMaterialCode || '-' }}
        </a-descriptions-item>
        <a-descriptions-item label="当前动作">
          {{ status.currentAction || '-' }}
        </a-descriptions-item>
        <a-descriptions-item label="搬运起点">
          {{ status.currentFromCell || '-' }}
        </a-descriptions-item>
        <a-descriptions-item label="搬运终点">
          {{ status.currentToCell || '-' }}
        </a-descriptions-item>
        <a-descriptions-item label="已整理物料组">
          {{ status.completedGroupCount || 0 }}
        </a-descriptions-item>
        <a-descriptions-item label="已完成搬运">
          {{ status.completedMoveCount || 0 }}
        </a-descriptions-item>
      </a-descriptions>

      <a-alert
        v-if="status.lastError"
        class="error-alert"
        type="error"
        show-icon
        :message="status.lastError"
      />
    </a-card>

    <div class="action-panel">
      <a-button
        type="primary"
        size="large"
        block
        :loading="starting"
        :disabled="!status.isEnabled || status.isRunning"
        @click="handleStart"
      >
        启动库存整理线程
      </a-button>

      <a-button
        danger
        size="large"
        block
        :loading="stopping"
        :disabled="!status.isRunning || status.isStopping"
        @click="handleStop"
      >
        停止库存整理线程
      </a-button>

      <a-button size="large" block :loading="refreshing" @click="refreshStatus">
        刷新线程状态
      </a-button>
    </div>

    <a-alert
      class="notice-alert"
      type="warning"
      show-icon
      message="停止线程不会取消已经下发的AGV任务，当前任务结束后才停止创建下一条任务。"
    />
  </div>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { Modal, message } from 'ant-design-vue';
import Header from '../header/Header.vue';
import {
  getStockConsolidationStatus,
  startStockConsolidation,
  stopStockConsolidation,
} from './StockConsolidation';

/** 后端线程状态，字段名称与StockConsolidationStatusDto保持一致。 */
const status = reactive<any>({
  isEnabled: false,
  isRunning: false,
  isStopping: false,
  status: '读取中',
  completedGroupCount: 0,
  completedMoveCount: 0,
});

const starting = ref(false);
const stopping = ref(false);
const refreshing = ref(false);
let refreshTimer: ReturnType<typeof setInterval> | undefined;

const statusText = computed(() => status.status || (status.isRunning ? '运行中' : '未启动'));
const statusColor = computed(() => {
  if (status.lastError || status.status === '异常停止') return 'red';
  if (status.isStopping) return 'orange';
  if (status.isRunning) return 'green';
  return 'default';
});

/** 兼容HTTP封装返回原始响应或直接返回data的两种形式。 */
function unwrapResponse(result: any): any {
  return result?.data ?? result;
}

/** 刷新库存整理线程状态。 */
async function refreshStatus(showLoading = true) {
  if (showLoading) refreshing.value = true;
  try {
    const result = unwrapResponse(await getStockConsolidationStatus());
    Object.assign(status, result || {});
  } catch (error: any) {
    message.error(error?.message || '查询库存整理线程状态失败');
  } finally {
    refreshing.value = false;
  }
}

/** 启动前进行二次确认，避免误触发真实AGV任务。 */
function handleStart() {
  Modal.confirm({
    title: '确认启动库存整理？',
    content: '启动后系统将按照配置的S型顺序创建真实搬运任务。',
    okText: '确认启动',
    cancelText: '取消',
    async onOk() {
      starting.value = true;
      try {
        const result = unwrapResponse(await startStockConsolidation());
        result?.success ? message.success(result.message || '库存整理线程已启动') : message.error(result?.message || '启动失败');
        await refreshStatus(false);
      } catch (error: any) {
        message.error(error?.message || '启动库存整理线程失败');
      } finally {
        starting.value = false;
      }
    },
  });
}

/** 请求安全停止线程，不调用AGV取消接口。 */
function handleStop() {
  Modal.confirm({
    title: '确认停止库存整理？',
    content: '当前已下发任务仍会完成，系统将不再创建下一条整理任务。',
    okText: '确认停止',
    cancelText: '取消',
    async onOk() {
      stopping.value = true;
      try {
        const result = unwrapResponse(await stopStockConsolidation());
        result?.success ? message.success(result.message || '已请求停止') : message.warning(result?.message || '停止失败');
        await refreshStatus(false);
      } catch (error: any) {
        message.error(error?.message || '停止库存整理线程失败');
      } finally {
        stopping.value = false;
      }
    },
  });
}

onMounted(async () => {
  await refreshStatus();
  // 页面停留期间每五秒刷新一次线程状态。
  refreshTimer = setInterval(() => refreshStatus(false), 5000);
});

onBeforeUnmount(() => {
  if (refreshTimer) clearInterval(refreshTimer);
});
</script>

<style scoped>
.stock-consolidation-page {
  min-height: 100vh;
  padding-bottom: 24px;
  background: #f5f7fa;
}

.status-card {
  margin: 16px;
  border-radius: 12px;
}

.status-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
  font-size: 17px;
  font-weight: 600;
}

.action-panel {
  display: grid;
  gap: 12px;
  margin: 16px;
}

.error-alert,
.notice-alert {
  margin: 16px;
}
</style>
