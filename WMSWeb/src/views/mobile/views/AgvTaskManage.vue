<template>
  <div class="agv-task-manage-container">
    <Header numb="AGV任务管理"></Header>

    <!-- 筛选条件 -->
    <div class="filter-section">
      <a-row :gutter="[8, 8]" style="padding: 10px 16px;">
        <a-col :span="24">
          <a-select
            v-model:value="filterStatus"
            placeholder="任务状态筛选"
            style="width: 100%"
            allowClear
            @change="handleFilterChange"
          >
            <a-select-option :value="0">被创建</a-select-option>
            <a-select-option :value="1">等待执行</a-select-option>
            <a-select-option :value="2">执行中</a-select-option>
            <a-select-option :value="3">任务开始</a-select-option>
            <a-select-option :value="4">出储位</a-select-option>
            <a-select-option :value="5">等待任务继续</a-select-option>
            <a-select-option :value="9">任务完成</a-select-option>
            <a-select-option :value="10">调度删除任务</a-select-option>
            <a-select-option :value="11">设备错误</a-select-option>
            <a-select-option :value="12">异常完成</a-select-option>
          </a-select>
        </a-col>
        <a-col :span="24">
          <a-input
            v-model:value="filterStartPosition"
            placeholder="起点位置筛选（支持模糊查询）"
            allowClear
            @change="handleFilterChange"
            @focus="focusFn"
          >
            <template #prefix>
              <SearchOutlined style="color: #1890ff;" />
            </template>
          </a-input>
        </a-col>
      </a-row>
    </div>

    <!-- 任务列表 -->
    <div class="task-list" :style="{ height: listHeight + 'px' }">
      <a-spin :spinning="loading">
        <div v-if="taskList.length === 0" class="empty-state">
          <InboxOutlined style="font-size: 48px; color: #d9d9d9;" />
          <p style="color: #999; margin-top: 16px;">暂无任务数据</p>
        </div>

        <a-card
          v-for="task in taskList"
          :key="task.id"
          class="task-card"
          :class="getTaskCardClass(task.agvTaskStatus)"
        >
          <template #title>
            <div class="task-card-header">
              <span class="task-code">{{ task.reqCode }}</span>
              <a-tag :color="getStatusColor(task.agvTaskStatus)">
                {{ getStatusText(task.agvTaskStatus) }}
              </a-tag>
            </div>
          </template>

          <a-row :gutter="[8, 8]">
            <a-col :span="12">
              <div class="info-item">
                <span class="info-label">任务类型:</span>
                <span class="info-value">{{ task.taskTyp || '-' }}</span>
              </div>
            </a-col>
            <a-col :span="12">
              <div class="info-item">
                <span class="info-label">优先级:</span>
                <span class="info-value">{{ task.priority || '-' }}</span>
              </div>
            </a-col>
            <a-col :span="12">
              <div class="info-item">
                <span class="info-label">起点:</span>
                <span class="info-value">{{ task.startPositionCode || '-' }}</span>
              </div>
            </a-col>
            <a-col :span="12">
              <div class="info-item">
                <span class="info-label">终点:</span>
                <span class="info-value">{{ task.endPositionCode || '-' }}</span>
              </div>
            </a-col>
            <a-col :span="12">
              <div class="info-item">
                <span class="info-label">料箱:</span>
                <span class="info-value">{{ task.boxCode || '-' }}</span>
              </div>
            </a-col>
            <a-col :span="12">
              <div class="info-item">
                <span class="info-label">货架:</span>
                <span class="info-value">{{ task.podCode || '-' }}</span>
              </div>
            </a-col>
            <a-col :span="24">
              <div class="info-item">
                <span class="info-label">创建时间:</span>
                <span class="info-value">{{ formatTime(task.creationTime) }}</span>
              </div>
            </a-col>
            <a-col :span="24" v-if="task.taskStartTime">
              <div class="info-item">
                <span class="info-label">开始时间:</span>
                <span class="info-value">{{ formatTime(task.taskStartTime) }}</span>
              </div>
            </a-col>
          </a-row>

          <template #actions>
            <a-button
              type="link"
              size="small"
              @click="viewDetail(task)"
            >
              详情
            </a-button>
            <a-button
              v-if="canCancel(task.agvTaskStatus)"
              type="link"
              danger
              size="small"
              @click="showCancelConfirm(task)"
            >
              取消任务
            </a-button>
          </template>
        </a-card>
      </a-spin>

      <!-- 分页 -->
      <div class="pagination-wrapper" v-if="total > 0">
        <a-pagination
          v-model:current="currentPage"
          v-model:page-size="pageSize"
          :total="total"
          :show-size-changer="false"
          :show-total="(total) => `共 ${total} 条`"
          size="small"
          @change="handlePageChange"
        />
      </div>
    </div>

    <!-- 详情弹窗 -->
    <a-modal
      v-model:visible="detailVisible"
      title="任务详情"
      :footer="null"
      width="90%"
    >
      <div v-if="currentTask" class="task-detail">
        <a-descriptions :column="1" bordered size="small">
          <a-descriptions-item label="任务编号">{{ currentTask.reqCode }}</a-descriptions-item>
          <a-descriptions-item label="任务状态">
            <a-tag :color="getStatusColor(currentTask.agvTaskStatus)">
              {{ getStatusText(currentTask.agvTaskStatus) }}
            </a-tag>
          </a-descriptions-item>
          <a-descriptions-item label="任务类型">{{ currentTask.taskTyp || '-' }}</a-descriptions-item>
          <a-descriptions-item label="物料任务类型">{{ currentTask.stockTyp || '-' }}</a-descriptions-item>
          <a-descriptions-item label="工作位">{{ currentTask.wbCode || '-' }}</a-descriptions-item>
          <a-descriptions-item label="货架编号">{{ currentTask.podCode || '-' }}</a-descriptions-item>
          <a-descriptions-item label="货架方向">{{ currentTask.podDir || '-' }}</a-descriptions-item>
          <a-descriptions-item label="优先级">{{ currentTask.priority || '-' }}</a-descriptions-item>
          <a-descriptions-item label="任务单号">{{ currentTask.taskCode || '-' }}</a-descriptions-item>
          <a-descriptions-item label="AGV编号">{{ currentTask.agvCode || '-' }}</a-descriptions-item>
          <a-descriptions-item label="料箱编码">{{ currentTask.boxCode || '-' }}</a-descriptions-item>
          <a-descriptions-item label="起点位置">{{ currentTask.startPositionCode || '-' }}</a-descriptions-item>
          <a-descriptions-item label="终点位置">{{ currentTask.endPositionCode || '-' }}</a-descriptions-item>
          <a-descriptions-item label="出库单">{{ currentTask.pickListCode || '-' }}</a-descriptions-item>
          <a-descriptions-item label="创建时间">{{ formatTime(currentTask.creationTime) }}</a-descriptions-item>
          <a-descriptions-item label="开始时间">{{ formatTime(currentTask.taskStartTime) }}</a-descriptions-item>
          <a-descriptions-item label="最后修改">{{ formatTime(currentTask.lastModificationTime) }}</a-descriptions-item>
        </a-descriptions>
      </div>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue'
import { InboxOutlined, SearchOutlined } from '@ant-design/icons-vue'
import { message, Modal } from 'ant-design-vue'
import Header from '../header/Header.vue'
import { getAgvTaskList, cancelAgvTask } from './AgvTaskApi'
import moment from 'moment'

const loading = ref(false)
const taskList = ref<any[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)
const filterStatus = ref<number | undefined>(undefined)
const filterStartPosition = ref<string>('')
const detailVisible = ref(false)
const currentTask = ref<any>(null)

const screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight)
const listHeight = computed(() => screenHeight.value - 230)

onMounted(() => {
  loadTaskList()

  window.addEventListener('resize', () => {
    screenHeight.value = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight
  })
})

// 加载任务列表
const loadTaskList = async () => {
  loading.value = true
  try {
    const params = {
      pageIndex: currentPage.value,
      pageSize: pageSize.value,
      agvTaskStatus: filterStatus.value,
      startPositionCode: filterStartPosition.value || undefined
    }

    const res = await getAgvTaskList(params)
    taskList.value = res.items || []
    total.value = res.totalCount || 0
  } catch (error: any) {
    message.error(error?.message || '加载任务列表失败')
  } finally {
    loading.value = false
  }
}

// 状态筛选变化
const handleFilterChange = () => {
  currentPage.value = 1
  loadTaskList()
}

// 分页变化
const handlePageChange = () => {
  loadTaskList()
}

// 查看详情
const viewDetail = (task: any) => {
  currentTask.value = task
  detailVisible.value = true
}

// 显示取消确认
const showCancelConfirm = (task: any) => {
  Modal.confirm({
    title: '确认取消任务',
    content: `确定要取消任务 ${task.reqCode} 吗？此操作将下发给RCS取消任务、容器解绑、恢复库位状态。`,
    okText: '确认取消',
    okType: 'danger',
    cancelText: '我再想想',
    onOk: async () => {
      await handleCancelTask(task.id)
    }
  })
}

// 取消任务
const handleCancelTask = async (taskId: number) => {
  loading.value = true
  try {
    const res = await cancelAgvTask(taskId)
    if (res.success !== false) {
      message.success('任务取消成功')
      loadTaskList()
    } else {
      message.error(res.message || '任务取消失败')
    }
  } catch (error: any) {
    message.error(error?.message || '任务取消失败')
  } finally {
    loading.value = false
  }
}

// 判断是否可以取消
const canCancel = (status: number) => {
  // 可取消的状态：被创建、等待执行、执行中、任务开始、出储位、等待任务继续
  return [0, 1, 2, 3, 4, 5].includes(status)
}

// 获取状态文本
const getStatusText = (status: number) => {
  const statusMap: Record<number, string> = {
    0: '被创建',
    1: '等待执行',
    2: '执行中',
    3: '任务开始',
    4: '出储位',
    5: '等待任务继续',
    6: '等待继续任务响应',
    7: '继续执行',
    8: '等待取消响应',
    9: '任务完成',
    10: '调度删除任务',
    11: '设备错误',
    12: '异常完成'
  }
  return statusMap[status] || '未知状态'
}

// 获取状态颜色
const getStatusColor = (status: number) => {
  const colorMap: Record<number, string> = {
    0: 'default',
    1: 'blue',
    2: 'processing',
    3: 'cyan',
    4: 'geekblue',
    5: 'orange',
    6: 'orange',
    7: 'processing',
    8: 'warning',
    9: 'success',
    10: 'default',
    11: 'error',
    12: 'warning'
  }
  return colorMap[status] || 'default'
}

// 获取任务卡片样式类
const getTaskCardClass = (status: number) => {
  if (status === 11 || status === 12) return 'task-card-error'
  if (status === 9) return 'task-card-success'
  if ([2, 3, 4].includes(status)) return 'task-card-active'
  return ''
}

// 格式化时间
const formatTime = (time: string) => {
  if (!time) return '-'
  return moment(time).format('YYYY-MM-DD HH:mm:ss')
}

// 防止软键盘弹出影响布局
const focusFn = (e: any) => {
  e.target.setAttribute('readonly', 'readonly')
  setTimeout(() => {
    e.target.removeAttribute('readonly')
  }, 200)
}
</script>

<style scoped lang="less">
.agv-task-manage-container {
  min-height: 100vh;
  background: #f5f5f5;
  padding-bottom: 20px;
}

.filter-section {
  background: #ffffff;
  border-radius: 8px;
  margin: 10px 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.task-list {
  overflow-y: auto;
  padding: 0 16px;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  background: #ffffff;
  border-radius: 8px;
  margin-top: 20px;
}

.task-card {
  margin-bottom: 12px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  border-left: 4px solid #1890ff;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
    transform: translateY(-2px);
  }
}

.task-card-active {
  border-left-color: #52c41a;
  background: linear-gradient(135deg, #ffffff 0%, #f6ffed 100%);
}

.task-card-success {
  border-left-color: #52c41a;
  opacity: 0.85;
}

.task-card-error {
  border-left-color: #ff4d4f;
  background: linear-gradient(135deg, #ffffff 0%, #fff1f0 100%);
}

.task-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.task-code {
  font-size: 14px;
  font-weight: 600;
  color: #1890ff;
}

.info-item {
  display: flex;
  justify-content: space-between;
  padding: 4px 0;
  font-size: 12px;

  .info-label {
    color: #666;
    margin-right: 8px;
  }

  .info-value {
    color: #333;
    font-weight: 500;
    flex: 1;
    text-align: right;
  }
}

.pagination-wrapper {
  display: flex;
  justify-content: center;
  padding: 16px 0;
  background: #ffffff;
  border-radius: 8px;
  margin-top: 12px;
}

.task-detail {
  max-height: 60vh;
  overflow-y: auto;
}

::v-deep(.ant-card-head) {
  padding: 12px 16px;
  min-height: auto;
  border-bottom: 1px solid #f0f0f0;
}

::v-deep(.ant-card-head-title) {
  padding: 0;
  font-size: 14px;
}

::v-deep(.ant-card-body) {
  padding: 12px;
}

::v-deep(.ant-card-actions) {
  background: #fafafa;
  border-top: 1px solid #f0f0f0;
}

::v-deep(.ant-card-actions > li) {
  margin: 8px 0;
}
</style>
