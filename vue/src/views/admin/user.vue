<template>
  <div class="user-management">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">用户管理</h1>
        <p class="page-description">管理系统中的用户信息</p>
      </div>
    </div>

    <!-- 操作栏 -->
    <div class="action-section">
      <div class="action-left">
        <el-button type="primary" @click="showAddForm">
          <el-icon><Plus /></el-icon>
          添加用户
        </el-button>
      </div>
      
      <div class="action-right">
        <div class="search-container">
          <el-select
            v-model="searchType"
            placeholder="搜索类型"
            style="width: 120px; margin-right: 8px"
          >
            <el-option label="按用户名" value="username" />
            <el-option label="按角色" value="role" />
          </el-select>
          
          <el-input
            v-model="searchKeyword"
            :placeholder="getSearchPlaceholder()"
            style="width: 200px"
            clearable
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </div>
        
        <el-button @click="refreshData" :loading="loading">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>
    </div>

    <!-- 数据表格 -->
    <el-card class="table-card" shadow="never">
      <!-- 表格工具栏 -->
      <div class="table-toolbar">
        <div class="toolbar-left">
          <el-text type="info">共 {{ totalCount }} 条记录</el-text>
        </div>
      </div>

      <!-- 表格 -->
      <el-table
        v-loading="loading"
        :data="filteredUsers"
        style="width: 100%"
        empty-text="暂无数据"
        :header-cell-style="{ background: '#fafafa', color: '#606266' }"
      >
        <el-table-column prop="username" label="用户名" min-width="120">
          <template #default="{ row }">
            <el-text>{{ row.username || '-' }}</el-text>
          </template>
        </el-table-column>

        <el-table-column prop="role" label="角色" min-width="100">
          <template #default="{ row }">
            <el-tag :type="getRoleTagType(row.role)" size="small">
              {{ getRoleText(row.role) }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="status" label="状态" min-width="100">
          <template #default="{ row }">
            <el-switch
              v-model="row.status"
              :active-value="false"
              :inactive-value="true"
              active-text="启用"
              inactive-text="禁用"
              inline-prompt
              @change="toggleUserStatus(row)"
            />
          </template>
        </el-table-column>
        
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <div class="operation-buttons">
              <el-button size="small" text @click="editUser(row)">
                <el-icon><Edit /></el-icon>
                编辑
              </el-button>
              <el-button size="small" text @click="resetPassword(row)">
                <el-icon><Key /></el-icon>
                重置密码
              </el-button>
              <el-button size="small" text type="danger" @click="deleteUser(row)">
                <el-icon><Delete /></el-icon>
                删除
              </el-button>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-wrapper">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="totalCount"
          layout="total, sizes, prev, pager, next, jumper"
          background
        />
      </div>
    </el-card>

    <!-- 添加/编辑对话框 -->
    <el-dialog
      v-model="showAddDialog"
      :title="isEdit ? '编辑用户' : '添加用户'"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="100px"
        label-position="left"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="用户名" prop="username">
              <el-input
                v-model="formData.username"
                placeholder="请输入用户名"
                :disabled="isEdit"
                clearable
              />
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="角色" prop="role">
              <el-select v-model="formData.role" placeholder="请选择角色" style="width: 100%" clearable>
                <el-option label="管理员" value="Admin" />
                <el-option label="教务处" value="Jwc" />
                <el-option label="二级学院" value="Xy" />
                <el-option label="教师" value="Teacher" />
                <el-option label="学生" value="Student" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-form-item v-if="!isEdit" label="密码" prop="password">
          <el-input
            v-model="formData.password"
            type="password"
            placeholder="请输入密码"
            show-password
            clearable
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="closeDialog">取消</el-button>
        <el-button type="primary" @click="saveUser" :loading="saving">
          {{ isEdit ? '保存修改' : '添加用户' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script>
import { ref, onMounted, computed, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { 
  Plus, 
  Search, 
  Refresh, 
  Edit, 
  Delete,
  Key
} from '@element-plus/icons-vue'
import request from '@/utils/request'

export default {
  name: 'UserManagement',
  components: {
    Plus,
    Search,
    Refresh,
    Edit,
    Delete,
    Key
  },
  setup() {
    const searchKeyword = ref('')
    const searchType = ref('username')
    const showAddDialog = ref(false)
    const isEdit = ref(false)
    const loading = ref(false)
    const saving = ref(false)
    const userList = ref([])
    const currentPage = ref(1)
    const pageSize = ref(20)
    const formRef = ref()
    
    const formData = ref({
      id: null,
      username: '',
      role: null,
      password: ''
    })

    const formRules = {
      username: [
        { required: true, message: '请输入用户名', trigger: 'blur' },
        { min: 3, max: 20, message: '用户名长度在 3 到 20 个字符', trigger: 'blur' }
      ],
      role: [
        { required: true, message: '请选择角色', trigger: 'change' }
      ],
      password: [
        { required: true, message: '请输入密码', trigger: 'blur' },
        { min: 6, max: 20, message: '密码长度在 6 到 20 个字符', trigger: 'blur' }
      ]
    }

    // 获取搜索提示文本
    const getSearchPlaceholder = () => {
      const placeholders = {
        username: '搜索用户名...',
        role: '搜索角色...'
      }
      return placeholders[searchType.value] || '搜索...'
    }

    // 计算属性：过滤后的用户列表
    const filteredUsers = computed(() => {
      if (!searchKeyword.value) {
        return userList.value
      }
      
      return userList.value.filter(user => {
        const keyword = searchKeyword.value.toLowerCase()
        
        if (searchType.value === 'username') {
          return user.username?.toLowerCase().includes(keyword)
        } else if (searchType.value === 'role') {
          return getRoleText(user.role)?.toLowerCase().includes(keyword)
        }
        
        return false
      })
    })

    // 计算属性：总记录数
    const totalCount = computed(() => filteredUsers.value.length)

    // 获取角色标签类型
    const getRoleTagType = (role) => {
      const typeMap = {
        'Admin': 'danger',
        'Jwc': 'warning',
        'Xy': 'success',
        'Teacher': 'primary',
        'Student': 'info'
      }
      return typeMap[role] || 'info'
    }

    // 获取角色文本
    const getRoleText = (role) => {
      const roleMap = {
        'Admin': '管理员',
        'Jwc': '教务处',
        'Xy': '二级学院',
        'Teacher': '教师',
        'Student': '学生'
      }
      return roleMap[role] || '未知角色'
    }

    // 切换用户状态
    const toggleUserStatus = async (user) => {
      try {
        const isClosing = user.status === true
        const apiUrl = isClosing ? '/user/Closeuser' : '/user/Openuser'
        const actionText = isClosing ? '禁用' : '启用'
        
        const requestData = { id: user.id }
        
        const response = await request.post(apiUrl, requestData)
        const apiResponse = response.data
        
        if (apiResponse.statusCode === 200 && apiResponse.succeeded) {
          ElMessage.success(`用户已${actionText}`)
        } else {
          user.status = !user.status
          ElMessage.error(apiResponse.message || `${actionText}失败`)
        }
      } catch (error) {
        console.error('更新用户状态失败:', error)
        user.status = !user.status
        ElMessage.error('状态更新失败，请重试')
      }
    }

    // 获取用户数据
    const fetchUsers = async () => {
      try {
        loading.value = true
        const response = await request.get('/user/Getalluser')
        const apiResponse = response.data
        if (apiResponse.statusCode === 200 && apiResponse.succeeded && Array.isArray(apiResponse.data)) {
          userList.value = apiResponse.data.map(item => ({
            id: item.id,
            username: item.userName,
            password: item.password,
            role: item.role,
            status: item.isDeleted
          }))
        } else {
          userList.value = []
        }
      } catch (error) {
        console.error('获取用户数据出错:', error)
        userList.value = []
        ElMessage.error('获取用户数据失败')
      } finally {
        loading.value = false
      }
    }

    // 刷新数据
    const refreshData = () => {
      fetchUsers()
    }

    // 显示添加表单
    const showAddForm = async () => {
      showAddDialog.value = false
      isEdit.value = false
      
      formData.value = {
        id: null,
        username: '',
        role: null,
        password: ''
      }
      
      await nextTick()
      
      if (formRef.value) {
        formRef.value.resetFields()
      }
      
      showAddDialog.value = true
    }

    const closeDialog = async () => {
      showAddDialog.value = false
      isEdit.value = false
      
      formData.value = {
        id: null,
        username: '',
        role: null,
        password: ''
      }
      
      await nextTick()
      
      if (formRef.value) {
        formRef.value.resetFields()
      }
    }

    const saveUser = async () => {
      if (!formRef.value) return
      
      try {
        const valid = await formRef.value.validate()
        if (!valid) return
      } catch (error) {
        return
      }

      saving.value = true
      
      try {
        if (isEdit.value) {
          // 编辑用户
          const requestData = {
            Id: formData.value.id,
            Role: formData.value.role
          }
          
          const response = await request.post('/user/Updateuser', requestData)
          const apiResponse = response.data
          if (apiResponse.statusCode === 200 && apiResponse.succeeded) {
            ElMessage.success('编辑用户成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse.message || '编辑用户失败')
          }
        } else {
          // 添加用户
          const requestData = {
            Account: formData.value.username,
            Password: formData.value.password,
            Role: formData.value.role
          }
          
          const response = await request.post('/user/Adduser', requestData)
          const apiResponse = response.data
          
          if (apiResponse.statusCode === 200 && apiResponse.data && apiResponse.data.code === 200) {
            ElMessage.success('添加用户成功')
            closeDialog()
            refreshData()
          } else if (apiResponse.statusCode === 200 && apiResponse.succeeded) {
            ElMessage.success('添加用户成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse.message || '添加用户失败')
          }
        }
      } catch (error) {
        console.error('保存失败:', error)
        ElMessage.error('保存失败，请重试')
      } finally {
        saving.value = false
      }
    }

    // 编辑用户
    const editUser = async (user) => {
      showAddDialog.value = false
      isEdit.value = true
      
      formData.value = {
        id: user.id,
        username: user.username || '',
        role: user.role || null,
        password: ''
      }
      
      await nextTick()
      
      if (formRef.value) {
        formRef.value.clearValidate()
      }
      
      showAddDialog.value = true
    }

    // 重置密码
    const resetPassword = async (user) => {
      try {
        await ElMessageBox.confirm(
          `确定要重置用户"${user.username}"的密码吗？`,
          '重置密码确认',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }
        )
        
        try {
          const requestData = { Id: user.id }
          const response = await request.post('/user/Refreshpassword', requestData)
          const apiResponse = response.data
          if (apiResponse.statusCode === 200 && apiResponse.succeeded) {
            ElMessage.success('密码重置成功')
          } else {
            ElMessage.error(apiResponse.message || '密码重置失败')
          }
        } catch (error) {
          console.error('重置密码失败:', error)
          ElMessage.error('重置密码失败，请重试')
        }
      } catch (error) {
        console.log('用户取消重置密码')
      }
    }

    // 删除用户
    const deleteUser = async (user) => {
      try {
        await ElMessageBox.confirm(
          `确定要删除用户"${user.username}"吗？`,
          '删除确认',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }
        )
        
        try {
          const requestData = {
            Account: user.username,
            password: user.password,
            Id: user.id
          }
          const response = await request.post('/user/Deleteuser', requestData)
          const apiResponse = response.data
          if (apiResponse.statusCode === 200 && apiResponse.succeeded) {
            ElMessage.success('删除用户成功')
            refreshData()
          } else {
            ElMessage.error(apiResponse.message || '删除用户失败')
          }
        } catch (error) {
          console.error('删除用户失败:', error)
          ElMessage.error('删除失败，请重试')
        }
      } catch (error) {
        console.log('用户取消删除操作')
      }
    }

    // 组件挂载时获取数据
    onMounted(() => {
      fetchUsers()
    })

    return {
      searchKeyword,
      searchType,
      showAddDialog,
      isEdit,
      loading,
      saving,
      userList,
      filteredUsers,
      totalCount,
      currentPage,
      pageSize,
      formRef,
      formData,
      formRules,
      getSearchPlaceholder,
      getRoleTagType,
      getRoleText,
      toggleUserStatus,
      showAddForm,
      closeDialog,
      saveUser,
      editUser,
      resetPassword,
      deleteUser,
      refreshData,
      fetchUsers,
      // 图标
      Plus,
      Search,
      Refresh,
      Edit,
      Delete,
      Key
    }
  }
}
</script>

<style scoped>
.user-management {
  padding: 0;
  background: transparent;
}

/* ========== 页面头部 ========== */
.page-header {
  margin-bottom: 24px;
}

.header-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.page-title {
  font-size: 20px;
  font-weight: 600;
  color: #262626;
  margin: 0;
  line-height: 1.4;
}

.page-description {
  font-size: 14px;
  color: #8c8c8c;
  margin: 0;
  line-height: 1.4;
}

/* ========== 操作区域 ========== */
.action-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  gap: 16px;
}

.action-left {
  flex-shrink: 0;
}

.action-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.search-container {
  display: flex;
  align-items: center;
}

/* ========== 表格卡片 ========== */
.table-card {
  border: 1px solid #e8e8e8;
  border-radius: 8px;
}

:deep(.el-card__body) {
  padding: 0;
}

/* 表格工具栏 */
.table-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #f0f0f0;
  background: #fafafa;
}

.toolbar-left {
  display: flex;
  align-items: center;
}

/* ========== Element Plus 表格样式覆盖 ========== */
:deep(.el-table) {
  border: none;
}

:deep(.el-table__header-wrapper) {
  border-bottom: 1px solid #e8e8e8;
}

:deep(.el-table th) {
  background: #fafafa;
  border-bottom: 1px solid #e8e8e8;
  font-weight: 500;
  color: #606266;
}

:deep(.el-table td) {
  border-bottom: 1px solid #f5f5f5;
}

:deep(.el-table__row:hover > td) {
  background-color: #f8f9fa;
}

:deep(.el-table__empty-text) {
  color: #8c8c8c;
}

/* 分页样式 */
.pagination-wrapper {
  padding: 16px 20px;
  display: flex;
  justify-content: flex-end;
  border-top: 1px solid #f0f0f0;
  background: #fafafa;
}

:deep(.el-pagination) {
  --el-pagination-bg-color: transparent;
}

/* ========== 对话框样式 ========== */
:deep(.el-dialog) {
  border-radius: 8px;
}

:deep(.el-dialog__header) {
  padding: 20px 24px 16px;
  border-bottom: 1px solid #f0f0f0;
}

:deep(.el-dialog__title) {
  font-size: 16px;
  font-weight: 600;
  color: #262626;
}

:deep(.el-dialog__body) {
  padding: 24px;
}

:deep(.el-dialog__footer) {
  padding: 16px 24px 20px;
  border-top: 1px solid #f0f0f0;
}

/* ========== 表单样式 ========== */
:deep(.el-form-item__label) {
  color: #262626;
  font-weight: 500;
}

:deep(.el-input__wrapper) {
  border-radius: 6px;
  border: 1px solid #d9d9d9;
  transition: all 0.3s ease;
}

:deep(.el-input__wrapper:hover) {
  border-color: #40a9ff;
}

:deep(.el-input.is-focus .el-input__wrapper) {
  border-color: #1890ff;
  box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2);
}

/* ========== 按钮样式 ========== */
:deep(.el-button) {
  border-radius: 6px;
  font-weight: 400;
}

:deep(.el-button--primary) {
  background-color: #1890ff;
  border-color: #1890ff;
}

:deep(.el-button--primary:hover) {
  background-color: #40a9ff;
  border-color: #40a9ff;
}

:deep(.el-button--danger) {
  background-color: #ff4d4f;
  border-color: #ff4d4f;
}

:deep(.el-button--danger:hover) {
  background-color: #ff7875;
  border-color: #ff7875;
}

/* 文本按钮样式 */
:deep(.el-button.is-text) {
  padding: 4px 8px;
  color: #1890ff;
}

:deep(.el-button.is-text:hover) {
  background-color: #f0f9ff;
}

:deep(.el-button.is-text.el-button--danger) {
  color: #ff4d4f;
  background-color: transparent;
}

:deep(.el-button.is-text.el-button--danger:hover) {
  background-color: transparent;
}

/* ========== 标签样式 ========== */
:deep(.el-tag) {
  border-radius: 4px;
}

/* ========== 操作按钮样式 ========== */
.operation-buttons {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-wrap: nowrap;
}

.operation-buttons .el-button {
  margin: 0;
  padding: 4px 6px;
  font-size: 12px;
  white-space: nowrap;
}

.operation-buttons .el-button .el-icon {
  margin-right: 2px;
  font-size: 12px;
}

/* ========== 加载和空状态 ========== */
:deep(.el-loading-mask) {
  border-radius: 8px;
}

:deep(.el-table__empty-block) {
  min-height: 200px;
}
</style>