<template>
  <div class="college-management">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">学院管理</h1>
        <p class="page-description">管理系统中的学院信息</p>
      </div>
    </div>

    <!-- 操作栏 -->
    <div class="action-section">
      <div class="action-left">
        <el-button type="primary" @click="showAddForm">
          <el-icon><Plus /></el-icon>
          添加学院
        </el-button>
      </div>
      
      <div class="action-right">
        <div class="search-container">
          <el-select
            v-model="searchType"
            placeholder="搜索类型"
            style="width: 120px; margin-right: 8px"
          >
            <el-option label="按代码" value="code" />
            <el-option label="按名称" value="name" />
          </el-select>
          
          <el-input
            v-model="searchKeyword"
            :placeholder="searchType === 'code' ? '搜索学院代码...' : '搜索学院名称...'"
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
        <div class="toolbar-right">
          <el-button size="small" type="danger" :disabled="!selectedRows.length" @click="batchDeleteColleges">
            <el-icon><Delete /></el-icon>
            批量删除 ({{ selectedRows.length }})
          </el-button>
        </div>
      </div>

      <!-- 表格 -->
      <el-table
        v-loading="loading"
        :data="filteredColleges"
        style="width: 100%"
        @selection-change="handleSelectionChange"
        empty-text="暂无数据"
        :header-cell-style="{ background: '#fafafa', color: '#606266' }"
      >
        <el-table-column type="selection" width="55" />
        
        <el-table-column prop="code" label="学院代码" min-width="120">
          <template #default="{ row }">
            <el-text>{{ row.code || '-' }}</el-text>
          </template>
        </el-table-column>
        
        <el-table-column prop="name" label="学院名称" min-width="200">
          <template #default="{ row }">
            <el-text>{{ row.name || '-' }}</el-text>
          </template>
        </el-table-column>
        
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" text @click="editCollege(row)">
              <el-icon><Edit /></el-icon>
              编辑
            </el-button>
            <el-button size="small" text type="danger" @click="deleteCollege(row)">
              <el-icon><Delete /></el-icon>
              删除
            </el-button>
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
      :title="isEdit ? '编辑学院' : '添加学院'"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="100px"
        label-position="left"
      >
        <el-form-item label="学院代码" prop="code">
          <el-input
            v-model="formData.code"
            placeholder="请输入学院代码"
            clearable
          />
        </el-form-item>
        
        <el-form-item label="学院名称" prop="name">
          <el-input
            v-model="formData.name"
            placeholder="请输入学院名称"
            clearable
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="closeDialog">取消</el-button>
        <el-button type="primary" @click="saveCollege" :loading="saving">
          {{ isEdit ? '保存修改' : '添加学院' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { 
  Plus, 
  Search, 
  Refresh, 
  Edit, 
  Delete 
} from '@element-plus/icons-vue'
import request from '@/utils/request'

export default {
  name: 'CollegeManagement',
  components: {
    Plus,
    Search,
    Refresh,
    Edit,
    Delete
  },
  setup() {
    const searchKeyword = ref('')
    const searchType = ref('code') // 默认按代码搜索
    const showAddDialog = ref(false)
    const isEdit = ref(false)
    const loading = ref(false)
    const saving = ref(false)
    const collegeList = ref([])
    const selectedRows = ref([])
    const currentPage = ref(1)
    const pageSize = ref(20)
    const formRef = ref()
    
    const formData = ref({
      id: null,
      code: '',
      name: '',
      description: ''
    })

    const formRules = {
      code: [
        { required: true, message: '请输入学院代码', trigger: 'blur' },
        { min: 2, max: 20, message: '学院代码长度在 2 到 20 个字符', trigger: 'blur' }
      ],
      name: [
        { required: true, message: '请输入学院名称', trigger: 'blur' },
        { min: 2, max: 50, message: '学院名称长度在 2 到 50 个字符', trigger: 'blur' }
      ]
    }

    // 计算属性：过滤后的学院列表
    const filteredColleges = computed(() => {
      if (!searchKeyword.value) {
        return collegeList.value
      }
      
      return collegeList.value.filter(college => {
        const keyword = searchKeyword.value.toLowerCase()
        
        if (searchType.value === 'code') {
          // 按代码搜索
          return college.code?.toLowerCase().includes(keyword)
        } else if (searchType.value === 'name') {
          // 按名称搜索
          return college.name?.toLowerCase().includes(keyword)
        }
        
        return false
      })
    })

    // 计算属性：总记录数
    const totalCount = computed(() => filteredColleges.value.length)

    

    // 获取学院数据
    const fetchColleges = async () => {
      try {
        loading.value = true
        const response = await request.get('/xy/Getallxy')
        
        console.log('完整响应:', response)
        console.log('响应数据:', response.data)
        
        // 修正：response.data 才是接口返回的数据
        const apiResponse = response.data
        
        // 根据新的接口返回格式处理数据
        if (apiResponse.statusCode === 200 && apiResponse.succeeded && Array.isArray(apiResponse.data)) {
          // 将接口字段映射到前端使用的字段
          collegeList.value = apiResponse.data.map(item => ({
            id: item.id,
            code: item.dm,        // 将 dm 映射为 code
            name: item.name,
            isDeleted: item.isDeleted
          }))
          console.log('处理后的数据:', collegeList.value)
        } else {
          collegeList.value = []
          console.log('暂无学院数据或数据格式不正确', apiResponse)
        }
      } catch (error) {
        console.error('获取学院数据出错:', error)
        collegeList.value = []
        ElMessage.error('获取学院数据失败')
      } finally {
        loading.value = false
      }
    }

    // 刷新数据
    const refreshData = () => {
      fetchColleges()
    }

    // 处理表格选择变化
    const handleSelectionChange = (selection) => {
      selectedRows.value = selection
    }

    // 显示添加表单
    const showAddForm = () => {
      // 重置表单数据
      formData.value = {
        id: null,
        code: '',
        name: '',
        description: ''
      }
      // 重置编辑状态
      isEdit.value = false
      // 重置表单验证
      if (formRef.value) {
        formRef.value.resetFields()
      }
      // 显示对话框
      showAddDialog.value = true
    }

    const closeDialog = () => {
      showAddDialog.value = false
      isEdit.value = false
      formData.value = {
        id: null,
        code: '',
        name: '',
        description: ''
      }
      if (formRef.value) {
        formRef.value.resetFields()
      }
    }

    const saveCollege = async () => {
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
          // 编辑学院
          console.log('编辑学院信息:', formData.value)
          
          // 准备发送给后端的数据，包含id、学院代码和学院名称
          const requestData = {
            id: formData.value.id,      // 学院ID
            dm: formData.value.code,    // 学院代码字段名为 dm
            name: formData.value.name   // 学院名称字段名为 name
          }
          
          console.log('发送到后端的编辑数据:', requestData)
          
          // 调用编辑学院接口
          const response = await request.post('/xy/Updatexy', requestData)
          
          console.log('编辑学院响应:', response)
          
          // 检查响应结果
          const apiResponse = response.data
          if (apiResponse.statusCode === 200 && apiResponse.data.code ===200) {
            ElMessage.success('编辑学院成功')
            closeDialog()
            refreshData() // 刷新列表数据
          }
          else if(apiResponse.statusCode === 200 && apiResponse.data.code ===400) {
            ElMessage.error(apiResponse.data.message)
          }
          else {
            ElMessage.error(apiResponse.message || '编辑学院失败')
          }
        } else {
          // 添加学院
          console.log('添加学院信息:', formData.value)
          
          // 准备发送给后端的数据，根据后端接口要求调整字段名
          const requestData = {
            dm: formData.value.code,    // 学院代码字段名为 dm
            name: formData.value.name   // 学院名称字段名为 name
          }
          
          console.log('发送到后端的数据:', requestData)
          
          // 调用添加学院接口
          const response = await request.post('/xy/Addxy', requestData)
          
          console.log('添加学院响应:', response)
          
          // 检查响应结果
          const apiResponse = response.data
          if (apiResponse.statusCode === 200 && apiResponse.data.code === 200) {
            ElMessage.success('添加学院成功')
            closeDialog()
            refreshData() // 刷新列表数据
          } 
          else if(apiResponse.statusCode === 200 && apiResponse.data.code === 400 ){
             ElMessage.error(apiResponse.data.message )
          }
          else {
            ElMessage.error(apiResponse.data.message)
          }
        }
      } catch (error) {
        console.error('保存失败:', error)
        if (error.response && error.response.data) {
          ElMessage.error(error.response.data.message || '保存失败，请重试')
        } else {
          ElMessage.error('保存失败，请重试')
        }
      } finally {
        saving.value = false
      }
    }

    // 编辑学院
    const editCollege = (college) => {
      formData.value = {
        id: college.id,              // 保存学院ID
        code: college.code || '',
        name: college.name || '',
        description: college.description || ''
      }
      isEdit.value = true
      showAddDialog.value = true
    }

    // 删除学院
    const deleteCollege = async (college) => {
      try {
        await ElMessageBox.confirm(
          `确定要删除学院"${college.name}"吗？`,
          '删除确认',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }
        )
        
        try {
          console.log('删除学院:', college)
          console.log('学院ID:', college.id)
          
          // 准备发送给后端的数据 - 参考添加学院的格式
          const requestData = {
            id: college.id
          }
          
          console.log('发送到后端的删除数据:', requestData)
          
          // 调用删除学院接口 - 参考添加学院的方式
          const response = await request.post('/xy/Deletexy', requestData)
          
          console.log('删除学院响应:', response)
          
          // 检查响应结果 - 参考添加学院的响应处理
          const apiResponse = response.data
          if (apiResponse.statusCode === 200 && apiResponse.succeeded) {
            ElMessage.success('删除学院成功')
            refreshData() // 刷新列表数据
          } else {
            ElMessage.error(apiResponse.message || '删除学院失败')
          }
        } catch (error) {
          console.error('删除学院失败:', error)
          console.error('错误详情:', error.response)
          
          if (error.response && error.response.data) {
            const errorData = error.response.data
            ElMessage.error(errorData.Message || `删除失败: ${error.response.status}`)
          } else {
            ElMessage.error('删除失败，请重试')
          }
        }
      } catch (error) {
        // 用户取消删除
        console.log('用户取消删除操作')
      }
    }

    // 批量删除学院
    const batchDeleteColleges = async () => {
      if (!selectedRows.value.length) {
        ElMessage.warning('请先选择要删除的学院')
        return
      }

      try {
        await ElMessageBox.confirm(
          `确定要删除选中的 ${selectedRows.value.length} 个学院吗？`,
          '批量删除确认',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }
        )

        try {
          console.log('批量删除学院:', selectedRows.value)
          
          // 提取所有选中学院的ID
          const collegeIds = selectedRows.value.map(college => college.id)
          console.log('要删除的学院ID列表:', collegeIds)

          // 并发删除所有选中的学院
          const deletePromises = collegeIds.map(async (id) => {
            const requestData = { id }
            return request.post('/xy/Deletexy', requestData)
          })

          // 等待所有删除请求完成
          const responses = await Promise.allSettled(deletePromises)
          
          // 统计删除结果
          let successCount = 0
          let failCount = 0
          
          responses.forEach((result, index) => {
            if (result.status === 'fulfilled') {
              const apiResponse = result.value.data
              if (apiResponse.statusCode === 200 && apiResponse.succeeded) {
                successCount++
              } else {
                failCount++
                console.error(`删除学院ID ${collegeIds[index]} 失败:`, apiResponse.message)
              }
            } else {
              failCount++
              console.error(`删除学院ID ${collegeIds[index]} 请求失败:`, result.reason)
            }
          })

          // 显示删除结果
          if (successCount > 0 && failCount === 0) {
            ElMessage.success(`成功删除 ${successCount} 个学院`)
          } else if (successCount > 0 && failCount > 0) {
            ElMessage.warning(`成功删除 ${successCount} 个学院，${failCount} 个删除失败`)
          } else {
            ElMessage.error('批量删除失败')
          }

          // 刷新数据并清空选择
          refreshData()
          selectedRows.value = []
          
        } catch (error) {
          console.error('批量删除失败:', error)
          ElMessage.error('批量删除失败，请重试')
        }
      } catch (error) {
        // 用户取消批量删除
        console.log('用户取消批量删除操作')
      }
    }

    // 组件挂载时获取数据
    onMounted(() => {
      fetchColleges()
    })

    return {
      searchKeyword,
      searchType,
      showAddDialog,
      isEdit,
      loading,
      saving,
      collegeList,
      filteredColleges,
      totalCount,
      selectedRows,
      currentPage,
      pageSize,
      formRef,
      formData,
      formRules,
      //formatDate,
      showAddForm,
      closeDialog,
      saveCollege,
      editCollege,
      deleteCollege,
      batchDeleteColleges,
      refreshData,
      fetchColleges,
      handleSelectionChange,
      // 图标
      Plus,
      Search,
      Refresh,
      Edit,
      Delete
    }
  }
}
</script>

<style scoped>
.college-management {
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

.toolbar-right {
  display: flex;
  align-items: center;
  gap: 12px;
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

:deep(.el-textarea__inner) {
  border-radius: 6px;
  border: 1px solid #d9d9d9;
  transition: all 0.3s ease;
}

:deep(.el-textarea__inner:hover) {
  border-color: #40a9ff;
}

:deep(.el-textarea__inner:focus) {
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

:deep(.el-button.is-text.el-button--danger:active) {
  background-color: transparent;
}

:deep(.el-button.is-text.el-button--danger:focus) {
  background-color: transparent;
}

/* ========== 加载和空状态 ========== */
:deep(.el-loading-mask) {
  border-radius: 8px;
}

:deep(.el-table__empty-block) {
  min-height: 200px;
}
</style>

