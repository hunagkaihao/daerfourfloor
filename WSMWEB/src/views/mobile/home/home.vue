<template>
  <div class="components-page-header-demo-content">
    <a-page-header class="custom-page-header" title="主页">
      <template #extra>
        <a-space class="user-space">
          <a-dropdown>
            <a class="user-dropdown-link" @click.prevent>
              <UserOutlined class="user-icon" />
              <span class="user-name">{{ getUserInfo.realName }}</span>
            </a>
            <template #overlay>
              <a-menu class="user-menu">
                <a-menu-item class="logout-item" @click="handleLoginOut">
                  <LogoutOutlined />
                  退出登录
                </a-menu-item>
              </a-menu>
            </template>
          </a-dropdown>
        </a-space>
      </template>
    </a-page-header>
    <a-tabs 
      v-model:activeKey="activeKey" 
      @change="tabclick"
                :tabBarStyle="{ 
                  background: 'linear-gradient(135deg, #1890ff 0%, #40a9ff 100%)', 
                  borderRadius: '12px 12px 0 0',
                  margin: '0 0 0 0',
                  boxShadow: '0 4px 20px rgba(24, 144, 255, 0.2)',
                  padding: '2px 4px 0 4px'
                }"
              :tabBarGutter="2"
      size="large"
      type="line"
      centered
    >

      <a-tab-pane key="1" tab="入库">
        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <a-col :span="8">
            <div class="menu-item-card" @click="boxdisk">
              <div class="menu-item-icon-wrapper">
                <AppstoreOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">容器组盘</div>
            </div>
          </a-col>
        </a-row>
        <!-- <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <a-col :span="8">
            <div class="menu-item-card" @click="boxbind">
              <div class="menu-item-icon-wrapper">
                <SafetyCertificateOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">容器绑定</div>
            </div>
          </a-col>
        </a-row>-->
        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <!--<a-col :span="8">
            <div class="menu-item-card" @click="boxIncell">
              <div class="menu-item-icon-wrapper">
                <InboxOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">容器入库</div>
            </div>
          </a-col>-->
          <a-col :span="8">
            <div class="menu-item-card" @click="createStockTask">
              <div class="menu-item-icon-wrapper">
                <InboxOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">创建入库任务</div>
            </div>
          </a-col>
        </a-row>
        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <a-col :span="8">
            <div class="menu-item-card" @click="containerUnbind">
              <div class="menu-item-icon-wrapper">
                <DisconnectOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">容器解绑</div>
            </div>
          </a-col>
        </a-row>

        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">



        </a-row>



      </a-tab-pane>
      <!-- 检验标签页已完全移除 -->
      <a-tab-pane key="2" tab="ASN管理" force-render>
        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <a-col :span="8">
            <div class="menu-item-card" @click="scanAsn">
              <div class="menu-item-icon-wrapper">
                <FileTextOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">扫描ASN</div>
            </div>
          </a-col>
          <a-col :span="8">
            <div class="menu-item-card" @click="scanOutboundOrder">
              <div class="menu-item-icon-wrapper">
                <ExportOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">扫描出库单</div>
            </div>
          </a-col>
        </a-row>
        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <a-col :span="8">
            <div class="menu-item-card" @click="boxdiskWithAsn">
              <div class="menu-item-icon-wrapper">
                <AppstoreOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">ASN组盘</div>
            </div>
          </a-col>
          <a-col :span="8">
            <div class="menu-item-card" @click="createOutboundTaskFromBarcode">
              <div class="menu-item-icon-wrapper">
                <InboxOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">创建出库任务</div>
            </div>
          </a-col>
        </a-row>
        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <a-col :span="8">
            <div class="menu-item-card" @click="materialSampling">
              <div class="menu-item-icon-wrapper">
                <AppstoreOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">物料抽检</div>
            </div>
          </a-col>
          <a-col :span="8">
              <div class="menu-item-card" @click="materialPartialOut">
                <div class="menu-item-icon-wrapper">
                  <GiftOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">拆箱出库</div>
              </div>
            </a-col>
        </a-row> 
        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <a-col :span="8">
            <div class="menu-item-card" @click="materialStatusChange">
              <div class="menu-item-icon-wrapper">
                <AppstoreOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">物料状态变更</div>
            </div>
          </a-col>
          <a-col :span="8">
              <div class="menu-item-card" @click="materialOut">
                <div class="menu-item-icon-wrapper">
                  <ExportOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">整箱出库</div>
              </div>
            </a-col>
        </a-row>
        <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
          <a-col :span="8">
            <div class="menu-item-card" @click="createStockTask">
              <div class="menu-item-icon-wrapper">
                <InboxOutlined class="menu-item-icon" />
              </div>
              <div class="menu-item-text">创建入库任务</div>
            </div>
          </a-col>
          <a-col :span="8">
            <!-- 暂时没有东西 -->
          </a-col>
        </a-row> 
      </a-tab-pane>

      <a-tab-pane key="3" tab="出库" force-render>
        <div>
          <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
            <a-col :span="8">
              <div class="menu-item-card" @click="createOutStockTask">
                <div class="menu-item-icon-wrapper">
                  <InboxOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">创建出库任务</div>
              </div>
            </a-col>
          </a-row>
        </div>

      </a-tab-pane>
      <a-tab-pane key="4" tab="出库管理" force-render>
        <div>
          <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
            <a-col :span="8">
              <div class="menu-item-card" @click="outStockOrder">
                <div class="menu-item-icon-wrapper">
                  <ExportOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">出库单出库</div>
              </div>
            </a-col>
            <a-col :span="8">
              <div class="menu-item-card" @click="outStockSort">
                <div class="menu-item-icon-wrapper">
                  <SortAscendingOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">出库分拣</div>
              </div>
            </a-col>
          </a-row>
          <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
            <a-col :span="8">
              <div class="menu-item-card" @click="materialOut">
                <div class="menu-item-icon-wrapper">
                  <ExportOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">整箱出库</div>
              </div>
            </a-col>
            <a-col :span="8">
              <div class="menu-item-card" @click="materialPartialOut">
                <div class="menu-item-icon-wrapper">
                  <GiftOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">拆箱出库</div>
              </div>
            </a-col>
          </a-row>
          <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
            <a-col :span="8">
              <div class="menu-item-card" @click="stockQuery">
                <div class="menu-item-icon-wrapper">
                  <DatabaseOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">库存查询</div>
              </div>
            </a-col>
          </a-row>
          <a-row justify="center" style="margin-top: 20px; margin-bottom: 20px">
            <a-col :span="8">
              <div class="menu-item-card" @click="createOutStockTaskSummary">
                <div class="menu-item-icon-wrapper">
                  <InboxOutlined class="menu-item-icon" />
                </div>
                <div class="menu-item-text">创建出库任务(汇总)</div>
              </div>
            </a-col>
          </a-row>
        </div>
      </a-tab-pane>
     
    </a-tabs>









  </div>
</template>
<script lang="ts" setup>
import {
  DownSquareTwoTone,
  SearchOutlined,
  SendOutlined,
  InboxOutlined,
  RollbackOutlined,
  LoginOutlined,
  AppstoreOutlined,
  SafetyCertificateOutlined,
  ShopOutlined,
  AuditOutlined,
  ExportOutlined,
  PackageOutlined,
  UnlockOutlined,
  GiftOutlined,
  UploadOutlined,
  DatabaseOutlined,
  UserOutlined,
  DeleteOutlined,
  ShareAltOutlined,
  SwapOutlined,
  DisconnectOutlined,
  GlobalOutlined,
  BoxPlotOutlined,
  LogoutOutlined,
  FileTextOutlined,
  SortAscendingOutlined,
} from '@ant-design/icons-vue';
import { computed, ref, onMounted } from 'vue';
import { router } from '/@/router';
// 移除API调用
// import { pickItemsCnt, recheckItemsCount } from './home';
import { useUserStore } from '/@/store/modules/user';
import { useViewStore } from '/@/store/modules/view';
const userStore = useUserStore();
const viewStore = useViewStore();
const getUserInfo = computed(() => {
  const { realName = '', avatar, desc } = userStore.getUserInfo || {};
  return { realName, avatar: avatar || desc };
});
const activeKey = ref('1');
console.log(viewStore.getTab)
// 确保tab值有效
activeKey.value = ['1', '2', '3', '4'].includes(viewStore.getTab) ? viewStore.getTab : '1'
function tabclick(key) {
  viewStore.setTab(key)
  console.log(viewStore.getTab)
}

const handleLoginOut = async () => {
  userStore.mobilelogout(true);
}
let recheckcount = ref();
const incellByPeople = async () => {
  await router.replace('/incellByPeople');
};

// incellByOverdue函数已删除
// overdueCall函数已删除

const acceptanceCall = async () => {
  await router.replace('/acceptanceCall');
};
const acceptanceCall2 = async () => {
  await router.replace('/acceptanceCall2');
};
// acceptanceOut2函数已删除
// acceptanceOut2test函数已删除
// shelfIncell函数已删除
// tiaoboIncell函数已删除
const boxbind = async () => {
  await router.replace('/boxBind');
};
const containerUnbind = async () => {
  await router.replace('/containerUnbind');
};
const boxdisk = async () => {
  await router.replace('/boxDisk');
};
const boxdiskWithAsn = async () => {
  await router.replace('/boxDiskWithAsn');
};
const scanAsn = async () => {
  await router.replace('/scanAsn');
};
const scanOutboundOrder = async () => {
  await router.replace('/deliveryOrderScan');
};

const createOutboundTaskFromBarcode = async () => {
  await router.replace('/createOutboundTaskFromBarcode');
};
const boxIncell = async () => {
  await router.replace('/boxIncell');
};
const createStockTask = async () => {
  await router.replace('/createStockTask');
};

const createOutStockTask = async () => {
  await router.replace('/createOutStockTask');
};

const outStockOrder = async () => {
  await router.replace('/outStockOrder');
};

const outStockSort = async () => {
  await router.replace('/outStockSort');
};

const materialOut = async () => {
  await router.replace('/materialOut');
};

const materialPartialOut = async () => {
  await router.replace('/materialPartialOut');
};

const materialSampling = async () => {
  await router.replace('/materialSampling');
};

const materialStatusChange = async () => {
  await router.replace('/materialStatusChange');
};

const agvIncell = async () => {
  await router.replace('/agvIncell');
};
// boxSelect函数已删除
// 已删除的函数：goodSpotCheck, handincell, createAgvtask, createAgvBacktask
const handoutcell = async () => {
  await router.replace('/handoutcell');
};
// handoutcelltest函数已删除
// emptyShelfEdit函数已删除
const checkNoGet = async () => {
  await router.replace('/checkNoGet');
};
// bindstation函数已删除
// dptiaobo函数已删除
// dpcall函数已删除

// zzoutcell函数已删除
// zzoutcelltest函数已删除
const goodsbind = async () => {
  await router.replace('/goodsBind');
};

const stockQuery = async () => {
  await router.replace('/stockQuery');
};

const createOutStockTaskSummary = async () => {
  await router.replace('/createOutStockTaskSummary');
};
// 已删除的函数：goodsbindtest, zzoutcelltest, handoutcelltest, emptyboxout, boxoutcell
// goodsdevan函数已删除
// goodsadd函数已删除
// tiaobo函数已删除
onMounted(async () => {
  // 移除API调用，不加载数据
  recheckcount.value = 0
})


</script>
<style scoped>
/* 自定义页面头部样式 */
.custom-page-header {
  background: transparent;
  border: none;
  border-radius: 0;
  margin-bottom: 2px;
  box-shadow: none;
  padding: 4px 24px;
}

::v-deep(.custom-page-header .ant-page-header-heading-title) {
  color: #1890ff;
  font-size: 16px;
  font-weight: 700;
  text-shadow: none;
  line-height: 1.1;
}

::v-deep(.custom-page-header .ant-page-header-heading-sub-title) {
  color: rgba(255,255,255,0.8);
}

/* 用户区域样式 */
.user-space {
  display: flex;
  align-items: center;
}

.user-dropdown-link {
  display: flex;
  align-items: center;
  gap: 2px;
  padding: 2px 6px;
  background: transparent;
  border: 2px solid #1890ff;
  border-radius: 12px;
  color: #1890ff;
  text-decoration: none;
  transition: all 0.3s ease;
}

.user-dropdown-link:hover {
  background: #1890ff;
  border-color: #1890ff;
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(24, 144, 255, 0.3);
}

.user-icon {
  font-size: 10px;
  color: #1890ff;
}

.user-name {
  font-weight: 600;
  font-size: 11px;
}

/* 下拉菜单样式 */
.user-menu {
  background: rgba(255, 255, 255, 0.95);
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.15);
  border: 1px solid rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  overflow: hidden;
}

.logout-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  color: #ff4d4f;
  font-weight: 500;
  transition: all 0.3s ease;
}

.logout-item:hover {
  background: rgba(255, 77, 79, 0.1);
  color: #ff4d4f;
}

::v-deep(.ant-page-header) {
  padding: 0 24px;
}

/* 现代化标签样式 */
::v-deep(.ant-tabs-nav) {
  margin: 0 !important;
}

::v-deep(.ant-tabs-nav-wrap) {
  padding: 0 16px;
}

::v-deep(.ant-tabs-nav .ant-tabs-tab) {
  background: rgba(255, 255, 255, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 10px 10px 0 0;
  margin: 0 1px;
  color: rgba(255, 255, 255, 0.9);
  font-weight: 600;
  font-size: 14px;
  padding: 6px 12px;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

::v-deep(.ant-tabs-nav .ant-tabs-tab::before) {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
  transition: left 0.5s;
}

::v-deep(.ant-tabs-nav .ant-tabs-tab:hover) {
  background: rgba(255, 255, 255, 0.25);
  border-color: rgba(255, 255, 255, 0.4);
  transform: translateY(-4px) scale(1.02);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
  color: white;
}

::v-deep(.ant-tabs-nav .ant-tabs-tab:hover::before) {
  left: 100%;
}

::v-deep(.ant-tabs-nav .ant-tabs-tab-active) {
  background: rgba(255, 255, 255, 0.95);
  border-color: rgba(255, 255, 255, 0.95);
  color: #1890ff;
  transform: translateY(-1px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
  font-weight: 700;
  border-radius: 12px 12px 0 0;
}

::v-deep(.ant-tabs-nav .ant-tabs-tab-active .ant-tabs-tab-btn) {
  color: #1890ff;
  font-weight: 700;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
}

::v-deep(.ant-tabs-ink-bar) {
  display: none !important;
}

            ::v-deep(.ant-tabs-content-holder) {
              background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
              border-radius: 0 0 12px 12px;
              box-shadow: 0 4px 20px rgba(0,0,0,0.08);
              padding: 16px 16px;
              margin-top: -1px;
              min-height: 520px;
            }

::v-deep(.ant-tabs-tabpane) {
  padding: 0;
}

/* 菜单项容器优化 */
::v-deep(.ant-row) {
  margin-bottom: 4px !important;
}

::v-deep(.ant-col) {
  padding: 0 6px;
  margin-bottom: 0px;
}

/* 菜单项卡片样式 */
.menu-item-card {
  background: linear-gradient(135deg, rgba(24, 144, 255, 0.05) 0%, rgba(64, 169, 255, 0.1) 100%);
  border-radius: 12px;
  padding: 8px 6px;
  text-align: center;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid rgba(24, 144, 255, 0.15);
  box-shadow: 0 2px 8px rgba(24, 144, 255, 0.08);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  position: relative;
  overflow: hidden;
}

.menu-item-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: linear-gradient(90deg, #1890ff, #40a9ff);
  transform: scaleX(0);
  transition: transform 0.3s ease;
}

.menu-item-card:hover {
  transform: translateY(-4px) scale(1.02);
  box-shadow: 0 8px 25px rgba(24, 144, 255, 0.2);
  border-color: rgba(24, 144, 255, 0.4);
  background: linear-gradient(135deg, rgba(24, 144, 255, 0.1) 0%, rgba(64, 169, 255, 0.2) 100%);
}

.menu-item-card:hover::before {
  transform: scaleX(1);
}

.menu-item-icon {
  font-size: 36px !important;
  color: #1890ff !important;
  border: 2px solid #1890ff !important;
  border-radius: 8px !important;
  padding: 6px !important;
  transition: all 0.3s ease !important;
  background: rgba(24, 144, 255, 0.08) !important;
  display: inline-block !important;
}

/* 入库相关图标 - 绿色主题 */
.menu-item-card:has(.menu-item-icon[class*="InboxOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="LoginOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="UploadOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="ShopOutlined"]) {
  background: linear-gradient(135deg, rgba(82, 196, 26, 0.05) 0%, rgba(135, 208, 104, 0.1) 100%);
  border-color: rgba(82, 196, 26, 0.15);
  box-shadow: 0 2px 8px rgba(82, 196, 26, 0.08);
}

.menu-item-card:has(.menu-item-icon[class*="InboxOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="LoginOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="UploadOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="ShopOutlined"]):hover {
  background: linear-gradient(135deg, rgba(82, 196, 26, 0.1) 0%, rgba(135, 208, 104, 0.2) 100%);
  border-color: rgba(82, 196, 26, 0.4);
  box-shadow: 0 8px 25px rgba(82, 196, 26, 0.2);
}

/* 出库相关图标 - 橙色主题 */
.menu-item-card:has(.menu-item-icon[class*="ExportOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="UserOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="DeleteOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="ShareAltOutlined"]) {
  background: linear-gradient(135deg, rgba(255, 152, 0, 0.05) 0%, rgba(255, 193, 7, 0.1) 100%);
  border-color: rgba(255, 152, 0, 0.15);
  box-shadow: 0 2px 8px rgba(255, 152, 0, 0.08);
}

.menu-item-card:has(.menu-item-icon[class*="ExportOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="UserOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="DeleteOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="ShareAltOutlined"]):hover {
  background: linear-gradient(135deg, rgba(255, 152, 0, 0.1) 0%, rgba(255, 193, 7, 0.2) 100%);
  border-color: rgba(255, 152, 0, 0.4);
  box-shadow: 0 8px 25px rgba(255, 152, 0, 0.2);
}

/* 查询相关图标 - 紫色主题 */
.menu-item-card:has(.menu-item-icon[class*="SearchOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="DatabaseOutlined"]) {
  background: linear-gradient(135deg, rgba(114, 46, 209, 0.05) 0%, rgba(156, 39, 176, 0.1) 100%);
  border-color: rgba(114, 46, 209, 0.15);
  box-shadow: 0 2px 8px rgba(114, 46, 209, 0.08);
}

.menu-item-card:has(.menu-item-icon[class*="SearchOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="DatabaseOutlined"]):hover {
  background: linear-gradient(135deg, rgba(114, 46, 209, 0.1) 0%, rgba(156, 39, 176, 0.2) 100%);
  border-color: rgba(114, 46, 209, 0.4);
  box-shadow: 0 8px 25px rgba(114, 46, 209, 0.2);
}

/* 调拨相关图标 - 青色主题 */
.menu-item-card:has(.menu-item-icon[class*="SwapOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="GlobalOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="RollbackOutlined"]) {
  background: linear-gradient(135deg, rgba(0, 188, 212, 0.05) 0%, rgba(0, 229, 255, 0.1) 100%);
  border-color: rgba(0, 188, 212, 0.15);
  box-shadow: 0 2px 8px rgba(0, 188, 212, 0.08);
}

.menu-item-card:has(.menu-item-icon[class*="SwapOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="GlobalOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="RollbackOutlined"]):hover {
  background: linear-gradient(135deg, rgba(0, 188, 212, 0.1) 0%, rgba(0, 229, 255, 0.2) 100%);
  border-color: rgba(0, 188, 212, 0.4);
  box-shadow: 0 8px 25px rgba(0, 188, 212, 0.2);
}

/* 确认相关图标 - 绿色主题 */
.menu-item-card:has(.menu-item-icon[class*="CheckCircleOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="SafetyCertificateOutlined"]) {
  background: linear-gradient(135deg, rgba(76, 175, 80, 0.05) 0%, rgba(139, 195, 74, 0.1) 100%);
  border-color: rgba(76, 175, 80, 0.15);
  box-shadow: 0 2px 8px rgba(76, 175, 80, 0.08);
}

.menu-item-card:has(.menu-item-icon[class*="CheckCircleOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="SafetyCertificateOutlined"]):hover {
  background: linear-gradient(135deg, rgba(76, 175, 80, 0.1) 0%, rgba(139, 195, 74, 0.2) 100%);
  border-color: rgba(76, 175, 80, 0.4);
  box-shadow: 0 8px 25px rgba(76, 175, 80, 0.2);
}

/* 运输相关图标 - 红色主题 */
.menu-item-card:has(.menu-item-icon[class*="SendOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="DisconnectOutlined"]) {
  background: linear-gradient(135deg, rgba(244, 67, 54, 0.05) 0%, rgba(255, 87, 34, 0.1) 100%);
  border-color: rgba(244, 67, 54, 0.15);
  box-shadow: 0 2px 8px rgba(244, 67, 54, 0.08);
}

.menu-item-card:has(.menu-item-icon[class*="SendOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="DisconnectOutlined"]):hover {
  background: linear-gradient(135deg, rgba(244, 67, 54, 0.1) 0%, rgba(255, 87, 34, 0.2) 100%);
  border-color: rgba(244, 67, 54, 0.4);
  box-shadow: 0 8px 25px rgba(244, 67, 54, 0.2);
}

/* 编辑相关图标 - 粉色主题 */
.menu-item-card:has(.menu-item-icon[class*="EditOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="UnlockOutlined"]) {
  background: linear-gradient(135deg, rgba(233, 30, 99, 0.05) 0%, rgba(255, 64, 129, 0.1) 100%);
  border-color: rgba(233, 30, 99, 0.15);
  box-shadow: 0 2px 8px rgba(233, 30, 99, 0.08);
}

.menu-item-card:has(.menu-item-icon[class*="EditOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="UnlockOutlined"]):hover {
  background: linear-gradient(135deg, rgba(233, 30, 99, 0.1) 0%, rgba(255, 64, 129, 0.2) 100%);
  border-color: rgba(233, 30, 99, 0.4);
  box-shadow: 0 8px 25px rgba(233, 30, 99, 0.2);
}

/* 特殊功能图标 - 深蓝色主题 */
.menu-item-card:has(.menu-item-icon[class*="AppstoreOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="BoxPlotOutlined"]),
.menu-item-card:has(.menu-item-icon[class*="GiftOutlined"]) {
  background: linear-gradient(135deg, rgba(33, 150, 243, 0.05) 0%, rgba(63, 81, 181, 0.1) 100%);
  border-color: rgba(33, 150, 243, 0.15);
  box-shadow: 0 2px 8px rgba(33, 150, 243, 0.08);
}

.menu-item-card:has(.menu-item-icon[class*="AppstoreOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="BoxPlotOutlined"]):hover,
.menu-item-card:has(.menu-item-icon[class*="GiftOutlined"]):hover {
  background: linear-gradient(135deg, rgba(33, 150, 243, 0.1) 0%, rgba(63, 81, 181, 0.2) 100%);
  border-color: rgba(33, 150, 243, 0.4);
  box-shadow: 0 8px 25px rgba(33, 150, 243, 0.2);
}

.menu-item-card:hover .menu-item-icon {
  transform: scale(1.1) !important;
  background: rgba(24, 144, 255, 0.1) !important;
  box-shadow: 0 4px 15px rgba(24, 144, 255, 0.2) !important;
}

.menu-item-text {
  font-size: 12px;
  font-weight: 600;
  color: #1890ff;
  margin-top: 4px;
  transition: all 0.3s ease;
}

.menu-item-card:hover .menu-item-text {
  color: #0050b3;
  transform: translateY(-2px);
}

/* 添加一些动画效果 */
@keyframes slideInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

::v-deep(.ant-tabs-tabpane-active) {
  animation: slideInUp 0.6s ease-out;
}
</style>