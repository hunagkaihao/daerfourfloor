<template>
 <div class="components-page-header-demo-content">
      <a-page-header
        style="border: 1px solid rgb(235, 237, 240); "
        class="site-page-header"
        :title='numb'
        @back="() => $router.replace('/mobilehome')"
      >
      <template #extra> 
        <a-space style="line-height: 32px;" >
          <a-dropdown >
            <a class="ant-dropdown-link" @click.prevent>
              {{getUserInfo.realName}}              
            </a>
            <template #overlay>
              <a-menu>
                <a-menu-item>
                  <a @click="handleLoginOut">退出</a>
                </a-menu-item>
              </a-menu>
            </template>
          </a-dropdown> 
        </a-space>     
      </template>
      </a-page-header>
  </div>
</template>
<script lang="ts" setup>
import { defineComponent, defineProps, ref, computed, onMounted } from 'vue';
import { useUserStore } from '/@/store/modules/user';
import { useMessage } from '/@/hooks/web/useMessage';
defineProps({
		numb:{
			type:String,
		}
	})
//获取登录用户名
const userStore = useUserStore();
const { createConfirm } = useMessage();
const getUserInfo = computed(() => {
  const { realName = '', avatar, desc } = userStore.getUserInfo || {};
  return { realName, avatar: avatar || desc };
});
//退出登录
function handleLoginOut() {
  let msg = '是否确认退出系统?';
  createConfirm({
    iconType: 'warning',
    title: '温馨提醒',
    content: msg,
    onOk: async () => {
      userStore.mobilelogout(true);
    }
  })
}
</script>
<style scoped>
::v-deep(.ant-page-header){
  padding: 0 24px;
}
</style>