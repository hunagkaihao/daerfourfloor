<template>
  <LoginFormTitle v-show="getShow" class="enter-x" />
  <Form
    class="p-4 enter-x"
    :model="formData"
    :rules="getFormRules"
    ref="formRef"
    v-show="getShow"
    @keypress.enter="handleLogin"
  >
    <FormItem name="account" class="enter-x">
      <Input
        size="large"
        v-model:value="formData.account"
        :placeholder="t('sys.login.userName')"
        class="fix-auto-fill"
      />
    </FormItem>
    <FormItem name="password" class="enter-x">
      <InputPassword
        size="large"
        visibilityToggle
        v-model:value="formData.password"
        :placeholder="t('sys.login.password')"
      />
    </FormItem>

    <FormItem class="enter-x">
      <Button type="primary" size="large" block @click="handleLogin" :loading="loading">
        {{ t('sys.login.loginButton') }}
      </Button>
    </FormItem>
  </Form>
</template>
<script lang="ts" setup>
  import { reactive, ref, toRaw, unref, computed } from 'vue';
  import { Checkbox, Form, Input, Row, Col, Button, Divider } from 'ant-design-vue';
  import {
    GithubFilled,
    WechatFilled,
    AlipayCircleFilled,
    GoogleCircleFilled,
    TwitterCircleFilled,
  } from '@ant-design/icons-vue';
  import LoginFormTitle from './LoginFormTitle.vue';

  import { useI18n } from '/@/hooks/web/useI18n';
  import { useMessage } from '/@/hooks/web/useMessage';

  import { useUserStore } from '/@/store/modules/user';
  import {
    LoginStateEnum,
    useLoginState,
    useFormRules,
    useFormValid,
    useOidcLogin,
  } from './useLogin';
  import { useRouter } from 'vue-router';
  import { useDesign } from '/@/hooks/web/useDesign';
  //import { onKeyStroke } from '@vueuse/core';

  const ACol = Col;
  const ARow = Row;
  const FormItem = Form.Item;
  const InputPassword = Input.Password;
  const { t } = useI18n();
  const { notification, createErrorModal } = useMessage();
  const { prefixCls } = useDesign('login');
  const userStore = useUserStore();

  const { setLoginState, getLoginState } = useLoginState();
  const { getFormRules } = useFormRules();

  const formRef = ref();
  const loading = ref(false);
  const rememberMe = ref(false);

  const formData = reactive({
    account: '',
    password: '',
  });

  const { validForm } = useFormValid(formRef);

  //onKeyStroke('Enter', handleLogin);
  const usenameInfo = JSON.parse(window.sessionStorage.getItem('usenameInfo'))
    if (usenameInfo) {
        formData.account = usenameInfo.usename
      }
  const getShow = computed(() => unref(getLoginState) === LoginStateEnum.LOGIN);
  const route = useRouter()
  const stationAccount = route.currentRoute.value.params.id as string;
  console.log(stationAccount)
  userStore.setStationAccount(stationAccount);
      
  async function handleLogin() {
    const data = await validForm();
    if (!data) return;
    try {
      loading.value = true;
      const userInfo = await userStore.login(
        toRaw({
          password: data.password,
          username: data.account,
          tenantId: '',
          mode: 'none', //不要默认的错误提示
        })
      );
      // 1、设置条件
      const usenameInfo = {
          usename: data.account, // 这是当前的值
        }
        // 2、把它存储到 sessionStorage (使用JSON.stringify()将其转化为字符串)
        window.sessionStorage.setItem('usenameInfo', JSON.stringify(usenameInfo))
      // 移除登录成功提示
      // if (userInfo) {
      //   notification.success({
      //     message: t('sys.login.loginSuccessTitle'),
      //     description: `${t('sys.login.loginSuccessDesc')}: ${userInfo.realName}`,
      //     duration: 3,
      //   });
      // }
    } catch (error: any) {
      console.error('登录错误:', error);
      createErrorModal({
        title: t('sys.api.errorTip'),
        content: error?.message || error?.toString() || t('sys.api.networkExceptionMsg'),
        getContainer: () => document.body.querySelector(`.${prefixCls}`) || document.body,
      });
    } finally {
      loading.value = false;
    }
  }
</script>
