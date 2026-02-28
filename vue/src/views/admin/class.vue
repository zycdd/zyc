<template>
  <div class="course-management">
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">课程管理</h1>
        <p class="page-description">管理系统中的课程信息</p>
      </div>
    </div>

    <div class="action-section">
      <div class="action-left">
        <el-button type="primary" @click="showAddForm">
          <el-icon><Plus /></el-icon>
          添加课程
        </el-button>
      </div>

      <div class="action-right">
        <div class="search-container">
          <el-select v-model="searchType" placeholder="搜索类型" style="width: 120px; margin-right: 8px">
            <el-option label="按课程名称" value="courseName" />
            <el-option label="按课程编号" value="courseCode" />
            <el-option label="按开课学院" value="college" />
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
          <el-button size="small" type="danger" :disabled="!selectedRows.length" @click="batchDeleteCourses">
            <el-icon><Delete /></el-icon>
            批量删除 ({{ selectedRows.length }})
          </el-button>
        </div>
      </div>

      <el-table v-loading="loading" :data="filteredCourses" style="width: 100%" @selection-change="handleSelectionChange" empty-text="暂无数据" :header-cell-style="{ background: '#fafafa', color: '#606266' }">
        <el-table-column type="selection" width="55" />
        <el-table-column prop="courseCode" label="课程编号" min-width="120">
          <template #default="{ row }"><el-text>{{ row.courseCode || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="courseName" label="课程名称" min-width="150">
          <template #default="{ row }"><el-text>{{ row.courseName || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="credits" label="学分" min-width="80">
          <template #default="{ row }"><el-text>{{ row.credits || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="collegeName" label="开课学院" min-width="150">
          <template #default="{ row }"><el-text>{{ row.collegeName || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="majorName" label="开课专业" min-width="150">
          <template #default="{ row }"><el-text>{{ row.majorName || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column prop="termName" label="开课学期" min-width="120">
          <template #default="{ row }"><el-text>{{ row.termName || '-' }}</el-text></template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" text @click="editCourse(row)"><el-icon><Edit /></el-icon> 编辑</el-button>
            <el-button size="small" text type="danger" @click="deleteCourse(row)"><el-icon><Delete /></el-icon> 删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrapper">
        <el-pagination v-model:current-page="currentPage" v-model:page-size="pageSize" :page-sizes="[10, 20, 50, 100]" :total="totalCount" layout="total, sizes, prev, pager, next, jumper" background />
      </div>
    </el-card>

    <el-dialog v-model="showAddDialog" :title="isEdit ? '编辑课程' : '添加课程'" width="540px" :close-on-click-modal="false">
      <el-form ref="formRef" :model="formData" :rules="formRules" label-width="100px" label-position="left">
        <el-form-item label="课程编号" prop="courseCode"><el-input v-model="formData.courseCode" placeholder="请输入课程编号" clearable /></el-form-item>
        <el-form-item label="课程名称" prop="courseName"><el-input v-model="formData.courseName" placeholder="请输入课程名称" clearable /></el-form-item>
        <el-form-item label="学分" prop="credits"><el-input v-model="formData.credits" placeholder="请输入学分" clearable /></el-form-item>
        <el-form-item label="开课学院" prop="collegeId">
          <el-select v-model="formData.collegeId" placeholder="请选择开课学院" style="width: 100%" clearable @change="onCollegeChange">
            <el-option v-for="college in collegeList" :key="college.id" :label="college.name" :value="college.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="开课专业" prop="majorId">
          <el-select v-model="formData.majorId" placeholder="请选择开课专业" style="width: 100%" clearable :disabled="!formData.collegeId">
            <el-option v-for="major in majorList" :key="major.id" :label="major.name" :value="major.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="开课学期" prop="termId">
          <el-select v-model="formData.termId" placeholder="请选择开课学期" style="width: 100%" clearable>
            <el-option v-for="term in termList" :key="term.id" :label="term.name" :value="term.id" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="closeDialog">取消</el-button>
        <el-button type="primary" @click="saveCourse" :loading="saving">{{ isEdit ? '保存修改' : '添加课程' }}</el-button>
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
  name: 'CourseManagement',
  components: { Plus, Search, Refresh, Edit, Delete },
  setup() {
    const searchKeyword = ref('')
    const searchType = ref('courseName')
    const showAddDialog = ref(false)
    const isEdit = ref(false)
    const loading = ref(false)
    const saving = ref(false)
    const courseList = ref([])
    const collegeList = ref([])
    const majorList = ref([])
    const termList = ref([])
    const selectedRows = ref([])
    const currentPage = ref(1)
    const pageSize = ref(20)
    const formRef = ref()

    const formData = ref({ id: null, courseCode: '', courseName: '', credits: '', collegeId: null, majorId: null, termId: null, status: false })

    const formRules = {
      courseCode: [{ required: true, message: '请输入课程编号', trigger: 'blur' }],
      courseName: [{ required: true, message: '请输入课程名称', trigger: 'blur' }],
      credits: [{ required: true, message: '请输入学分', trigger: 'blur' }],
      collegeId: [{ required: true, message: '请选择开课学院', trigger: 'change' }],
      majorId: [{ required: true, message: '请选择开课专业', trigger: 'change' }],
      termId: [{ required: true, message: '请选择开课学期', trigger: 'change' }]
    }

    const searchPlaceholder = computed(() => {
      switch (searchType.value) {
        case 'courseName': return '搜索课程名称...'
        case 'courseCode': return '搜索课程编号...'
        case 'college': return '搜索开课学院...'
        default: return '搜索...'
      }
    })

    const filteredCourses = computed(() => {
      if (!searchKeyword.value) return courseList.value
      const keyword = searchKeyword.value.toLowerCase()
      return courseList.value.filter(c => {
        if (searchType.value === 'courseName') return (c.courseName || '').toLowerCase().includes(keyword)
        if (searchType.value === 'courseCode') return (c.courseCode || '').toLowerCase().includes(keyword)
        if (searchType.value === 'college') return (c.collegeName || '').toLowerCase().includes(keyword)
        return false
      })
    })

    const totalCount = computed(() => filteredCourses.value.length)

    // 获取学院列表
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

    // 获取学期列表
    const fetchTerms = async () => {
      try {
        const response = await request.get('/xq/Getallxq')
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded && Array.isArray(apiResponse?.data)) {
          termList.value = apiResponse.data.map(item => ({ id: item.id, name: item.name }))
        }
      } catch (error) {
        console.error('获取学期列表失败:', error)
      }
    }

    // 获取课程信息
    const fetchCourses = async () => {
      try {
        loading.value = true
        const response = await request.get('/kc/Getnowkc')
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded && Array.isArray(apiResponse?.data)) {
          const courses = []
          for (const item of apiResponse?.data) {
            let collegeName = '', majorName = '', termName = ''
            try {
              const formData = new URLSearchParams()
              formData.append('id', item.xyid)
              const collegeRes = await request.post(`/kc/Getxybyid`, formData, {
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
              const majorRes = await request.post(`/kc/Getzybyid`, formData, {
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
            try {
              const formData = new URLSearchParams()
              formData.append('id', item.xqid)
              const termRes = await request.post(`/kc/Getxqbyid`, formData, {
                headers: {
                  'Content-Type': 'application/x-www-form-urlencoded'
                }
              })
              if (termRes.data?.statusCode === 200 && termRes.data?.succeeded) {
                termName = termRes.data.data || ''
              }
            } catch (error) {
              console.error('获取学期名称失败:', error)
            }
            courses.push({
              id: item.id,
              courseCode: item.kcbh,
              courseName: item.name,
              credits: item.xf,
              collegeId: item.xyid,
              collegeName,
              majorId: item.zyid,
              majorName,
              termId: item.xqid,
              termName,
              status: item.isDeleted
            })
          }
          courseList.value = courses
        } else {
          courseList.value = []
        }
      } catch (error) {
        console.error('获取课程数据出错:', error)
        courseList.value = []
        ElMessage.error('获取课程数据失败')
      } finally {
        loading.value = false
      }
    }

    // 获取专业信息
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

    const refreshData = () => { fetchCourses() }

    const handleSelectionChange = (selection) => { selectedRows.value = selection }

    const showAddForm = () => {
      formData.value = { id: null, courseCode: '', courseName: '', credits: '', collegeId: null, majorId: null, termId: null, status: false }
      majorList.value = []
      isEdit.value = false
      if (formRef.value) formRef.value.resetFields()
      showAddDialog.value = true
    }

    const closeDialog = () => {
      showAddDialog.value = false
      isEdit.value = false
      formData.value = { id: null, courseCode: '', courseName: '', credits: '', collegeId: null, majorId: null, termId: null, status: false }
      majorList.value = []
      if (formRef.value) formRef.value.resetFields()
    }

    const saveCourse = async () => {
      if (!formRef.value) return
      try {
        const valid = await formRef.value.validate()
        if (!valid) return
      } catch { return }

      saving.value = true
      try {
        if (isEdit.value) {
          const requestData = { id: formData.value.id, kcbh: formData.value.courseCode, name: formData.value.courseName, xf: formData.value.credits, xyid: formData.value.collegeId, zyid: formData.value.majorId, xqid: formData.value.termId }
          const response = await request.post('/kc/Updatekc', requestData)
          const apiResponse = response.data
          if (apiResponse?.statusCode === 200 && (apiResponse?.succeeded || apiResponse?.data?.code === 200)) {
            ElMessage.success('编辑课程成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse?.message || apiResponse?.data?.message || '编辑课程失败')
          }
        } else {
          // 添加课程
          const requestData = { kcbh: formData.value.courseCode, name: formData.value.courseName, xf: formData.value.credits, xyid: formData.value.collegeId, zyid: formData.value.majorId, xqid: formData.value.termId }
          const response = await request.post('/kc/Addkc', requestData)
          const apiResponse = response.data
          if (apiResponse?.statusCode === 200 && (apiResponse?.succeeded || apiResponse?.data?.code === 200)) {
            ElMessage.success('添加课程成功')
            closeDialog()
            refreshData()
          } else {
            ElMessage.error(apiResponse?.message || apiResponse?.data?.message || '添加课程失败')
          }
        }
      } catch (error) {
        console.error('保存失败:', error)
        ElMessage.error('保存失败，请重试')
      } finally {
        saving.value = false
      }
    }

    const editCourse = async (course) => {
      formData.value = { id: course.id, courseCode: course.courseCode, courseName: course.courseName, credits: course.credits, collegeId: course.collegeId, majorId: course.majorId, termId: course.termId, status: course.status ?? false }
      if (course.collegeId) {
        await fetchMajors(course.collegeId)
      }
      isEdit.value = true
      showAddDialog.value = true
    }

    // 删除课程
    const deleteCourse = async (course) => {
      try {
        await ElMessageBox.confirm(`确定要删除课程"${course.courseName}"吗？`, '删除确认', { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' })
        const formData = new URLSearchParams()
        formData.append('id', course.id)
        const response = await request.post('/kc/Deletedkc', formData, {
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
          }
        })
        const apiResponse = response.data
        if (apiResponse?.statusCode === 200 && apiResponse?.succeeded) {
          ElMessage.success('删除课程成功')
          refreshData()
        } else {
          ElMessage.error(apiResponse?.message || '删除课程失败')
        }
      } catch (error) {
        if (error === 'cancel' || error === 'close') return
        console.error('删除课程失败:', error)
      }
    }

    const batchDeleteCourses = async () => {
      if (!selectedRows.value.length) { ElMessage.warning('请先选择要删除的课程'); return }
      try {
        await ElMessageBox.confirm(`确定要删除选中的 ${selectedRows.value.length} 个课程吗？`, '批量删除确认', { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' })
        const deletePromises = selectedRows.value.map(c => {
          const formData = new URLSearchParams()
          formData.append('id', c.id)
          return request.post('/kc/Deletedkc', formData, {
            headers: {
              'Content-Type': 'application/x-www-form-urlencoded'
            }
          })
        })
        const results = await Promise.allSettled(deletePromises)
        let successCount = 0, failCount = 0
        results.forEach(r => { if (r.status === 'fulfilled') { const apiResponse = r.value.data; if (apiResponse?.statusCode === 200 && apiResponse?.succeeded) successCount++; else failCount++; } else failCount++; })
        if (successCount > 0 && failCount === 0) ElMessage.success(`成功删除 ${successCount} 个课程`)
        else if (successCount > 0 && failCount > 0) ElMessage.warning(`成功删除 ${successCount} 个课程，${failCount} 个删除失败`)
        else ElMessage.error('批量删除失败')
        selectedRows.value = []
        refreshData()
      } catch (error) {
        if (error === 'cancel' || error === 'close') return
        console.error('批量删除失败:', error)
        ElMessage.error('批量删除失败，请重试')
      }
    }

    onMounted(() => { fetchColleges(); fetchTerms(); fetchCourses() })

    return {
      searchKeyword, searchType, showAddDialog, isEdit, loading, saving, courseList, collegeList, majorList, termList, filteredCourses, totalCount, selectedRows, currentPage, pageSize, formRef, formData, formRules,
      searchPlaceholder, showAddForm, closeDialog, saveCourse, editCourse, deleteCourse, batchDeleteCourses, refreshData, fetchCourses, handleSelectionChange,
      onCollegeChange, fetchMajors, fetchTerms,
      Plus, Search, Refresh, Edit, Delete
    }
  }
}
</script>

<style scoped>
.course-management { padding: 0; background: transparent }
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
:deep(.el-input-number__wrapper) { border-radius: 6px; border: 1px solid #d9d9d9; transition: all 0.3s ease }
:deep(.el-input-number__wrapper:hover) { border-color: #40a9ff }
:deep(.el-input-number.is-focus .el-input-number__wrapper) { border-color: #1890ff; box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2) }
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