<template>
    <div class="scan-asn-container">
        <Header numb="扫描ASN"></Header>
        
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1>ASN码:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="asnCode" placeholder="扫描ASN码" @keyup.enter="scanAsn" 
                    :allowClear="true" @focus="focusFn" class="modern-input" autofocus ref="asnInput">
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
                </div>
            </div>
        </div>

        <div v-if="showSaveSuccess" class="save-success">
            <a-alert type="success" message="保存成功" description="ASN信息已成功保存到数据库" closable @close="showSaveSuccess = false" />
        </div>

        <div class="bottom-action-bar">
            <a-button type="primary" @click="scanAsn" class="modern-btn" :loading="loading">
                查询ASN信息
            </a-button>
            <a-button type="default" @click="saveAsnInfo" class="modern-btn" :loading="saving">
                保存ASN信息
            </a-button>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../../header/Header.vue';
import { ERP_ASNServiceProxy, ErpAsnDto, ErpAsnValidateResponseDto, ErpAsnSaveResponseDto } from '/@/services/ServiceProxies';

const asnInput = ref<any>();
const asnCode = ref<string>('');
const asnDataList = ref<ErpAsnDto[]>([]);
const loading = ref(false);
const saving = ref(false);
const showSaveSuccess = ref(false);

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

async function scanAsn() {
    if (!asnCode.value || asnCode.value.trim() === '') {
        message.error('请输入ASN码');
        return;
    }

    loading.value = true;
    showSaveSuccess.value = false;

    try {
        const result: ErpAsnValidateResponseDto = await asnService.get(asnCode.value.trim());
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
    overflow: hidden;
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
