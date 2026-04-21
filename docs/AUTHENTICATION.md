# Аутентификация и Авторизация в WebStorage API

## 📋 Содержание

- [Обзор](#-обзор)
- [JWT токены](#-jwt-токены)
- [Access vs Refresh токены](#-access-vs-refresh-токены)
- [Безопасность](#-безопасность)
- [Ошибки аутентификации](#-ошибки-аутентификации)
- [Best Practices](#-best-practices)

---

## 🔍 Обзор

WebStorage использует **JWT (JSON Web Tokens)** с двухуровневой системой токенов:

```
┌─────────────────────────────────────────────────────────────┐
│                    1. Регистрация/Вход                      │
│                  POST /api/auth/register                    │
│                    POST /api/auth/login                     │
└────────────────────────┬────────────────────────────────────┘
                         │
         ┌───────────────┴───────────────┐
         ↓                               ↓
    Access Token                  Refresh Token
    (15-60 мин)                   (7-30 дней)
    В теле ответа             В HttpOnly Cookie
    Короткий срок             Длительный срок
    Для каждого запроса      Для обновления
```

---

## 🎫 JWT Токены

### Структура JWT

JWT состоит из 3 частей, разделённых точками:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJzdXNlciIsImlhdCI6MTUxNjIzOTAyMn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

[Header].[Payload].[Signature]
```

**Декодирование (base64):**

```json
// Header
{
  "alg": "HS256",
  "typ": "JWT"
}

// Payload
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "roles": ["User"],
  "iat": 1516239022,
  "exp": 1516242622
}

// Signature
HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  secret
)
```

### Декодирование токена в JavaScript

```javascript
function decodeToken(token) {
  const parts = token.split('.');
  const payload = JSON.parse(atob(parts[1]));
  return payload;
}

const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
const decoded = decodeToken(token);
console.log("User ID:", decoded.sub);
console.log("Expires:", new Date(decoded.exp * 1000));
```

### Проверка истечения токена

```javascript
function isTokenExpired(token) {
  const decoded = decodeToken(token);
  const now = Math.floor(Date.now() / 1000);
  return decoded.exp < now;
}

// Использование
if (isTokenExpired(accessToken)) {
  console.log("Токен истёк, обновить!");
}
```

---

## 🔑 Access vs Refresh токены

### Access Token (Токен доступа)

**Назначение:** Аутентификация в каждом запросе к API

**Характеристики:**
- ✅ Короткоживущий (15-60 минут)
- ✅ Передаётся в заголовке `Authorization: Bearer {token}`
- ✅ Используется для защиты endpoints
- ✅ Содержит информацию о пользователе и ролях
- ❌ При утечке угроза ограничена (небольшое время жизни)

**Когда использовать:**
```javascript
const response = await fetch('/api/files', {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});
```

### Refresh Token (Токен обновления)

**Назначение:** Получение нового access token без повторного входа

**Характеристики:**
- ✅ Длительный срок (7-30 дней)
- ✅ Хранится в HttpOnly cookie (не доступен для JavaScript)
- ✅ Безопаснее access token
- ✅ Может быть отозван на сервере
- ✅ Для XSS атак использовать невозможно

**Автоматическое обновление cookie:**
```bash
curl -b cookies.txt -X POST /api/auth/refresh
```

Сервер отправляет новый refresh token:
```
Set-Cookie: RefreshToken=new_token; HttpOnly; Secure; Path=/api/auth
```

---

## 🔒 Безопасность

### ✅ Хорошо

1. **HttpOnly Cookie для Refresh Token**
   - JavaScript код не может получить доступ (`document.cookie` вернёт пусто)
   - Защита от XSS атак
   - Отправляется автоматически браузером в запросе

2. **JWT с подписью**
   - Подпись проверяется на сервере
   - Гарантирует, что токен не был изменён

3. **Разделение токенов**
   - Access token короткоживущий
   - Refresh token долгоживущий и защищённый

4. **Secure флаг в Production**
   - Токен передаётся только по HTTPS
   - Защита от перехвата в открытых WiFi сетях

### ❌ Опасно

1. **Хранить Access Token в localStorage**
   ```javascript
   // ❌ НЕПРАВИЛЬНО
   localStorage.setItem('accessToken', token);
   // XSS атака может украсть его
   ```

2. **Передавать Refresh Token в URL**
   ```javascript
   // ❌ НЕПРАВИЛЬНО
   fetch(`/api/auth/refresh?token=${refreshToken}`);
   // Токен попадёт в логи сервера и истории браузера
   ```

3. **Игнорировать истечение токена**
   ```javascript
   // ❌ НЕПРАВИЛЬНО
   fetch('/api/files', {
     headers: { 'Authorization': `Bearer ${expiredToken}` }
   });
   ```

### ✅ Лучшие практики

1. **Access Token в памяти (RAM)**
   ```javascript
   // ✅ ПРАВИЛЬНО
   let accessToken = null; // В переменной
   
   async function login() {
    const response = await fetch('/api/auth/login', /* ... */);
    const data = await response.json();
    accessToken = data.accessToken; // В памяти
  }
   ```

2. **Refresh Token в HttpOnly Cookie (автоматически)**
   ```javascript
   // ✅ Сервер сам управляет cookie
   // Клиент просто отправляет запрос:
   const response = await fetch('/api/auth/refresh', {
     credentials: 'include' // Включить cookies
   });
   ```

3. **Автоматическое обновление токена**
   ```javascript
   async function fetchWithRefresh(url, options = {}) {
     let response = await fetch(url, {
       ...options,
       headers: {
         ...options.headers,
         'Authorization': `Bearer ${accessToken}`
       }
     });

     // Если 401, обновить и повторить
     if (response.status === 401) {
       const refreshResponse = await fetch('/api/auth/refresh', {
         credentials: 'include'
       });
       
       if (refreshResponse.ok) {
         const data = await refreshResponse.json();
         accessToken = data.accessToken;
         
         // Повторить исходный запрос
         response = await fetch(url, {
           ...options,
           headers: {
             ...options.headers,
             'Authorization': `Bearer ${accessToken}`
           }
         });
       } else {
         // Redirect на страницу входа
         window.location.href = '/login';
       }
     }

     return response;
   }

   // Использование
   await fetchWithRefresh('/api/files');
   ```

4. **Выход при закрытии вкладки**
   ```javascript
   window.addEventListener('unload', async () => {
     await fetch('/api/auth/logout', { 
       method: 'POST',
       credentials: 'include'
     });
   });
   ```

---

## ⚠️ Ошибки аутентификации

### 401 Unauthorized

Причины:
- ❌ Токен отсутствует в заголовке
- ❌ Токен невалиден (повреждён)
- ❌ Токен истёк
- ❌ Неверный email/пароль при входе
- ❌ Refresh token отсутствует/истёк

**Решение:**
```javascript
try {
  const response = await fetch('/api/files', {
    headers: { 'Authorization': `Bearer ${accessToken}` }
  });

  if (response.status === 401) {
    // Токен истёк, обновить
    const refreshResponse = await fetch('/api/auth/refresh', {
      credentials: 'include'
    });

    if (refreshResponse.ok) {
      const data = await refreshResponse.json();
      accessToken = data.accessToken;
      // Повторить запрос
    } else {
      // Redirect на страницу входа
      window.location.href = '/login';
    }
  }
} catch (error) {
  console.error('Ошибка:', error);
}
```

### 403 Forbidden

Причины:
- ❌ Роль пользователя не имеет доступа (например, требуется Admin)
- ❌ Недостаточные разрешения

**Решение:**
```javascript
// Проверить роль до запроса
const decoded = decodeToken(accessToken);
if (!decoded.roles.includes('Admin')) {
  console.log('Недостаточно прав для этой операции');
  // Не отправлять запрос
}
```

### 409 Conflict

При регистрации:
- ❌ Email уже зарегистрирован

**Решение:**
```javascript
const response = await fetch('/api/auth/register', {
  method: 'POST',
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'password'
  })
});

if (response.status === 409) {
  console.log('Email уже зарегистрирован');
  // Предложить восстановление пароля
}
```

---

## 🚀 Best Practices

### 1. Хранение токенов

```javascript
// ✅ ПРАВИЛЬНО: Access token в памяти
let accessToken = null;

// ✅ ПРАВИЛЬНО: Refresh token в cookie (автоматически)
// Нам не нужно ничего делать!

// ❌ НЕПРАВИЛЬНО: Хранить в localStorage
localStorage.setItem('accessToken', token);

// ❌ НЕПРАВИЛЬНО: Хранить в sessionStorage
sessionStorage.setItem('accessToken', token);
```

### 2. Обновление перед использованием

```javascript
// ✅ ПРАВИЛЬНО: Проверить перед использованием
if (isTokenExpired(accessToken)) {
  const response = await fetch('/api/auth/refresh', {
    credentials: 'include'
  });
  const data = await response.json();
  accessToken = data.accessToken;
}

// Затем использовать токен
```

### 3. Обновление при 401

```javascript
// ✅ ПРАВИЛЬНО: Обновить при получении 401
const makeAuthenticatedRequest = async (url) => {
  let response = await fetch(url, {
    headers: { 'Authorization': `Bearer ${accessToken}` }
  });

  if (response.status === 401) {
    // Обновить и повторить
    const refreshResp = await fetch('/api/auth/refresh', {
      credentials: 'include'
    });
    if (refreshResp.ok) {
      const data = await refreshResp.json();
      accessToken = data.accessToken;
      // Повторить исходный запрос
      response = await fetch(url, {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });
    }
  }

  return response;
};
```

### 4. Логирование и мониторинг

```javascript
// ✅ Логировать попытки входа
async function login(email, password) {
  console.log(`[Auth] Попытка входа: ${email}`);
  
  try {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });

    if (response.ok) {
      console.log(`[Auth] ✅ Успешный вход: ${email}`);
      const data = await response.json();
      accessToken = data.accessToken;
    } else {
      console.warn(`[Auth] ❌ Ошибка входа: ${response.status}`);
    }
  } catch (error) {
    console.error(`[Auth] 💥 Ошибка при входе:`, error);
  }
}

// ✅ Логировать обновления
async function refreshToken() {
  console.log('[Auth] Обновление токена...');
  const response = await fetch('/api/auth/refresh', {
    credentials: 'include'
  });

  if (response.ok) {
    console.log('[Auth] ✅ Токен обновлён');
  } else {
    console.warn('[Auth] ❌ Обновление не удалось, требуется повторный вход');
  }
}
```

### 5. Обработка CSRF

```javascript
// ✅ ПРАВИЛЬНО: Использовать credentials для cookies
const response = await fetch('/api/auth/refresh', {
  method: 'POST',
  credentials: 'include', // Включить cookies
  headers: {
    'Content-Type': 'application/json'
  }
});
```

### 6. Тайм-ауты

```javascript
// ✅ ПРАВИЛЬНО: Установить тайм-ауты
const controller = new AbortController();
const timeoutId = setTimeout(() => controller.abort(), 5000);

try {
  const response = await fetch('/api/auth/login', {
    signal: controller.signal,
    // ... остальные параметры
  });
} finally {
  clearTimeout(timeoutId);
}
```

---

## 🔗 Дополнительные ресурсы

- **API Documentation**: `API.md`
- **SETUP.md**: Инструкция по установке
- **README.md**: Основная документация проекта

