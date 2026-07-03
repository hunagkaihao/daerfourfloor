<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="t('库存调整')"
    :width="800"
    :can-fullscreen="true"
    @ok="handleSubmit"
    @cancel="handleCancel"
  >
    <div class="stock-adjustment-modal">
      <!-- 库存信息 -->
      <div class="section">
        <div class="section-title">
          <span class="title-text">库存信息</span>
        </div>
        <div class="info-grid">
          <div class="info-column">
            <div class="info-item">
              <span class="label">物料编码:</span>
              <span class="value">{{ record.materialCode || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="label">规格:</span>
              <span class="value">{{ record.specs || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="label">当前库存数量:</span>
              <span class="value highlight">{{ record.totalCountInTime || 0 }}</span>
            </div>
            <div class="info-item">
              <span class="label">检验编号:</span>
              <span class="value">{{ record.checkNo || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="label">库位编号:</span>
              <span class="value">{{ record.cellCode || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="label">所在库区:</span>
              <span class="value">{{ record.areaName || '-' }}</span>
            </div>
          </div>
          <div class="info-column">
            <div class="info-item">
              <span class="label">物料名称:</span>
              <span class="value">{{ record.materialName || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="label">收料条形码:</span>
              <span class="value">{{ record.barcode || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="label">库存状态:</span>
              <span class="value">{{ getStatusText(record.status) }}</span>
            </div>
            <div class="info-item">
              <span class="label">容器编号:</span>
              <span class="value">{{ record.boxCode || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="label">所在仓库:</span>
              <span class="value">{{ record.houseName || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="label">供应商名称:</span>
              <span class="value">{{ record.supplierName || '-' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 调整信息 -->
      <div class="section">
        <div class="section-title">
          <span class="title-text">调整信息</span>
        </div>
        <div class="adjustment-info">
          <div class="info-box">
            <span class="info-label">调整说明:</span>
            <span class="info-text">请输入调整数量,正数为增加库存,负数为减少库存</span>
          </div>
        </div>
      </div>

      <!-- 调整参数 -->
      <div class="section">
        <div class="section-title">
          <span class="title-text">调整参数</span>
        </div>
        <div class="form-content">
          <div class="form-item">
            <label class="form-label required">调整数量:</label>
            <a-input
              v-model:value="formData.adjustmentQuantity"
              placeholder="请输入调整数量"
              type="number"
            />
          </div>
          <div class="form-item">
            <label class="form-label required">调整原因:</label>
            <a-textarea
              v-model:value="formData.adjustmentReason"
              placeholder="请输入调整原因"
              :rows="4"
              :maxlength="500"
              show-count
            />
          </div>
        </div>
      </div>
    </div>
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, reactive } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { useI18n } from '/@/hooks/web/useI18n';
import { message } from 'ant-design-vue';
import { StockServiceProxy } from '/@/services/ServiceProxies';
// 移除不存在的StockAdjustmentDto导入

const { t } = useI18n();

// API服务实例
const stockService = new StockServiceProxy();

interface StockRecord {
  id?: string;
  materialCode?: string;
  materialName?: string;
  specs?: string;
  totalCountInTime?: number;
  checkNo?: string;
  cellCode?: string;
  cellName?: string;
  areaName?: string;
  barcode?: string;
  status?: string;
  boxCode?: string;
  boxName?: string;
  houseName?: string;
  supplierName?: string;
  targetWarehouseName?: string;
  checkType?: string;
  checkResult?: string;
  avaType?: string;
}

const record = ref<StockRecord>({});

const formData = reactive({
  adjustmentQuantity: '',
  adjustmentReason: '',
});

const [registerModal, { closeModal }] = useModalInner((data: StockRecord) => {
  if (data && typeof data === 'object' && Object.keys(data).length > 0) {
    record.value = data;
  } else {
    record.value = {};
  }
  
  formData.adjustmentQuantity = '';
  formData.adjustmentReason = '';
});

const getStatusText = (status: string) => {
  const statusMap: Record<string, string> = {
    'Available': '可用',
    'Locked': '锁定',
    'Waiting': '待入库',
    'Filtrate': '筛选',
    'StockOut': '发送车间',
    'Freezing': '冻结',
  };
  return statusMap[status] || status || '-';
};

const handleSubmit = async () => {
  // 表单验证
  if (!formData.adjustmentQuantity || formData.adjustmentQuantity === '') {
    message.error('请输入调整数量');
    return;
  }
  
  if (!formData.adjustmentReason || formData.adjustmentReason.trim() === '') {
    message.error('请输入调整原因');
    return;
  }
  
  // 验证调整数量是否为有效数字
  const quantity = Number(formData.adjustmentQuantity);
  if (isNaN(quantity)) {
    message.error('调整数量必须为有效数字');
    return;
  }
  
  if (quantity === 0) {
    message.error('调整数量不能为0');
    return;
  }
  
  try {
    // 构建API请求参数
    // 移除不存在的StockAdjustmentDto，使用普通对象
    const adjustmentDto = {
      stockId: record.value.id || '',
      adjustmentQuantity: quantity,
      adjustmentReason: formData.adjustmentReason.trim(),
    };
    
    // 调用后端API
    const response = await stockService.stockAdjustment(adjustmentDto);
    
    if (response.success) {
      message.success('库存调整成功');
      closeModal();
      // 可以触发父组件刷新数据
      window.dispatchEvent(new Event('stockAdjustmentSuccess'));
    } else {
      message.error(response.message || '库存调整失败');
    }
  } catch (error) {
    console.error('库存调整失败:', error);
    message.error('库存调整失败，请稍后重试');
  }
};

const handleCancel = () => {
  closeModal();
};
</script>

<style scoped lang="less">
.stock-adjustment-modal {
  .section {
    margin-bottom: 24px;
    
    .section-title {
      margin-bottom: 16px;
      padding-bottom: 8px;
      border-bottom: 2px solid #1890ff;
      
      .title-text {
        color: #1890ff;
        font-weight: 600;
        font-size: 16px;
      }
    }
  }
  
  .info-grid {
    display: flex;
    gap: 24px;
    
    .info-column {
      flex: 1;
      
      .info-item {
        display: flex;
        margin-bottom: 12px;
        
        .label {
          width: 100px;
          color: #666;
          font-size: 14px;
        }
        
        .value {
          flex: 1;
          color: #333;
          font-size: 14px;
          
          &.highlight {
            color: #1890ff;
            font-weight: 600;
            font-size: 16px;
          }
        }
      }
    }
  }
  
  .adjustment-info {
    .info-box {
      background-color: #e6f7ff;
      padding: 12px 16px;
      border-radius: 6px;
      border: 1px solid #91d5ff;
      
      .info-label {
        color: #1890ff;
        font-weight: 500;
        margin-right: 8px;
      }
      
      .info-text {
        color: #666;
        font-size: 14px;
      }
    }
  }
  
  .form-content {
    .form-item {
      margin-bottom: 20px;
      
      .form-label {
        display: block;
        margin-bottom: 8px;
        color: #333;
        font-size: 14px;
        font-weight: 500;
        
        &.required::before {
          content: '*';
          color: #ff4d4f;
          margin-right: 4px;
        }
      }
    }
  }
}
</style>