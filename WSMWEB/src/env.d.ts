/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_APP_TITLE: string
    // 更多环境变量...
    readonly VITE_GLOB_API_URL: string

    readonly VITE_API_URL: string
    
  }
  
  interface ImportMeta {
    readonly env: ImportMetaEnv
  }