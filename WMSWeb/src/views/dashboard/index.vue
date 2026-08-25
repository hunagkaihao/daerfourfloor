<template>
  <div class="dashboard">
    <div class="dashboard-header">
      <h1>欢迎使用仓储管理系统</h1>
      <p>WMS - Warehouse Management System</p>
    </div>
    
    <div class="dashboard-content">
      <a-row :gutter="16">
        <a-col :span="8">
          <a-card title="系统状态" :bordered="false">
            <p>系统运行正常</p>
            <a-tag color="green">在线</a-tag>
          </a-card>
        </a-col>
        <a-col :span="8">
          <a-card title="用户信息" :bordered="false">
            <p>当前用户: {{ userStore.getUserInfo?.username || 'admin' }}</p>
            <p>登录时间: {{ new Date().toLocaleString() }}</p>
          </a-card>
        </a-col>
        <a-col :span="8">
          <a-card title="快速操作" :bordered="false">
            <a-space direction="vertical" style="width: 100%">
              <a-button type="primary" block>物料管理</a-button>
              <a-button block>库存查询</a-button>
              <a-button block>报表统计</a-button>
            </a-space>
          </a-card>
        </a-col>
      </a-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useUserStore } from '/@/store/modules/user';

const userStore = useUserStore();
</script>

<style scoped>
.dashboard {
  padding: 24px;
  background: #f0f2f5;
  min-height: 100vh;
}

.dashboard-header {
  text-align: center;
  margin-bottom: 32px;
  padding: 32px 0;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.dashboard-header h1 {
  color: #1890ff;
  margin-bottom: 8px;
  font-size: 32px;
}

.dashboard-header p {
  color: #666;
  font-size: 16px;
}

.dashboard-content {
  margin-top: 24px;
}

.ant-card {
  height: 200px;
  display: flex;
  flex-direction: column;
}

.ant-card-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}
</style>
