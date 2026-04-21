# WebStorage API Документация

## 📚 Оглавление

- [Аутентификация](#-аутентификация)
- [API Endpoints](#-api-endpoints)
  - [Auth](#auth-endpoints)
  - [Files](#files-endpoints)
  - [Storage](#storage-endpoints)
  - [Admin Storage](#admin-storage-endpoints)
- [Модели данных](#-модели-данных)
- [Коды ошибок](#-коды-ошибок)
- [Примеры использования](#-примеры-использования)

---

## 🔐 Аутентификация

WebStorage API использует **JWT (JSON Web Tokens)** для аутентификации.

### Как это работает:

1. **Регистрация / Вход** → Получаете `accessToken` и `refreshToken`
2. **AccessToken** → Короткоживущий (обычно 15-60 минут), используется в каждом запросе
3. **RefreshToken** → Длительный (обычно 7-30 дней), хранится в HttpOnly cookie для безопасности

### Использование токена:

Передавайте `accessToken` в заголовке всех авторизованных запросов:

```http
Authorization: Bearer {accessToken}
```

Пример:
```bash
curl -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
     https://api.webstorage.local/api/files
```

### Обновление токена:

Когда `accessToken` истекает, используйте `refreshToken` (автоматически в cookie):

```bash
curl -X POST \
     -H "Content-Type: application/json" \
     https://api.webstorage.local/api/auth/refresh
```

---

## 🔌 API Endpoints

### Auth Endpoints

#### 📝 POST `/api/auth/register`

Регистрирует нового пользователя.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "expiresAtUtc": "2026-04-21T00:35:19.4473059Z"
}
```

**Error Responses:**
- `400 Bad Request` - Некорректные данные (пароль слабый, email занят и т.д.)
- `409 Conflict` - Пользователь уже существует

**cURL Пример:**
```bash
curl -X POST https://api.webstorage.local/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'
```

---

#### 🔑 POST `/api/auth/login`

Входит в систему с email и паролем.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "expiresAtUtc": "2026-04-21T00:35:19.4473059Z"
}
```

**Refresh Token Cookie:**
- Автоматически установлен в HttpOnly cookie
- Путь: `/api/auth`
- Secure флаг установлен в production

**Error Responses:**
- `401 Unauthorized` - Неверный email или пароль

**cURL Пример:**
```bash
curl -c cookies.txt -X POST https://api.webstorage.local/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'
```

---

#### 🔄 POST `/api/auth/refresh`

Обновляет `accessToken` используя `refreshToken` из cookie.

**Request:**
```
POST /api/auth/refresh
```

Тело запроса не требуется. Refresh token берётся автоматически из cookie.

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "expiresAtUtc": "2026-04-21T00:35:19.4473059Z"
}
```

**Error Responses:**
- `401 Unauthorized` - Refresh token отсутствует или невалиден

**cURL Пример:**
```bash
curl -b cookies.txt -X POST https://api.webstorage.local/api/auth/refresh \
  -H "Content-Type: application/json"
```

---

#### 🚪 POST `/api/auth/logout`

Выходит из системы и очищает refresh token.

**Request:**
```
POST /api/auth/logout
Authorization: Bearer {accessToken}
```

**Response (204 No Content):**
```
(пусто)
```

**Cookie:**
- Refresh token удаляется из cookie

**cURL Пример:**
```bash
curl -b cookies.txt -X POST https://api.webstorage.local/api/auth/logout \
  -H "Authorization: Bearer {accessToken}"
```

---

### Files Endpoints

#### 📂 GET `/api/files`

Получает список всех файлов текущего пользователя.

**Request:**
```
GET /api/files
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "name": "document.pdf",
    "size": 2048576,
    "uploadedAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "image.jpg",
    "size": 1024000,
    "uploadedAt": "2024-01-14T15:45:00Z"
  }
]
```

**Поля ответа:**
- `id` - Уникальный идентификатор файла (Guid)
- `name` - Имя файла
- `size` - Размер файла в байтах
- `uploadedAt` - Дата и время загрузки (ISO 8601 UTC)

**Error Responses:**
- `401 Unauthorized` - Токен отсутствует или невалиден

**cURL Пример:**
```bash
curl -X GET https://api.webstorage.local/api/files \
  -H "Authorization: Bearer {accessToken}"
```

---

#### 📤 POST `/api/files/upload`

Загружает новый файл в хранилище.

**Request:**
```
POST /api/files/upload
Content-Type: multipart/form-data
Authorization: Bearer {accessToken}

[binary file data]
```

**Response (200 OK):**
```json
"550e8400-e29b-41d4-a716-446655440003"
```

(Возвращается ID загруженного файла)

**Ограничения:**
- Поддерживаются файлы любого формата
- Размер должен быть в пределах квоты пользователя
- Пустые файлы не допускаются

**Error Responses:**
- `400 Bad Request` - Файл пуст
- `401 Unauthorized` - Не авторизован
- `413 Payload Too Large` - Файл превышает квоту пользователя

**cURL Пример:**
```bash
curl -X POST https://api.webstorage.local/api/files/upload \
  -H "Authorization: Bearer {accessToken}" \
  -F "file=@/path/to/file.pdf"
```

**JavaScript Fetch Пример:**
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);

const response = await fetch('https://api.webstorage.local/api/files/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`
  },
  body: formData
});

const fileId = await response.json();
console.log('File uploaded with ID:', fileId);
```

---

#### 📥 GET `/api/files/{fileId}/download`

Скачивает файл из хранилища.

**Request:**
```
GET /api/files/{fileId}/download
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```
[binary file data]
```

**Headers:**
- `Content-Type`: application/octet-stream
- `Content-Disposition`: attachment; filename="{fileName}"

**Error Responses:**
- `401 Unauthorized` - Не авторизован
- `404 Not Found` - Файл не найден или не принадлежит пользователю

**cURL Пример:**
```bash
curl -O -J -X GET https://api.webstorage.local/api/files/550e8400-e29b-41d4-a716-446655440001/download \
  -H "Authorization: Bearer {accessToken}"
```

---

#### 🗑️ DELETE `/api/files/{fileId}`

Удаляет файл из хранилища.

**Request:**
```
DELETE /api/files/{fileId}
Authorization: Bearer {accessToken}
```

**Response (204 No Content):**
```
(пусто)
```

**Эффекты:**
- Файл удаляется безвозвратно
- Место в квоте пользователя освобождается

**Error Responses:**
- `401 Unauthorized` - Не авторизован
- `404 Not Found` - Файл не найден или не принадлежит пользователю

**cURL Пример:**
```bash
curl -X DELETE https://api.webstorage.local/api/files/550e8400-e29b-41d4-a716-446655440001 \
  -H "Authorization: Bearer {accessToken}"
```

---

### Storage Endpoints

#### 📊 GET `/api/storage/me`

Получает информацию о хранилище текущего пользователя (использованное место и квота).

**Request:**
```
GET /api/storage/me
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "usedBytes": 3072576,
  "maxBytes": 10737418240
}
```

**Поля ответа:**
- `userId` - ID пользователя
- `usedBytes` - Использованное место (в байтах)
- `maxBytes` - Максимальная квота (в байтах)

**Расчёт свободного места:**
```
freeBytes = maxBytes - usedBytes
percentageUsed = (usedBytes / maxBytes) * 100
```

**Error Responses:**
- `401 Unauthorized` - Не авторизован

**cURL Пример:**
```bash
curl -X GET https://api.webstorage.local/api/storage/me \
  -H "Authorization: Bearer {accessToken}"
```

---

### Admin Storage Endpoints

#### 🔧 PUT `/api/admin/storage/users/{userId}/quota`

*(Требует роль Administrator)*

Устанавливает квоту (максимальный размер) хранилища для пользователя.

**Request:**
```
PUT /api/admin/storage/users/{userId}/quota
Authorization: Bearer {adminToken}
Content-Type: application/json

{
  "maxBytes": 1073741824
}
```

**Параметры:**
- `userId` - ID пользователя (string, URL parameter)
- `maxBytes` - Новое значение максимального размера в байтах (long, в теле запроса)

**Примеры maxBytes:**
- `1073741824` = 1 GB
- `10737418240` = 10 GB
- `107374182400` = 100 GB

**Response (200 OK):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "usedBytes": 1024000,
  "maxBytes": 1073741824
}
```

**Error Responses:**
- `400 Bad Request` - Некорректное значение квоты (меньше используемого места)
- `401 Unauthorized` - Не авторизован
- `403 Forbidden` - Нет прав администратора
- `404 Not Found` - Пользователь не найден

**cURL Пример:**
```bash
curl -X PUT https://api.webstorage.local/api/admin/storage/users/550e8400-e29b-41d4-a716-446655440000/quota \
  -H "Authorization: Bearer {adminToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "maxBytes": 1073741824
  }'
```

---

## 📦 Модели данных

### AuthResponse
```json
{
  "accessToken": "string (JWT)",
  "email": "string (email)",
  "expiresAtUtc": "string (ISO 8601 UTC)"
}
```

### FileEntryDto
```json
{
  "id": "string (Guid)",
  "name": "string",
  "size": "integer (байты)",
  "uploadedAt": "string (ISO 8601 UTC)"
}
```

### UserStorageDto
```json
{
  "userId": "string (Guid)",
  "usedBytes": "integer (байты)",
  "maxBytes": "integer (байты)"
}
```

### Ошибка (ProblemDetails)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Описание ошибки",
  "instance": "/api/endpoint"
}
```

---

## ⚠️ Коды ошибок

| Код | Статус | Описание |
|-----|--------|---------|
| 200 | OK | Успешный запрос |
| 204 | No Content | Успешно, но без содержимого (например, при удалении) |
| 400 | Bad Request | Некорректные данные в запросе |
| 401 | Unauthorized | Требуется аутентификация / токен невалиден |
| 403 | Forbidden | Недостаточно прав доступа |
| 404 | Not Found | Ресурс не найден |
| 409 | Conflict | Конфликт (например, пользователь уже существует) |
| 413 | Payload Too Large | Файл превышает лимит |
| 500 | Internal Server Error | Ошибка на сервере |

---

## 💡 Примеры использования

### Пример 1: Полный цикл в Bash

```bash
#!/bin/bash

API="https://api.webstorage.local"
COOKIES="cookies.txt"

# 1. Регистрация
echo "🔐 Регистрация..."
RESPONSE=$(curl -s -c $COOKIES -X POST $API/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }')

ACCESS_TOKEN=$(echo $RESPONSE | grep -o '"accessToken":"[^"]*' | cut -d'"' -f4)
echo "✅ Токен: $ACCESS_TOKEN"

# 2. Получение информации о хранилище
echo "📊 Информация о хранилище..."
curl -s -X GET $API/api/storage/me \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq .

# 3. Загрузка файла
echo "📤 Загрузка файла..."
FILE_ID=$(curl -s -X POST $API/api/files/upload \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -F "file=@/path/to/file.txt")

echo "✅ Файл загружен с ID: $FILE_ID"

# 4. Получение списка файлов
echo "📂 Список файлов..."
curl -s -X GET $API/api/files \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq .

# 5. Скачивание файла
echo "📥 Скачивание файла..."
curl -X GET $API/api/files/$FILE_ID/download \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -o downloaded_file.txt

# 6. Удаление файла
echo "🗑️ Удаление файла..."
curl -X DELETE $API/api/files/$FILE_ID \
  -H "Authorization: Bearer $ACCESS_TOKEN"

# 7. Выход
echo "🚪 Выход..."
curl -s -b $COOKIES -X POST $API/api/auth/logout \
  -H "Authorization: Bearer $ACCESS_TOKEN"

echo "✅ Готово!"
```

---

### Пример 2: JavaScript/Fetch

```javascript
class WebStorageClient {
  constructor(baseUrl = 'https://api.webstorage.local') {
    this.baseUrl = baseUrl;
    this.accessToken = null;
  }

  async register(email, password) {
    const response = await fetch(`${this.baseUrl}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ email, password })
    });
    
    const data = await response.json();
    this.accessToken = data.accessToken;
    return data;
  }

  async login(email, password) {
    const response = await fetch(`${this.baseUrl}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ email, password })
    });
    
    const data = await response.json();
    this.accessToken = data.accessToken;
    return data;
  }

  async getStorageInfo() {
    const response = await fetch(`${this.baseUrl}/api/storage/me`, {
      headers: { 'Authorization': `Bearer ${this.accessToken}` }
    });
    return response.json();
  }

  async uploadFile(file) {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${this.baseUrl}/api/files/upload`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${this.accessToken}` },
      body: formData
    });

    return response.json();
  }

  async listFiles() {
    const response = await fetch(`${this.baseUrl}/api/files`, {
      headers: { 'Authorization': `Bearer ${this.accessToken}` }
    });
    return response.json();
  }

  async downloadFile(fileId, fileName) {
    const response = await fetch(`${this.baseUrl}/api/files/${fileId}/download`, {
      headers: { 'Authorization': `Bearer ${this.accessToken}` }
    });

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
  }

  async deleteFile(fileId) {
    const response = await fetch(`${this.baseUrl}/api/files/${fileId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${this.accessToken}` }
    });
    return response.ok;
  }

  async logout() {
    await fetch(`${this.baseUrl}/api/auth/logout`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${this.accessToken}` },
      credentials: 'include'
    });
    this.accessToken = null;
  }
}

// Использование:
const client = new WebStorageClient();

async function main() {
  // Вход
  await client.login('user@example.com', 'password');

  // Получить информацию
  const storage = await client.getStorageInfo();
  console.log(`Использовано: ${storage.usedBytes} / ${storage.maxBytes} байт`);

  // Загрузить файл
  const file = document.querySelector('input[type="file"]').files[0];
  const fileId = await client.uploadFile(file);
  console.log('Файл загружен:', fileId);

  // Список файлов
  const files = await client.listFiles();
  console.log('Ваши файлы:', files);
}
```

---

### Пример 3: cURL с обновлением токена

```bash
#!/bin/bash

API="https://api.webstorage.local"
COOKIES="cookies.txt"

# Логин
curl -c $COOKIES -X POST $API/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password"
  }' > auth_response.json

# Извлечение токена (требует jq)
ACCESS_TOKEN=$(jq -r '.accessToken' auth_response.json)

echo "Токен: $ACCESS_TOKEN"

# Использование токена
curl -X GET $API/api/storage/me \
  -H "Authorization: Bearer $ACCESS_TOKEN"

# Если токен истёк, обновить
curl -b $COOKIES -X POST $API/api/auth/refresh \
  -H "Content-Type: application/json" > refresh_response.json

NEW_ACCESS_TOKEN=$(jq -r '.accessToken' refresh_response.json)

echo "Новый токен: $NEW_ACCESS_TOKEN"
```

---

## 🔗 Ссылки

- **Swagger UI**: `/swagger/ui` (в разработке)
- **OpenAPI Schema**: `/openapi/v1.json`
- **Главный README**: `../README.md`
- **Инструкция по запуску**: `SETUP.md`
- **Аутентификация (подробнее)**: `AUTHENTICATION.md`

