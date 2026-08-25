<template>
  <div class="page-container">
    <Header numb="发货单扫码"></Header>

    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">发货单据行条码:</div>
      </a-col>
      <a-col :span="17">
        <a-input v-model:value="barcode" placeholder="扫描发货单条码" @keyup.enter="onScan" :allowClear="true" class="search-input" autofocus>
          <template #suffix>
            <scan-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="button-row">
      <a-button type="primary" @click="onScan" :loading="loading">保存出库单</a-button>
    </a-row>

    <div v-if="parsed" class="parsed-card">
      <a-descriptions title="条码解析" :column="2" bordered size="small">
        <a-descriptions-item label="仓库">{{ parsed.fields[0] }}</a-descriptions-item>
        <a-descriptions-item label="客户编码">{{ parsed.fields[1] }}</a-descriptions-item>
        <a-descriptions-item label="主表id">{{ parsed.fields[2] }}</a-descriptions-item>
        <a-descriptions-item label="数量">{{ parsed.fields[3] }}</a-descriptions-item>
        <a-descriptions-item label="存货编码" :span="2"><b>{{ parsed.fields[4] }}</b></a-descriptions-item>
        <a-descriptions-item label="包装">{{ parsed.fields[5] }}</a-descriptions-item>
        <a-descriptions-item label="等级">{{ parsed.fields[6] }}</a-descriptions-item>
        <a-descriptions-item label="标贴打字">{{ parsed.fields[7] }}</a-descriptions-item>
        <a-descriptions-item label="发货单号" :span="2">{{ parsed.fields[8] }}</a-descriptions-item>
        <a-descriptions-item label="每箱数量">{{ parsed.fields[9] }}</a-descriptions-item>
      </a-descriptions>
    </div>

    <div v-if="errorMsg" class="alert-box">
      <a-alert type="error" :message="errorMsg" closable @close="errorMsg = ''" />
    </div>

    <div v-if="resultMsg" class="alert-box">
      <a-alert :type="resultMsg.type" closable @close="resultMsg = null">
        <template #message>
          <div>{{ resultMsg.text }}</div>
          <div v-if="resultMsg.detail" style="font-size:12px;margin-top:4px;">{{ resultMsg.detail }}</div>
        </template>
      </a-alert>
    </div>

    <div v-if="record" class="info-card">
      <a-descriptions title="出库单保存记录" :column="2" bordered size="small">
        <a-descriptions-item label="存货编码" :span="2"><b>{{ record.materialCode }}</b></a-descriptions-item>
        <a-descriptions-item label="仓库">{{ record.warehouse }}</a-descriptions-item>
        <a-descriptions-item label="客户编码">{{ record.customerCode }}</a-descriptions-item>
        <a-descriptions-item label="数量">{{ record.quantity }}</a-descriptions-item>
        <a-descriptions-item label="每箱数量">{{ record.qtyPerBox }}</a-descriptions-item>
        <a-descriptions-item label="包装">{{ record.package }}</a-descriptions-item>
        <a-descriptions-item label="等级">{{ record.grade }}</a-descriptions-item>
        <a-descriptions-item label="标贴打字">{{ record.labelText }}</a-descriptions-item>
        <a-descriptions-item label="发货单号" :span="2">{{ record.deliveryOrderNo }}</a-descriptions-item>
        <a-descriptions-item label="主表id" :span="2">{{ record.masterId }}</a-descriptions-item>
      </a-descriptions>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue';
import { ErpOutboundOrderServiceProxy, CreateFromBarcodeDto, ErpOutboundRecordDto } from '/@/services/ServiceProxies';

const barcode = ref('');
const loading = ref(false);
const errorMsg = ref('');
const resultMsg = ref<{ type: 'success' | 'warning' | 'error'; text: string; detail?: string } | null>(null);
const record = ref<ErpOutboundRecordDto | null>(null);

const outboundService = new ErpOutboundOrderServiceProxy();

interface ParsedBarcode {
  raw: string;
  fields: string[];
  warehouseCode: string;
  customerCode: string;
  masterId: string;
  quantity: number;
  materialCode: string;
  packaging: string;
  grade: string;
  labelPrint: string;
  deliveryOrderNo: string;
  qtyPerBox: number;
}

const parsed = computed<ParsedBarcode | null>(() => {
  const v = barcode.value.trim();
  if (!v) return null;
  const parts = v.split('@');
  if (parts.length !== 10) return null;
  return {
    raw: v,
    fields: parts,
    warehouseCode: parts[0],
    customerCode: parts[1],
    masterId: parts[2],
    quantity: Number(parts[3]) || 0,
    materialCode: parts[4],
    packaging: parts[5],
    grade: parts[6],
    labelPrint: parts[7],
    deliveryOrderNo: parts[8],
    qtyPerBox: Number(parts[9]) || 0,
  };
});

async function onScan() {
  const p = parsed.value;
  if (!p) {
    message.error('条码格式错误，需要10个@分隔的字段');
    return;
  }

  loading.value = true;
  errorMsg.value = '';
  resultMsg.value = null;

  const dto = new CreateFromBarcodeDto();
  dto.warehouseCode = p.warehouseCode;
  dto.customerCode = p.customerCode;
  dto.masterId = p.masterId;
  dto.quantity = p.quantity;
  dto.materialCode = p.materialCode;
  dto.packaging = p.packaging;
  dto.grade = p.grade;
  dto.labelPrint = p.labelPrint;
  dto.deliveryOrderNo = p.deliveryOrderNo;
  dto.qtyPerBox = p.qtyPerBox;

  try {
    const result = await outboundService.createFromBarcode(dto);
    record.value = result;
    resultMsg.value = {
      type: 'success',
      text: `存货编码 ${p.materialCode} 保存成功`,
      detail: `发货单号: ${p.deliveryOrderNo}`,
    };
    message.success('保存成功');
    barcode.value = '';
  } catch (error: any) {
    const msg = error?.response?.data?.message || error?.message || '保存失败';
    errorMsg.value = msg;
    if (msg.includes('已存在')) {
      resultMsg.value = { type: 'warning', text: `存货编码 ${p.materialCode} 已存在`, detail: '跳过重复录入' };
    } else {
      resultMsg.value = { type: 'error', text: msg };
    }
    // message.error(msg);
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.page-container { padding: 10px; background: #fff; min-height: 100vh; }
.input-row { margin: 10px 0; }
.label { text-align: center; line-height: 32px; font-weight: 500; }
.search-input { width: 100%; }
.icon { color: #1890ff; }
.button-row { display: flex; gap: 10px; padding: 10px 0; }
.button-row button { flex: 1; }
.parsed-card { margin: 10px 0; }
.alert-box { margin: 10px 0; }
.info-card { margin: 10px 0; }
</style>
