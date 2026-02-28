<template>
  <div class="teacher-management">
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">教师管理</h1>
        <p class="page-description">管理系统中的教师信息</p>
      </div>
    </div>

    <div class="action-section">
      <div class="action-left">
        <el-button type="primary" @click="showAddForm">
          <el-icon><Plus /></el-icon>
          添加教师
        </el-button>
      </div>

      <div class="action-right">
        <div class="search-container">
          <el-select v-model="searchType" placeholder="搜索类型" style="width: 120px; margin-right: 8px">
            <el-option label="按姓名" value="name" />
            <el-option label="按工号" value="workId" />
            <el-option label="按学院" value="college" />
            <el-option label="按专业" value="major" />
          </el-select>
          <el-input v-model="searchKeyword" :placeholder="searchPlaceholder" style="width: 200px" clearable>
            <template #prefix><el-icon><Search /></el-icon></template>
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
        <div class="toolbar-left"><el-text type="info">共 {{ totalCount }} 条记录</el-text></div>
        <div class="toolbar-right">
          <el-button size="small" type="danger" :disabled="!selectedRows.length" @click="batchDeleteTeachers">
            <el-icon><Delete /></el-icon>
            批量删除 ({{ selectedRows.length }})
          </el-button>
        </div>
      </div>

      <el-table v-loading="loading" :data="filteredTeachers" style="width: 100%" @selection-change="handleSelectionChange" empty-text="暂无数据" :header-cell-style="{ background: '#fafafa', color: '#606266' }">
        <el-table-column type="selection" width="55" />
        <el-table-column prop="workId" label="工号" min-width="140">
          <template #default="{ row }"><el-text>{{ row.workId || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="name" label="姓名" min-width="120">
          <template #default="{ row }"><el-text>{{ row.name || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="contact" label="联系方式" min-width="140">
          <template #default="{ row }"><el-text>{{ row.contact || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="collegeName" label="所属学院" min-width="150">
          <template #default="{ row }"><el-text>{{ row.collegeName || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="majorName" label="所属专业" min-width="150">
          <template #default="{ row }"><el-text>{{ row.majorName || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" text @click="editTeacher(row)"><el-icon><Edit /></el-icon> 编辑</el-button>
            <el-button size="small" text type="danger" @click="deleteTeacher(row)"><el-icon><Delete /></el-icon> 删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrapper">
        <el-pagination v-model:current-page="currentPage" v-model:page-size="pageSize" :page-sizes="[10, 20, 50, 100]" :total="totalCount" layout="total, sizes, prev, pager, next, jumper" background />
      </div>
    </el-card>

    <el-dialog v-model="showAddDialog" :title="isEdit ? '编辑教师' : '添加教师'" width="540px" :close-on-click-modal="false">
      <el-form ref="formRef" :model="formData" :rules="formRules" label-width="100px" label-position="left">
        <el-form-item label="工号" prop="workId"><el-input v-model="formData.workId" placeholder="请输入工号" clearable /></el-form-item>
        <el-form-item label="姓名" prop="name"><el-input v-model="formData.name" placeholder="请输入姓名" clearable /></el-form-item>
        <el-form-item label="联系方式" prop="contact"><el-input v-model="formData.contact" placeholder="请输入联系方式" clearable /></el-form-item>
        <el-form-item label="所属学院" prop="collegeId">
          <el-select v-model="formData.collegeId" placeholder="请选择所属学院" style="width: 100%" clearable @change="onCollegeChange">
            <el-option v-for="college in collegeList" :key="college.id" :label="college.name" :value="college.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="所属专业" prop="majorId">
          <el-select v-model="formData.majorId" placeholder="请选择所属专业" style="width: 100%" clearable :disabled="!formData.collegeId">
            <el-option v-for="major in majorList" :key="major.id" :label="major.name" :value="major.id" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="closeDialog">取消</el-button>
        <el-button type="primary" @click="saveTeacher" :loading="saving">{{ isEdit ? '保存修改' : '添加教师' }}</el-button>
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
  name: 'TeacherManagement',
  components: { Plus, Search, Refresh, Edit, Delete },
  setup() {
    const searchKeyword = ref('')
    const searchType = ref('name')
    const showAddDialog = ref(false)
    const isEdit = ref(false)
    const loading = ref(false)
    const saving = ref(false)
    const teacherList = ref([])
    const collegeList = ref([])
    const majorList = ref([])
    const selectedRows = ref([])
    const currentPage = ref(1)
    const pageSize = ref(20)
    const formRef = ref()

    const formData = ref({ id: null, workId: '', name: '', contact: '', collegeId: null, majorId: null, status: false })

    const formRules = {
      workId: [{ required: true, message: '请输入工号', trigger: 'blur' }],
      name: [{ required: true, message: '请输入姓名', trigger: 'blur' }],
      contact: [{ required: true, message: '请输入联系方式', trigger: 'blur' }],
      collegeId: [{ required: true, message: '请选择所属学院', trigger: 'change' }],
      majorId: [{ required: true, message: '请选择所属专业', trigger: 'change' }]
    }

    const searchPlaceholder = computed(() => {
      switch (searchType.value) {
        case 'name': return '搜索姓名...'
        case 'workId': return '搜索工号...'
        case 'college': return '搜索学院...'
        case 'major': return '搜索专业...'
        default: return '搜索...'
      }
    })

    const filteredTeachers = computed(() => {
      if (!searchKeyword.value) return teacherList.value
      const keyword = searchKeyword.value.toLowerCase()
      return teacherList.value.filter(t => {
        if (searchType.value === 'name') return (t.name || '').toLowerCase().includes(keyword)
        if (searchType.value === 'workId') return (t.workId || '').toLowerCase().includes(keyword)
        if (searchType.value === 'college') return (t.collegeName || '').toLowerCase().includes(keyword)
        if (searchType.value === 'major') return (t.majorName || '').toLowerCase().includes(keyword)
        return false
      })
    })

    const totalCount = computed(() => filteredTeachers.value.length)
//获取学院列表
    const fetchColleges = async () => {
      try {
        const response = await request.get('/xy/Getallxy')
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded && Array.isArray(apiResponse?.data)) {
          collegeList.value = apiResponse.data.map(item => ({ id: item.id, name: item.name }))
        }
      } catch (error) {
        console.error('获取学院列表失败:', error)
      }
    }
//获取教师信息
    const fetchTeachers = async () => {
      try {
        loading.value = true
        const response = await request.get('/tea/Getalltea')
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded && Array.isArray(apiResponse?.data)) {
          const teachers = []
          for (const item of apiResponse?.data) {
            let collegeName = '', majorName = ''
            try {
              const formData = new URLSearchParams()
              formData.append('id', item.xyid)
              const collegeRes = await request.post(`/tea/Getxybyid`, formData, {
                headers: {
                  'Content-Type': 'application/x-www-form-urlencoded'
                }
              })
               if (collegeRes.data?.statusCode === 200 && collegeRes.data?.succeeded) {
                 collegeName = collegeRes.data.data || ''
               }
            } catch (error) {
              console.error('获取学院名称失败:', error)
            }
            try {
              const formData = new URLSearchParams()
              formData.append('id', item.zyid)
              const majorRes = await request.post(`/tea/Getzybyid`, formData, {
                headers: {
                  'Content-Type': 'application/x-www-form-urlencoded'
                }
              })
              if (majorRes.data?.statusCode === 200 && majorRes.data?.succeeded) {
                majorName = majorRes.data.data || ''
              }
            } catch (error) {
              console.error('获取专业名称失败:', error)
            }
            teachers.push({
              id: item.id,
              workId: item.gh,
              name: item.name,
              contact: item.phone,
              collegeId: item.xyid,
              collegeName,
              majorId: item.zyid,
              majorName,
              status: item.isDeleted
            })
          }
          teacherList.value = teachers
        } else {
          teacherList.value = []
        }
      } catch (error) {
        console.error('获取教师数据出错:', error)
        teacherList.value = []
        ElMessage.error('获取教师数据失败')
      } finally {
        loading.value = false
      }
    }
//获取专业信息
    const fetchMajors = async (collegeId) => {
      try {
        const response = await request.get('/zy/Getallzy')
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded && Array.isArray(apiResponse?.data)) {
          majorList.value = apiResponse.data
            .filter(item => item.xyid === collegeId)
            .map(item => ({ id: item.id, name: item.name }))
        } else {
          majorList.value = []
        }
      } catch (error) {
        console.error('获取专业列表失败:', error)
        majorList.value = []
      }
    }

    const onCollegeChange = (collegeId) => {
      formData.value.majorId = null
      if (collegeId) {
        fetchMajors(collegeId)
      } else {
        majorList.value = []
      }
    }

    const refreshData = () => { fetchTeachers() }

    const handleSelectionChange = (selection) => { selectedRows.value = selection }

    const showAddForm = () => {
      formData.value = { id: null, workId: '', name: '', contact: '', collegeId: null, majorId: null, status: false }
      majorList.value = []
      isEdit.value = false
      if (formRef.value) formRef.value.resetFields()
      showAddDialog.value = true
    }

    const closeDialog = () => {
      showAddDialog.value = false
      isEdit.value = false
      formData.value = { id: null, workId: '', name: '', contact: '', collegeId: null, majorId: null, status: false }
      majorList.value = []
      if (formRef.value) formRef.value.resetFields()
    }

    const saveTeacher = async () => {
      if (!formRef.value) return
      try {
        const valid = await formRef.value.validate()
        if (!valid) return
      } catch { return }

      saving.value = true
      try {
        if (isEdit.value) {
          const requestData = { id: formData.value.id, gh: formData.value.workId, name: formData.value.name, phone: formData.value.contact, xyid: formData.value.collegeId, zyid: formData.value.majorId }
          const response = await request.post('/tea/Updatetea', requestData)
          const apiResponse = response.data
          if (apiResponse?.statusCode === 200 && (apiResponse?.succeeded || apiResponse?.data?.code === 200)) {
            ElMessage.success('编辑教师成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse?.message || apiResponse?.data?.message || '编辑教师失败')
          }
        } else {
          // 添加教师
          const requestData = { gh: formData.value.workId, name: formData.value.name, phone: formData.value.contact, xyid: formData.value.collegeId, zyid: formData.value.majorId }
          const response = await request.post('/tea/Addtea', requestData)
          const apiResponse = response.data
          if (apiResponse?.statusCode === 200 && (apiResponse?.succeeded || apiResponse?.data?.code === 200)) {
            ElMessage.success('添加教师成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse?.message || apiResponse?.data?.message || '添加教师失败')
          }
        }
      } catch (error) {
        console.error('保存失败:', error)
        ElMessage.error('保存失败，请重试')
      } finally {
        saving.value = false
      }
    }

    const editTeacher = async (teacher) => {
      formData.value = { id: teacher.id, workId: teacher.workId, name: teacher.name, contact: teacher.contact, collegeId: teacher.collegeId, majorId: teacher.majorId, status: teacher.status ?? false }
      if (teacher.collegeId) {
        await fetchMajors(teacher.collegeId)
      }
      isEdit.value = true
      showAddDialog.value = true
    }
//删除教师
    const deleteTeacher = async (teacher) => {
      try {
        await ElMessageBox.confirm(`确定要删除教师"${teacher.name}"吗？`, '删除确认', { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' })
        const formData = new URLSearchParams()
        formData.append('id', teacher.id)
        const response = await request.post('/tea/Deletedtea', formData, {
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
          }
        })
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded) {
          ElMessage.success('删除教师成功')
          refreshData()
        } else {
          ElMessage.error(apiResponse?.message || '删除教师失败')
        }
      } catch (error) {
        if (error === 'cancel' || error === 'close') return
        console.error('删除教师失败:', error)
      }
    }

    const batchDeleteTeachers = async () => {
      if (!selectedRows.value.length) { ElMessage.warning('请先选择要删除的教师'); return }
      try {
        await ElMessageBox.confirm(`确定要删除选中的 ${selectedRows.value.length} 个教师吗？`, '批量删除确认', { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' })
        const deletePromises = selectedRows.value.map(t => {
          const formData = new URLSearchParams()
          formData.append('id', t.id)
          return request.post('/tea/Deletedtea', formData, {
            headers: {
              'Content-Type': 'application/x-www-form-urlencoded'
            }
          })
        })
        const results = await Promise.allSettled(deletePromises)
        let successCount = 0, failCount = 0
        results.forEach(r => { if (r.status === 'fulfilled') { const apiResponse = r.value.data; if (apiResponse?.statusCode === 200 && apiResponse?.succeeded) successCount++; else failCount++; } else failCount++; })
        if (successCount > 0 && failCount === 0) ElMessage.success(`成功删除 ${successCount} 个教师`)
        else if (successCount > 0 && failCount > 0) ElMessage.warning(`成功删除 ${successCount} 个教师，${failCount} 个删除失败`)
        else ElMessage.error('批量删除失败')
        selectedRows.value = []
        refreshData()
      } catch (error) {
        if (error === 'cancel' || error === 'close') return
        console.error('批量删除失败:', error)
        ElMessage.error('批量删除失败，请重试')
      }
    }

    const toggleTeacherStatus = async (teacher) => {
      try {
        const requestData = { id: teacher.id, status: teacher.status }
        const apiUrl = teacher.status ? '/teacher/OpenTeacher' : '/teacher/CloseTeacher'
        const response = await request.post(apiUrl, requestData)
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded) {
          const actionText = teacher.status ? '启用' : '禁用'
          ElMessage.success(`${actionText}教师成功`)
          refreshData()
        } else {
          teacher.status = !teacher.status
          ElMessage.error(apiResponse?.message || '状态更新失败')
        }
      } catch (error) {
        console.error('更新状态失败:', error)
        teacher.status = !teacher.status
        ElMessage.error('状态更新失败，请重试')
      }
    }

    onMounted(() => { fetchColleges(); fetchTeachers() })

    return {
      searchKeyword, searchType, showAddDialog, isEdit, loading, saving, teacherList, collegeList, majorList, filteredTeachers, totalCount, selectedRows, currentPage, pageSize, formRef, formData, formRules,
      searchPlaceholder, showAddForm, closeDialog, saveTeacher, editTeacher, deleteTeacher, batchDeleteTeachers, refreshData, fetchTeachers, handleSelectionChange, toggleTeacherStatus,
      onCollegeChange, fetchMajors,
      Plus, Search, Refresh, Edit, Delete
    }
  }
}
</script>

<style scoped>
.teacher-management { padding: 0; background: transparent }
.page-header { margin-bottom: 24px }
.header-content { display: flex; flex-direction: column; gap: 4px }
.page-title { font-size: 20px; font-weight: 600; color: #262626; margin: 0; line-height: 1.4 }
.page-description { font-size: 14px; color: #8c8c8c; margin: 0; line-height: 1.4 }
.action-section { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; gap: 16px }
.action-left { flex-shrink: 0 }
.action-right { display: flex; align-items: center; gap: 12px }
.search-container { display: flex; align-items: center }
.table-card { border: 1px solid #e8e8e8; border-radius: 8px }
:deep(.el-card__body) { padding: 0 }
.table-toolbar { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #f0f0f0; background: #fafafa }
.toolbar-left { display: flex; align-items: center }
.toolbar-right { display: flex; align-items: center; gap: 12px }
:deep(.el-table) { border: none }
:deep(.el-table__header-wrapper) { border-bottom: 1px solid #e8e8e8 }
:deep(.el-table th) { background: #fafafa; border-bottom: 1px solid #e8e8e8; font-weight: 500; color: #606266 }
:deep(.el-table td) { border-bottom: 1px solid #f5f5f5 }
:deep(.el-table__row:hover > td) { background-color: #f8f9fa }
:deep(.el-table__empty-text) { color: #8c8c8c }
.pagination-wrapper { padding: 16px 20px; display: flex; justify-content: flex-end; border-top: 1px solid #f0f0f0; background: #fafafa }
:deep(.el-pagination) { --el-pagination-bg-color: transparent }
:deep(.el-dialog) { border-radius: 8px }
:deep(.el-dialog__header) { padding: 20px 24px 16px; border-bottom: 1px solid #f0f0f0 }
:deep(.el-dialog__title) { font-size: 16px; font-weight: 600; color: #262626 }
:deep(.el-dialog__body) { padding: 24px }
:deep(.el-dialog__footer) { padding: 16px 24px 20px; border-top: 1px solid #f0f0f0 }
:deep(.el-form-item__label) { color: #262626; font-weight: 500 }
:deep(.el-input__wrapper) { border-radius: 6px; border: 1px solid #d9d9d9; transition: all 0.3s ease }
:deep(.el-input__wrapper:hover) { border-color: #40a9ff }
:deep(.el-input.is-focus .el-input__wrapper) { border-color: #1890ff; box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2) }
:deep(.el-button) { border-radius: 6px; font-weight: 400 }
:deep(.el-button--primary) { background-color: #1890ff; border-color: #1890ff }
:deep(.el-button--primary:hover) { background-color: #40a9ff; border-color: #40a9ff }
:deep(.el-button--danger) { background-color: #ff4d4f; border-color: #ff4d4f }
:deep(.el-button--danger:hover) { background-color: #ff7875; border-color: #ff7875 }
:deep(.el-button.is-text) { padding: 4px 8px; color: #1890ff }
:deep(.el-button.is-text:hover) { background-color: #f0f9ff }
:deep(.el-button.is-text.el-button--danger) { color: #ff4d4f; background-color: transparent }
:deep(.el-button.is-text.el-button--danger:hover) { background-color: transparent }
:deep(.el-loading-mask) { border-radius: 8px }
:deep(.el-table__empty-block) { min-height: 200px }
</style>