<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
      <template #toolbar>
        <a-button
          type="primary"
          @click="openModal"
        >
          {{ t('Excel导出') }}
        </a-button>
      </template>
      <template #agvTaskStatus="{ record }">
        <Tag :color="getStatusColor(record.agvTaskStatus)">
          {{ getStatusText(record.agvTaskStatus) }}
        </Tag>
      </template>
    </BasicTable>

    <ExpExcelModal @register="register" @success="defaultHeader" />
  </div>
</template>

<script lang="ts" setup>
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { jsonToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';
import { Tag } from 'ant-design-vue';
import { AgvTaskDto, AgvTaskStatus, ManageType } from '/@/services/ServiceProxies';
import {
  tableColumns,
  searchFormSchema,
  getAgvTaskListAsync,
  getAllAgvTasksAsync,
} from './AgvTaskManagement';
import moment from 'moment';
import { useI18n } from '/@/hooks/web/useI18n';
import { useRoute } from 'vue-router';
import { onMounted } from 'vue';

const [register, { openModal: openExportModal }] = useModal();
const { createMessage } = useMessage();
const { t } = useI18n();
const route = useRoute();

// 表格配置
const [registerTable, { reload, getForm, setFieldsValue }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 80,
    schemas: searchFormSchema,
    fieldMapToTime: [['creationTime', ['creationTimeStart', 'creationTimeEnd'], 'YYYY-MM-DD HH:mm:ss']],
  },
  api: getTableData,
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: true,
});

// 获取表格数据
async function getTableData(params) {
  console.log('查询参数:', params);
  
  // 处理时间范围参数
  if (params.creationTime && params.creationTime.length === 2) {
    // 开始时间设置为当天的 00:00:00
    params.creationTimeStart = moment(params.creationTime[0]).startOf('day').format('YYYY-MM-DD HH:mm:ss');
    // 结束时间设置为当天的 23:59:59
    params.creationTimeEnd = moment(params.creationTime[1]).endOf('day').format('YYYY-MM-DD HH:mm:ss');
    delete params.creationTime;
  }
  
  // 如果URL参数中有时间参数，也要处理
  if (params.creationTimeStart && params.creationTimeEnd) {
    // 确保开始时间是当天的 00:00:00
    params.creationTimeStart = moment(params.creationTimeStart).startOf('day').format('YYYY-MM-DD HH:mm:ss');
    // 确保结束时间是当天的 23:59:59
    params.creationTimeEnd = moment(params.creationTimeEnd).endOf('day').format('YYYY-MM-DD HH:mm:ss');
  }
  
  // 清理空值
  if (params.podCode === '') {
    params.podCode = undefined;
  }
  if (params.boxCode === '') {
    params.boxCode = undefined;
  }
  if (params.startPositionCode === '') {
    params.startPositionCode = undefined;
  }
  if (params.endPositionCode === '') {
    params.endPositionCode = undefined;
  }

  try {
    const result = await getAgvTaskListAsync(params);
    return result;
  } catch (error) {
    console.error('获取AGV任务列表失败:', error);
    createMessage.error('获取AGV任务列表失败');
    throw error;
  }
}

// 获取状态颜色
function getStatusColor(status: number) {
  const colorMap = {
    0: 'default',   // 被创建
    1: 'orange',    // 等待执行
    2: 'blue',      // 执行中
    3: 'cyan',      // 任务开始
    4: 'purple',    // 出库
    5: 'orange',    // 等待任务继续
    6: 'orange',    // 等待继续任务响应
    7: 'blue',      // 继续执行
    8: 'orange',    // 等待取消响应
    9: 'green',     // 任务完成
    10: 'red',      // 调度删除任务
    11: 'red',      // 设备错误
    12: 'red',      // 异常完成
  };
  return colorMap[status] || 'default';
}

// 获取状态文本
function getStatusText(status: number) {
  const textMap = {
    0: '被创建',
    1: '等待执行',
    2: '执行中',
    3: '任务开始',
    4: '出库',
    5: '等待任务继续',
    6: '等待继续任务响应',
    7: '继续执行',
    8: '等待取消响应',
    9: '任务完成',
    10: '调度删除任务',
    11: '设备错误',
    12: '异常完成',
  };
  return textMap[status] || '未知';
}

// 获取管理类型文本
function getManageTypeText(type: ManageType) {
  const typeMap = {
    [ManageType._0]: '入库',
    [ManageType._1]: '出库',
    [ManageType._2]: '移库',
  };
  return typeMap[type] || '未知';
}

// 页面挂载时处理URL参数
onMounted(() => {
  // 检查URL参数
  const { podCode, creationTimeStart, creationTimeEnd } = route.query;
  
  console.log('接收到的URL参数:', { podCode, creationTimeStart, creationTimeEnd });
  
  if (podCode || creationTimeStart || creationTimeEnd) {
    // 延迟一下确保表格完全初始化
    setTimeout(() => {
      // 构建表单初始值
      const formValues: any = {};
      
      if (podCode) {
        formValues.podCode = podCode;
      }
      
      if (creationTimeStart && creationTimeEnd) {
        // 将日期字符串转换为moment对象数组
        formValues.creationTime = [
          moment(creationTimeStart as string),
          moment(creationTimeEnd as string)
        ];
      }
      
      console.log('设置的表单值:', formValues);
      
      // 使用getForm获取表单实例，然后设置值
      const form = getForm();
      if (form) {
        form.setFieldsValue(formValues);
        
        // 设置表单值后触发查询
        setTimeout(() => {
          reload();
        }, 100);
      } else {
        console.error('无法获取表单实例');
      }
    }, 200);
  }
});

// 导出数据
var exportData: any[] = [];
var allAgvTasks: AgvTaskDto[] = [];

function defaultHeader({ filename, bookType }: ExportModalResult) {
  // 默认Object.keys(data[0])作为header
  exportData.length = 0;
  for (let index = 0; index < allAgvTasks.length; index++) {
    const task = allAgvTasks[index];
    exportData.push({
      创建时间: task.creationTime ? moment(task.creationTime).format('YYYY-MM-DD HH:mm:ss') : '',
      容器编号: task.boxCode,
      货架编号: task.podCode,
      起始位置: task.startPositionCode,
      目标位置: task.endPositionCode,
      任务状态: getStatusText(task.agvTaskStatus),
      任务类型: task.taskTyp,
      容器类型: task.ctnrTyp,
      物料批次: task.materialLot,
      数据: task.data,
      用户调用码路径: task.userCallCodePath,
      参考任务: task.refTask,
    });
  }
  console.log('导出数据:', exportData);
  jsonToSheetXlsx({
    data: exportData,
    filename,
    write2excelOpts: {
      bookType,
    },
  });
}

// 打开导出模态框
async function openModal() {
  try {
    // 获取当前搜索条件
    const searchParams = registerTable.getFormValues();
    
    // 获取所有数据用于导出
    const result = await getAllAgvTasksAsync(searchParams);
    allAgvTasks = result.items || [];
    
    if (allAgvTasks.length === 0) {
      createMessage.warning('没有数据可导出');
      return;
    }
    
    // 打开导出模态框
    openExportModal();
  } catch (error) {
    console.error('获取导出数据失败:', error);
    createMessage.error('获取导出数据失败');
  }
}
</script>

<style scoped>
/* 可以添加自定义样式 */
</style>
