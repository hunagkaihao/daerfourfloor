<template>
  <div ref="pageRef" class="remainder-cabinet-page">
    <Header numb="零头柜" />
<!--    <p class="warehouse-label">4楼叉车库</p>-->
    <a-tabs v-model:activeKey="activeTab" centered size="small">
      <a-tab-pane key="deposit" tab="寄存产品" :disabled="operationLocked && activeTab !== 'deposit'">
        <RemainderCabinetProductOperation operation="deposit" @lock-change="operationLocked = $event" />
      </a-tab-pane>
      <a-tab-pane key="withdraw" tab="领用产品" :disabled="operationLocked && activeTab !== 'withdraw'">
        <RemainderCabinetProductOperation operation="withdraw" @lock-change="operationLocked = $event" />
      </a-tab-pane>
      <a-tab-pane key="history" tab="流水查询" :disabled="operationLocked && activeTab !== 'history'">
        <RemainderCabinetHistoryQuery />
      </a-tab-pane>
      <a-tab-pane key="stock" tab="库存查询" :disabled="operationLocked && activeTab !== 'stock'">
        <RemainderCabinetStockQuery />
      </a-tab-pane>
    </a-tabs>
  </div>
</template>

<script setup lang="ts">
import { nextTick, onActivated, onBeforeUnmount, onDeactivated, onMounted, ref, watch } from 'vue';
import Header from '../../header/Header.vue';
import RemainderCabinetProductOperation from '../../components/RemainderCabinetProductOperation.vue';
import RemainderCabinetHistoryQuery from '../../components/RemainderCabinetHistoryQuery.vue';
import RemainderCabinetStockQuery from '../../components/RemainderCabinetStockQuery.vue';

/** 四个子页面直接显示；寄存、领用结果未确认时保持当前操作页，防止重复记账。 */
const activeTab = ref('deposit');
const operationLocked = ref(false);
/** 仅在当前可见标签页查找扫码框，避免缓存子页面争抢焦点。 */
const pageRef = ref<HTMLElement>();
let focusTimer: number | undefined;
let readonlyTimer: number | undefined;
let focusVersion = 0;
let focusedInput: HTMLInputElement | undefined;
let originalReadonly = false;

/** 恢复输入框原有只读状态，页面切走时也不会遗留临时锁定。 */
function restoreReadonly() {
  if (readonlyTimer !== undefined) window.clearTimeout(readonlyTimer);
  readonlyTimer = undefined;
  if (focusedInput) focusedInput.readOnly = originalReadonly;
  focusedInput = undefined;
}

/** 取消上一个面板的延时聚焦，快速切页或退出页面时不抢占新页面焦点。 */
function cancelFocus() {
  focusVersion++;
  if (focusTimer !== undefined) window.clearTimeout(focusTimer);
  focusTimer = undefined;
  restoreReadonly();
}

/** 首次进入及切换标签后聚焦第一个输入框；沿用组盘页的延时及临时只读方式抑制PDA软键盘。 */
async function focusFirstInput() {
  cancelFocus();
  const version = focusVersion;
  const tab = activeTab.value;
  await nextTick();
  if (version !== focusVersion) return;
  focusTimer = window.setTimeout(() => {
    focusTimer = undefined;
    if (version !== focusVersion || tab !== activeTab.value || operationLocked.value) return;
    const input = pageRef.value?.querySelector<HTMLInputElement>(
      '.ant-tabs-tabpane-active input:not([disabled]):not([type="hidden"])',
    );
    if (!input || input.disabled) return;
    focusedInput = input;
    originalReadonly = input.readOnly;
    input.readOnly = true;
    input.focus();
    readonlyTimer = window.setTimeout(restoreReadonly, 200);
  }, 100);
}

watch(activeTab, focusFirstInput, { flush: 'post' });
onMounted(focusFirstInput);
onActivated(focusFirstInput);
onDeactivated(cancelFocus);
onBeforeUnmount(cancelFocus);
</script>

<style scoped>
.remainder-cabinet-page { min-height: 100vh; background: #fff; }
.warehouse-label { margin: 10px 16px; color: #666; }
</style>
