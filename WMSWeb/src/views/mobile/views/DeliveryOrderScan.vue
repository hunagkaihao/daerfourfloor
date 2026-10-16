<template>
  <div class="page-container">
    <Header numb="发货单扫码"></Header>
    <a-row class="input-row">
      <a-col :span="17"><a-input v-model:value="lookupOrderNo" placeholder="发货单号：查询已保存记录、补录或重试" @keyup.enter="queryRecords" /></a-col>
      <a-col :span="6"><a-button @click="queryRecords" :loading="loading">查询</a-button></a-col>
    </a-row>
    <a-select v-if="savedRecords.length" :value="record?.id" style="width:100%"
      :options="savedRecords.map(row => ({ value: row.id, label: row.materialCode + '（实际出库 ' + (row.actualOutboundQuantity || 0) + '/' + row.quantity + '）' }))"
      @change="selectSavedRecord" />


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

    <a-row class="input-row">
      <a-col :span="6"><div class="label">U8明细ID:</div></a-col>
      <a-col :span="17"><a-input-number v-model:value="sourceDetailId" :min="1" :precision="0" placeholder="可稍后补充发货来源明细ID" style="width:100%" /></a-col>
    </a-row>
    <a-row class="input-row">
      <a-col :span="6"><div class="label">销售部门:</div></a-col>
      <a-col :span="17"><a-input v-model:value="salesDepartmentCode" placeholder="U8销售部门编码，可稍后补充" /></a-col>
    </a-row>
    <a-alert type="info" show-icon message="二维码只含主表ID。U8销售生单需补充来源明细ID和销售部门，本地搬运及实际出库可照常进行。" />

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
        <a-descriptions-item label="实际出库">{{ record.actualOutboundQuantity || 0 }}</a-descriptions-item>
        <a-descriptions-item label="U8销售状态">{{ record.isSalesPushed ? '已推送' : '未推送' }}</a-descriptions-item>
        <a-descriptions-item label="U8失败原因" :span="2">{{ record.salesPushError || '-' }}</a-descriptions-item>
      </a-descriptions>
      <a-row class="button-row">
        <a-button @click="confirmScan" :loading="loading">确认整单扫码完成</a-button>
        <a-button @click="saveSource" :loading="loading">补充U8来源</a-button>
        <a-button @click="retrySales" :loading="loading">推送／重试销售单</a-button>
      </a-row>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue';
import { defHttp } from '/@/utils/http/axios';
import { useMessage } from '/@/hooks/web/useMessage';
import { CreateFromBarcodeDto, ErpOutboundRecordDto } from '/@/services/ServiceProxies';
const { createConfirm } = useMessage();

const barcode = ref('');
const sourceDetailId = ref<number | null>(null);
const salesDepartmentCode = ref('');
const loading = ref(false);
const errorMsg = ref('');
const resultMsg = ref<{ type: 'success' | 'warning' | 'error'; text: string; detail?: string } | null>(null);
/** 兼容尚未重新生成代理的U8销售状态扩展字段。 */
type SalesRecord = ErpOutboundRecordDto & {
  sourceDetailId?: number; salesDepartmentCode?: string; isSalesPushed?: boolean;
  salesPushError?: string; isDeliveryScanCompleted?: boolean;
};
const record = ref<SalesRecord | null>(null);
const savedRecords = ref<SalesRecord[]>([]);
const lookupOrderNo = ref('');


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
    const result = await defHttp.post<SalesRecord>(
      { url: '/api/erp/outboundOrder/create-from-barcode',
        data: { ...dto.toJSON(), sourceDetailId: sourceDetailId.value || null,
          salesDepartmentCode: salesDepartmentCode.value.trim() || null } },
      { isTransformResponse: false },
    );
    record.value = result;
    lookupOrderNo.value = result.deliveryOrderNo || p.deliveryOrderNo;
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
/** 查询保存记录，页面重新进入后仍可补充来源或重试。 */
async function queryRecords() {
  if (!lookupOrderNo.value.trim()) { message.error('请输入发货单号'); return; }
  loading.value = true;
  try {
    savedRecords.value = await defHttp.get<SalesRecord[]>(
      { url: '/erp/asn/sales-records', params: { deliveryOrderNo: lookupOrderNo.value.trim() } },
      { isTransformResponse: false });
    if (savedRecords.value.length) selectSavedRecord(savedRecords.value[0].id || '');
    else { record.value = null; message.warning('未找到已保存发货记录'); }
  } catch (error: any) { message.error(error?.message || '查询失败'); }
  finally { loading.value = false; }
}

/** 选择对应物料行补录，不把主表ID当来源明细ID。 */
function selectSavedRecord(id: string) {
  record.value = savedRecords.value.find(row => row.id === id) || null;
  sourceDetailId.value = record.value?.sourceDetailId || null;
  salesDepartmentCode.value = record.value?.salesDepartmentCode || '';
}

/** 仓管员明确确认所有明细已扫码，避免只有部分明细录入时提前生单。 */
async function confirmScan() {
  if (!record.value?.deliveryOrderNo) return;
  createConfirm({ title: '确认整单扫码完成',
    content: '请核对发货单全部物料均已扫码保存。确认后不能再追加明细，是否完成扫码？',
    onOk: async () => {
      const result = await defHttp.post<{ success: boolean; message: string }>(
        { url: '/erp/asn/confirm-delivery-scan?deliveryOrderNo=' + encodeURIComponent(record.value!.deliveryOrderNo || '') },
        { isTransformResponse: false });
      if (result.success) message.success(result.message); else message.error(result.message);
      await queryRecords();
    } });
}

/** U8来源不包含在原10段二维码中，可针对当前记录补充。 */
async function saveSource() {
  if (!record.value || !sourceDetailId.value || !salesDepartmentCode.value.trim()) {
    message.error('请先保存发货记录，并填写U8明细ID及销售部门');
    return;
  }
  loading.value = true;
  try {
    const query = new URLSearchParams({ recordId: record.value.id || '',
      sourceDetailId: String(sourceDetailId.value), departmentCode: salesDepartmentCode.value.trim() });
    const result = await defHttp.post<{ success: boolean; message: string }>(
      { url: '/erp/asn/sales-source?' + query.toString() }, { isTransformResponse: false });
    if (result.success) message.success(result.message); else message.error(result.message);
    await queryRecords();
  } catch (error: any) { message.error(error?.response?.data?.error?.message || error?.message || '来源保存失败'); }
  finally { loading.value = false; }
}

/** 本地出库完成后可重试U8生单，尚未出库完成时后端拒绝提前生成。 */
async function retrySales() {
  if (!record.value?.deliveryOrderNo) return;
  loading.value = true;
  try {
    const result = await defHttp.post<{ success: boolean; message: string }>(
      { url: '/erp/asn/push-sales-outbound?deliveryOrderNo=' + encodeURIComponent(record.value.deliveryOrderNo) },
      { isTransformResponse: false });
    if (result.success) message.success(result.message); else message.error(result.message);
    await queryRecords();
  } catch (error: any) { message.error(error?.response?.data?.error?.message || error?.message || '销售推送失败'); }
  finally { loading.value = false; }
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
