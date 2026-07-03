<template>
    <BasicModal title="出库单打印" :width="1000" :canFullscreen="false" :show-cancel-btn="false" :confirm-loading="confirmLoading" @cancel="cancel" @ok="submit"
      @register="registerModal" @visible-change="visibleChange" :destroyOnClose="true" :maskClosable="false">
      
      <section id="printJS-form" ref="textarea01">
            <div v-for="item in data" original-height="140" style="width: 200mm; height: 140mm; position: relative; 
              page-break-after: always;
              padding: 1mm 1mm 1mm 1mm;
              overflow-x: hidden;
              overflow: hidden;
              font-size: 14px;
              font-family: 微软雅黑;">
              <div>
                <div class="print-div" style="margin: 1mm;">
                  <div tabindex="1" class="hiprint-printElement hiprint-printElement-text"
                  style="position: absolute; width: 196mm; height: 39pt; font-family: 微软雅黑; font-size:15pt; text-align: center; line-height: 18pt; top: 24.5pt;">
                    <div class="hiprint-printElement-text-content hiprint-printElement-content"
                    style="height:100%;width:100%">东方综合库出库单</div>
                  </div>
                    
                    <div class="hiprint-printElement hiprint-printElement-table"
                style="position: absolute; width: 196mm;  top: 45pt;">
                <div class="hiprint-printElement-table-handle"></div>
                <div class="hiprint-printElement-table-content" style="height:100%;width:100%">
                  <!-- <a-table></a-table> -->
                 <table class="hiprint-printElement-tableTarget" style="border-collapse: collapse;width:100%; font-size:14px;">
                 
                    <tr>
                      <td width="15%" style="text-align: center;">出库编号:</td>
                      <td width="35%" style="text-align: center;">{{ item?.outboundListCode }}</td>
                      <td width="15%" style="text-align: center;">出库日期:</td>
                      <td width="35%" style="text-align: center;">{{ moment(item?.outboundDate).format("YYYY-MM-DD") }}</td>

                    </tr>
                    <tr>
                      <td width="15%" style="text-align: center;">领料单位:</td>
                      <td width="35%" style="text-align: center;">{{ item?.receivingUnit }}</td>
                      <td width="15%" style="text-align: center;">出库类型:</td>
                      <td width="35%" style="text-align: center;">{{ item?.type }}</td>

                    </tr>
                    
                 </table>
                  <table class="hiprint-printElement-tableTarget" style="border-collapse: collapse;width:100%;font-size:14px;">
                    <thead>
                      <tr>
                        <td  haswidth="haswidth" style="text-align: center; width: 25pt;">序号</td>
                        <td id="10" haswidth="haswidth" style="text-align: center; width: 75pt;">材料编号</td>
                        <td id="11" haswidth="haswidth" style="text-align: center; width: 75pt;">材料名称</td>
                        <td id="12" haswidth="haswidth" style="text-align: center;  width: 120pt;">规格型号</td>
                        <td id="13" haswidth="haswidth" style="text-align: center; width: 75pt;">生产批号</td>
                        <td id="13" haswidth="haswidth" style="text-align: center; width: 75pt;">成品名称</td>
                        <td id="14" haswidth="haswidth" style="text-align: center; width: 75pt;">保质期</td>
                        <td id="9" haswidth="haswidth" style="text-align: center; width: 30pt;">单位</td>
                        <td id="15" haswidth="haswidth" style="text-align: center; width: 55pt;">数量</td>
                        <td id="16" haswidth="haswidth" style="text-align: center; width: 75pt;">检验编号</td>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="i in item?.items.length">
                        <td style="text-align: center;">{{ i }}</td>
                        <td style="text-align: center;">{{ item?.items[i - 1]?.materialCode }}</td>
                        <td style="text-align: center;">{{ item?.items[i - 1]?.materialName }}</td>
                        <td style="text-align: center;">{{ item?.items[i - 1]?.specs }}</td>
                        <td style="text-align: center;">{{ item?.items[i - 1]?.batchNo }}</td>
                        <td style="text-align: center;">{{ item?.items[i - 1]?.cpName }}</td>
                        <td style="text-align: center;">{{ moment(item?.items[i - 1]?.expiryDate).format("YYYY-MM-DD") }}</td>
                        <td style="text-align: center;">{{ item?.items[i - 1]?.unit }}</td>
                        <td style="text-align: center;">{{ item?.items[i - 1]?.quantity }}</td>
                        <td style="text-align: center;">{{ item?.items[i - 1]?.checkNo }}</td>

                      </tr>
                     
            
                    </tbody>
                  </table>
                 
                </div>

              </div>

                
                
                </div>

               
              </div>
            </div>
  
      </section>

      <!-- <a-button type="primary" @click="jsonPrint1" style="margin-left: 700px; margin-top: 30px;">打印</a-button> -->
    </BasicModal>
  </template>
  <script lang="ts" setup>
  import { ref, defineProps } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import printJS from 'print-js';
  import hi from './hiprint.css';
  import pr from './print-lock.css';
  import moment from 'moment';
  let data = ref()

 const [registerModal, { changeOkLoading, closeModal }] = useModalInner((record)=>{
  data.value = record
  console.log(data.value.length)
});
  //用户id
  let userId = ''
  const props = defineProps({
    // 数据
    value: String,
    userId:String
  });
  const confirmLoading = ref<boolean>(false);

  const emit = defineEmits(['clickChild','print'])

  const visibleChange = async (visible: boolean) => {
      if (visible) {
      } else {
      }
  };
  
  





  
  
  
  const submit = async () => {
    confirmLoading.value = true;
      try{
        printJS({
          printable: 'printJS-form',
          type: 'html',
          style: hi + pr,
          scanStyles: false,

        });
      }catch{
  
      }finally{
        //emit('print')
        confirmLoading.value = false;
      }
  
  };
  const cancel = () => {
    closeModal();
  };

  
  let options = {
    text: props.value,
    displayValue: true,
    fontSize: 20,
    height: 40,
    width: 1,
  };
  
 
  const textarea01 = ref(null)

  </script>
  <style scoped>
  .print-div {
    padding: 8px;
    line-height: 12px;
  }
  table, th, td {
    border: 1px solid black; /* 设置边框宽度、样式和颜色 */
  }
  .fonts {
    font-size: 10px
  }
  

  </style>
  <style scoped src="./hiprint.css"></style>
  <style scoped src="./print-lock.css"></style>