// JWT token 解析工具
export function parseJwt(token) {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    )
    return JSON.parse(jsonPayload)
  } catch (error) {
    console.error('JWT解析失败:', error)
    return null
  }
}

// 从token中获取用户角色
export function getRoleFromToken(token) {
  const payload = parseJwt(token)
  return payload?.Role || null
}

// 从token中获取用户ID
export function getUserIdFromToken(token) {
  const payload = parseJwt(token)
  return payload?.UserId || null
}

// 从token中获取账号
export function getAccountFromToken(token) {
  const payload = parseJwt(token)
  return payload?.Account || null
}
