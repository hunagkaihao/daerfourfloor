<template>
  <div :class="prefixCls" class="relative w-full h-full px-4">
    <AppLocalePicker
      class="absolute text-white top-4 right-4 enter-x xl:text-gray-600"
      :showText="false"
      v-if="!sessionTimeout && showLocale"
    />
    <AppDarkModeToggle class="absolute top-3 right-7 enter-x" v-if="!sessionTimeout" />

    <span class="-enter-x xl:hidden">
      <AppLogo :alwaysShowTitle="true" />
    </span>

    <div class="container relative h-full py-2 mx-auto sm:px-10">
      <div class="flex h-full">
        <div class="hidden min-h-full pl-4 mr-4 xl:flex xl:flex-col xl:w-6/12">
          <AppLogo class="-enter-x" />
          <div class="my-auto">
            <img
              :alt="title"
              src="../../../assets/svg/login-box-bg.svg"
              class="w-1/2 -mt-16 -enter-x"
            />
            <div class="mt-10 font-medium text-white -enter-x">
              <span class="inline-block mt-4 text-3xl">PDA（WMS系统）</span>
            </div>
            <div class="mt-5 font-normal text-white text-md dark:text-gray-500 -enter-x">
              {{ t('sys.login.signInDesc') }}
            </div>
          </div>
        </div>
        <div class="flex w-full h-full py-5 xl:h-auto xl:py-0 xl:my-0 xl:w-6/12">
          <div
            :class="`${prefixCls}-form`"
            class="
              relative
              w-full
              px-5
              py-8
              mx-auto
              my-auto
              rounded-md
              shadow-md
              xl:ml-16 xl:bg-transparent
              sm:px-8
              xl:p-4 xl:shadow-none
              sm:w-3/4
              lg:w-2/4
              xl:w-auto
              enter-x
            "
          >
            <LoginForm />
            <ForgetPasswordForm />
            <RegisterForm />
            <MobileForm />
            <QrCodeForm />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { computed } from 'vue';
  import { AppLogo } from '/@/components/Application';
  import { AppLocalePicker, AppDarkModeToggle } from '/@/components/Application';
  import LoginForm from './LoginForm.vue';
  import ForgetPasswordForm from '/@/views/sys/login/ForgetPasswordForm.vue';
  import RegisterForm from '/@/views/sys/login/RegisterForm.vue';
  import MobileForm from '/@/views/sys/login/MobileForm.vue';
  import QrCodeForm from '/@/views/sys/login/QrCodeForm.vue';
  import { useGlobSetting } from '/@/hooks/setting';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useDesign } from '/@/hooks/web/useDesign';
  import { useLocaleStore } from '/@/store/modules/locale';

  defineProps({
    sessionTimeout: {
      type: Boolean,
    },
  });

  const globSetting = useGlobSetting();
  const { prefixCls } = useDesign('login');
  const { t } = useI18n();
  const localeStore = useLocaleStore();
  const showLocale = localeStore.getShowPicker;
  const title = computed(() => globSetting?.title ?? '');
</script>
<style lang="less">
  @prefix-cls: ~'@{namespace}-login';
  @logo-prefix-cls: ~'@{namespace}-app-logo';
  @countdown-prefix-cls: ~'@{namespace}-countdown-input';
  @dark-bg: #293146;

  html[data-theme='dark'] {
    .@{prefix-cls} {
      background-color: @dark-bg;

      &::before {
        background-image: url(/@/assets/svg/login-bg-dark.svg);
      }

      .ant-input,
      .ant-input-password {
        background-color: #232a3b;
      }

      .ant-btn:not(.ant-btn-link):not(.ant-btn-primary) {
        border: 1px solid #4a5569;
      }

      &-form {
        background: transparent !important;
      }

      .app-iconify {
        color: #fff;
      }
    }

    input.fix-auto-fill,
    .fix-auto-fill input {
      -webkit-text-fill-color: #c9d1d9 !important;
      box-shadow: inherit !important;
    }
  }

  .@{prefix-cls} {
    min-height: 100%;
    overflow: hidden;
    background: 
      radial-gradient(2px 2px at 20px 30px, #eee, transparent),
      radial-gradient(2px 2px at 40px 70px, rgba(255,255,255,0.8), transparent),
      radial-gradient(1px 1px at 90px 40px, #fff, transparent),
      radial-gradient(1px 1px at 130px 80px, rgba(255,255,255,0.6), transparent),
      radial-gradient(2px 2px at 160px 30px, #ddd, transparent),
      radial-gradient(1px 1px at 200px 60px, rgba(255,255,255,0.9), transparent),
      radial-gradient(1px 1px at 240px 20px, #fff, transparent),
      radial-gradient(1px 1px at 280px 70px, rgba(255,255,255,0.7), transparent),
      radial-gradient(1px 1px at 320px 40px, #eee, transparent),
      radial-gradient(1px 1px at 360px 80px, rgba(255,255,255,0.8), transparent),
      radial-gradient(1px 1px at 400px 20px, #fff, transparent),
      radial-gradient(1px 1px at 440px 60px, rgba(255,255,255,0.6), transparent),
      radial-gradient(1px 1px at 480px 30px, #ddd, transparent),
      radial-gradient(1px 1px at 520px 70px, rgba(255,255,255,0.9), transparent),
      radial-gradient(1px 1px at 560px 50px, #fff, transparent),
      radial-gradient(1px 1px at 600px 10px, rgba(255,255,255,0.7), transparent),
      linear-gradient(135deg, #0c0c0c 0%, #1a1a2e 25%, #16213e 50%, #0f3460 75%, #0c0c0c 100%);
    background-size: 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 600px 600px, 100% 100%;
    background-position: 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0, 0 0;

    .@{prefix-cls}-form {
      background: rgba(0, 0, 0, 0.3);
      backdrop-filter: blur(20px);
      -webkit-backdrop-filter: blur(20px);
      border: 1px solid rgba(255, 255, 255, 0.2);
      box-shadow: 
        0 8px 32px rgba(0, 0, 0, 0.4),
        inset 0 1px 0 rgba(255, 255, 255, 0.1),
        0 0 0 1px rgba(255, 255, 255, 0.05);
      border-radius: 16px;
    }

    &::before {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      margin-left: -48%;
      background-image: url(/@/assets/svg/login-bg.svg);
      background-position: 100%;
      background-repeat: no-repeat;
      background-size: auto 100%;
      content: '';
      display: none;
    }

    .@{logo-prefix-cls} {
      position: absolute;
      top: 12px;
      height: 30px;

      &__title {
        font-size: 16px;
        color: #fff;
      }

      img {
        width: 32px;
      }
    }

    .container {
      .@{logo-prefix-cls} {
        display: flex;
        width: 60%;
        height: 80px;

        &__title {
          font-size: 24px;
          color: #fff;
        }

        img {
          width: 48px;
        }
      }
    }

    &-sign-in-way {
      .anticon {
        font-size: 22px;
        color: #888;
        cursor: pointer;

        &:hover {
          color: @primary-color;
        }
      }
    }

    input:not([type='checkbox']) {
      min-width: 360px;
      border-radius: 12px !important;
      border: 1px solid rgba(255, 255, 255, 0.4) !important;
      background: rgba(0, 0, 0, 0.2) !important;
      backdrop-filter: blur(15px);
      -webkit-backdrop-filter: blur(15px);
      transition: all 0.3s ease !important;
      color: white !important;
      font-weight: 500 !important;

      &:focus {
        border-color: rgba(255, 255, 255, 0.8) !important;
        background: rgba(0, 0, 0, 0.3) !important;
        box-shadow: 0 0 20px rgba(255, 255, 255, 0.3) !important;
      }

      &::placeholder {
        color: rgba(255, 255, 255, 0.8) !important;
      }

      @media (max-width: @screen-xl) {
        min-width: 320px;
      }

      @media (max-width: @screen-lg) {
        min-width: 260px;
      }

      @media (max-width: @screen-md) {
        min-width: 240px;
      }

      @media (max-width: @screen-sm) {
        min-width: 160px;
      }
    }

    .@{countdown-prefix-cls} input {
      min-width: unset;
      border-radius: 12px !important;
    }

    /* 密码输入框特殊样式 - 强制覆盖 */
    .ant-input-password,
    .ant-input-password * {
      border-radius: 12px !important;
      border: 1px solid rgba(255, 255, 255, 0.4) !important;
      background: rgba(0, 0, 0, 0.2) !important;
      backdrop-filter: blur(15px) !important;
      -webkit-backdrop-filter: blur(15px) !important;
      transition: all 0.3s ease !important;
    }
    
    .ant-input-password .ant-input,
    .ant-input-password input {
      border-radius: 12px !important;
      background: rgba(0, 0, 0, 0.2) !important;
      border: none !important;
      color: white !important;
      font-weight: 500 !important;
      box-shadow: none !important;
    }
    
    .ant-input-password .ant-input-suffix,
    .ant-input-password .ant-input-suffix * {
      color: rgba(255, 255, 255, 0.8) !important;
      background: transparent !important;
    }

    .ant-input-password:focus-within,
    .ant-input-password.ant-input-focused {
      border-color: rgba(255, 255, 255, 0.8) !important;
      background: rgba(0, 0, 0, 0.3) !important;
      box-shadow: 0 0 20px rgba(255, 255, 255, 0.3) !important;
      
      .ant-input,
      input {
        background: rgba(0, 0, 0, 0.3) !important;
      }
    }

    .ant-divider-inner-text {
      font-size: 12px;
      color: @text-color-secondary;
    }

    /* 登录按钮样式优化 - 星空主题 */
    .ant-btn-primary {
      border-radius: 12px !important;
      background: linear-gradient(135deg, #1e3c72 0%, #2a5298 50%, #1e3c72 100%) !important;
      border: 1px solid rgba(255, 255, 255, 0.3) !important;
      height: 48px !important;
      font-weight: 600 !important;
      transition: all 0.3s ease !important;
      box-shadow: 0 4px 15px rgba(30, 60, 114, 0.4) !important;
      color: white !important;

      &:hover {
        background: linear-gradient(135deg, #2a4a7c 0%, #3a5f9f 50%, #2a4a7c 100%) !important;
        transform: translateY(-2px) !important;
        box-shadow: 0 8px 25px rgba(30, 60, 114, 0.6) !important;
        border-color: rgba(255, 255, 255, 0.5) !important;
      }

      &:active {
        transform: translateY(0) !important;
      }
    }
  }

  /* 星空背景保持静态，移除动画效果 */

  /* 星空装饰元素 - 静态效果 */
  .@{prefix-cls}::after {
    content: '';
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: 
      radial-gradient(circle at 15% 25%, rgba(255, 255, 255, 0.1) 0%, transparent 50%),
      radial-gradient(circle at 85% 75%, rgba(255, 255, 255, 0.08) 0%, transparent 50%),
      radial-gradient(circle at 45% 65%, rgba(255, 255, 255, 0.06) 0%, transparent 50%),
      radial-gradient(circle at 75% 35%, rgba(255, 255, 255, 0.05) 0%, transparent 50%);
    pointer-events: none;
    z-index: 0;
  }

  .@{prefix-cls}-form {
    position: relative;
    z-index: 1;
    transition: all 0.3s ease;
  }

  .@{prefix-cls}-form:hover {
    background: rgba(0, 0, 0, 0.4);
    border-color: rgba(255, 255, 255, 0.3);
    transform: translateY(-2px);
    box-shadow: 
      0 12px 40px rgba(0, 0, 0, 0.5),
      inset 0 1px 0 rgba(255, 255, 255, 0.2),
      0 0 20px rgba(255, 255, 255, 0.1);
  }

  /* 星空背景保持静态，移除网格装饰 */

  /* 登录标题样式 - 星空主题 */
  .@{prefix-cls} h2 {
    color: white !important;
    text-shadow: 0 2px 4px rgba(0, 0, 0, 0.5);
  }

  /* 星空背景保持静态，移除所有动画效果 */
</style>
