<template>
  <div class="mobile-browser-container">
    <!-- 浏览器模拟器框架 -->
    <div class="mobile-browser">
      <!-- 手机边框 -->
      <div class="phone-frame">
         <!-- 浏览器工具栏 - 隐藏 -->
         <div class="browser-toolbar" style="display: none;">
           <div class="toolbar-left">
             <a-button 
               type="text" 
               size="small" 
               @click="goBack"
               :disabled="!canGoBack"
             >
               <template #icon>
                 <ArrowLeftOutlined />
               </template>
             </a-button>
             <a-button 
               type="text" 
               size="small" 
               @click="goForward"
               :disabled="!canGoForward"
             >
               <template #icon>
                 <ArrowRightOutlined />
               </template>
             </a-button>
             <a-button 
               type="text" 
               size="small" 
               @click="refresh"
             >
               <template #icon>
                 <ReloadOutlined />
               </template>
             </a-button>
           </div>
           
           <div class="toolbar-center">
             <div class="address-bar">
               <GlobalOutlined class="address-icon" />
               <a-input
                 v-model:value="currentUrl"
                 class="address-input"
                 placeholder="输入网址..."
                 @pressEnter="navigateToUrl"
                 @blur="navigateToUrl"
               />
               <a-button 
                 type="text" 
                 size="small" 
                 @click="navigateToUrl"
                 class="go-button"
               >
                 <template #icon>
                   <RightOutlined />
                 </template>
               </a-button>
             </div>
           </div>
           
           <div class="toolbar-right">
             <a-button 
               type="text" 
               size="small" 
               @click="toggleFullscreen"
             >
               <template #icon>
                 <FullscreenOutlined v-if="!isFullscreen" />
                 <FullscreenExitOutlined v-else />
               </template>
             </a-button>
           </div>
         </div>
        
        <!-- 手机屏幕内容区域 -->
        <div class="phone-screen" :class="{ 'fullscreen': isFullscreen }">
          <div class="screen-content">
            <!-- 使用 iframe 加载移动端页面 -->
            <iframe
              ref="mobileFrame"
              :src="iframeUrl"
              frameborder="0"
              class="mobile-iframe"
              @load="onIframeLoad"
              @error="onIframeError"
              sandbox="allow-same-origin allow-scripts allow-forms allow-popups allow-top-navigation allow-downloads"
            ></iframe>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue';
import { useRouter } from 'vue-router';
import {
  ArrowLeftOutlined,
  ArrowRightOutlined,
  ReloadOutlined,
  GlobalOutlined,
  FullscreenOutlined,
  FullscreenExitOutlined,
  RightOutlined,
} from '@ant-design/icons-vue';

const router = useRouter();

// 响应式数据
const mobileFrame = ref<HTMLIFrameElement>();
const isFullscreen = ref(false);
const canGoBack = ref(false);
const canGoForward = ref(false);
const currentUrl = ref('http://localhost:4200/#/mobilehome');

// 计算属性
const iframeUrl = computed(() => {
  let url = currentUrl.value;
  
  // 如果是外部网址（包含 . 或 :），直接使用
  if (url.includes('.') || url.includes(':')) {
    // 如果以 http 或 https 开头，直接使用
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    // 否则添加 https:// 前缀
    return `https://${url}`;
  }
  
  // 内部路由，如果没有以 / 开头，添加 /
  if (!url.startsWith('/')) {
    url = '/' + url;
  }
  
  return url;
});

// 方法
const goBack = () => {
  if (mobileFrame.value && mobileFrame.value.contentWindow) {
    mobileFrame.value.contentWindow.history.back();
  }
};

const goForward = () => {
  if (mobileFrame.value && mobileFrame.value.contentWindow) {
    mobileFrame.value.contentWindow.history.forward();
  }
};

const refresh = () => {
  if (mobileFrame.value) {
    mobileFrame.value.src = mobileFrame.value.src;
  }
};

const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value;
};

const navigateToUrl = () => {
  // 触发 iframe 重新加载
  if (mobileFrame.value) {
    mobileFrame.value.src = mobileFrame.value.src;
  }
  console.log('导航到:', currentUrl.value);
};

const setUrl = (url: string) => {
  currentUrl.value = url;
  navigateToUrl();
};

const onIframeLoad = () => {
  console.log('移动端页面加载完成');
  // 可以在这里添加页面加载完成后的逻辑
};

const onIframeError = (error: any) => {
  console.error('移动端页面加载失败:', error);
  // 可以在这里添加错误处理逻辑
};

onMounted(() => {
  nextTick(() => {
    // 初始化时的一些设置
    console.log('移动端浏览器模拟器已加载');
    console.log('iframe URL:', iframeUrl.value);
    
    // 延迟检查 iframe 是否加载成功
    setTimeout(() => {
      if (mobileFrame.value) {
        try {
          const iframeDoc = mobileFrame.value.contentDocument || mobileFrame.value.contentWindow?.document;
          if (iframeDoc) {
            console.log('iframe 内容加载成功');
          } else {
            console.log('iframe 内容可能未加载或存在跨域问题');
          }
        } catch (error) {
          console.log('iframe 跨域访问被阻止，这是正常的:', error);
        }
      }
    }, 2000);
  });
});
</script>

<style lang="less" scoped>
.mobile-browser-container {
  padding: 20px;
  background: #f5f5f5;
  height: 100vh;
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  overflow: auto;
  box-sizing: border-box;
}

.mobile-browser {
  width: 100%;
  height: calc(850vh - 40px);
  max-width: none;
  max-height: calc(85vh - 40px);
}

.phone-frame {
  background: #2c3e50;
  border-radius: 15px;
  padding: 0;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  position: relative;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  
  &::before {
    display: none;
  }
}

.browser-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.95);
  border-radius: 15px 15px 0 0;
  margin-bottom: 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  flex-shrink: 0;
  
  .toolbar-left,
  .toolbar-right {
    display: flex;
    gap: 4px;
    
    .ant-btn {
      border: none;
      box-shadow: none;
      
      &:hover {
        background: rgba(0, 0, 0, 0.05);
      }
    }
  }
  
  .toolbar-center {
    flex: 1;
    margin: 0 12px;
  }
  
  .address-bar {
    display: flex;
    align-items: center;
    background: #f8f9fa;
    border: 1px solid #e9ecef;
    border-radius: 8px;
    padding: 2px 8px;
    font-size: 12px;
    
    .address-icon {
      color: #6c757d;
      margin-right: 6px;
    }
    
    .address-input {
      flex: 1;
      border: none;
      background: transparent;
      font-size: 12px;
      color: #495057;
      
      &:focus {
        box-shadow: none;
      }
      
      .ant-input {
        border: none;
        background: transparent;
        font-size: 12px;
        color: #495057;
        
        &:focus {
          box-shadow: none;
        }
      }
    }
    
    .go-button {
      margin-left: 4px;
      padding: 0 4px;
      height: 20px;
      min-width: 20px;
      
      .anticon {
        font-size: 10px;
      }
    }
  }
}

.phone-screen {
  background: #000;
  border-radius: 15px;
  overflow: auto;
  transition: all 0.3s ease;
  flex: 1;
  height: auto;
  
  /* 隐藏滚动条但保持滚动功能 */
  scrollbar-width: none; /* Firefox */
  -ms-overflow-style: none; /* IE and Edge */
  
  &::-webkit-scrollbar {
    display: none; /* Chrome, Safari, Opera */
  }
  
  &.fullscreen {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    height: 100vh;
    border-radius: 0;
    z-index: 9999;
    background: #000;
  }
}

.screen-content {
  width: 100%;
  height: 100%;
  position: relative;
}

.mobile-iframe {
  width: 100%;
  height: 100%;
  border: none;
  background: #fff;
  overflow: auto;
  
  /* 隐藏iframe滚动条但保持滚动功能 */
  scrollbar-width: none; /* Firefox */
  -ms-overflow-style: none; /* IE and Edge */
}

// 响应式设计
@media (max-width: 1200px) {
  .mobile-browser-container {
    padding: 15px;
  }
  
  .mobile-browser {
    height: calc(100vh - 50px);
    max-height: calc(100vh - 50px);
  }
}

@media (max-width: 768px) {
  .mobile-browser-container {
    padding: 10px;
  }
  
  .mobile-browser {
    height: calc(100vh - 40px);
    max-height: calc(100vh - 40px);
  }
  
  .browser-toolbar {
    padding: 6px 8px;
    
    .address-bar {
      font-size: 11px;
      padding: 3px 6px;
    }
  }
}

@media (max-width: 480px) {
  .mobile-browser-container {
    padding: 5px;
  }
  
  .mobile-browser {
    height: calc(100vh - 30px);
    max-height: calc(100vh - 30px);
  }
  
  .browser-toolbar {
    .toolbar-left,
    .toolbar-right {
      gap: 2px;
    }
    
    .toolbar-center {
      margin: 0 8px;
    }
  }
}
</style>
