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
        <a-descriptions-item label="运行状态">
          {{ status.isRunning ? '运行中' : '未运行' }}
        </a-descriptions-item>
        <a-descriptions-item label="停止状态">
          {{ status.isStopping ? '正在停止' : '未请求停止' }}
        </a-descriptions-item>
        <a-descriptions-item label="启动时间">
          {{ formatDateTime(status.startedAt) }}
        </a-descriptions-item>
        <a-descriptions-item label="停止时间">
          {{ formatDateTime(status.stoppedAt) }}
        </a-descriptions-item>
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

      <a-button size="large" block :loading="refreshing" @click="() => refreshStatus()">
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
import { computed, onMounted, reactive, ref } from 'vue';
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
  startedAt: undefined,
  stoppedAt: undefined,
  currentCellCode: '',
  currentMaterialCode: '',
  currentAction: '',
  currentFromCell: '',
  currentToCell: '',
  completedGroupCount: 0,
  completedMoveCount: 0,
  lastError: '',
});

const starting = ref(false);
const stopping = ref(false);
const refreshing = ref(false);

const statusText = computed(() => status.status || (status.isRunning ? '运行中' : '未启动'));
const statusColor = computed(() => {
  if (status.lastError || status.status === '异常停止') return 'red';
  if (status.isStopping) return 'orange';
  if (status.isRunning) return 'green';
  return 'default';
});

/**
 * 兼容库存整理接口在不同部署环境中的响应包装形式。
 * 支持Axios原生响应、ABP result包装、data包装以及直接返回DTO。
 */
function unwrapResponse(result: any): any {
  return result?.data?.result ?? result?.result ?? result?.data ?? result;
}

/** 同时读取后端小驼峰和PascalCase字段，避免序列化策略差异导致页面状态始终使用初始值。 */
function readStatusField(payload: any, camelName: string, pascalName: string, fallback?: any): any {
  if (payload && payload[camelName] !== undefined) return payload[camelName];
  if (payload && payload[pascalName] !== undefined) return payload[pascalName];
  return fallback;
}

/**
 * 将后端状态DTO显式映射到页面状态。
 * 不使用Object.assign直接合并，防止IsRunning等PascalCase字段被新增到对象后，
 * 模板仍然读取旧的isRunning=false，导致停止按钮一直禁用。
 */
function applyStatusResponse(result: any) {
  const payload = unwrapResponse(result);
  const isRunning = readStatusField(payload, 'isRunning', 'IsRunning');
  if (typeof isRunning !== 'boolean') {
    throw new Error('库存整理状态接口返回格式不正确，缺少isRunning字段');
  }

  status.isEnabled = Boolean(readStatusField(payload, 'isEnabled', 'IsEnabled', false));
  status.isRunning = isRunning;
  status.isStopping = Boolean(readStatusField(payload, 'isStopping', 'IsStopping', false));
  status.status = readStatusField(payload, 'status', 'Status', status.isRunning ? '运行中' : '未运行');
  status.startedAt = readStatusField(payload, 'startedAt', 'StartedAt');
  status.stoppedAt = readStatusField(payload, 'stoppedAt', 'StoppedAt');
  status.currentCellCode = readStatusField(payload, 'currentCellCode', 'CurrentCellCode', '');
  status.currentMaterialCode = readStatusField(payload, 'currentMaterialCode', 'CurrentMaterialCode', '');
  status.currentAction = readStatusField(payload, 'currentAction', 'CurrentAction', '');
  status.currentFromCell = readStatusField(payload, 'currentFromCell', 'CurrentFromCell', '');
  status.currentToCell = readStatusField(payload, 'currentToCell', 'CurrentToCell', '');
  status.completedGroupCount = Number(readStatusField(payload, 'completedGroupCount', 'CompletedGroupCount', 0));
  status.completedMoveCount = Number(readStatusField(payload, 'completedMoveCount', 'CompletedMoveCount', 0));
  status.lastError = readStatusField(payload, 'lastError', 'LastError', '');
}

/** 将后端时间显示为本地时间；没有时间时统一显示短横线。 */
function formatDateTime(value?: string | Date): string {
  if (!value) return '-';
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString();
}

/** 刷新库存整理线程状态。 */
async function refreshStatus(showLoading = true) {
  if (showLoading) refreshing.value = true;
  try {
    applyStatusResponse(await getStockConsolidationStatus());
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
        if (result?.success) {
          // 启动接口成功即表示后台线程已经创建，先同步本地按钮状态，再读取后端完整状态。
          status.isRunning = true;
          status.isStopping = false;
          status.status = '正在启动';
          message.success(result.message || '库存整理线程已启动');
          await refreshStatus(false);
        } else {
          message.error(result?.message || '启动失败');
        }
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
        if (result?.success) {
          status.isStopping = true;
          status.status = '正在停止，等待当前搬运任务结束';
          message.success(result.message || '已请求停止');
          await refreshStatus(false);
        } else {
          message.warning(result?.message || '停止失败');
        }
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
  // 按业务要求不启用轮询，后续状态由启动、停止和手动刷新按钮主动获取。
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
