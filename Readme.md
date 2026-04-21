# WebStorage — облачное файловое хранилище на ASP.NET Core 10

![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![C#](https://img.shields.io/badge/C%23-14.0-green)
![License](https://img.shields.io/badge/license-MIT-brightgreen)

## 📝 Описание

**WebStorage** — это современное RESTful API для облачного файлового хранилища, построенное на **ASP.NET Core 10** с использованием чистой архитектуры (Clean Architecture). Проект позволяет пользователям загружать, хранить, скачивать и управлять файлами с полной системой аутентификации и авторизации.

### ✨ Ключевые возможности

- 🔐 **JWT аутентификация** (Access + Refresh токены)
- 📁 **Управление файлами** (загрузка, скачивание, удаление, список)
- 📊 **Система квот** для ограничения использования места
- 👥 **Ролевая авторизация** (User, Admin)
- 🛡️ **OpenAPI/Swagger** документация
- 🎯 **Clean Architecture** (Domain, Application, Infrastructure, Api слои)
- ⚡ **Async/await** для всех операций
- 🔄 **CORS** поддержка
- 📋 **Глобальная обработка ошибок** (ProblemDetails)
- 🪵 **Логирование** всех операций

---

## 🛠 Технологии

| Технология | Версия | Назначение |
|-----------|--------|-----------|
| **.NET** | 10.0 | Framework |
| **C#** | 14.0 | Language |
| **ASP.NET Core** | 10.0 | Web framework |
| **Entity Framework Core** | 10.0 | ORM |
| **SQL Server / SQLite** | - | Database |
| **JWT** | - | Authentication |

---

## 📚 Документация

| Документ | Описание |
|----------|---------|
| **[API.md](./API.md)** | 📋 Полное описание всех API endpoints с примерами |
| **[AUTHENTICATION.md](./AUTHENTICATION.md)** | 🔐 Подробный гайд по JWT аутентификации и безопасности |
| **[SETUP.md](./SETUP.md)** | ⚙️ Инструкция по установке, конфигурации и запуску |

---

## 🚀 Быстрый старт

### Требования

- ✅ [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- ✅ Git
- ✅ Текстовый редактор или IDE (VS Code, Visual Studio)

### Установка

```bash
# 1. Клонируйте репозиторий
git clone https://github.com/LibSyfer/WebStorage.git
cd WebStorage

# 2. Восстановите зависимости
dotnet restore

# 3. Примените миграции БД
cd src/WebStorage.Api
dotnet ef database update

# 4. Запустите приложение
dotnet run

# 5. Откройте в браузере
# API: https://localhost:7001
# Swagger: https://localhost:7001/swagger/ui (если включен)
```

Подробнее см. [SETUP.md](./SETUP.md) 📖

---

## 📖 Структура проекта

```
WebStorage/
├── src/
│   ├── WebStorage.Api/              # ASP.NET Core REST API
│   │   ├── Controllers/             # API контроллеры
│   │   ├── ExceptionHandling/       # Глобальная обработка ошибок
│   │   └── Program.cs               # Конфигурация приложения
│   │
│   ├── WebStorage.Application/      # Business Logic (Use Cases)
│   │   ├── Auth/                    # Сервис аутентификации
│   │   ├── Storage/                 # Сервис управления файлами
│   │   └── StorageAccounts/         # Сервис управления аккаунтами
│   │
│   ├── WebStorage.Domain/           # Domain Models & Entities
│   │   ├── Entities/                # Domain entities
│   │   └── ValueObjects/            # Value objects
│   │
│   └── WebStorage.Infrastructure/   # External services & DB
│       ├── Data/                    # EF Core DbContext
│       └── Options/                 # Configuration options
│
├── API.md                           # 📋 API документация
├── AUTHENTICATION.md                # 🔐 Аутентификация
├── SETUP.md                         # ⚙️ Установка
└── README.md                        # Этот файл

```

---

## 🔌 API Endpoints

### 🔐 Аутентификация (`/api/auth`)

| Метод | Endpoint | Описание | Auth |
|-------|----------|---------|------|
| `POST` | `/register` | Регистрация пользователя | ❌ |
| `POST` | `/login` | Вход в систему | ❌ |
| `POST` | `/refresh` | Обновление токена | ❌* |
| `POST` | `/logout` | Выход из системы | ✅ |

### 📁 Файлы (`/api/files`)

| Метод | Endpoint | Описание | Auth |
|-------|----------|---------|------|
| `GET` | `/` | Список файлов пользователя | ✅ |
| `POST` | `/upload` | Загрузить файл | ✅ |
| `GET` | `/{fileId}/download` | Скачать файл | ✅ |
| `DELETE` | `/{fileId}` | Удалить файл | ✅ |

### 📊 Хранилище (`/api/storage`)

| Метод | Endpoint | Описание | Auth |
|-------|----------|---------|------|
| `GET` | `/me` | Информация о хранилище | ✅ |

### 🔧 Администрирование (`/api/admin/storage`)

| Метод | Endpoint | Описание | Auth | Роль |
|-------|----------|---------|------|------|
| `PUT` | `/users/{userId}/quota` | Установить квоту пользователю | ✅ | Admin |

---

## 💡 Примеры использования

### 1️⃣ Регистрация и вход

```bash
# Регистрация
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'

# Ответ:
# {
#   "accessToken": "eyJhbGc...",
#   "expiresIn": 3600,
#   "tokenType": "Bearer",
#   "user": {...}
# }
```

### 2️⃣ Загрузка файла

```bash
# Используйте accessToken из предыдущего ответа
curl -X POST https://localhost:7001/api/files/upload \
  -H "Authorization: Bearer {accessToken}" \
  -F "file=@/path/to/file.pdf"

# Ответ: ID файла (GUID)
```

### 3️⃣ Список файлов

```bash
curl -X GET https://localhost:7001/api/files \
  -H "Authorization: Bearer {accessToken}"

# Ответ:
# [
#   {
#     "id": "550e8400-...",
#     "name": "document.pdf",
#     "size": 2048576,
#     "uploadedAt": "2024-01-15T10:30:00Z"
#   }
# ]
```

### 4️⃣ Скачивание файла

```bash
curl -X GET https://localhost:7001/api/files/550e8400-e29b-41d4-a716-446655440001/download \
  -H "Authorization: Bearer {accessToken}" \
  -o downloaded_file.pdf
```

### 5️⃣ Получить информацию о хранилище

```bash
curl -X GET https://localhost:7001/api/storage/me \
  -H "Authorization: Bearer {accessToken}"

# Ответ:
# {
#   "userId": "550e8400-...",
#   "usedBytes": 3072576,
#   "maxBytes": 10737418240,
#   "filesCount": 2
# }
```

Больше примеров см. в [API.md](./API.md) 📖

---

## 🔐 Безопасность

### Аутентификация

- ✅ **JWT токены** с подписью (HS256)
- ✅ **Access Token** (короткоживущий, 15-60 мин)
- ✅ **Refresh Token** (длительный, в HttpOnly cookie)
- ✅ **Защита от XSS** атак (HttpOnly cookies)
- ✅ **Защита от CSRF** атак

### Авторизация

- ✅ **Ролевая система** (User, Admin)
- ✅ **Изоляция данных** (пользователь видит только свои файлы)
- ✅ **Проверка прав** на каждый endpoint

### Остальное

- ✅ **HTTPS** в production
- ✅ **CORS** для контроля доступа
- ✅ **Глобальная обработка ошибок** (не утечка информации)
- ✅ **Валидация входных данных**

Подробнее см. [AUTHENTICATION.md](./AUTHENTICATION.md) 🔐

---

## 📦 Требования

### Системные

- Windows, macOS, Linux
- .NET SDK 10.0+
- Git

### Базы данных (выберите одну)

- SQL Server 2019+
- PostgreSQL 12+
- SQLite 3

### Опционально

- Docker (для контейнеризации)
- Visual Studio 2025+ или VS Code

---

## 🚀 Развёртывание

### Development

```bash
dotnet run
```

### Production

```bash
# Сборка
dotnet publish -c Release -o ./publish

# Запуск
cd publish
dotnet WebStorage.Api.dll
```

### Docker

```bash
docker build -t webstorage-api .
docker run -p 8080:80 webstorage-api
```

### Azure

```bash
az webapp deployment source config-zip --resource-group mygroup \
  --name webstorage-api \
  --src app.zip
```

Подробнее см. [SETUP.md](./SETUP.md) ⚙️

---

## 🧪 Тестирование

### Модульные тесты

```bash
dotnet test
```

### Интеграционные тесты

```bash
dotnet test --filter Category=Integration
```

### cURL примеры для ручного тестирования

Используйте готовые примеры из документации:
- [API.md - Примеры](./API.md#-примеры-использования)

---

## 🐛 Решение проблем

### Ошибка подключения к БД

```bash
# Проверьте строку подключения в appsettings.json
# Убедитесь, что БД запущена

# Создайте БД с нуля:
dotnet ef database drop --force
dotnet ef database update
```

### Ошибка SSL сертификата

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Port уже занят

```bash
dotnet run --urls "https://localhost:7002"
```

Больше решений см. в [SETUP.md - Решение проблем](./SETUP.md#-решение-проблем) 🔧

---

## 📋 Конфигурация

### Переменные окружения

```bash
# JWT конфигурация
JWT__SECRET=your-secret-key-min-32-chars
JWT__ISSUER=WebStorage
JWT__AUDIENCE=WebStorageUsers

# База данных
ConnectionStrings__DefaultConnection=Server=localhost;Database=WebStorage;...

# Хранилище файлов
Storage__RootPath=/var/storage

# Логирование
Logging__LogLevel__Default=Information
```

### appsettings.json

```json
{
  "Jwt": {
    "Secret": "your-secret-key-min-32-chars",
    "Issuer": "WebStorage",
    "Audience": "WebStorageUsers",
    "ExpirationMinutes": 60
  },
  "Storage": {
    "MaxFileSize": 1073741824,
    "RootPath": "./files"
  }
}
```

Подробнее см. [SETUP.md - Конфигурация](./SETUP.md#-конфигурация) ⚙️

---

## 📊 Архитектура

### Clean Architecture слои

```
┌─────────────────────────────────────┐
│         WebStorage.Api              │ REST Controllers
├─────────────────────────────────────┤
│      WebStorage.Application         │ Use Cases, Services
├─────────────────────────────────────┤
│       WebStorage.Domain             │ Entities, Value Objects
├─────────────────────────────────────┤
│     WebStorage.Infrastructure       │ DB, External Services
└─────────────────────────────────────┘
```

### Dependency Flow

```
API Controllers
    ↓
Application Services (Use Cases)
    ↓
Domain Entities & Logic
    ↓
Infrastructure (DB, File System)
```

**Принципы:**
- ✅ Слой выше не знает о слое ниже
- ✅ Зависимости направлены внутрь (к Domain)
- ✅ Легко тестировать каждый слой отдельно

---

## 👥 Вклад в проект

1. Fork репозиторий
2. Создайте feature branch (`git checkout -b feature/amazing-feature`)
3. Commit изменения (`git commit -m 'Add amazing feature'`)
4. Push в branch (`git push origin feature/amazing-feature`)
5. Откройте Pull Request

### Стандарты кода

- ✅ Используйте **C# 14** возможности
- ✅ Следуйте **Microsoft Coding Conventions**
- ✅ Добавляйте **XML комментарии** для public методов
- ✅ Пишите **unit тесты** для новой логики

---

## 📜 Лицензия

Этот проект лицензирован под MIT License — см. [LICENSE](./LICENSE) файл для деталей.

---

## 🔗 Полезные ссылки

### Документация проекта

- **[API.md](./API.md)** — Полное описание всех endpoints
- **[AUTHENTICATION.md](./AUTHENTICATION.md)** — JWT аутентификация и безопасность
- **[SETUP.md](./SETUP.md)** — Установка и конфигурация

### Официальные ресурсы

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

### GitHub

- **Repository**: https://github.com/LibSyfer/WebStorage
- **Issues**: https://github.com/LibSyfer/WebStorage/issues
- **Discussions**: https://github.com/LibSyfer/WebStorage/discussions

---

## 📞 Контакты и поддержка

- 📧 Email: support@webstorage.local
- 🐛 Issues: Используйте [GitHub Issues](https://github.com/LibSyfer/WebStorage/issues)
- 💬 Discussions: Используйте [GitHub Discussions](https://github.com/LibSyfer/WebStorage/discussions)

---

## 🙏 Благодарности

Спасибо всем контрибьюторам и пользователям за поддержку проекта!

---

**Создано с ❤️ на ASP.NET Core 10**