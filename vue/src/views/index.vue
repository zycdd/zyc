<template>
  <el-container class="layout-container">
    <!-- 左侧菜单 -->
    <el-aside class="sidebar" :width="sidebarWidth">
      <!-- Logo区域 -->
      <div class="logo-section">
        <el-avatar :size="collapsed ? 32 : 40" class="logo-avatar">
          <el-icon><School /></el-icon>
        </el-avatar>
        <div v-if="!collapsed" class="logo-text">
          <span class="system-name">实习管理</span>
          <span class="system-desc">Management</span>
        </div>
      </div>

      <!-- 菜单 -->
      <el-menu
        :default-active="activeMenu"
        class="sidebar-menu"
        :collapse="collapsed"
        :collapse-transition="false"
        background-color="transparent"
        text-color="#8b949e"
        active-text-color="#ffffff"
        @select="handleMenuSelect"
      >
        <el-menu-item
          v-for="item in menuItems"
          :key="item.path"
          :index="item.path"
          class="menu-item"
        >
          <el-icon><component :is="item.icon" /></el-icon>
          <template #title>{{ item.name }}</template>
        </el-menu-item>
      </el-menu>

      <!-- 用户信息 -->
      <div class="user-section">
        <div class="user-info">
          <el-avatar :size="collapsed ? 32 : 36" class="user-avatar">
            {{ username.charAt(0).toUpperCase() }}
          </el-avatar>
          <div v-if="!collapsed" class="user-details">
            <span class="user-name">{{ username }}</span>
            <span class="user-role">{{ userRole === 'Admin' ? '管理员' : '用户' }}</span>
          </div>
        </div>
        
        <!-- 退出登录按钮 -->
        <el-button 
          v-if="!collapsed"
          text 
          @click="logout" 
          class="logout-btn"
          size="small"
        >
          <el-icon><SwitchButton /></el-icon>
          退出
        </el-button>
        
        <!-- 折叠状态下的退出按钮 -->
        <el-button 
          v-else
          text 
          @click="logout" 
          class="logout-btn-collapsed"
          size="small"
        >
          <el-icon><SwitchButton /></el-icon>
        </el-button>
      </div>
    </el-aside>

    <!-- 主内容区 -->
    <el-container class="main-container">
      <!-- 顶部导航 -->
      <el-header class="header" height="60px">
        <div class="header-left">
          <el-button
            text
            @click="toggleSidebar"
            class="collapse-btn"
          >
            <el-icon :size="18">
              <component :is="collapsed ? 'Expand' : 'Fold'" />
            </el-icon>
          </el-button>
          
          <el-breadcrumb separator="/" class="breadcrumb">
            <el-breadcrumb-item>首页</el-breadcrumb-item>
            <el-breadcrumb-item>{{ currentPageTitle }}</el-breadcrumb-item>
          </el-breadcrumb>
        </div>

        <div class="header-right">
          <el-space :size="16">
            <el-text class="date-info" type="info">
              <el-icon><Calendar /></el-icon>
              {{ currentDate }}
            </el-text>
          </el-space>
        </div>
      </el-header>

      <!-- 内容区域 -->
      <el-main class="main-content">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { 
  School, 
  Management, 
  User,
  Fold, 
  Expand, 
  Calendar, 
  SwitchButton 
} from '@element-plus/icons-vue'

export default {
  name: 'IndexLayout',
  components: {
    School,
    Management,
    User,
    Fold,
    Expand,
    Calendar,
    SwitchButton
  },
  setup() {
    const router = useRouter()
    const route = useRoute()
    const username = ref('')
    const userRole = ref('')
    const activeMenu = ref('/college')
    const currentDate = ref('')
    const collapsed = ref(false)
    
    const menuItems = ref([])
    
    // 计算侧边栏宽度
    const sidebarWidth = computed(() => collapsed.value ? '64px' : '240px')
    
    // 根据用户角色初始化菜单
    const initMenuByRole = () => {
      const role = localStorage.getItem('userRole') || ''
      userRole.value = role
      
      // 基础菜单（所有用户都有）
      const baseMenu = []
      
      // 管理员专属菜单
      const adminMenu = [
        { name: '学院管理', path: '/college', icon: 'Management' },
        { name: '专业管理', path: '/major', icon: 'Management' },
        { name: '用户管理', path: '/user', icon: 'User' },
        { name : '学期管理', path: '/term', icon: 'Management' },
        { name : '教师管理', path: '/teacher', icon: 'User' },
        { name : '学生管理',path:'/student',icon: 'User'},
        { name : '课程管理',path:'/course',icon: 'Management'}
      ]
      
      // 根据角色组合菜单 - 只有Role为Admin的用户才是管理员
      if (role === 'Admin') {
        menuItems.value = [...baseMenu, ...adminMenu]
      } else {
        menuItems.value = baseMenu
      }
      
      console.log('当前用户角色:', role, '菜单数量:', menuItems.value.length)
    }
    
    const currentPageTitle = computed(() => {
      const item = menuItems.value.find(m => m.path === activeMenu.value)
      return item ? item.name : '学院管理'
    })
    
    // 切换侧边栏
    const toggleSidebar = () => {
      collapsed.value = !collapsed.value
    }
    
    // 菜单选择
    const handleMenuSelect = (path) => {
      activeMenu.value = path
      router.push(path)
    }
    
    const logout = async () => {
      try {
        await ElMessageBox.confirm(
          '确定要退出登录吗？',
          '退出确认',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }
        )
        
        // 清除所有认证信息
        localStorage.removeItem('token')
        localStorage.removeItem('username')
        localStorage.removeItem('userRole')
        
        ElMessage.success('退出成功')
        
        // 跳转到登录页面
        router.push('/login')
      } catch (error) {
        // 用户取消退出
        console.log('用户取消退出')
      }
    }
    
    const updateDate = () => {
      const now = new Date()
      const year = now.getFullYear()
      const month = String(now.getMonth() + 1).padStart(2, '0')
      const day = String(now.getDate()).padStart(2, '0')
      currentDate.value = `${year}-${month}-${day}`
    }
    
    onMounted(() => {
      username.value = localStorage.getItem('username') || '用户'
      activeMenu.value = route.path
      updateDate()
      initMenuByRole()
    })
    
    return {
      username,
      userRole,
      menuItems,
      activeMenu,
      currentPageTitle,
      currentDate,
      collapsed,
      sidebarWidth,
      toggleSidebar,
      handleMenuSelect,
      logout,
      // 图标组件
      School,
      Management,
      User,
      Fold,
      Expand,
      Calendar,
      SwitchButton
    }
  }
}
</script>
<style scoped>
.layout-container {
  height: 100vh;
  background: #fafafa;
}

/* ========== 侧边栏 ========== */
.sidebar {
  background: #ffffff;
  border-right: 1px solid #e8e8e8;
  transition: width 0.3s ease;
  display: flex;
  flex-direction: column;
}

.logo-section {
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 12px;
  border-bottom: 1px solid #f0f0f0;
  min-height: 80px;
}

.logo-avatar {
  background: #1890ff;
  flex-shrink: 0;
}

.logo-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.system-name {
  font-size: 16px;
  font-weight: 600;
  color: #262626;
  line-height: 1.2;
}

.system-desc {
  font-size: 12px;
  color: #8c8c8c;
  line-height: 1.2;
}

/* 菜单样式 */
.sidebar-menu {
  flex: 1;
  border: none;
  padding: 8px 0;
}

:deep(.el-menu-item) {
  height: 48px;
  line-height: 48px;
  margin: 0 12px 4px;
  border-radius: 6px;
  color: #595959;
  transition: all 0.3s ease;
}

:deep(.el-menu-item:hover) {
  background-color: #f5f5f5;
  color: #262626;
}

:deep(.el-menu-item.is-active) {
  background-color: #e6f7ff;
  color: #1890ff;
  font-weight: 500;
}

:deep(.el-menu-item .el-icon) {
  margin-right: 8px;
  font-size: 16px;
}

/* 用户信息区域 */
.user-section {
  padding: 16px 20px;
  border-top: 1px solid #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

.user-avatar {
  background: linear-gradient(135deg, #1890ff, #096dd9);
  color: white;
  font-weight: 600;
  flex-shrink: 0;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
  flex: 1;
}

.user-name {
  font-size: 14px;
  font-weight: 500;
  color: #262626;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-role {
  font-size: 12px;
  color: #8c8c8c;
}

/* 退出登录按钮 */
.logout-btn {
  color: #ff4d4f;
  padding: 4px 8px;
  font-size: 12px;
  flex-shrink: 0;
}

.logout-btn:hover {
  background-color: #fff2f0;
  color: #ff4d4f;
}

.logout-btn-collapsed {
  color: #ff4d4f;
  padding: 6px;
  flex-shrink: 0;
}

.logout-btn-collapsed:hover {
  background-color: #fff2f0;
  color: #ff4d4f;
}

/* ========== 主容器 ========== */
.main-container {
  background: #fafafa;
}

/* 顶部导航 */
.header {
  background: #ffffff;
  border-bottom: 1px solid #e8e8e8;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.collapse-btn {
  color: #595959;
  padding: 8px;
}

.collapse-btn:hover {
  color: #262626;
  background-color: #f5f5f5;
}

.breadcrumb {
  font-size: 14px;
}

:deep(.el-breadcrumb__inner) {
  color: #8c8c8c;
  font-weight: normal;
}

:deep(.el-breadcrumb__inner.is-link) {
  color: #1890ff;
}

.header-right {
  display: flex;
  align-items: center;
}

.date-info {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  color: #8c8c8c;
}

/* 主内容区 */
.main-content {
  background: #fafafa;
  padding: 24px;
  overflow-y: auto;
}

/* ========== 下拉菜单样式 ========== */
:deep(.el-dropdown-menu__item) {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
}

/* ========== 响应式设计 ========== */
@media (max-width: 768px) {
  .header {
    padding: 0 16px;
  }
  
  .main-content {
    padding: 16px;
  }
  
  .breadcrumb {
    display: none;
  }
}

/* ========== 滚动条样式 ========== */
.main-content::-webkit-scrollbar {
  width: 6px;
}

.main-content::-webkit-scrollbar-track {
  background: #f0f0f0;
  border-radius: 3px;
}

.main-content::-webkit-scrollbar-thumb {
  background: #d9d9d9;
  border-radius: 3px;
}

.main-content::-webkit-scrollbar-thumb:hover {
  background: #bfbfbf;
}

/* ========== 动画效果 ========== */
.layout-container {
  animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
</style>
