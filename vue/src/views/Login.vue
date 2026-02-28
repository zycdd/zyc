<template>
  <div class="login-container">
    <!-- 左侧插图区域 -->
    <div class="login-left">
      <div class="brand-section">
        <div class="brand-logo">
          <el-icon :size="32" class="logo-icon">
            <School />
          </el-icon>
          <span class="brand-name">实习管理系统</span>
        </div>
        
  
      </div>

      <!-- 插图区域 -->
      <div class="illustration-section">
        <div class="computer-illustration">
          <div class="monitor">
            <div class="screen">
              <div class="screen-content">
                <div class="window-bar">
                  <div class="window-dots">
                    <span class="dot red"></span>
                    <span class="dot yellow"></span>
                    <span class="dot green"></span>
                  </div>
                </div>
                <div class="screen-body">
                  <div class="chart-line"></div>
                  <div class="chart-bars">
                    <div class="bar" style="height: 60%"></div>
                    <div class="bar" style="height: 80%"></div>
                    <div class="bar" style="height: 40%"></div>
                    <div class="bar" style="height: 90%"></div>
                  </div>
                </div>
              </div>
            </div>
            <div class="monitor-stand"></div>
            <div class="monitor-base"></div>
          </div>
          
          <!-- 装饰元素 -->
          <div class="decoration-plant left-plant">
            <div class="pot"></div>
            <div class="leaves">
              <div class="leaf leaf-1"></div>
              <div class="leaf leaf-2"></div>
              <div class="leaf leaf-3"></div>
            </div>
          </div>
          
          <div class="decoration-plant right-plant">
            <div class="pot"></div>
            <div class="leaves">
              <div class="leaf leaf-1"></div>
              <div class="leaf leaf-2"></div>
            </div>
          </div>
        </div>

        <div class="text-section">
        
      
        </div>
      </div>
    </div>

    <!-- 右侧登录表单区域 -->
    <div class="login-right">
      <div class="login-form-container">
        <div class="form-header">
          <h2 class="form-title">登录</h2>
        </div>

        <el-form
          ref="loginFormRef"
          :model="loginForm"
          :rules="loginRules"
          class="login-form"
          size="large"
        >
          <el-form-item prop="username">
            <el-input
              v-model="loginForm.username"
              placeholder="vben"
              clearable
            >
              <template #prefix>
                <el-icon><User /></el-icon>
              </template>
            </el-input>
          </el-form-item>

          <el-form-item prop="password">
            <el-input
              v-model="loginForm.password"
              type="password"
              placeholder="123456"
              show-password
              clearable
              @keyup.enter="handleLogin"
            >
              <template #prefix>
                <el-icon><Lock /></el-icon>
              </template>
            </el-input>
          </el-form-item>

          <div class="form-options">
            <el-checkbox v-model="rememberMe">记住我</el-checkbox>
          </div>

          <el-button
            type="primary"
            class="login-button"
            :loading="loading"
            @click="handleLogin"
          >
            登录
          </el-button>
        </el-form>

        <!-- 错误提示 -->
        <el-alert
          v-if="errorMsg"
          :title="errorMsg"
          type="error"
          :closable="false"
          show-icon
          class="error-alert"
        />
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { 
  User, 
  Lock, 
  School, 
  Moon, 
  Sunny, 
  Platform, 
  ChatDotRound, 
  Search, 
  Share 
} from '@element-plus/icons-vue'
import axios from 'axios'
import { getRoleFromToken } from '@/utils/jwt'

export default {
  name: 'LoginVue',
  setup() {
    const router = useRouter()
    const loginFormRef = ref()
    
    const loginForm = reactive({
      username: '',
      password: ''
    })
    
    const loginRules = {
      username: [
        { required: true, message: '请输入用户名', trigger: 'blur' },
        { min: 2, max: 20, message: '用户名长度在 2 到 20 个字符', trigger: 'blur' }
      ],
      password: [
        { required: true, message: '请输入密码', trigger: 'blur' },
        { min: 6, max: 20, message: '密码长度在 6 到 20 个字符', trigger: 'blur' }
      ]
    }
    
    const loading = ref(false)
    const errorMsg = ref('')
    const rememberMe = ref(false)
    const isDark = ref(false)

    const toggleTheme = () => {
      // 主题切换逻辑
      console.log('切换主题:', isDark.value ? '暗色' : '亮色')
    }

    const handleLogin = async () => {
      if (!loginFormRef.value) return
      
      try {
        const valid = await loginFormRef.value.validate()
        if (!valid) return
      } catch (error) {
        return
      }

      loading.value = true
      errorMsg.value = ''
      
      try {
        // 构造符合后端接口的请求参数
        const requestData = {
          Account: loginForm.username,
          Password: loginForm.password
        }
        
        const response = await axios.post('/api/user/login', requestData)
        
        // 根据后端返回的数据结构判断登录结果
        if (response.data.succeeded && response.data.data.code === 200) {
          // 登录成功
          const token = response.data.data.token
          localStorage.setItem('token', token)
          localStorage.setItem('username', loginForm.username)
          
          // 从JWT token中解析用户角色
          const userRole = getRoleFromToken(token)
          if (userRole) {
            localStorage.setItem('userRole', userRole)
          }
          
          ElMessage.success({
            message: '登录成功！',
            duration: 1500
          })
          
          console.log('登录成功，用户角色:', userRole)
          
          // 延迟跳转，让用户看到成功提示
          setTimeout(() => {
            router.push('/college')
          }, 1000)
        } else {
          // 登录失败
          const message = response.data.data?.message || '登录失败'
          errorMsg.value = message
          ElMessage.error(message)
        }
      } catch (error) {
        console.error('登录错误:', error)
        
        let message = '登录失败，请稍后重试'
        
        // 处理HTTP错误状态码
        if (error.response) {
          const { status, data } = error.response
          if (status === 401) {
            message = data.Message || '用户名或密码错误'
          } else if (status >= 500) {
            message = '服务器错误，请稍后重试'
          } else {
            message = data.Message || '登录失败，请重试'
          }
        } else if (error.request) {
          message = '网络连接失败，请检查网络'
        }
        
        errorMsg.value = message
        ElMessage.error(message)
      } finally {
        loading.value = false
      }
    }

    return {
      loginFormRef,
      loginForm,
      loginRules,
      loading,
      errorMsg,
      rememberMe,
      isDark,
      handleLogin,
      toggleTheme,
      // 图标
      User,
      Lock,
      School,
      Moon,
      Sunny,
      Platform,
      ChatDotRound,
      Search,
      Share
    }
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  min-height: 100vh;
  background: #f0f2f5;
}

/* ========== 左侧插图区域 ========== */
.login-left {
  flex: 1;
  background: linear-gradient(135deg, #a19deeff 0%, #4e7cfaff 50%, #6e95e9ff 100%);
  position: relative;
  display: flex;
  flex-direction: column;
  padding: 40px;
  overflow: hidden;
}

.brand-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 60px;
}

.brand-logo {
  display: flex;
  align-items: center;
  gap: 12px;
  color: white;
}

.logo-icon {
  color: white;
}

.brand-name {
  font-size: 20px;
  font-weight: 600;
  color: white;
}

.illustration-section {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  text-align: center;
}

/* 电脑插图 */
.computer-illustration {
  position: relative;
  margin-bottom: 60px;
}

.monitor {
  position: relative;
  z-index: 2;
}

.screen {
  width: 280px;
  height: 180px;
  background: #1f2937;
  border-radius: 12px 12px 0 0;
  border: 8px solid #374151;
  position: relative;
  overflow: hidden;
}

.screen-content {
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
  position: relative;
}

.window-bar {
  height: 24px;
  background: #374151;
  display: flex;
  align-items: center;
  padding: 0 12px;
}

.window-dots {
  display: flex;
  gap: 6px;
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.dot.red { background: #ef4444; }
.dot.yellow { background: #f59e0b; }
.dot.green { background: #10b981; }

.screen-body {
  padding: 20px;
  height: calc(100% - 24px);
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  gap: 16px;
}

.chart-line {
  width: 80%;
  height: 2px;
  background: linear-gradient(90deg, #3b82f6, #8b5cf6, #06b6d4);
  border-radius: 1px;
  position: relative;
  animation: pulse 2s ease-in-out infinite;
}

.chart-bars {
  display: flex;
  gap: 8px;
  align-items: end;
  height: 40px;
}

.bar {
  width: 12px;
  background: linear-gradient(180deg, #3b82f6, #1d4ed8);
  border-radius: 2px 2px 0 0;
  animation: grow 2s ease-in-out infinite;
}

.bar:nth-child(2) { animation-delay: 0.2s; }
.bar:nth-child(3) { animation-delay: 0.4s; }
.bar:nth-child(4) { animation-delay: 0.6s; }

@keyframes pulse {
  0%, 100% { opacity: 0.6; }
  50% { opacity: 1; }
}

@keyframes grow {
  0%, 100% { transform: scaleY(0.8); }
  50% { transform: scaleY(1.2); }
}

.monitor-stand {
  width: 60px;
  height: 40px;
  background: #6b7280;
  margin: 0 auto;
  border-radius: 0 0 8px 8px;
}

.monitor-base {
  width: 120px;
  height: 20px;
  background: #9ca3af;
  margin: 0 auto;
  border-radius: 20px;
  position: relative;
  top: -5px;
}

/* 装饰植物 */
.decoration-plant {
  position: absolute;
  bottom: 100px;
}

.left-plant {
  left: 60px;
}

.right-plant {
  right: 60px;
}

.pot {
  width: 40px;
  height: 30px;
  background: #92400e;
  border-radius: 0 0 20px 20px;
  margin: 0 auto;
}

.leaves {
  position: relative;
  margin-bottom: 10px;
}

.leaf {
  position: absolute;
  width: 20px;
  height: 30px;
  background: #10b981;
  border-radius: 50% 0;
  bottom: 0;
  left: 50%;
  transform-origin: bottom center;
  animation: sway 3s ease-in-out infinite;
}

.leaf-1 {
  transform: translateX(-50%) rotate(-20deg);
  animation-delay: 0s;
}

.leaf-2 {
  transform: translateX(-50%) rotate(0deg);
  animation-delay: 0.5s;
}

.leaf-3 {
  transform: translateX(-50%) rotate(20deg);
  animation-delay: 1s;
}

@keyframes sway {
  0%, 100% { transform: translateX(-50%) rotate(var(--rotation, 0deg)); }
  50% { transform: translateX(-50%) rotate(calc(var(--rotation, 0deg) + 10deg)); }
}

.text-section {
  color: white;
}

/* ========== 右侧登录表单区域 ========== */
.login-right {
  width: 480px;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
}

.login-form-container {
  width: 100%;
  max-width: 360px;
}

.form-header {
  text-align: center;
  margin-bottom: 40px;
}

.form-title {
  font-size: 24px;
  font-weight: 600;
  color: #1f2937;
  margin: 0;
}

.login-form {
  margin-bottom: 24px;
}

:deep(.el-form-item) {
  margin-bottom: 20px;
}

:deep(.el-input) {
  height: 48px;
}

:deep(.el-input__wrapper) {
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
}

:deep(.el-input.is-focus .el-input__wrapper) {
  border-color: #4f46e5;
  box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.1);
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.login-button {
  width: 100%;
  height: 48px;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
  border: none;
  margin-bottom: 24px;
}

.login-button:hover {
  background: linear-gradient(135deg, #4338ca 0%, #6d28d9 100%);
}

.error-alert {
  margin-bottom: 20px;
  border-radius: 8px;
}

/* ========== 响应式设计 ========== */
@media (max-width: 1024px) {
  .login-left {
    display: none;
  }
  
  .login-right {
    width: 100%;
  }
}

@media (max-width: 640px) {
  .login-right {
    padding: 20px;
  }
  
  .login-form-container {
    max-width: none;
  }
}

/* ========== 动画效果 ========== */
.login-right {
  animation: slideInRight 0.6s ease-out;
}

.login-left {
  animation: slideInLeft 0.6s ease-out;
}

@keyframes slideInLeft {
  from {
    opacity: 0;
    transform: translateX(-30px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(30px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}
</style>
