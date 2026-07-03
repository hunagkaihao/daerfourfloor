<template>

    <div class="components-input-demo-presuffix">
      <Header numb="物料查询"></Header>

  
      <a-row class="input-row">
        <a-col :span="6">
          <div class="htext">
            <h1>物料条形码:</h1>
          </div>
        </a-col>
        <a-col :span="17">
          <a-input v-model:value="boxCode" @keyup.enter="createAgv" placeholder="扫描物料条形码"  ref="Ref"
            :allowClear="true" class="modern-input">
            <template #prefix>
              <scan-outlined class="scan-icon" />
            </template>
            <template #suffix>
              <TableOutlined class="scan-icon"/>
            </template>
          </a-input>
        </a-col>
      </a-row>
  
  
  
  
      <a-card style="margin:5px" v-for="(i, index) in goods">
                <template #title>

                    物料:{{i.materialName}}{{i.specs}}

                </template>

                <a-row>
                    <a-col :span="12">
                        <p>物料编号:{{i.materialCode}}</p>
                        <p>计量单位:{{i.unit}}</p>
                        <p>检验编号:{{i.checkNo}}</p>


                    </a-col>
                    <a-col :span="12">
                        <p>领用生产批号:{{i.supplierCode}}</p>
                        <p>领用单位:{{i.supplierName}}</p>
                        <p>时间:{{i.date}}</p>

                    </a-col>
                </a-row>
            </a-card>
  
     
  

  

    </div>
    <div style="height: 49px;">
  
    </div>
    <div class="tab-bar">
  
  

      <a-button @click="clear"  type="primary" class="modern-btn">
        {{ t('清空') }}
      </a-button>
      <a-button @click="createAgv"  type="primary" class="modern-btn">
            {{ t('查询') }}
          </a-button>     
  
    </div>
  
    <CellSelectModal @register="registerCellSelectModal"></CellSelectModal>
</template>
<script lang="ts" setup>
  import { ScanOutlined } from '@ant-design/icons-vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import {  onMounted, ref, computed, reactive, nextTick, toRefs, h } from 'vue';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { router } from '/@/router';
  import Header from '../header/Header.vue'
  // 已删除的CellSelectModal.vue文件，使用简单的替代组件
const CellSelectModal = { template: '<div>功能已禁用</div>' };
  import { useUserStore } from '/@/store/modules/user';
  import { message } from 'ant-design-vue';
  import { useModal } from '/@/components/Modal';
  // 已删除的CellSelectModal.ts文件，使用内联实现
const stockMerge = async () => {
  message.info('功能已禁用');
  return null;
};
const barcodeGet = async () => {
  message.info('功能已禁用');
  return null;
};
  import moment from 'moment';
  const { createConfirm } = useMessage();
  const { t } = useI18n();

  let boxCode = ref<string>('');
  let goods = ref([])
  let newBoxCode = ref();

  const userStore = useUserStore();
  const [registerCellSelectModal, { openModal: openCellSelectModal }] = useModal();

  

  
 
  
  function clear(){
    boxCode.value = ''
    goods.value.length = 0
    Ref.value.focus()
  }
  
 
  //下达紧急agv任务
  async function createAgv() {
    if(boxCode.value == ''){
      message.error("请输入物料条形码")
      clear()
      return
    }
    try{
      let res =   await barcodeGet(boxCode.value)
        message.success("操作成功")
        goods.value[0] = res
        goods.value[0].date = moment(goods.value[0].date).format('YYYY-MM-DD HH:mm:ss')
    }catch(error){
        message.error(error.error.message)
    }
    
  
  }

  
  
  
  //获取焦点
  const Ref = ref()
  const Ref1 = ref()
  
  nextTick(() => {
    Ref.value.focus()
  })
  

  onMounted(async()=>{
    
  })
  </script>
<style scoped lang="less">
  
  /* 输入行样式 */
  .input-row {
      margin: 10px 0;
      padding: 0 16px;
  }
  
  /* 文字标签样式 */
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
  
  /* 现代化输入框样式 */
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
  
  /* 扫描图标样式 */
  .scan-icon {
      color: #1890ff !important;
      font-size: 18px;
  }
  
  /* 现代化按钮样式 */
  .modern-btn {
      margin: 0 5px;
      height: 32px !important;
      border-radius: 6px !important;
      background: #1890ff !important;
      border: none !important;
      font-size: 14px !important;
      font-weight: 500 !important;
      color: white !important;
      box-shadow: none !important;
      transition: all 0.2s ease !important;
      
      &:hover {
          background: #40a9ff !important;
          box-shadow: none !important;
      }
      
      &:active {
          background: #096dd9 !important;
      }
  }
  
  /* 底部操作栏样式 */
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
      box-shadow: none;
      padding: 0 16px;
      justify-content: center;
  }
  
  ::v-deep(.ant-table-thead > tr > th) {
    padding: 5px 0px;
  
  }
  
  ::v-deep(.ant-table-tbody > tr > td) {
    padding: 5px 0px;
  }
  
  ::v-deep(.ant-card-head) {
    padding: 0px;
    font-size: 14px;
    min-height: 0px;
  }
  
  ::v-deep(.ant-card-head-title) {
    padding: 0px;
    white-space: normal;
  }
  
  ::v-deep(.ant-card-extra) {
    padding: 0px;
  }
  
  ::v-deep(.ant-card-body) {
    padding: 0 2px;
  }
  
  ::v-deep(.ant-input-number-input) {
    height: 22px;
  }
  
  ::v-deep(.ant-table-header.ant-table-hide-scrollbar) {
    margin-bottom: -20px;
    padding-bottom: 10px;
    overflow: scroll;
    opacity: 1;
  }
  
  ::v-deep(.ant-table-hide-scrollbar) {
    scrollbar-color: initial !important;
  }
  
  ::v-deep(.ant-table-placeholder) {
    padding: 0 0px;
  }
  p {
    margin-bottom: 0em
  }
  </style>
  