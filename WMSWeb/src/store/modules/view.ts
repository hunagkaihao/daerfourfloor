import { defineStore } from 'pinia';
import { store } from '/@/store';
interface ViewState {
    tab: string;
}
export const useViewStore = defineStore({
    id: 'app-view',
    state: (): ViewState => ({
      tab: "1"
    }),
    getters: {
      getTab(): string {
        //console.log(this.tab)
        return this.tab ;
      },


    },
    actions: {
      setTab(info: string) {
        this.tab = info;
      },
      
  
    },
  });
  
  // Need to be used outside the setup
  export function useViewStoreWithOut() {
    return useViewStore(store);
  }