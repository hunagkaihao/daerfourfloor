<template>
    <div class="scan-asn-container">
        <Header numb="扫描ASN"></Header>
        <U8InboundActions :asn-code="asnCode" :show-asn-input="false" :show-inspection-query="false" :disabled="loading || saving || !!linePushing"
            @busy-change="documentBusy = $event" @workflow-completed="refreshLocalAsn" />
        
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1>ASN码:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="asnCode" placeholder="扫描ASN码" @keyup.enter="scanAsn" 
                    :allowClear="true" :disabled="workflowLocked" @focus="focusFn" class="modern-input" autofocus ref="asnInput">
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
        </a-row>

        <div v-if="asnDataList.length" class="asn-info">
            <div class="asn-header-bar">
                <div class="asn-header-main">
                    <span class="asn-header-label">ASN单号：</span>
                    <span class="asn-header-value">{{ asnDataList[0]?.ccode || '-' }}</span>
                </div>
                <span class="asn-header-count">共 {{ asnDataList.length }} 条</span>
            </div>
            <div
                v-for="(item, index) in asnDataList"
                :key="item.autoid || index"
                class="asn-card"
            >
                <div class="asn-card-header">
                    <span class="asn-card-index">明细 {{ index + 1 }}</span>
                    <span class="asn-card-order">{{ item.cordercode || '-' }}</span>
                </div>
                <div class="asn-field-grid">
                    <div class="asn-field">
                        <span class="asn-label">计划日期</span>
                        <span class="asn-value">{{ item.darridate || '-' }}</span>
                    </div>
                    <div class="asn-field">
                        <span class="asn-label">批号</span>
                        <span class="asn-value">{{ item.cbatch || '-' }}</span>
                    </div>
                    <div class="asn-field">
                        <span class="asn-label">存货编号</span>
                        <span class="asn-value highlight">{{ item.cinvcode || '-' }}</span>
                    </div>
                    <div class="asn-field">
                        <span class="asn-label">数量</span>
                        <span class="asn-value qty">{{ item.iquantity ?? '-' }}</span>
                    </div>
                    <div class="asn-field asn-field-full">
                        <span class="asn-label">存货名称</span>
                        <span class="asn-value highlight">{{ item.cinvname || '-' }}</span>
                    </div>
                    <div class="asn-field">
                        <span class="asn-label">包装</span>
                        <span class="asn-value">{{ item.cfree3 || '-' }}</span>
                    </div>
                    <div class="asn-field">
                        <span class="asn-label">等级</span>
                        <span class="asn-value">{{ item.cfree5 || '-' }}</span>
                    </div>
                    <div class="asn-field">
                        <span class="asn-label">物料检验状态</span>
                        <span class="asn-value">{{ item.materialInspectionStatusName || '待检' }}</span>
                    </div>
                </div>
                <div class="asn-line-actions">
                    <a-button :disabled="!canPushLine(item, 'arrival')"
                        :loading="linePushing === 'arrival:' + item.cordercode" @click="pushLine(item, 'arrival')">
                        {{ item.isPushedToErp ? '到货单已推送' : '推送到货单' }}
                    </a-button>
                    <a-button :disabled="!canPushLine(item, 'inspection')"
                        :loading="linePushing === 'inspection:' + item.cordercode" @click="pushLine(item, 'inspection')">
                        {{ item.isInspectionReportPushed ? '报检单已推送' : '推送来料报检单' }}
                    </a-button>
                    <a-alert v-if="item.cordercode && lineNotices[item.cordercode]"
                        :message="lineNotices[item.cordercode].message" :type="lineNotices[item.cordercode].type" show-icon />
                </div>
            </div>
        </div>

        <div v-if="showSaveSuccess" class="save-success">
            <a-alert type="success" message="保存成功" description="ASN信息已成功保存到数据库" closable @close="showSaveSuccess = false" />
        </div>

        <div class="bottom-action-bar">
            <a-button type="primary" @click="scanAsn" class="modern-btn" :loading="loading" :disabled="workflowLocked">
                查询ASN信息
            </a-button>
            <a-button type="default" @click="saveAsnInfo" class="modern-btn" :loading="saving" :disabled="workflowLocked">
                保存ASN信息
            </a-button>
        </div>
    </div>
</template>

<script lang="ts" setup>
import U8InboundActions from "../../components/U8InboundActions.vue";
import { createU8Arrival, queryAsnMaterialInspection } from "../U8InboundApi";
import type { AsnMaterialInspectionLine } from "../U8InboundApi";
import { computed, reactive, ref, onMounted, watch } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../../header/Header.vue';
import { ERP_ASNServiceProxy, ErpAsnDto, ErpAsnSaveResponseDto } from '/@/services/ServiceProxies';
import { getAsnOrderPushState, pushAsnOrderArrival, pushAsnOrderInspection } from '/@/api/erp/asn';
import { useMessage } from '/@/hooks/web/useMessage';

const asnInput = ref<any>();
const asnCode = ref<string>('');
/** 扫码展示保留完整ASN信息及人工检验状态，不经过旧代理丢弃新增字段。 */
type AsnDisplayLine = Partial<ErpAsnDto> & AsnMaterialInspectionLine;
const asnDataList = ref<AsnDisplayLine[]>([]);
const loading = ref(false);
const saving = ref(false);
const showSaveSuccess = ref(false);
/** 整单和明细推送共用操作锁，推送期间不能切换ASN或再次保存。 */
const documentBusy = ref(false);
const linePushing = ref('');
const workflowLocked = computed(() => loading.value || saving.value || documentBusy.value || !!linePushing.value);
/** 各采购订单保留最近一次明细推送回执，重复物料明细共享对应单据结果。 */
const lineNotices = reactive<Record<string, { message: string; type: 'success' | 'warning' | 'error' }>>({});
const { createConfirm } = useMessage();
watch(asnCode, () => Object.keys(lineNotices).forEach(key => delete lineNotices[key]));

const asnService = new ERP_ASNServiceProxy();

onMounted(() => {
    setTimeout(() => {
        if (asnInput.value) {
            asnInput.value.focus();
        }
    }, 100);
});

const focusFn = (e: any) => {
    e.target.setAttribute('readonly', 'readonly');
    setTimeout(() => {
        e.target.removeAttribute('readonly');
    }, 200);
};

/** 推送结束后按当前ASN刷新本地状态，忽略已经切换到其他ASN的旧请求。 */
async function refreshLocalAsn(code: string) {
    if (code !== asnCode.value.trim()) return;
    try {
        const result = await queryAsnMaterialInspection(code);
        if (code !== asnCode.value.trim()) return;
        if (result.success && result.data?.length) asnDataList.value = result.data;
        else if (!result.success) message.warning(result.message || '检验状态刷新失败，请重新查询');
    } catch (error: any) {
        if (code === asnCode.value.trim()) message.warning(error?.message || '检验状态刷新失败，请重新查询');
    }
}

/** 明细按钮按当前ASN下的完整采购订单判断条件，旧ASN卡片及取消行不能推送。 */
function canPushLine(line: AsnDisplayLine, kind: 'arrival' | 'inspection') {
    if (workflowLocked.value || line.status === 4 || line.ccode !== asnCode.value.trim()) return false;
    const state = getAsnOrderPushState(asnDataList.value, line.cordercode);
    return kind === 'arrival' ? state.canPushArrival : state.canPushInspection;
}

/** 明细入口仅处理所属采购订单，回执和状态刷新不会影响其他ASN。 */
function pushLine(line: AsnDisplayLine, kind: 'arrival' | 'inspection') {
    if (!canPushLine(line, kind) || !line.cordercode) return;
    const code = asnCode.value.trim();
    const order = line.cordercode;
    createConfirm({
        title: kind === 'arrival' ? '推送到货单' : '推送来料报检单',
        content: `将推送采购订单 ${order} 的完整${kind === 'arrival' ? '到货单' : '来料报检单'}，其他采购订单不受影响。${kind === 'inspection' ? '请先确认仓管员已在U8审核对应到货单。' : ''}`,
        onOk: async () => {
            if (code !== asnCode.value.trim() || !canPushLine(line, kind)) return;
            linePushing.value = kind + ':' + order;
            try {
                const result = await (kind === 'arrival' ? pushAsnOrderArrival : pushAsnOrderInspection)(code, order);
                if (code === asnCode.value.trim()) lineNotices[order] = {
                    message: result.message || (result.success ? '推送成功' : '推送未完成'),
                    type: result.success ? 'success' : 'warning',
                };
                await refreshLocalAsn(code);
            } catch (error: any) {
                if (code === asnCode.value.trim()) lineNotices[order] = { message: error?.message || '推送结果未确认，请刷新后重试', type: 'error' };
            } finally { linePushing.value = ''; }
        },
    });
}

async function scanAsn() {
    if (workflowLocked.value) return;
    if (!asnCode.value || asnCode.value.trim() === '') {
        message.error('请输入ASN码');
        return;
    }

    loading.value = true;
    showSaveSuccess.value = false;

    try {
        const code = asnCode.value.trim();
        const result = await queryAsnMaterialInspection(code, true);
        if (code !== asnCode.value.trim()) return;
        if (result.success && result.data?.length) {
            message.success(`获取ASN信息成功，共 ${result.data.length} 条明细`);
            asnDataList.value = result.data;
        } else {
            message.error(result.message || '获取ASN信息失败');
            asnDataList.value = [];
        }
    } catch (error) {
        message.error('获取ASN信息异常');
        console.error('获取ASN信息异常:', error);
        asnDataList.value = [];
    } finally {
        loading.value = false;
    }
}

async function saveAsnInfo() {
    if (workflowLocked.value) return;
    if (!asnCode.value || asnCode.value.trim() === '') {
        message.error('请先输入ASN码');
        return;
    }

    if (!asnDataList.value.length) {
        message.error('请先查询ASN信息');
        return;
    }

    saving.value = true;

    try {
        const result: ErpAsnSaveResponseDto = await asnService.save(asnCode.value.trim());
        if (result.success) {
            message.success(result.message);
            showSaveSuccess.value = true;
            try {
                const arrival = await createU8Arrival(asnCode.value.trim());
                if (arrival.success) message.success(arrival.message || '到货单已生成，请在U8审核后推送报检');
                else message.warning('ASN已保存，到货单待重试：' + (arrival.message || 'U8未确认成功'));
            } catch (error: any) {
                message.warning('ASN已保存，到货单请求失败，请点击生成或重试到货单');
            }
            await refreshLocalAsn(asnCode.value.trim());
        } else {
            message.error(result.message || '保存失败');
        }
    } catch (error) {
        message.error('保存异常');
        console.error('保存ASN信息异常:', error);
    } finally {
        saving.value = false;
    }
}
</script>

<style scoped lang="less">
.asn-line-actions {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
    padding: 12px;
}
.asn-line-actions .ant-alert { width: 100%; }

.scan-asn-container {
    min-height: 100vh;
    background: #ffffff;
    padding: 0 0 72px;
    position: relative;
    overflow-x: hidden;
}

.input-row {
    margin: 10px 0;
    padding: 0 16px;
}

.htext {
    text-align: center;
    line-height: 32px;

    h1 {
        color: #333333;
        font-size: 14px;
        font-weight: 500;
        margin: 0;
        letter-spacing: 0.3px;
    }
}

.asn-info {
    margin: 10px 16px 20px;
}

.asn-header-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    margin-bottom: 10px;
    background: linear-gradient(135deg, #1890ff 0%, #096dd9 100%);
    border-radius: 8px;
    color: #fff;
    box-shadow: 0 2px 8px rgba(24, 144, 255, 0.25);
}

.asn-header-main {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 0;
    min-width: 0;
    flex: 1;
}

.asn-header-label {
    font-size: 13px;
    opacity: 0.9;
    white-space: nowrap;
    flex-shrink: 0;
}

.asn-header-value {
    font-size: 14px;
    font-weight: 700;
    letter-spacing: 0.3px;
    overflow: auto;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.asn-header-count {
    font-size: 12px;
    opacity: 0.9;
    white-space: nowrap;
    flex-shrink: 0;
    margin-left: 8px;
}

.asn-card {
    background: #fafafa;
    border: 1px solid #e8e8e8;
    border-radius: 8px;
    margin-bottom: 12px;
    overflow: hidden;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}

.asn-card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 14px;
    background: #f0f5ff;
    border-bottom: 1px solid #e8e8e8;
    color: #333;
}

.asn-card-index {
    font-size: 12px;
    font-weight: 600;
    color: #666;
}

.asn-card-order {
    font-size: 13px;
    font-weight: 600;
    color: #1890ff;
    max-width: 65%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.asn-field-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 0;
    padding: 4px 0;
}

.asn-field {
    display: flex;
    flex-direction: column;
    padding: 10px 14px;
    border-bottom: 1px solid #f0f0f0;

    &:nth-child(odd):not(.asn-field-full) {
        border-right: 1px solid #f0f0f0;
    }
}

.asn-field-full {
    grid-column: 1 / -1;
    border-right: none !important;
}

.asn-label {
    font-size: 11px;
    color: #999;
    margin-bottom: 4px;
    line-height: 1.2;
}

.asn-value {
    font-size: 13px;
    color: #333;
    word-break: break-all;
    line-height: 1.4;

    &.highlight {
        color: #c41d7f;
        font-weight: 500;
    }

    &.qty {
        color: #1890ff;
        font-weight: 600;
        font-size: 15px;
    }
}

.save-success {
    margin: 10px 16px;
}

.bottom-action-bar {
    position: fixed;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    gap: 10px;
    padding: 10px 16px;
    background: #fff;
    border-top: 1px solid #f0f0f0;
    box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.06);
    z-index: 100;
}

.modern-input {
    height: 32px;
    border-radius: 6px !important;
    border: 1px solid #d9d9d9 !important;
    background: #ffffff !important;
    transition: all 0.2s ease !important;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1) !important;

    &:focus,
    &:hover {
        border-color: #1890ff !important;
        box-shadow: 0 1px 6px rgba(24, 144, 255, 0.2) !important;
    }

    &::placeholder {
        color: rgba(0, 0, 0, 0.45) !important;
        font-weight: 400;
    }
}

.scan-icon {
    color: #1890ff !important;
    font-size: 18px;
    transition: all 0.2s ease;

    &:hover {
        color: #40a9ff !important;
    }
}

.modern-btn {
    flex: 1;
    height: 36px !important;
    border-radius: 6px !important;
    font-size: 14px !important;
    font-weight: 500 !important;
    transition: all 0.2s ease !important;
}

::v-deep(.ant-alert) {
    margin-bottom: 0;
}
</style>
