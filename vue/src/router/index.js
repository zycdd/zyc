import { createRouter, createWebHistory } from 'vue-router'
import Login from '@/views/Login.vue'
import Index from '@/views/index.vue'

const routes = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    name: 'Login',
    component: Login
  },
  {
    path: '/',
    component: Index,
    meta: { requiresAuth: true },
    children: [
      {
        path: 'college',
        name: 'CollegeManagement',
        component: () => import('@/views/admin/xygl.vue')
      },
      {
        path: 'major',
        name: 'MajorManagement',
        component: () => import('@/views/admin/zygl.vue')
      },
      {
        path: 'user',
        name: 'UserManagement',
        component: () => import('@/views/admin/user.vue')
      },
      {
        path: 'term',
        name: 'TermManagement',
        component: () => import('@/views/admin/xqgl.vue')
      },
      {
        path: 'teacher',
        name: 'TeacherManagement',
        component: () => import('@/views/admin/teacher.vue')
      },
      {
        path: 'student',
        name: 'StudentManagement',
        component: () => import('@/views/admin/student.vue')
      },
      {
        path: 'course',
        name: 'CourseManagement',
        component: () => import('@/views/admin/class.vue')
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  
  if (to.meta.requiresAuth && !token) {
    // 需要登录但没有token，跳转到登录页
    next('/login')
  } else if (to.path === '/login' && token) {
    // 已登录用户访问登录页，跳转到首页
    next('/college')
  } else {
    next()
  }
})

export default router
