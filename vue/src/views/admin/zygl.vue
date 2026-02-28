<template>
  <div class="major-management">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">专业管理</h1>
        <p class="page-description">管理系统中的专业信息</p>
      </div>
    </div>

    <!-- 操作栏 -->
    <div class="action-section">
      <div class="action-left">
        <el-button type="primary" @click="showAddForm">
          <el-icon><Plus /></el-icon>
          添加专业
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
            :placeholder="searchType === 'code' ? '搜索专业代码...' : '搜索专业名称...'"
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
          <el-button size="small" type="danger" :disabled="!selectedRows.length" @click="batchDeleteMajors">
            <el-icon><Delete /></el-icon>
            批量删除 ({{ selectedRows.length }})
          </el-button>
        </div>
      </div>

      <!-- 表格 -->
      <el-table
        v-loading="loading"
        :data="filteredMajors"
        style="width: 100%"
        @selection-change="handleSelectionChange"
        empty-text="暂无数据"
        :header-cell-style="{ background: '#fafafa', color: '#606266' }"
      >
        <el-table-column type="selection" width="55" />
        
        <el-table-column prop="code" label="专业代码" min-width="120">
          <template #default="{ row }">
            <el-text>{{ row.code || '-' }}</el-text>
          </template>
        </el-table-column>
        
        <el-table-column prop="name" label="专业名称" min-width="200">
          <template #default="{ row }">
            <el-text>{{ row.name || '-' }}</el-text>
          </template>
        </el-table-column>
        
        <el-table-column prop="collegeName" label="所属学院" min-width="150">
          <template #default="{ row }">
            <el-text>{{ row.collegeName || '-' }}</el-text>
          </template>
        </el-table-column>
        
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" text @click="editMajor(row)">
              <el-icon><Edit /></el-icon>
              编辑
            </el-button>
            <el-button size="small" text type="danger" @click="deleteMajor(row)">
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
      :title="isEdit ? '编辑专业' : '添加专业'"
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
        <el-form-item label="专业代码" prop="code">
          <el-input
            v-model="formData.code"
            placeholder="请输入专业代码"
            clearable
          />
        </el-form-item>
        
        <el-form-item label="专业名称" prop="name">
          <el-input
            v-model="formData.name"
            placeholder="请输入专业名称"
            clearable
          />
        </el-form-item>
        
        <el-form-item label="所属学院" prop="collegeId">
          <el-select
            v-model="formData.collegeId"
            placeholder="请选择所属学院"
            style="width: 100%"
            clearable
          >
            <el-option
              v-for="college in collegeList"
              :key="college.id"
              :label="college.name"
              :value="college.id"
            />
          </el-select>
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="closeDialog">取消</el-button>
        <el-button type="primary" @click="saveMajor" :loading="saving">
          {{ isEdit ? '保存修改' : '添加专业' }}
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
  name: 'MajorManagement',
  components: {
    Plus,
    Search,
    Refresh,
    Edit,
    Delete
  },
  setup() {
    const searchKeyword = ref('')
    const searchType = ref('code')
    const showAddDialog = ref(false)
    const isEdit = ref(false)
    const loading = ref(false)
    const saving = ref(false)
    const majorList = ref([])
    const collegeList = ref([])
    const selectedRows = ref([])
    const currentPage = ref(1)
    const pageSize = ref(20)
    const formRef = ref()
    
    const formData = ref({
      id: null,
      code: '',
      name: '',
      collegeId: null
    })

    const formRules = {
      code: [
        { required: true, message: '请输入专业代码', trigger: 'blur' },
        { min: 2, max: 20, message: '专业代码长度在 2 到 20 个字符', trigger: 'blur' }
      ],
      name: [
        { required: true, message: '请输入专业名称', trigger: 'blur' },
        { min: 2, max: 50, message: '专业名称长度在 2 到 50 个字符', trigger: 'blur' }
      ],
      collegeId: [
        { required: true, message: '请选择所属学院', trigger: 'change' }
      ]
    }

    // 计算属性：过滤后的专业列表
    const filteredMajors = computed(() => {
      if (!searchKeyword.value) {
        return majorList.value
      }
      
      return majorList.value.filter(major => {
        const keyword = searchKeyword.value.toLowerCase()
        
        if (searchType.value === 'code') {
          return major.code?.toLowerCase().includes(keyword)
        } else if (searchType.value === 'name') {
          return major.name?.toLowerCase().includes(keyword)
        }
        
        return false
      })
    })

    // 计算属性：总记录数
    const totalCount = computed(() => filteredMajors.value.length)

    // 获取学院列表
    const fetchColleges = async () => {
      try {
        const response = await request.get('/xy/Getallxy')
        const apiResponse = response.data
        
        if (apiResponse.statusCode === 200 && apiResponse.succeeded && Array.isArray(apiResponse.data)) {
          collegeList.value = apiResponse.data.map(item => ({
            id: item.id,
            name: item.name
          }))
        } else {
          collegeList.value = []
        }
      } catch (error) {
        console.error('获取学院列表失败:', error)
        collegeList.value = []
        ElMessage.error('获取学院列表失败')
      }
    }

    // 根据学院ID获取学院名称
    const getCollegeNameById = async (xyid) => {
      try {
        const requestData = {
          Xyid: xyid
        }
        const response = await request.post('/zy/Getxybyxyid', requestData)
        const apiResponse = response.data
        
        if (apiResponse.statusCode === 200 && apiResponse.succeeded && apiResponse.data) {
          return apiResponse.data|| '未知学院'
        }
        return '未知学院'
      } catch (error) {
        console.error(`获取学院名称失败 (xyid: ${xyid}):`, error)
        return '未知学院'
      }
    }

    // 获取专业数据
    const fetchMajors = async () => {
      try {
        loading.value = true
        const response = await request.get('/zy/Getallzy')
        const apiResponse = response.data
        
        if (apiResponse.statusCode === 200 && apiResponse.succeeded && Array.isArray(apiResponse.data)) {
          // 先映射基本数据
          const majors = apiResponse.data.map(item => ({
            id: item.id,
            code: item.dm,
            name: item.name,
            collegeId: item.xyid,
            collegeName: '加载中...',  // 初始显示加载中
            isDeleted: item.isDeleted
          }))
          
          majorList.value = majors
          
          // 并发获取所有学院名称
          const collegePromises = majors.map(async (major) => {
            const collegeName = await getCollegeNameById(major.collegeId)
            return { id: major.id, collegeName }
          })
          
          const collegeResults = await Promise.allSettled(collegePromises)
          
          // 更新学院名称
          collegeResults.forEach((result, index) => {
            if (result.status === 'fulfilled') {
              const majorIndex = majorList.value.findIndex(m => m.id === majors[index].id)
              if (majorIndex !== -1) {
                majorList.value[majorIndex].collegeName = result.value.collegeName
              }
            }
          })
        } else {
          majorList.value = []
        }
        
        console.log('专业数据加载完成:', majorList.value)
      } catch (error) {
        console.error('获取专业数据出错:', error)
        majorList.value = []
        ElMessage.error('获取专业数据失败')
      } finally {
        loading.value = false
      }
    }

    // 刷新数据
    const refreshData = () => {
      fetchMajors()
    }

    // 处理表格选择变化
    const handleSelectionChange = (selection) => {
      selectedRows.value = selection
    }

    // 显示添加表单
    const showAddForm = () => {
      formData.value = {
        id: null,
        code: '',
        name: '',
        collegeId: null
      }
      isEdit.value = false
      if (formRef.value) {
        formRef.value.resetFields()
      }
      showAddDialog.value = true
    }

    const closeDialog = () => {
      showAddDialog.value = false
      isEdit.value = false
      formData.value = {
        id: null,
        code: '',
        name: '',
        collegeId: null
      }
      if (formRef.value) {
        formRef.value.resetFields()
      }
    }

    const saveMajor = async () => {
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
          // 编辑专业
          console.log('编辑专业信息:', formData.value)
          
          const requestData = {
            id: formData.value.id,
            dm: formData.value.code,
            name: formData.value.name,
            xyid: formData.value.collegeId
          }
          
          const response = await request.post('/zy/Updatezy', requestData)
          const apiResponse = response.data
          
          if (apiResponse.statusCode === 200 && apiResponse.data.code == 200) {
            ElMessage.success('编辑专业成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse.data.message || '编辑专业失败')
          }
        } else {
          // 添加专业
          console.log('添加专业信息:', formData.value)
          
          const requestData = {
            Dm: formData.value.code,
            Name: formData.value.name,
            Xyid: formData.value.collegeId
          }
          
          const response = await request.post('/zy/Addzy', requestData)
          const apiResponse = response.data
          
          if (apiResponse.statusCode === 200 && apiResponse.data.code ==200) {
            ElMessage.success('添加专业成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse.data.message || '添加专业失败')
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

    // 编辑专业
    const editMajor = (major) => {
      formData.value = {
        id: major.id,
        code: major.code || '',
        name: major.name || '',
        collegeId: major.collegeId || null
      }
      isEdit.value = true
      showAddDialog.value = true
    }

    // 删除专业
    const deleteMajor = async (major) => {
      try {
        await ElMessageBox.confirm(
          `确定要删除专业"${major.name}"吗？`,
          '删除确认',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }
        )
        try {
          console.log('删除专业:', major)
          
          const requestData = {
            id: major.id
          }
          
          const response = await request.post('/zy/Deletezy', requestData)
          const apiResponse = response.data
          
          if (apiResponse.statusCode === 200 && apiResponse.succeeded) {
            ElMessage.success('删除专业成功')
            refreshData()
          } else {
            ElMessage.error(apiResponse.message || '删除专业失败')
          }
        } catch (error) {
          console.error('删除专业失败:', error)
          if (error.response && error.response.data) {
            ElMessage.error(error.response.data.message || '删除失败，请重试')
          } else {
            ElMessage.error('删除失败，请重试')
          }
        }
      } catch (error) {
        console.log('用户取消删除操作')
      }
    }

    // 批量删除专业
    const batchDeleteMajors = async () => {
      if (!selectedRows.value.length) {
        ElMessage.warning('请先选择要删除的专业')
        return
      }

      try {
        await ElMessageBox.confirm(
          `确定要删除选中的 ${selectedRows.value.length} 个专业吗？`,
          '批量删除确认',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }
        )

        try {
          console.log('批量删除专业:', selectedRows.value)
          
          // 并发删除所有选中的专业
          const deletePromises = selectedRows.value.map(async (major) => {
            const requestData = { id: major.id }
            return request.post('/zy/Deletezy', requestData)
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
                console.error(`专业 ${selectedRows.value[index].name} 删除失败:`, apiResponse.message)
              }
            } else {
              failCount++
              console.error(`专业 ${selectedRows.value[index].name} 请求失败:`, result.reason)
            }
          })

          // 显示删除结果
          if (successCount > 0 && failCount === 0) {
            ElMessage.success(`成功删除 ${successCount} 个专业`)
          } else if (successCount > 0 && failCount > 0) {
            ElMessage.warning(`成功删除 ${successCount} 个专业，${failCount} 个删除失败`)
          } else {
            ElMessage.error('批量删除失败')
          }

          selectedRows.value = []
          refreshData()
        } catch (error) {
          console.error('批量删除失败:', error)
          ElMessage.error('批量删除失败，请重试')
        }
      } catch (error) {
        console.log('用户取消批量删除操作')
      }
    }

    // 组件挂载时获取数据
    onMounted(() => {
      fetchColleges()
      fetchMajors()
    })

    return {
      searchKeyword,
      searchType,
      showAddDialog,
      isEdit,
      loading,
      saving,
      majorList,
      collegeList,
      filteredMajors,
      totalCount,
      selectedRows,
      currentPage,
      pageSize,
      formRef,
      formData,
      formRules,
      showAddForm,
      closeDialog,
      saveMajor,
      editMajor,
      deleteMajor,
      batchDeleteMajors,
      refreshData,
      fetchMajors,
      getCollegeNameById,
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
/* 复用学院管理的样式 */
.major-management {
  padding: 0;
  background: transparent;
}

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

.table-card {
  border: 1px solid #e8e8e8;
  border-radius: 8px;
}

:deep(.el-card__body) {
  padding: 0;
}

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

.pagination-wrapper {
  padding: 16px 20px;
  display: flex;
  justify-content: flex-end;
  border-top: 1px solid #f0f0f0;
  background: #fafafa;
}
</style>