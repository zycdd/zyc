
<template>
  <div class="term-management">
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">学期管理</h1>
        <p class="page-description">管理系统中的学期信息</p>
      </div>
    </div>

    <div class="action-section">
      <div class="action-left">
        <el-button type="primary" @click="showAddForm">
          <el-icon><Plus /></el-icon>
          添加学期
        </el-button>
      </div>

      <div class="action-right">
        <div class="search-container">
          <el-input
            v-model="searchKeyword"
            placeholder="搜索学期名称..."
            style="width: 260px"
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

    <el-card class="table-card" shadow="never">
      <div class="table-toolbar">
        <div class="toolbar-left">
          <el-text type="info">共 {{ totalCount }} 条记录</el-text>
        </div>
        <div class="toolbar-right">
          <el-button
            size="small"
            type="danger"
            :disabled="!selectedRows.length"
            @click="batchDeleteTerms"
          >
            <el-icon><Delete /></el-icon>
            批量删除 ({{ selectedRows.length }})
          </el-button>
        </div>
      </div>

      <el-table
        v-loading="loading"
        :data="filteredTerms"
        style="width: 100%"
        @selection-change="handleSelectionChange"
        empty-text="暂无数据"
        :header-cell-style="{ background: '#fafafa', color: '#606266' }"
      >
        <el-table-column type="selection" width="55" />

        <el-table-column prop="name" label="学期名称" min-width="180">
          <template #default="{ row }">
            <el-text>{{ row.name || '-' }}</el-text>
          </template>
        </el-table-column>

        <el-table-column prop="startTime" label="开始时间" min-width="160">
          <template #default="{ row }">
            <el-text>{{ formatDate(row.startTime) }}</el-text>
          </template>
        </el-table-column>

        <el-table-column prop="status" label="状态" min-width="120">
          <template #default="{ row }">
            <el-switch
              v-model="row.status"
              :active-value="ture"
              :inactive-value="false"
              active-text="启用"
              inactive-text="禁用"
              inline-prompt
              @change="toggleTermStatus(row)"
            />
          </template>
        </el-table-column>

        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" text @click="editTerm(row)">
              <el-icon><Edit /></el-icon>
              编辑
            </el-button>
            <el-button size="small" text type="danger" @click="deleteTerm(row)">
              <el-icon><Delete /></el-icon>
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>

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

    <el-dialog
      v-model="showAddDialog"
      :title="isEdit ? '编辑学期' : '添加学期'"
      width="520px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="100px"
        label-position="left"
      >
        <el-form-item label="学期名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入学期名称" clearable />
        </el-form-item>

        <el-form-item label="开始时间" prop="startTime">
          <el-date-picker
            v-model="formData.startTime"
            type="date"
            placeholder="请选择开始时间"
            style="width: 100%"
            value-format="YYYY-MM-DD"
            clearable
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="closeDialog">取消</el-button>
        <el-button type="primary" @click="saveTerm" :loading="saving">
          {{ isEdit ? '保存修改' : '添加学期' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Refresh, Edit, Delete } from '@element-plus/icons-vue'
import request from '@/utils/request'

export default {
  name: 'TermManagement',
  components: {
    Plus,
    Search,
    Refresh,
    Edit,
    Delete
  },
  setup() {
    const searchKeyword = ref('')
    const showAddDialog = ref(false)
    const isEdit = ref(false)
    const loading = ref(false)
    const saving = ref(false)
    const termList = ref([])
    const selectedRows = ref([])
    const currentPage = ref(1)
    const pageSize = ref(20)
    const formRef = ref()

    const formData = ref({
      id: null,
      name: '',
      startTime: '',
      status: false
    })

    const formRules = {
      name: [
        { required: true, message: '请输入学期名称', trigger: 'blur' },
        { min: 2, max: 50, message: '学期名称长度在 2 到 50 个字符', trigger: 'blur' }
      ],
      startTime: [{ required: true, message: '请选择开始时间', trigger: 'change' }]
    }

    const formatDate = (value) => {
      if (!value) return '-'
      const d = new Date(value)
      if (Number.isNaN(d.getTime())) return String(value)
      const y = d.getFullYear()
      const m = String(d.getMonth() + 1).padStart(2, '0')
      const day = String(d.getDate()).padStart(2, '0')
      return `${y}-${m}-${day}`
    }

    const normalizeTerm = (item) => {
      const id = item?.id ?? item?.Id ?? null
      const name = item?.name ?? item?.mc ?? item?.Name ?? ''
      const startTime = item?.dateTime ?? ''
      const status = item?.status ?? item?.isDeleted ?? item?.IsDeleted ?? false
      return { id, name, startTime, status }
    }

    const filteredTerms = computed(() => {
      if (!searchKeyword.value) return termList.value
      const keyword = searchKeyword.value.toLowerCase()
      return termList.value.filter((t) => (t.name || '').toLowerCase().includes(keyword))
    })

    const totalCount = computed(() => filteredTerms.value.length)

    const fetchTerms = async () => {
        //加载
      try {
        loading.value = true
        const response = await request.get('/xq/Getallxq')
        const apiResponse = response.data

        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded && Array.isArray(apiResponse?.data)) {
          termList.value = apiResponse.data.map(normalizeTerm)
        } else {
          termList.value = []
        }
      } catch (error) {
        console.error('获取学期数据出错:', error)
        termList.value = []
        ElMessage.error('获取学期数据失败')
      } finally {
        loading.value = false
      }
    }

    const refreshData = () => {
      fetchTerms()
    }

    const handleSelectionChange = (selection) => {
      selectedRows.value = selection
    }

    const showAddForm = () => {
      formData.value = {
        id: null,
        name: '',
        startTime: '',
        status: false
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
        name: '',
        startTime: '',
        status: false
      }
      if (formRef.value) {
        formRef.value.resetFields()
      }
    }

    const saveTerm = async () => {
      if (!formRef.value) return
      try {
        const valid = await formRef.value.validate()
        if (!valid) return
      } catch {
        return
      }

      saving.value = true
      //编辑学期
      try {
        if (isEdit.value) {
          const requestData = {
            Id: formData.value.id,
            Name: formData.value.name,
            DateTime: formData.value.startTime,
          }
          const response = await request.post('/xq/Updatexq', requestData)
          const apiResponse = response.data

          if (apiResponse?.statusCode === 200 && (apiResponse?.succeeded || apiResponse?.data?.code === 200)) {
            ElMessage.success('编辑学期成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse?.message || apiResponse?.data?.message || '编辑学期失败')
          }
        } else {
          //添加学期
          const requestData = {
            Name: formData.value.name,
            DateTime: formData.value.startTime,
          }
          const response = await request.post('/xq/Addxq', requestData)
          const apiResponse = response.data

          if (apiResponse?.statusCode === 200 && (apiResponse?.succeeded || apiResponse?.data?.code === 200)) {
            ElMessage.success('添加学期成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse?.message || apiResponse?.data?.message || '添加学期失败')
          }
        }
      } catch (error) {
        console.error('保存失败:', error)
        ElMessage.error('保存失败，请重试')
      } finally {
        saving.value = false
      }
    }

    const editTerm = (term) => {
      formData.value = {
        id: term.id,
        name: term.name || '',
        startTime: term.startTime || '',
        status: term.status ?? false
      }
      isEdit.value = true
      showAddDialog.value = true
    }
//删除学期
    const deleteTerm = async (term) => {
      try {
        await ElMessageBox.confirm(`确定要删除学期"${term.name}"吗？`, '删除确认', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })

        const requestData = { id: term.id }
        const response = await request.post('/xq/Deletexq', requestData)
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded) {
          ElMessage.success('删除学期成功')
          refreshData()
        } else {
          ElMessage.error(apiResponse?.message || '删除学期失败')
        }
      } catch (error) {
        if (error === 'cancel' || error === 'close') return
        console.error('删除学期失败:', error)
      }
    }

    const batchDeleteTerms = async () => {
      if (!selectedRows.value.length) {
        ElMessage.warning('请先选择要删除的学期')
        return
      }

      try {
        await ElMessageBox.confirm(`确定要删除选中的 ${selectedRows.value.length} 个学期吗？`, '批量删除确认', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })

        const deletePromises = selectedRows.value.map((t) => request.post('/xq/Deletexq', { id: t.id }))
        const results = await Promise.allSettled(deletePromises)

        let successCount = 0
        let failCount = 0
        results.forEach((r) => {
          if (r.status === 'fulfilled') {
            const apiResponse = r.value.data
            if (apiResponse?.statusCode === 200 && apiResponse?.succeeded) successCount++
            else failCount++
          } else {
            failCount++
          }
        })

        if (successCount > 0 && failCount === 0) {
          ElMessage.success(`成功删除 ${successCount} 个学期`)
        } else if (successCount > 0 && failCount > 0) {
          ElMessage.warning(`成功删除 ${successCount} 个学期，${failCount} 个删除失败`)
        } else {
          ElMessage.error('批量删除失败')
        }

        selectedRows.value = []
        refreshData()
      } catch (error) {
        if (error === 'cancel' || error === 'close') return
        console.error('批量删除失败:', error)
        ElMessage.error('批量删除失败，请重试')
      }
    }

    const toggleTermStatus = async (term) => {
      try {
        const requestData = {
          id: term.id,
          status: term.status
        }
        // 根据状态选择不同的接口
        const apiUrl = term.status ? '/xq/Openxq' : '/xq/Closexq'
        const response = await request.post(apiUrl, requestData)
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded) {
          const actionText = term.status ? '启用' : '禁用'
          ElMessage.success(`${actionText}学期成功`)
          refreshData()
        } else {
          term.status = !term.status
          ElMessage.error(apiResponse?.message || '状态更新失败')
        }
      } catch (error) {
        console.error('更新状态失败:', error)
        term.status = !term.status
        ElMessage.error('状态更新失败，请重试')
      }
    }

    onMounted(() => {
      fetchTerms()
    })

    return {
      searchKeyword,
      showAddDialog,
      isEdit,
      loading,
      saving,
      termList,
      filteredTerms,
      totalCount,
      selectedRows,
      currentPage,
      pageSize,
      formRef,
      formData,
      formRules,
      formatDate,
      showAddForm,
      closeDialog,
      saveTerm,
      editTerm,
      deleteTerm,
      batchDeleteTerms,
      refreshData,
      fetchTerms,
      handleSelectionChange,
      toggleTermStatus,
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
.term-management {
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

:deep(.el-loading-mask) {
  border-radius: 8px;
}

:deep(.el-table__empty-block) {
  min-height: 200px;
}
</style>
