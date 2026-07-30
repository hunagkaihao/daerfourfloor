<template>
  <div class="container-unbind-container">
    <Header numb="容器解绑"></Header>
    <a-row class="input-row">
      <a-col :span="6">
        <div class="htext">
          <h1>仓位编号:</h1>
        </div>
      </a-col>
      <a-col :span="17">
        <a-input
          v-model:value="stgBinCode"
          placeholder="扫描仓位编号"
          @keyup.enter="handleUnbind"
          ref="inputRef"
          :allowClear="true"
          @focus="focusFn"
          class="modern-input"
        >
          <template #suffix>
            <scan-outlined class="scan-icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="info-row">
      <a-col :span="24">
        <div class="info-card">
          <div class="info-item">
            <span class="info-label">容器类型:</span>
            <span class="info-value">5（默认）</span>
          </div>
          <div class="info-item">
            <span class="info-label">操作类型:</span>
            <span class="info-value">解绑</span>
          </div>
          <div class="info-item" v-if="resolvedCode && resolvedCode !== stgBinCode.trim()">
            <span class="info-label">解析仓位:</span>
            <span class="info-value resolved">{{ resolvedCode }}</span>
          </div>
        </div>
      </a-col>
    </a-row>

    <div class="result-area" v-if="resultMessage">
      <a-alert
        :type="resultSuccess ? 'success' : 'error'"
        :message="resultMessage"
        show-icon
        :closable="true"
        @close="resultMessage = ''"
      />
    </div>

    <div class="tab-bar">
      <a-button @click="handleUnbind" type="primary" class="unbind-btn" :loading="loading">
        解绑容器
      </a-button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue'
import { ScanOutlined } from '@ant-design/icons-vue'
import { message } from 'ant-design-vue'
import Header from '../header/Header.vue'
import { containerUnbindByCell } from './Stock'

const stgBinCode = ref<string>('')
const loading = ref(false)
const resultMessage = ref<string>('')
const resultSuccess = ref(false)
const inputRef = ref<any>()
const resolvedCode = ref<string>('')

function resolveCellCode(input: string): string {
  const trimmed = input.trim()
  const match = trimmed.match(/(\d+)$/)
  if (!match) return trimmed
  const digits = match[1]
  if (digits.length <= 8) {
    const shelf = digits.substring(0, 4)
    const position = digits.substring(4)
    return shelf + '01095' + position + '01'
  }
  return digits
}

onMounted(() => {
  setTimeout(() => {
    if (inputRef.value) {
      inputRef.value.focus()
    }
  }, 100)
})

async function handleUnbind() {
  const raw = stgBinCode.value.trim()
  if (!raw) {
    message.error('请先扫描仓位编号')
    return
  }
  const code = resolveCellCode(raw)
  resolvedCode.value = code
  if (code !== raw) {
    message.info(`仓位编号已转换: ${raw} → ${code}`)
  }

  loading.value = true
  resultMessage.value = ''
  try {
    const res = await containerUnbindByCell(code)
    resultSuccess.value = res.success !== false
    resultMessage.value = res.message || '解绑完成'
  } catch (error: any) {
    resultSuccess.value = false
    resultMessage.value = error?.message || '解绑失败'
  } finally {
    loading.value = false
  }
}

function focusFn(e: any) {
  e.target.setAttribute('readonly', 'readonly')
  setTimeout(() => {
    e.target.removeAttribute('readonly')
  }, 200)
}
</script>

<style scoped lang="less">
.container-unbind-container {
  min-height: 100vh;
  background: #ffffff;
  padding: 0;
  position: relative;
  overflow-x: hidden;
}

.input-row {
  margin: 10px 0;
  padding: 0 16px;
}

.info-row {
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

.info-card {
  background: #f5f7fa;
  border-radius: 8px;
  padding: 12px 16px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 0;

  .info-label {
    font-size: 14px;
    color: #666;
  }

  .info-value {
    font-size: 14px;
    font-weight: 600;
    color: #333;
  }

  .info-value.resolved {
    color: #1890ff;
    font-size: 15px;
  }
}

.result-area {
  margin: 10px 16px;
}

.tab-bar {
  display: flex;
  align-items: center;
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  height: 60px;
  background: #ffffff;
  border-top: 1px solid #f0f0f0;
  padding: 0 16px 10px;
  z-index: 1000;
}

.unbind-btn {
  width: 100%;
  height: 40px !important;
  border-radius: 6px !important;
  background: #ff4d4f !important;
  border: none !important;
  font-size: 15px !important;
  font-weight: 600 !important;
  color: white !important;
  transition: all 0.2s ease !important;

  &:hover {
    background: #ff7875 !important;
  }

  &:active {
    background: #d9363e !important;
  }
}
</style>
